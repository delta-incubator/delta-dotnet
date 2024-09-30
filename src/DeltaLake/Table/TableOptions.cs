using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for creating a new delta table
    /// </summary>
    public class TableOptions
    {
        /// <summary>
        /// Optional version of the table to load
        /// </summary>
        public ulong? Version { get; set; }

        /// <summary>
        /// A map of string options
        /// </summary>
        public Dictionary<string, string> StorageOptions { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether or not to load files when building the table
        /// </summary>
        public bool WithoutFiles { get; set; }

        /// <summary>
        /// Buffer size for log files
        /// </summary>
        public uint? LogBufferSize { get; set; }
    }
}