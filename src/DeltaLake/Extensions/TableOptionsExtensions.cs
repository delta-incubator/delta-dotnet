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
        /// Returns a value indicating whether the kernel supports the table options.
        /// </summary>
        /// <param name="options">The table options.</param>
        /// <returns><c>true</c> if the kernel supports the table options; otherwise, <c>false</c>.</returns>
        public static bool IsKernelSupported(this TableOptions options)
        {
            // Kernel does not support working with a custom version
            //
            if (options.Version != default) return false;

            return true;
        }
    }
}
