// -----------------------------------------------------------------------------
// <summary>
// The Kernel representation of a Delta Table, contains the memory management
// and lifecycle of the table on both sides (C# and Kernel).
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.C;
using DeltaLake.Bridge.Interop;
using DeltaLake.Extensions;
using DeltaLake.Kernel.Arrow.Extensions;
using DeltaLake.Kernel.Builders;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.Shim.Async;
using DeltaLake.Kernel.State;
using DeltaLake.Table;
using Microsoft.Data.Analysis;
using DeltaRustBridge = DeltaLake.Bridge;
using ICancellationToken = System.Threading.CancellationToken;
using Methods = DeltaLake.Kernel.Interop.Methods;

namespace DeltaLake.Kernel.Core
{
    /// <summary>
    /// Reference to unmanaged delta table.
    ///
    /// The idea is, we prioritize the Kernel FFI implementations, and
    /// operations not supported by the FFI yet falls back to Delta RS Runtime
    /// implementation.
    /// </summary>
    internal class Table : DeltaRustBridge.Table
    {
        /// <summary>
        /// Behavioral flags.
        /// </summary>
        private bool isKernelSupported;
        private readonly bool isKernelAllocated;

        /// <summary>
        /// Regular managed objects.
        /// </summary>
        private readonly TableStorageOptions tableStorageOptions;

        /// <summary>
        /// Kernel InterOp objects.
        /// </summary>
        private readonly KernelStringSlice tableLocationSlice;
        private readonly KernelStringSlice[] storageOptionsKeySlices;
        private readonly KernelStringSlice[] storageOptionsValueSlices;
        private readonly ExternResultHandleSharedExternEngine sharedExternEngine;

        /// <summary>
        /// Disposable managed objects.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to dispose of these alongside this <see cref="Table"/> class.
        /// </remarks>
#pragma warning disable IDE0090, CA1859, CA2213 // state is disposed of in ReleaseHandle but the IDE does not recognize it as IDisposable
        private readonly ISafeState state;
#pragma warning restore IDE0090, CA1859, CA2213

        /// <summary>
        /// Pointers **WE** manage alongside this <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to release these pointers via the paired GC
        /// handles via <see cref="GCHandle.Free()"/>.
        /// </remarks>
        private readonly unsafe byte* gcPinnedTableLocationPtr;
        private readonly unsafe byte** gcPinnedStorageOptionsKeyPtrs;
        private readonly unsafe byte** gcPinnedStorageOptionsValuePtrs;

