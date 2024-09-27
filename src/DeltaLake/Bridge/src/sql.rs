use arrow::{
    datatypes::SchemaRef,
    error::ArrowError,
    record_batch::{RecordBatch, RecordBatchReader},
};
use deltalake::datafusion::{
    physical_plan::SendableRecordBatchStream,
    sql::sqlparser::{
        ast::{Assignment, Expr, Ident, TableFactor, Values},
        dialect::{Dialect, GenericDialect},
        keywords::Keyword,
        parser::{Parser, ParserError},
        tokenizer::{Token, Tokenizer},
    },
};
use futures::StreamExt;

use crate::error::{DeltaTableError, DeltaTableErrorCode};

macro_rules! make_update {
    ($update:ident, $predicate:ident, $assignments:ident) => {{
        if let Some(predicate) = $predicate {
            $update = $update.predicate(predicate.to_string());
        }

        for assign in $assignments {
            match assign.target {
                AssignmentTarget::ColumnName(object_name) => {
                    for col in object_name.0 {
                        $update = $update.update(col.to_string(), assign.value.to_string());
                    }
                },
                AssignmentTarget::Tuple(vec) => {
                    for item in vec {
                        for col in item.0 {
                            $update = $update.update(col.to_string(), assign.value.to_string());
                        }
                    }
                },
            };
        }
        $update
    }};
}

pub struct DeltaLakeParser<'a> {
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
            parser: Parser::new(dialect).with_tokens(tokens),
        })
    }

    pub fn parse_merge(&mut self) -> Result<Statement, ParserError> {
        self.parser.expect_keyword(Keyword::MERGE)?;
        self.parser.expect_keyword(Keyword::INTO)?;

        let table = self.parser.parse_table_factor()?;

        self.parser.expect_keyword(Keyword::USING)?;
        let source = self.parser.parse_table_factor()?;
        self.parser.expect_keyword(Keyword::ON)?;
        let on = self.parser.parse_expr()?;
        let clauses = self.parse_merge_clauses()?;

        Ok(Statement::MergeStatement {
            into: true,
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
// #[cfg_attr(feature = "visitor", derive(Visit, VisitMut))]
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

pub fn extract_table_factor_alias(table: TableFactor) -> Option<String> {
    match table {
        TableFactor::Table {
            name,
            alias,
            ..
        } => alias.map(|a| a.to_string()).or(Some(name.to_string())),
        TableFactor::Derived {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::TableFunction { expr: _, alias } => alias.map(|a| a.to_string()),
        TableFactor::Function {
            lateral: _,
            name: _,
            args: _,
            alias,
        } => alias.map(|a| a.to_string()),
        TableFactor::UNNEST {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::NestedJoin {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::Pivot {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::Unpivot {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::JsonTable {
            alias,
            ..
        } => alias.map(|a| a.to_string()),
        TableFactor::MatchRecognize { 
            alias,
            ..
         } => alias.map(|a| a.to_string()),
    }
}

pub(crate) struct DataFrameStreamIterator {
    stream: SendableRecordBatchStream,
    schema: SchemaRef,
}

impl DataFrameStreamIterator {
    #[allow(dead_code)]
    pub(crate) fn new(stream: SendableRecordBatchStream, schema: SchemaRef) -> Self {
        Self { stream, schema }
    }
}
impl RecordBatchReader for DataFrameStreamIterator {
    fn schema(&self) -> arrow::datatypes::SchemaRef {
        self.schema.clone()
    }
}

impl Iterator for DataFrameStreamIterator {
    type Item = Result<RecordBatch, ArrowError>;

    fn next(&mut self) -> Option<Self::Item> {
        let result = futures::executor::block_on(self.stream.next());
        result.map(|result| {
            result.map_err(|err| match err {
                deltalake::datafusion::error::DataFusionError::ArrowError(arrow, _) => arrow,
                _ => ArrowError::ComputeError(err.to_string()),
            })
        })
    }
}

#[cfg(test)]
mod tests {
    use crate::runtime::{Runtime, RuntimeOptions};

    use super::DeltaLakeParser;

    #[test]
    fn test_parser() {
        let mut runtime = Runtime::new(&RuntimeOptions{}).unwrap();
        let mut parser = DeltaLakeParser::new("UPDATE test SET test = test + CAST(1 AS INT) WHERE test > CAST(1 AS INT)").unwrap();
        parser.parse_update(&mut runtime).unwrap();
    }
}