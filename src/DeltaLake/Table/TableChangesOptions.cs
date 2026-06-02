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
        /// The first version to include in the change feed (inclusive).
        /// </summary>
        public ulong StartVersion { get; init; }

        /// <summary>
        /// The last version to include in the change feed (inclusive).
        /// When <see langword="null"/>, reads from <see cref="StartVersion"/> to the
        /// latest available version.
        /// </summary>
        public ulong? EndVersion { get; init; }
    }
}
