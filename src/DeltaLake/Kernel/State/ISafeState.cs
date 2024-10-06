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
        /// Gets the managed point in time scan.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed point in time scan state.</returns>
        public unsafe SharedScan* Scan(bool refresh);

        /// <summary>
        /// Gets the managed point in time global table scan state.
        /// </summary>
        /// <param name="refresh">Whether to refresh.</param>
        /// <returns>The managed point in time global scan state.</returns>
        public unsafe SharedGlobalScanState* GlobalScanState(bool refresh);

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
    }
}
