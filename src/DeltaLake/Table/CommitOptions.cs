using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for committing a write transaction (log-only commit).
    /// </summary>
    public record CommitOptions
    {
        /// <summary>
        /// Optional custom metadata to include in the commit.
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; }
    }
}
