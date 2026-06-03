// -----------------------------------------------------------------------------
// <summary>
// Extension methods for transforming Table Options.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Table;

namespace DeltaLake.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="TableOptions"/>.
    /// </summary>
    public static class TableOptionsExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the kernel engine can read the table given
        /// these load-time options.
        /// </summary>
        /// <param name="options">The table options.</param>
        /// <returns><c>true</c> if the kernel engine can be used; otherwise, <c>false</c>.</returns>
        public static bool IsKernelSupported(this TableOptions options)
        {
            // Kernel does not support opening at a specific historical version at construction.
            // When TableOptions.Version is set, the kernel engine is not built and kernel-only
            // operations (CheckpointAsync, ReadAsArrowTableAsync, etc.) will throw. Callers who
            // need version pinning should open the table without TableOptions.Version and call
            // LoadVersionAsync afterward, which pins both bridge and kernel state via
            // ManagedTableState.PinSnapshotTo.
            if (options.Version != default) return false;

            return true;
        }
    }
}
