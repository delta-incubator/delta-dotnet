// -----------------------------------------------------------------------------
// <summary>
// Arrow InterOp handler interface.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.State;

namespace DeltaLake.Kernel.Arrow.Handlers
{
    internal interface IArrowInteropHandler
    {
        /// <summary>
        /// Appends a batch of Kernel FFI data as globally allocated 
        /// pointers.
        /// </summary>
        /// <param name="context">The Arrow Context.</param>
        /// <param name="arrowData">The Arrow/FFI interOp data.</param>
        /// <param name="partitionCols">The partition columns.</param>
        /// <param name="partitionValues">The partition values.</param>
        public unsafe void StoreArrowInContext(
            ArrowContext* context,
            ArrowFFIData* arrowData,
            PartitionList* partitionCols,
            CStringMap* partitionValues
        );

        /// <summary>
        /// Reads a Parquet file as Arrow by invoking Arrow iterator callback.
        /// </summary>
        /// <param name="context">The Engine Context.</param>
        /// <param name="path">The parquet file path.</param>
        /// <param name="selectionVector">The selection vector.</param>
        public unsafe void ReadParquetAsArrow(
            EngineContext* context,
            KernelStringSlice path,
            KernelBoolSlice selectionVector
        );
    }
}
