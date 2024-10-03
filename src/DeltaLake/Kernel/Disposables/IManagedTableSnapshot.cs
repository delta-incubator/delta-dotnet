// -----------------------------------------------------------------------------
// <summary>
// A managed table snapshot.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Kernel.Interop;
using System;

namespace DeltaLake.Kernel.Disposables
{
    /// <summary>
    /// Managed table snapshot interface.
    /// </summary>
    internal interface IManagedTableSnapshot : IDisposable
    {
        /// <summary>
        /// Gets the managed point in time snapshot, safely auto refreshes on
        /// every get.
        /// </summary>
        /// <returns>The managed point in time snapshot.</returns>
        public unsafe SharedSnapshot* Snapshot { get; }
    }
}
