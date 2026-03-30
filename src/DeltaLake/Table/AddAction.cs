using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Represents a file add action for log-only commits.
    /// Describes a pre-written Parquet data file to be registered in the Delta log.
    /// </summary>
    public record AddAction
    {
        /// <summary>
        /// Relative path to the data file from the table root.
        /// Example: "year=2024/month=01/part-00000-abc123.snappy.parquet"
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// Size of the data file in bytes.
        /// </summary>
        public long Size { get; init; }

        /// <summary>
        /// Partition column values. Keys are partition column names,
        /// values are the partition values (null for null partitions).
        /// Empty dictionary for non-partitioned tables.
        /// </summary>
        public IReadOnlyDictionary<string, string?> PartitionValues { get; init; }
            = new Dictionary<string, string?>();

        /// <summary>
        /// Modification time of the file in milliseconds since Unix epoch.
        /// </summary>
        public long ModificationTime { get; init; }

        /// <summary>
        /// Whether this action represents a data change. True for new data writes.
        /// </summary>
        public bool DataChange { get; init; } = true;
    }
}
