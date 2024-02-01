use std::collections::VecDeque;

use deltalake::datafusion::sql::{
    parser::{DFParser, Statement as DFStatement},
    sqlparser::{
        ast::{Assignment, Expr, Ident, TableAlias, TableFactor, Values},
        dialect::{Dialect, GenericDialect},
        keywords::Keyword,
        parser::{Parser, ParserError},
        tokenizer::{Token, TokenWithLocation, Tokenizer},
    },
};

use crate::error::{DeltaTableError, DeltaTableErrorCode};

macro_rules! parser_err {
    ($MSG:expr) => {
        Err(ParserError::ParserError($MSG.to_string()))
    };
}

macro_rules! make_update {
    ($update:ident, $predicate:ident, $assignments:ident) => {{
        if let Some(predicate) = $predicate {
            $update = $update.predicate(predicate.to_string());
        }

        for assign in $assignments {
            for col in assign.id {
                $update = $update.update(col.to_string(), assign.value.to_string());
            }
        }
        $update
    }};
}

pub struct DeltaLakeParser<'a> {
    sql: &'a str,
    parser: Parser<'a>,
}

impl<'a> DeltaLakeParser<'a> {
    /// Create a new parser for the specified tokens using the [`GenericDialect`].
    pub fn new(sql: &'a str) -> Result<Self, ParserError> {
        let dialect = &GenericDialect {};
        Self::new_with_dialect(sql, dialect)
    }

    /// Create a new parser for the specified tokens with the
    /// specified dialect.
    pub fn new_with_dialect(sql: &'a str, dialect: &'a dyn Dialect) -> Result<Self, ParserError> {
        let mut tokenizer = Tokenizer::new(dialect, sql);
        let tokens = tokenizer.tokenize()?;

        Ok(Self {
            sql,
            parser: Parser::new(dialect).with_tokens(tokens),
        })
    }
    /// Parse a sql string into one or [`Statement`]s using the
    /// [`GenericDialect`].
    pub fn parse_sql(sql: impl AsRef<str>) -> Result<VecDeque<Statement>, ParserError> {
        let dialect: &GenericDialect = &GenericDialect {};
        DeltaLakeParser::parse_sql_with_dialect(sql.as_ref(), dialect)
    }

    /// Parse a SQL string and produce one or more [`Statement`]s with
    /// with the specified dialect.
    pub fn parse_sql_with_dialect(
        sql: &str,
        dialect: &dyn Dialect,
    ) -> Result<VecDeque<Statement>, ParserError> {
        let mut parser = DeltaLakeParser::new_with_dialect(sql, dialect)?;
        let mut stmts = VecDeque::new();
        let mut expecting_statement_delimiter = false;
        loop {
            // ignore empty statements (between successive statement delimiters)
            while parser.parser.consume_token(&Token::SemiColon) {
                expecting_statement_delimiter = false;
            }

            if parser.parser.peek_token() == Token::EOF {
                break;
            }
            if expecting_statement_delimiter {
                return parser.expected("end of statement", parser.parser.peek_token());
            }

            let statement = parser.parse_statement()?;
            stmts.push_back(statement);
            expecting_statement_delimiter = true;
        }

        Ok(stmts)
    }

    /// Report an unexpected token
    fn expected<T>(&self, expected: &str, found: TokenWithLocation) -> Result<T, ParserError> {
        parser_err!(format!("Expected {expected}, found: {found}"))
    }

    /// Parse a new expression
    pub fn parse_statement(&mut self) -> Result<Statement, ParserError> {
        match self.parser.peek_token().token {
            Token::Word(w) => {
                match w.keyword {
                    Keyword::MERGE => {
                        self.parser.next_token();
                        self.parse_merge()
                    }
                    _ => {
                        // use the native parser
                        // TODO fix for multiple statememnts and keeping parsers in sync
                        let mut df = DFParser::new(self.sql)?;
                        let stmt = df.parse_statement()?;
                        self.parser.parse_statement()?;
                        Ok(Statement::Datafusion(stmt))
                    }
                }
            }
            _ => {
                // use the native parser
                // TODO fix for multiple statememnts and keeping parsers in sync
                let mut df = DFParser::new(self.sql)?;
                let stmt = df.parse_statement()?;
                self.parser.parse_statement()?;
                Ok(Statement::Datafusion(stmt))
            }
        }
    }

    pub fn parse_merge(&mut self) -> Result<Statement, ParserError> {
        let into = self.parser.parse_keyword(Keyword::INTO);

        let table = self.parser.parse_table_factor()?;

        self.parser.expect_keyword(Keyword::USING)?;
        let source = self.parser.parse_table_factor()?;
        self.parser.expect_keyword(Keyword::ON)?;
        let on = self.parser.parse_expr()?;
        let clauses = self.parse_merge_clauses()?;

        Ok(Statement::MergeStatement {
            into,
            table,
            source,
            on: Box::new(on),
            clauses,
        })
    }

    pub fn parse_update(
        &mut self,
        runtime: &mut crate::runtime::Runtime,
    ) -> Result<(Option<Expr>, Vec<Assignment>), crate::error::DeltaTableError> {
        let statement = self
            .parser
            .parse_statement()
            .map_err(|error| crate::error::DeltaTableError::from_parser_error(runtime, error))?;
        match statement {
            deltalake::datafusion::sql::sqlparser::ast::Statement::Update {
                table: _,
                assignments,
                from: _,
                selection,
                returning: _,
            } => Ok((selection, assignments)),
            _ => Err(DeltaTableError::new(
                runtime,
                DeltaTableErrorCode::SqlParser,
                "invalid sql query",
            )),
        }
    }

