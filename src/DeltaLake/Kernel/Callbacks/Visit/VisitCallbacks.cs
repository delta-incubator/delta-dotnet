// -----------------------------------------------------------------------------
// <summary>
// Engine Visitor callbacks.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using DeltaLake.Kernel.Arrow.Handlers;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.State;

namespace DeltaLake.Kernel.Callbacks.Visit
{
    /// <summary>
    /// Callbacks when Kernel asks us to visit a particular context.
    /// </summary>
    internal static class VisitCallbacks
    {
        /// <summary>
        /// Visits the partition.
        /// </summary>
        /// <remarks>
        /// Caller is responsible for freeing the memory allocated to the column pointer.
        /// </remarks>
        /// <param name="context">The partition context.</param>
        /// <param name="partition">The partition kernel slice.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void VisitPartitionDelegate(
            void* context,
            KernelStringSlice partition
        );
        internal static unsafe VisitPartitionDelegate VisitPartition =
            new(
                (void* context, KernelStringSlice partition) =>
                {
                    PartitionList* list = (PartitionList*)context;
                    char* col = (char*)StringAllocatorCallbacks.AllocateString(partition);
                    list->Cols[list->Len] = col;
                    list->Len++;
                }
            );

        /// <summary>
        /// Visits the scanned data.
        /// </summary>
        /// <param name="engineContext">The engine context.</param>
        /// <param name="engineData">The engine data.</param>
        /// <param name="selectionVec">The selection vector.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void VisitScanDataDelegate(
            void* engineContext,
            ExclusiveEngineData* engineData,
            KernelBoolSlice selectionVec
        );
        internal static unsafe VisitScanDataDelegate VisitScanData =
            new(
                (
                    void* engineContext,
                    ExclusiveEngineData* engineData,
                    KernelBoolSlice selectionVec
                ) =>
                {
                    Methods.visit_scan_data(
                        engineData,
                        selectionVec,
                        engineContext,
#pragma warning disable CS8604 // ProcessScanData is not null, the delegate is defined literally 2 blocks below
                        Marshal.GetFunctionPointerForDelegate<ProcessScanDataDelegate>(ProcessScanData)
#pragma warning restore CS8604
                    );
                    Methods.free_bool_slice(selectionVec);
                }
            );

        /// <summary>
        /// Processes the scanned data.
        /// </summary>
        /// <param name="engineContext">The engine context.</param>
        /// <param name="parquetFilePath">The path of the parquet file to read.</param>
        /// <param name="parquetFileSize">The file size.</param>
        /// <param name="stats">The file statistics.</param>
        /// <param name="dvInfo">The selection vector information.</param>
        /// <param name="partitionMap">The partition map.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void ProcessScanDataDelegate(
            void* engineContext,
            KernelStringSlice parquetFilePath,
            long parquetFileSize,
            Stats* stats,
            DvInfo* dvInfo,
            CStringMap* partitionMap
        );
        internal static unsafe ProcessScanDataDelegate ProcessScanData =
            new(
                (
                    void* engineContext,
                    KernelStringSlice parquetFilePath,
                    long parquetFileSize,
                    Stats* stats,
                    DvInfo* dvInfo,
                    CStringMap* partitionMap
                ) =>
                {
                    EngineContext* context = (EngineContext*)engineContext;
                    ExternResultKernelBoolSlice selectionVectorRes = Methods.selection_vector_from_dv(dvInfo, context->Engine, context->GlobalScanState);
                    if (selectionVectorRes.tag != ExternResultKernelBoolSlice_Tag.OkKernelBoolSlice)
                    {
                        throw new InvalidOperationException("Could not get selection vector from kernel");
                    }
                    KernelBoolSlice selectionVector = selectionVectorRes.Anonymous.Anonymous1.ok;

                    context->PartitionValues = partitionMap;
                    new ArrowFFIInterOpHandler().ReadParquetFileAsArrow(
                        context,
                        parquetFilePath,
                        selectionVector
                    );

                    Methods.free_bool_slice(selectionVector);
                    context->PartitionValues = null;
                }
            );

        /// <summary>
        /// Physically reads the arrow data.
        /// </summary>
        /// <param name="engineContext">The engine context.</param>
        /// <param name="engineData">The engine data.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void VisitReadDataDelegate(
            void* engineContext,
            ExclusiveEngineData* engineData
        );
        internal static unsafe VisitReadDataDelegate IngestArrowData =
            new(
                (
                    void* engineContext,
                    ExclusiveEngineData* engineData
                ) =>
                {
                    EngineContext* context = (EngineContext*)engineContext;
                    ExternResultArrowFFIData isRawArrowReadOk = Methods.get_raw_arrow_data(engineData, context->Engine);
                    if (isRawArrowReadOk.tag != ExternResultArrowFFIData_Tag.OkArrowFFIData)
                    {
                        throw new InvalidOperationException("Could not read raw Arrow data with Delta Kernel");
                    }
                    ArrowFFIData* arrowData = isRawArrowReadOk.Anonymous.Anonymous1.ok;
                    new ArrowFFIInterOpHandler().ZeroCopyRecordBatchToArrowContext(
                        context->ArrowContext,
                        arrowData,
                        context->PartitionList,
                        context->PartitionValues
                    );
                }
            );
    }
}
