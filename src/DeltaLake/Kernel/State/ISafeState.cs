// -----------------------------------------------------------------------------
// <summary>
// Memory safe kernel state.
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
        /// Gets the managed point in time snapshot, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed point in time snapshot.</returns>
        public unsafe SharedSnapshot* Snapshot { get; }

        /// <summary>
        /// Gets the managed point in time scan, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed point in time scan state.</returns>
        public unsafe SharedScan* Scan { get; }

        /// <summary>
        /// Gets the managed point in time global table scan state, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed point in time global scan state.</returns>
        public unsafe SharedGlobalScanState* GlobalScanState { get; }

        /// <summary>
        /// Gets the managed point in time table shared schema, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed point in time table shared schema.</returns>
        public unsafe SharedSchema* Schema { get; }

        /// <summary>
        /// Gets the managed partition lists, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed partition list.</returns>
        public unsafe PartitionList* PartitionList { get; }
    }
}
