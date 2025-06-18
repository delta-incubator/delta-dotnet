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
using DeltaLake.Extensions;
using DeltaLake.Kernel.Arrow.Handlers;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
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
        /// <param name="scanMetadata">The scan metadata data.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void VisitScanDataDelegate(
            void* engineContext,
            SharedScanMetadata* scanMetadata
        );
        internal static unsafe VisitScanDataDelegate VisitScanData =
            new(
                (
                    void* engineContext,
                    SharedScanMetadata* scanMetadata
                ) =>
                {
                    Methods.visit_scan_metadata(
                        scanMetadata,
                        engineContext,
#pragma warning disable CS8604 // ProcessScanData is not null, the delegate is defined literally 2 blocks below
                        Marshal.GetFunctionPointerForDelegate<ProcessScanDataDelegate>(ProcessScanData)
#pragma warning restore CS8604
                    );
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
        /// <param name="transform">The transform to execute on the row</param>
        /// <param name="partitionMap">The partition map.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void ProcessScanDataDelegate(
            void* engineContext,
            KernelStringSlice parquetFilePath,
            long parquetFileSize,
            Stats* stats,
            DvInfo* dvInfo,
            Expression* transform,
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
                    Expression* transform,
                    CStringMap* partitionMap
                ) =>
                {
                    EngineContext* context = (EngineContext*)engineContext;
                    var rootString = MarshalExtensions.PtrToStringUTF8((IntPtr)context->TableRoot);
                    var tableRoot = context->KernelTableRoot();
                    ExternResultKernelBoolSlice selectionVectorRes = Methods.selection_vector_from_dv(
                        dvInfo,
                        context->Engine,
                        tableRoot);
                    if (selectionVectorRes.tag != ExternResultKernelBoolSlice_Tag.OkKernelBoolSlice)
                    {
                        var error = (KernelReadError*)selectionVectorRes.Anonymous.Anonymous2.err;
                        try
                        {
                            throw new InvalidOperationException($"Could not get selection vector from kernel `type={error->etype.etype}`: {error->msg}");
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(new IntPtr(error));
                        }
                    }

                    KernelBoolSlice selectionVector = selectionVectorRes.Anonymous.Anonymous1.ok;

                    context->PartitionKeyValueMap = partitionMap;
                    // TODO: Do something with the transform
                    new ArrowFFIInterOpHandler().ReadParquetAsArrow(
                        context,
                        parquetFilePath,
                        selectionVector
                    );

                    Methods.free_bool_slice(selectionVector);
                    context->PartitionKeyValueMap = null;
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
                    new ArrowFFIInterOpHandler().StoreArrowInContext(
                        context->ArrowContext,
                        arrowData,
                        context->PartitionList,
                        context->PartitionKeyValueMap
                    );
                }
            );
    }
}
