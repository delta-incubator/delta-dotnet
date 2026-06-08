// -----------------------------------------------------------------------------
// <summary>
// Options for querying Change Data Feed (CDC) records from a Delta table.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace DeltaLake.Table
{
    /// <summary>
    /// Options that control which versions are included in a Change Data Feed query.
    /// </summary>
    /// <remarks>
    /// The target table must have <c>delta.enableChangeDataFeed = true</c> set in its
    /// configuration. Each returned batch will include the system columns
    /// <c>_change_type</c>, <c>_commit_version</c>, and <c>_commit_timestamp</c>.
    /// </remarks>
    public record TableChangesOptions
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TableChangesOptions"/>.
        /// </summary>
        /// <param name="startVersion">The first commit version to include (inclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <see cref="EndVersion"/> is set and is less than <paramref name="startVersion"/>.
        /// </exception>
        public TableChangesOptions(ulong startVersion)
        {
            StartVersion = startVersion;
        }

        /// <summary>
        /// The first version to include in the change feed (inclusive).
        /// </summary>
        public ulong StartVersion { get; init; }

        private ulong? endVersion;

        /// <summary>
        /// The last version to include in the change feed (inclusive).
        /// When <see langword="null"/>, reads from <see cref="StartVersion"/> to the
        /// latest available version.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the value is less than <see cref="StartVersion"/>.
        /// </exception>
        public ulong? EndVersion
        {
            get => endVersion;
            init
            {
                if (value.HasValue && value.Value < StartVersion)
                {
                    throw new System.ArgumentOutOfRangeException(
                        nameof(EndVersion),
                        $"EndVersion ({value.Value}) must be greater than or equal to StartVersion ({StartVersion}).");
                }
                endVersion = value;
            }
        }
    }
}
