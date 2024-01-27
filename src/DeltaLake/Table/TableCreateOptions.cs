using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Arrow;

namespace DeltaLake.Table
{

    /// <summary>
    /// Options passed when creating a table
    /// </summary>
    public class TableCreateOptions
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
        public Schema Schema { get; } = new Schema(Enumerable.Empty<Field>(), Enumerable.Empty<KeyValuePair<string, string>>());

        /// <summary>
        /// Location of the delta table
        /// memory://, s3://, azure://, etc
        /// </summary>
        public string TableLocation { get; } = String.Empty;

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

        public Dictionary<string, string?>? Configuration { get; init; }

        /// <summary>
        /// Storage options to pass to table builder
        /// </summary>
        public Dictionary<string, string>? StorageOptions { get; init; }

        /// <summary>
        /// Table metadata
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; }
    }
}