// -----------------------------------------------------------------------------
// <summary>
// Handles the FFI transaction lifecycle for committing add-file actions
// to the Delta transaction log via delta-kernel-rs.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;
using Apache.Arrow;
using Apache.Arrow.C;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Interop;
using Methods = DeltaLake.Kernel.Interop.Methods;

namespace DeltaLake.Kernel.Transaction
{
    /// <summary>
    /// Handles the FFI transaction lifecycle for committing file metadata
    /// to the Delta transaction log. Converts an Arrow RecordBatch to
    /// kernel EngineData via the Arrow C Data Interface and commits
    /// via the kernel transaction API.
    /// </summary>
    internal static class TransactionCommitter
    {
        /// <summary>
        /// Execute a full transaction: create → add files → commit.
        /// All calls are synchronous FFI operations.
        /// </summary>
        /// <param name="tableLocationSlice">The table location as a KernelStringSlice.</param>
        /// <param name="enginePtr">The shared extern engine pointer.</param>
        /// <param name="addFilesBatch">The RecordBatch containing add-file metadata.</param>
        /// <param name="addFilesSchema">Pre-exported CArrowSchema pointer (reused across commits).</param>
        /// <param name="appId">Optional application identifier for idempotent writes (Delta Protocol txn action).</param>
        /// <param name="txnVersion">Optional application-specific version for idempotent writes (Delta Protocol txn action).</param>
        /// <returns>The committed table version number.</returns>
        internal static unsafe ulong Commit(
            KernelStringSlice tableLocationSlice,
            SharedExternEngine* enginePtr,
            RecordBatch addFilesBatch,
            CArrowSchema* addFilesSchema,
            string? appId = null,
            long? txnVersion = null)
        {
            ExternResultHandleExclusiveTransaction txnResult =
                Methods.transaction(tableLocationSlice, enginePtr);

            if (txnResult.tag != ExternResultHandleExclusiveTransaction_Tag.OkHandleExclusiveTransaction)
            {
                throw KernelException.FromEngineError(
                    txnResult.Anonymous.Anonymous2_1.err,
                    "Failed to create transaction");
            }

            ExclusiveTransaction* txnPtr = txnResult.Anonymous.Anonymous1_1.ok;
            bool committed = false;

            try
            {
                Methods.set_data_change(txnPtr, true);

                if (appId != null)
                {
                    byte[] appIdBytes = Encoding.UTF8.GetBytes(appId);
                    fixed (byte* appIdPtr = appIdBytes)
                    {
                        var appIdSlice = new KernelStringSlice
                        {
                            ptr = appIdPtr,
                            len = (nuint)appIdBytes.Length,
                        };

                        ExternResultHandleExclusiveTransaction txnIdResult =
                            Methods.with_transaction_id(txnPtr, appIdSlice, txnVersion.Value, enginePtr);

                        if (txnIdResult.tag != ExternResultHandleExclusiveTransaction_Tag.OkHandleExclusiveTransaction)
                        {
                            throw KernelException.FromEngineError(
                                txnIdResult.Anonymous.Anonymous2_1.err,
                                "Failed to set transaction identifier");
                        }

                        txnPtr = txnIdResult.Anonymous.Anonymous1_1.ok;
                    }
                }

                var errorAllocatorHandle = GCHandle.Alloc(
                    (AllocateErrorFn)AllocateErrorCallbacks.AllocateError);
                var nativeArray = CArrowArray.Create();

                try
                {
                    CArrowArrayExporter.ExportRecordBatch(addFilesBatch, nativeArray);

                    FFI_ArrowArray ffiArray = *(FFI_ArrowArray*)nativeArray;
                    FFI_ArrowSchema* ffiSchemaPtr = (FFI_ArrowSchema*)addFilesSchema;

                    IntPtr errorAllocatorPtr = Marshal.GetFunctionPointerForDelegate(
                        (AllocateErrorFn)errorAllocatorHandle.Target!);

                    ExternResultHandleExclusiveEngineData dataResult =
                        Methods.get_engine_data(ffiArray, ffiSchemaPtr, errorAllocatorPtr);

                    if (dataResult.tag != ExternResultHandleExclusiveEngineData_Tag.OkHandleExclusiveEngineData)
                    {
                        throw KernelException.FromEngineError(
                            dataResult.Anonymous.Anonymous2_1.err,
                            "Failed to create engine data for transaction");
                    }

                    ExclusiveEngineData* engineDataPtr = dataResult.Anonymous.Anonymous1_1.ok;

                    Methods.add_files(txnPtr, engineDataPtr);
                }
                finally
                {
                    errorAllocatorHandle.Free();

                    // Array: get_engine_data copies FFI_ArrowArray by value and the kernel
                    // takes ownership of the buffers. CArrowArray.Free() would double-free.
                    // Null out the release callback, then free the allocation.
                    // Schema is NOT freed here — it's cached and reused across commits.
                    ((FFI_ArrowArray*)nativeArray)->release = default;
                    Marshal.FreeHGlobal((IntPtr)nativeArray);
                }

                ExternResultu64 commitResult =
                    Methods.commit(txnPtr, enginePtr);
                committed = true;

                if (commitResult.tag != ExternResultu64_Tag.Oku64)
                {
                    throw KernelException.FromEngineError(
                        commitResult.Anonymous.Anonymous2_1.err,
                        "Failed to commit transaction");
                }

                return (ulong)commitResult.Anonymous.Anonymous1_1.ok;
            }
            finally
            {
                if (!committed && txnPtr != null)
                {
                    Methods.free_transaction(txnPtr);
                }
            }
        }

        /// <summary>
        /// Queries the latest transaction version for a given application ID from a snapshot.
        /// Returns null if no transaction has been recorded for this appId.
        /// </summary>
        /// <param name="snapshotPtr">The shared snapshot pointer.</param>
        /// <param name="appId">The application identifier to look up.</param>
        /// <param name="enginePtr">The shared extern engine pointer.</param>
        /// <returns>The last committed version for this appId, or null if none exists.</returns>
        internal static unsafe long? GetLatestTransactionVersion(
            SharedSnapshot* snapshotPtr,
            string appId,
            SharedExternEngine* enginePtr)
        {
            byte[] appIdBytes = Encoding.UTF8.GetBytes(appId);
            fixed (byte* appIdPtr = appIdBytes)
            {
                var appIdSlice = new KernelStringSlice
                {
                    ptr = appIdPtr,
                    len = (nuint)appIdBytes.Length,
                };

                ExternResultOptionalValuei64 result =
                    Methods.get_app_id_version(snapshotPtr, appIdSlice, enginePtr);

                if (result.tag != ExternResultOptionalValuei64_Tag.OkOptionalValuei64)
                {
                    throw KernelException.FromEngineError(
                        result.Anonymous.Anonymous2_1.err,
                        "Failed to get transaction version");
                }

                OptionalValuei64 optVal = result.Anonymous.Anonymous1_1.ok;
                return optVal.tag == OptionalValuei64_Tag.Somei64 ? optVal.Anonymous.Anonymous_1.some : null;
            }
        }
    }
}
