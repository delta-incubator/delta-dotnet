// -----------------------------------------------------------------------------
// <summary>
// The list of partitions.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// The list of partitions.
    /// </summary>
#pragma warning disable CA1815, CA1051 // Override equals and operator equals on value types
    public struct PartitionList
    {
        /// <summary>
        /// The number of partitions.
        /// </summary>
        public int Len;

        /// <summary>
        /// The list of partition columns.
        /// </summary>
        public unsafe char** Cols;
    }
#pragma warning restore CA1815, CA1051
}
