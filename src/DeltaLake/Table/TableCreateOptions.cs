using System.Collections.Generic;
using Apache.Arrow;

namespace DeltaLake.Table
{

    /// <summary>
    /// Options passed when creating a table
    /// </summary>
    public record TableCreateOptions : TableStorageOptions
    {
        /// <summary>
        /// Creates an instance of create options, ensuring its mandatory properties
        /// </summary>
        /// <param name="location">Table Uri</param>
        /// <param name="schema">Table schema in Arrow Format</param>
        public TableCreateOptions(string location, Schema schema)
        {
            Schema = schema;
            TableLocation = location;
        }

        /// <summary>
        /// Arrow Schema for the table
        /// </summary>
        public Schema Schema { get; }

        /// <summary>
        /// Location of the delta table
        /// memory://, s3://, azure://, etc
        /// </summary>
        public string TableLocation { get; }

        /// <summary>
        /// List of columns to use for partitioning
        /// </summary>
        public ICollection<string> PartitionBy { get; init; } = new List<string>();

        /// <summary>
        /// Save mode
        /// </summary>
        public SaveMode SaveMode { get; set; }

        /// <summary>
        /// Optional name for tabe
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Optional description for the  table
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional configuration
        /// </summary>

        public Dictionary<string, string>? Configuration { get; init; }

        /// <summary>
        /// Table metadata
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; }
    }
}