// -----------------------------------------------------------------------------
// <summary>
// Memory safe kernel state that makes it easy to encapsulate parent/child
// relationships.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// Memory safe state management interface.
    /// </summary>
    internal interface ISafeState : IDisposable
    {
        /// <summary>
        /// Gets the managed point in time snapshot.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed point in time snapshot.</returns>
        public unsafe SharedSnapshot* Snapshot(bool refresh);

        /// <summary>
        /// Pins the lifetime snapshot to the specified table version. Subsequent calls to
        /// <see cref="Snapshot(bool)"/> rebuild against this pinned version rather than the
        /// latest log version. Used to mirror the bridge's pinned state after
        /// construction-with-Version, <see cref="DeltaLake.Bridge.Table.LoadVersionAsync"/>,
        /// <see cref="DeltaLake.Bridge.Table.LoadTimestampAsync"/>, or
        /// <see cref="DeltaLake.Bridge.Table.UpdateIncrementalAsync"/>, so kernel-only reads
        /// (notably <see cref="DeltaLake.Kernel.Core.Table.CheckpointAsync"/>) honor the
        /// user-requested version. Invalidates dependent caches (scan, schema, partitions,
        /// arrow context) so they rebuild against the pinned snapshot on next access.
        /// </summary>
        /// <param name="version">The table version to pin the snapshot to.</param>
        public void PinSnapshotTo(long version);

        /// <summary>
        /// Removes any pin previously set by <see cref="PinSnapshotTo(long)"/>. Subsequent
        /// calls to <see cref="Snapshot(bool)"/> revert to building against the latest log
        /// version. Invalidates dependent caches.
        /// </summary>
        public void UnpinSnapshot();

        /// <summary>
        /// Gets the managed point in time scan.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed point in time scan state.</returns>
        public unsafe SharedScan* Scan(bool refresh);

        /// <summary>
        /// Gets the managed point in time table shared schema.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed point in time table shared schema.</returns>
        public unsafe SharedSchema* Schema(bool refresh);

        /// <summary>
        /// Gets the managed partition lists.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed partition list.</returns>
        public unsafe PartitionList* PartitionList(bool refresh);

        /// <summary>
        /// Gets the <see cref="ArrowContext"/> representing the table data.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed arrow context.</returns>
        public unsafe ArrowContext* ArrowContext(bool refresh);
    }
}
