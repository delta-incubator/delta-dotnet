using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for adding table features.
    /// </summary>
    public record AddTableFeatureOptions
    {
        /// <summary>
        /// Gets whether the operation may increase the table's minimum protocol versions.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// A protocol upgrade is irreversible and can make the table unreadable or unwritable by
        /// older clients. Adding <see cref="TableFeature.V2Checkpoint"/> to a classic table
        /// requires this option to be <see langword="true"/>.
        /// </remarks>
        public bool AllowProtocolVersionsIncrease { get; init; }

        /// <summary>
        /// Gets optional custom metadata to include in the commit information.
        /// </summary>
        public Dictionary<string, string>? CustomMetadata { get; init; }
    }
}