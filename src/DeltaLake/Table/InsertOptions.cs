namespace DeltaLake.Table
{
    /// <summary>
    /// Options for inserting data into a table
    /// </summary>
    public record InsertOptions
    {
        /// <summary>
        /// Predicate for insertion.
        /// Represents the WHERE clause, not including WHERE
        /// </summary>
        public string? Predicate { get; init; }

        /// <summary>
        /// <see cref="SaveMode" />
        /// </summary>
        public SaveMode SaveMode { get; init; }

        /// <summary>
        /// Maximum number of rows to write per row group
        /// </summary>
        public ulong MaxRowsPerGroup { get; init; } = 100;

        /// <summary>
        /// Overwrite schema with schema from record batch
        /// </summary>
        public bool OverwriteSchema { get; init; }

        /// <summary>
        /// Indicates a valid configuration
        /// </summary>
        public bool IsValid => !(this.SaveMode == SaveMode.Append && OverwriteSchema);
    }
}