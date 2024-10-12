// -----------------------------------------------------------------------------
// <summary>
// An arrow context holds Delta table data in arrow format.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// Delta table Arrow context.
    /// </summary>
    internal struct ArrowContext
    {
        /// <summary>
        /// The number of record structs stored in the pointers array.
        /// </summary>
        public int NumBatches;

        /// <summary>
        /// The record struct pointers.
        /// </summary>
        public unsafe ArrowFFIData** ArrowStructs;

        /// <summary>
        /// The parquet string partition pointers.
        /// </summary>
        public unsafe ParquetStringPartitions** Partitions;
    }
}
