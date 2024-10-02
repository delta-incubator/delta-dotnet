using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Detla table storage options.
    /// </summary>
    public record TableStorageOptions
    {
        /// <summary>
        /// A map of string options
        /// </summary>
        public Dictionary<string, string> StorageOptions { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// Location of the delta table
        /// memory://, s3://, azure://, etc
        /// </summary>
        public string TableLocation { get; init; } = string.Empty;
    }
}