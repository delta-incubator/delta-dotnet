namespace DeltaLake.Table
{
    /// <summary>
    /// Strongly types query wrapper for select queries
    /// </summary>
    public class SelectQuery
    {
        /// <summary>
        /// Create a query with the provided query and default table alias 'deltatable'
        /// </summary>
        /// <param name="query"></param>

        public SelectQuery(string query)
        {
            Query = query;
        }

        /// <summary>
        /// Select query in the form of a sql select statement
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// The name for the table used in the select query
        /// </summary>
        public string TableAlias { get; init; } = "deltatable";
    }
}