        private readonly GCHandle tableLocationHandle;
        private readonly GCHandle[] storageOptionsKeyHandles;
        private readonly GCHandle[] storageOptionsValueHandles;
        private readonly GCHandle? allocatorHandle;
        /// <summary>
        /// Pointers **KERNEL** manages related to this <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to ask Kernel to release these pointers
        /// when <see cref="Table"/> class is disposed.
        /// </remarks>
        private readonly unsafe EngineBuilder* kernelOwnedEngineBuilderPtr;
        private readonly unsafe SharedExternEngine* kernelOwnedSharedExternEnginePtr;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// If the user passes in a version at load time, Kernel cannot be used.
        /// </remarks>
        /// <param name="bridgeRuntime">The Delta Bridge runtime.</param>
        /// <param name="rawBridgetablePtr">The pre-allocated delta table pointer.</param>
        /// <param name="options">The table options.</param>
        internal unsafe Table(
            DeltaRustBridge.Runtime bridgeRuntime,
            RawDeltaTable* rawBridgetablePtr,
            DeltaLake.Table.TableOptions options
        )
            : this(bridgeRuntime, rawBridgetablePtr, options, options.IsKernelSupported()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="bridgeRuntime">The Delta Bridge runtime.</param>
        /// <param name="rawBridgetablePtr">The pre-allocated delta table pointer.</param>
        /// <param name="tableStorageOptions">The table storage options.</param>
        /// <param name="useKernel">Whether to use the Kernel or not.</param>
        internal unsafe Table(
            DeltaRustBridge.Runtime bridgeRuntime,
            RawDeltaTable* rawBridgetablePtr,
            TableStorageOptions tableStorageOptions,
            Boolean useKernel = true
        )
            : base(bridgeRuntime, rawBridgetablePtr)
        {
            this.tableStorageOptions = tableStorageOptions;
            this.isKernelSupported = useKernel && tableStorageOptions.IsKernelSupported();

            if (this.isKernelSupported)
            {
                // Kernel String Slice is used to communicate the table location.
                //
                (GCHandle handle, IntPtr ptr) = tableStorageOptions.TableLocation.ToPinnedBytePointer();
                this.tableLocationHandle = handle;
                this.gcPinnedTableLocationPtr = (byte*)ptr.ToPointer();
                this.tableLocationSlice = new KernelStringSlice { ptr = this.gcPinnedTableLocationPtr, len = (nuint)tableStorageOptions.TableLocation.Length };

                // Shared engine is the core runtime at the Kernel, tied to this table,
                // it is managed by the Kernel, but our responsibility to release it.
                var handleForAllocator = GCHandle.Alloc((AllocateErrorFn)AllocateErrorCallbacks.AllocateError);
                ExternResultEngineBuilder engineBuilder = Methods.get_engine_builder(this.tableLocationSlice, Marshal.GetFunctionPointerForDelegate(handleForAllocator.Target!));
                this.allocatorHandle = handleForAllocator;
                if (engineBuilder.tag != ExternResultEngineBuilder_Tag.OkEngineBuilder)
                {
                    throw new InvalidOperationException("Could not initiate engine builder from Delta Kernel");
                }
                this.kernelOwnedEngineBuilderPtr = engineBuilder.Anonymous.Anonymous1_1.ok;

                // The joys of unmanaged code, this is all to pass some Key:Value string pairs
                // to the Kernel's Engine Builder (e.g. Storage Account/S3 Keys etc.).
                //
                int index = 0;
                int count = tableStorageOptions.StorageOptions.Count;
                this.storageOptionsKeyHandles = new GCHandle[count];
                this.storageOptionsValueHandles = new GCHandle[count];
                this.gcPinnedStorageOptionsKeyPtrs = (byte**)Marshal.AllocHGlobal(count * sizeof(byte*));
                this.gcPinnedStorageOptionsValuePtrs = (byte**)Marshal.AllocHGlobal(count * sizeof(byte*));
                this.storageOptionsKeySlices = new KernelStringSlice[count];
                this.storageOptionsValueSlices = new KernelStringSlice[count];

                foreach (KeyValuePair<string, string> kvp in tableStorageOptions.StorageOptions)
                {
                    (GCHandle keyHandle, IntPtr keyPtr) = kvp.Key.ToPinnedBytePointer();
                    (GCHandle valueHandle, IntPtr valuePtr) = kvp.Value.ToPinnedBytePointer();

                    this.storageOptionsKeyHandles[index] = keyHandle;
                    this.storageOptionsValueHandles[index] = valueHandle;
                    this.gcPinnedStorageOptionsKeyPtrs[index] = (byte*)keyPtr.ToPointer();
                    this.gcPinnedStorageOptionsValuePtrs[index] = (byte*)valuePtr.ToPointer();
                    this.storageOptionsKeySlices[index] = new KernelStringSlice { ptr = this.gcPinnedStorageOptionsKeyPtrs[index], len = (nuint)kvp.Key.Length };
                    this.storageOptionsValueSlices[index] = new KernelStringSlice { ptr = this.gcPinnedStorageOptionsValuePtrs[index], len = (nuint)kvp.Value.Length };

                    Methods.set_builder_option(this.kernelOwnedEngineBuilderPtr, this.storageOptionsKeySlices[index], this.storageOptionsValueSlices[index]);

                    index++;
                }

                this.sharedExternEngine = Methods.builder_build(this.kernelOwnedEngineBuilderPtr);
                if (this.sharedExternEngine.tag != ExternResultHandleSharedExternEngine_Tag.OkHandleSharedExternEngine)
                {
                    throw new InvalidOperationException("Could not build engine from the engine builder sent to Delta Kernel.");
                }
                this.kernelOwnedSharedExternEnginePtr = this.sharedExternEngine.Anonymous.Anonymous1_1.ok;
                this.state = new ManagedTableState(this.tableLocationSlice, this.kernelOwnedSharedExternEnginePtr);
                this.isKernelAllocated = true;
            }
        }

        #region Delta Kernel table operations

        internal async Task<Apache.Arrow.Table> ReadAsArrowTableAsync(
            ICancellationToken cancellationToken
        )
        {
            this.ThrowIfKernelNotSupported();

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        unsafe
                        {
                            return this.state.ArrowContext(true)->ToTable();
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        internal async Task<DataFrame> ReadAsDataFrameAsync(ICancellationToken cancellationToken)
        {
            this.ThrowIfKernelNotSupported();

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        unsafe
                        {
#pragma warning disable CA2000 // DataFrames use the RecordBatch, so we don't need to dispose of it
                            return DataFrame.FromArrowRecordBatch(this.state.ArrowContext(true)->ToRecordBatch());
#pragma warning restore CA2000
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        internal override long? Version()
        {
            if (this.isKernelAllocated && this.isKernelSupported)
            {
                unsafe
                {
                    return unchecked((long)Methods.version(this.state.Snapshot(true)));
                }
            }
            return base.Version();
        }

        internal override string Uri()
        {
            if (this.isKernelAllocated && this.isKernelSupported)
            {
                unsafe
                {
                    IntPtr tableRootPtr = IntPtr.Zero;
                    try
                    {
                        tableRootPtr = (IntPtr)Methods.snapshot_table_root(this.state.Snapshot(true), Marshal.GetFunctionPointerForDelegate<AllocateStringFn>(StringAllocatorCallbacks.AllocateString));

                        // Kernel returns an extra "/", delta-rs does not
                        //
                        return MarshalExtensions.PtrToStringUTF8(tableRootPtr)?.TrimEnd('/') ?? string.Empty;
                    }
                    finally
                    {
                        if (tableRootPtr != IntPtr.Zero) Marshal.FreeHGlobal(tableRootPtr);
                    }
                }
            }
            return base.Uri();
        }

        /// <remarks>
        /// Kernel does not support "loading". The moment the user invokes
        /// this, we run into a state inconsistency, because further calls to "Version()" via Kernel
        /// will return the latest version, as opposed to the version the user loaded.
        ///
        /// So - we un-support Kernel and invoke "delta-rs" going forward.
        /// </remarks>
        internal override async Task LoadVersionAsync(ulong version, ICancellationToken cancellationToken)
        {
            this.isKernelSupported = false;
            await base.LoadVersionAsync(version, cancellationToken).ConfigureAwait(false);
        }

        /// <remarks>
        /// <see cref="LoadVersionAsync"/> remarks.
        /// </remarks>
        internal override async Task LoadTimestampAsync(long timestampMilliseconds, ICancellationToken cancellationToken)
        {
            this.isKernelSupported = false;
            await base.LoadTimestampAsync(timestampMilliseconds, cancellationToken).ConfigureAwait(false);
        }

        /// <remarks>
        /// Kernel does not support all Metadata - so we get what we can from
        /// delta-rs, and override with Kernel values when supported. The idea
        /// is, as Kernel exposes more metadata that delta-rs does not, we can
        /// continue to expose the Kernel view to end users.
        /// </remarks>
        internal override DeltaLake.Table.TableMetadata Metadata()
        {
            DeltaLake.Table.TableMetadata metadata = base.Metadata();
            if (this.isKernelAllocated && this.isKernelSupported)
            {
                unsafe
                {
                    metadata.PartitionColumns = PartitionColumns();
                }
            }
            return metadata;
        }

        /// <summary>
        /// Commits add-file actions to the Delta log without writing data files.
        /// This is a synchronous method — all FFI calls are blocking.
        /// Returns the new table version after commit.
        /// </summary>
        /// <param name="actions">File metadata for pre-written Parquet files.</param>
        /// <param name="options">Commit configuration options.</param>
        /// <returns>The committed table version number.</returns>
        internal unsafe ulong CommitAddActions(
            IReadOnlyList<AddAction> actions,
            CommitOptions options)
        {
            this.ThrowIfKernelNotSupported();

            ExternResultHandleExclusiveTransaction txnResult =
                Methods.transaction(this.tableLocationSlice, this.kernelOwnedSharedExternEnginePtr);

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
                if (options.EngineInfo != null)
                {
                    (GCHandle engineInfoHandle, IntPtr engineInfoPtr) =
                        options.EngineInfo.ToPinnedBytePointer();
                    try
                    {
                        var engineInfoSlice = new KernelStringSlice
                        {
                            ptr = (byte*)engineInfoPtr,
                            len = (nuint)options.EngineInfo.Length,
                        };

                        ExternResultHandleExclusiveTransaction infoResult =
                            Methods.with_engine_info(txnPtr, engineInfoSlice, this.kernelOwnedSharedExternEnginePtr);

                        if (infoResult.tag != ExternResultHandleExclusiveTransaction_Tag.OkHandleExclusiveTransaction)
                        {
                            txnPtr = null;
                            throw KernelException.FromEngineError(
                                infoResult.Anonymous.Anonymous2_1.err,
                                "Failed to set engine info on transaction");
                        }

                        txnPtr = infoResult.Anonymous.Anonymous1_1.ok;
                    }
                    finally
                    {
                        engineInfoHandle.Free();
                    }
                }

                using RecordBatch addFilesBatch = AddActionRecordBatchBuilder.Build(actions);

                var nativeArray = CArrowArray.Create();
                var nativeSchema = CArrowSchema.Create();

                try
                {
                    CArrowArrayExporter.ExportRecordBatch(addFilesBatch, nativeArray);
                    CArrowSchemaExporter.ExportSchema(addFilesBatch.Schema, nativeSchema);

                    FFI_ArrowArray ffiArray = *(FFI_ArrowArray*)nativeArray;
                    FFI_ArrowSchema* ffiSchemaPtr = (FFI_ArrowSchema*)nativeSchema;

                    ExternResultHandleExclusiveEngineData dataResult =
                        Methods.get_engine_data(ffiArray, ffiSchemaPtr, this.kernelOwnedSharedExternEnginePtr);

                    if (dataResult.tag != ExternResultHandleExclusiveEngineData_Tag.OkHandleExclusiveEngineData)
                    {
                        throw KernelException.FromEngineError(
                            dataResult.Anonymous.Anonymous2_1.err,
                            "Failed to create engine data from add-files RecordBatch");
                    }

                    ExclusiveEngineData* engineDataPtr = dataResult.Anonymous.Anonymous1_1.ok;

                    Methods.add_files(txnPtr, engineDataPtr);
                }
                finally
                {
                    CArrowSchema.Free(nativeSchema);
                    // Array: get_engine_data copies FFI_ArrowArray by value and the kernel
                    // takes ownership of the underlying buffers. The original CArrowArray*
                    // still has its release callback set. CArrowArray.Free() would call it
                    // and double-free. We cast to FFI_ArrowArray* to null the release
                    // callback (which is accessible on the internal struct), then free
                    // the allocation.
                    ((FFI_ArrowArray*)nativeArray)->release = default;
                    Marshal.FreeHGlobal((IntPtr)nativeArray);
                }

                ExternResultu64 commitResult =
                    Methods.commit(txnPtr, this.kernelOwnedSharedExternEnginePtr);
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

        #endregion Delta Kernel table operations

        #region SafeHandle implementation

        protected override unsafe bool ReleaseHandle()
        {
            if (this.isKernelAllocated)
            {
                this.state.Dispose();
                this.allocatorHandle?.Free();
                if (this.tableLocationHandle.IsAllocated) this.tableLocationHandle.Free();
                foreach (GCHandle handle in this.storageOptionsKeyHandles) if (handle.IsAllocated) handle.Free();
                foreach (GCHandle handle in this.storageOptionsValueHandles) if (handle.IsAllocated) handle.Free();
                Marshal.FreeHGlobal((IntPtr)this.gcPinnedStorageOptionsKeyPtrs);
                Marshal.FreeHGlobal((IntPtr)this.gcPinnedStorageOptionsValuePtrs);

                // EngineBuilder* does not need to be deallocated
                //
                // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1727978348653369
                //
                Methods.free_engine(kernelOwnedSharedExternEnginePtr);
            }

            return base.ReleaseHandle();
        }

        #endregion SafeHandle implementation

        #region Private methods

        private List<string> PartitionColumns()
        {
            List<string> partitionColumns = new();
            unsafe
            {
                PartitionList* managedPartitionListPtr = this.state.PartitionList(true);
                int numPartitions = managedPartitionListPtr->Len;
                if (numPartitions > 0)
                {
                    for (int i = 0; i < numPartitions; i++)
                    {
                        partitionColumns.Add(
                            MarshalExtensions.PtrToStringUTF8((IntPtr)managedPartitionListPtr->Cols[i])
                                ?? throw new InvalidOperationException(
                                    $"Delta Kernel returned a null partition column name despite reporting {numPartitions} > 0 partition(s) exist."
                                )
                        );
                    }
                }

                return partitionColumns;
            }
        }

        private void ThrowIfKernelNotSupported()
        {
            if (!this.isKernelAllocated || !this.isKernelSupported)
            {
                // There's currently no direct equivalent to this in delta-rs,
                // so we throw if the Kernel is not being used.
                //
                throw new InvalidOperationException("This operation is not supported without using the Delta Kernel.");
            }
        }

        #endregion Private methods
    }
}
