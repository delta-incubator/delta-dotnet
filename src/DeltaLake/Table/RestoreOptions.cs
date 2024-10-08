using System;
using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for restoring a delta table
    /// </summary>
    public record RestoreOptions
    {
        /// <summary>
        /// Optional version. Mutually excluse with <see cref="Timestamp"/>
        /// </summary>
        public ulong? Version { get; init; }

        /// <summary>
        /// Timestamp restore point. Mutually exclusive with <see cref="Version"/>
        /// </summary>
        public DateTimeOffset? Timestamp { get; init; }

        /// <summary>
        /// Ignore missing files during restoration
        /// </summary>
        public bool IgnoreMissingFiles { get; init; }

        /// <summary>
        /// Allow protocol downgrades
        /// </summary>
        public bool ProtocolDowngradeAllowed { get; init; }

        /// <summary>
        /// Custom metadata
        /// </summary>
        public Dictionary<string, string> CustomMetadata { get; init; } = new Dictionary<string, string>();
    }
}