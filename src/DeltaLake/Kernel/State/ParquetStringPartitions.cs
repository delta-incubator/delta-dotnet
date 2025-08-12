// -----------------------------------------------------------------------------
// <summary>
// The list of parquet partitions.
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
    /// The list of parquet string partitions.
    /// </summary>
    internal struct ParquetStringPartitions
    {
        /// <summary>
        /// The number of partitions.
        /// </summary>
        public int Len;

        /// <summary>
        /// The list of partition column names.
        /// </summary>
        public unsafe byte** ColNames;

        /// <summary>
        /// The list of partition column values, all strings, even if they are
        /// numbers - as they are stored as strings in the parquet the Kernel reads back.
        /// </summary>
        public unsafe byte** ColValues;
    }
}
