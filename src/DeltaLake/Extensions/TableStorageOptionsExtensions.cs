// -----------------------------------------------------------------------------
// <summary>
// Extension methods for transforming Table Storage Options.
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
    /// Extension methods for <see cref="TableStorageOptions"/>.
    /// </summary>
    public static class TableStorageOptionsExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the kernel supports the table storage options.
        /// </summary>
        /// <param name="options">The table storage options.</param>
        /// <returns><c>true</c> if the kernel supports the table storage options; otherwise, <c>false</c>.</returns>
        public static bool IsKernelSupported(this TableStorageOptions options)
        {
            // Kernel does not recognize memory locations
            //
            if (options.TableLocation.StartsWith("memory://")) return false;

            return true;
        }
    }
}
