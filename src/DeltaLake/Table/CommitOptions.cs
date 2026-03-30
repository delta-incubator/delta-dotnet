using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for committing a write transaction (log-only commit).
    /// </summary>
    public record CommitOptions
    {
        /// <summary>
        /// Engine info string included in the commitInfo action.
        /// Example: "delta-dotnet/0.31.1". Null to skip.
        /// </summary>
        public string? EngineInfo { get; init; }

        /// <summary>
        /// Optional custom metadata to include in the commit.
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; }
    }
}
