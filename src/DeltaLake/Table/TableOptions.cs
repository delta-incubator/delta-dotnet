using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for loading a new delta table
    /// </summary>
    public record TableOptions : TableStorageOptions
    {
        /// <summary>
        /// Optional version of the table to load
        /// </summary>
        public ulong? Version { get; set; }

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