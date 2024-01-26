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

        public Schema Schema { get; } = new Schema(Enumerable.Empty<Field>(), Enumerable.Empty<KeyValuePair<string, string>>());

        public string TableLocation { get; } = String.Empty;

        public List<string> PartitionBy { get; } = new List<string>();

        public SaveMode SaveMode { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public Dictionary<string, string?>? Configuration { get; set; }

        public Dictionary<string, string>? StorageOptions { get; set; }

        public Dictionary<string, string>? CustomMetadata { get; set; }
    }
}