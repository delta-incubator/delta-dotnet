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

        /// <summary>
        /// Application identifier for idempotent writes (Delta Protocol txn action).
        /// When set along with <see cref="TransactionVersion"/>, a SetTransaction action
        /// is included in the commit for exactly-once semantics.
        /// </summary>
        public string? AppId { get; init; }

        /// <summary>
        /// Application-specific version for idempotent writes (Delta Protocol txn action).
        /// Must be set together with <see cref="AppId"/>.
        /// </summary>
        public long? TransactionVersion { get; init; }
    }
}