    pub fn parse_merge_clauses(&mut self) -> Result<Vec<MergeClause>, ParserError> {
        let mut clauses: Vec<MergeClause> = vec![];
        loop {
            if self.parser.peek_token() == Token::EOF
                || self.parser.peek_token() == Token::SemiColon
            {
                break;
            }
            self.parser.expect_keyword(Keyword::WHEN)?;

            let is_not_matched = self.parser.parse_keyword(Keyword::NOT);
            self.parser.expect_keyword(Keyword::MATCHED)?;

            let is_by_source = if is_not_matched {
                match self.parser.peek_token().token {
                    Token::Word(w) if w.value.to_lowercase() == "by" => {
                        self.parser.next_token();
                        match self.parser.peek_token().token {
                            Token::Word(w) if w.value.to_lowercase() == "source" => {
                                self.parser.next_token();
                                true
                            }
                            Token::Word(w) if w.value.to_lowercase() == "target" => {
                                self.parser.next_token();
                                false
                            }
                            _ => false,
                        }
                    }
                    _ => false,
                }
            } else {
                false
            };

            let predicate = if self.parser.parse_keyword(Keyword::AND) {
                Some(self.parser.parse_expr()?)
            } else {
                None
            };

            self.parser.expect_keyword(Keyword::THEN)?;

            clauses.push(
                match self.parser.parse_one_of_keywords(&[
                    Keyword::UPDATE,
                    Keyword::INSERT,
                    Keyword::DELETE,
                ]) {
                    Some(Keyword::UPDATE) => {
                        if is_not_matched && !is_by_source {
                            return Err(ParserError::ParserError(
                                "UPDATE in NOT MATCHED merge clause".to_string(),
                            ));
                        }
                        self.parser.expect_keyword(Keyword::SET)?;
                        let assignments = self
                            .parser
                            .parse_comma_separated(Parser::parse_assignment)?;
                        if is_by_source {
                            MergeClause::NotMatchedBySourceUpdate {
                                predicate,
                                assignments,
                            }
                        } else {
                            MergeClause::MatchedUpdate {
                                predicate,
                                assignments,
                            }
                        }
                    }
                    Some(Keyword::DELETE) => {
                        if is_not_matched && !is_by_source {
                            return Err(ParserError::ParserError(
                                "DELETE in NOT MATCHED merge clause".to_string(),
                            ));
                        }

                        if is_not_matched {
                            MergeClause::NotMatchedBySourceDelete(predicate)
                        } else {
                            MergeClause::MatchedDelete(predicate)
                        }
                    }
                    Some(Keyword::INSERT) => {
                        if is_by_source {
                            return Err(ParserError::ParserError(
                                "INSERT in NOT MATCHED BY SOURCE merge clause".into(),
                            ));
                        }

                        if !is_not_matched {
                            return Err(ParserError::ParserError(
                                "INSERT in MATCHED merge clause".to_string(),
                            ));
                        }
                        let columns = self.parser.parse_parenthesized_column_list(
                            deltalake::datafusion::sql::sqlparser::parser::IsOptional::Optional,
                            false,
                        )?;
                        self.parser.expect_keyword(Keyword::VALUES)?;
                        let values = self.parser.parse_values(false)?;
                        MergeClause::NotMatched {
                            predicate,
                            columns,
                            values,
                        }
                    }
                    Some(_) => {
                        return Err(ParserError::ParserError(
                            "expected UPDATE, DELETE or INSERT in merge clause".to_string(),
                        ));
                    }
                    None => {
                        return Err(ParserError::ParserError(
                            "expected UPDATE, DELETE or INSERT in merge clause".to_string(),
                        ));
                    }
                },
            );
        }
        Ok(clauses)
    }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Statement {
    /// Datafusion AST node (from datafusion-sql)
    Datafusion(DFStatement),
    MergeStatement {
        into: bool,
        // Specifies the table to merge
        table: TableFactor,
        // Specifies the table or subquery to join with the target table
        source: TableFactor,
        // Specifies the expression on which to join the target table and source
        on: Box<Expr>,
        // Specifies the actions to perform when values match or do not match.
        clauses: Vec<MergeClause>,
    },
}

#[derive(Debug, Clone, PartialEq, PartialOrd, Eq, Ord, Hash)]
#[cfg_attr(feature = "visitor", derive(Visit, VisitMut))]
pub enum MergeClause {
    MatchedUpdate {
        predicate: Option<Expr>,
        assignments: Vec<Assignment>,
    },
    MatchedDelete(Option<Expr>),
    NotMatched {
        predicate: Option<Expr>,
        columns: Vec<Ident>,
        values: Values,
    },
    NotMatchedBySourceUpdate {
        predicate: Option<Expr>,
        assignments: Vec<Assignment>,
    },
    NotMatchedBySourceDelete(Option<Expr>),
}

pub fn extract_table_factor_alias(table: TableFactor) -> Option<TableAlias> {
    match table {
        TableFactor::Table {
            name: _,
            alias,
            args: _,
            with_hints: _,
            version: _,
            partitions: _,
        } => alias,
        TableFactor::Derived {
            lateral: _,
            subquery: _,
            alias,
        } => alias,
        TableFactor::TableFunction { expr: _, alias } => alias,
        TableFactor::Function {
            lateral: _,
            name: _,
            args: _,
            alias,
        } => alias,
        TableFactor::UNNEST {
            alias,
            array_exprs: _,
            with_offset: _,
            with_offset_alias: _,
        } => alias,
        TableFactor::NestedJoin {
            table_with_joins: _,
            alias,
        } => alias,
        TableFactor::Pivot {
            table: _,
            aggregate_function: _,
            value_column: _,
            pivot_values: _,
            alias,
        } => alias,
        TableFactor::Unpivot {
            table: _,
            value: _,
            name: _,
            columns: _,
            alias,
        } => alias,
    }
}
