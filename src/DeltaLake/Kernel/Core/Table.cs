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
using DeltaLake.Bridge.Interop;
using DeltaLake.Extensions;
using DeltaLake.Kernel.Arrow.Builders;
using DeltaLake.Kernel.Arrow.Extensions;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.Shim.Async;
using DeltaLake.Kernel.State;
using DeltaLake.Kernel.Transaction;
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
        private readonly unsafe Apache.Arrow.C.CArrowSchema* addFilesNativeSchema;
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
        /// When the caller supplies <see cref="DeltaLake.Table.TableOptions.Version"/>, the kernel
        /// snapshot is pinned to that version via <see cref="ISafeState.PinSnapshotTo(long)"/> so
        /// the first snapshot materialization matches the bridge's pinned table version.
        /// </remarks>
        /// <param name="bridgeRuntime">The Delta Bridge runtime.</param>
        /// <param name="rawBridgetablePtr">The pre-allocated delta table pointer.</param>
        /// <param name="options">The table options.</param>
        internal unsafe Table(
            DeltaRustBridge.Runtime bridgeRuntime,
            RawDeltaTable* rawBridgetablePtr,
            DeltaLake.Table.TableOptions options
        )
            : this(bridgeRuntime, rawBridgetablePtr, options, options.IsKernelSupported())
        {
            if (this.isKernelAllocated && options.Version is ulong pinVersion)
            {
                this.state.PinSnapshotTo((long)pinVersion);
            }
        }

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
            bool shouldBuildKernel = useKernel && tableStorageOptions.IsKernelSupported();

            if (shouldBuildKernel)
            {
                // Kernel String Slice is used to communicate the table location.
                //
                (GCHandle handle, IntPtr ptr) = tableStorageOptions.TableLocation.ToPinnedBytePointer();
                this.tableLocationHandle = handle;
                this.gcPinnedTableLocationPtr = (byte*)ptr.ToPointer();
                this.tableLocationSlice = new KernelStringSlice { ptr = (sbyte*)this.gcPinnedTableLocationPtr, len = (ulong)tableStorageOptions.TableLocation.Length };

                // Shared engine is the core runtime at the Kernel, tied to this table,
                // it is managed by the Kernel, but our responsibility to release it.
                var handleForAllocator = GCHandle.Alloc((AllocateErrorFn)AllocateErrorCallbacks.AllocateError);
                ExternResultEngineBuilder engineBuilder = Methods.get_engine_builder(this.tableLocationSlice, Marshal.GetFunctionPointerForDelegate(handleForAllocator.Target!));
                this.allocatorHandle = handleForAllocator;
                if (engineBuilder.tag != ExternResultEngineBuilder_Tag.OkEngineBuilder)
                {
                    throw new InvalidOperationException("Could not initiate engine builder from Delta Kernel");
                }
                this.kernelOwnedEngineBuilderPtr = engineBuilder.Anonymous.Anonymous1.ok;

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
                    this.storageOptionsKeySlices[index] = new KernelStringSlice { ptr = (sbyte*)this.gcPinnedStorageOptionsKeyPtrs[index], len = (ulong)kvp.Key.Length };
                    this.storageOptionsValueSlices[index] = new KernelStringSlice { ptr = (sbyte*)this.gcPinnedStorageOptionsValuePtrs[index], len = (ulong)kvp.Value.Length };

                    Methods.set_builder_option(this.kernelOwnedEngineBuilderPtr, this.storageOptionsKeySlices[index], this.storageOptionsValueSlices[index]);

                    index++;
                }

                // Required by Snapshot::checkpoint to avoid deadlocks on the default
                // single-threaded TokioBackgroundExecutor.
                Methods.set_builder_with_multithreaded_executor(this.kernelOwnedEngineBuilderPtr, 2, 0);

                this.sharedExternEngine = Methods.builder_build(this.kernelOwnedEngineBuilderPtr);
                if (this.sharedExternEngine.tag != ExternResultHandleSharedExternEngine_Tag.OkHandleSharedExternEngine)
                {
                    throw new InvalidOperationException("Could not build engine from the engine builder sent to Delta Kernel.");
                }
                this.kernelOwnedSharedExternEnginePtr = this.sharedExternEngine.Anonymous.Anonymous1.ok;
                this.state = new ManagedTableState(this.tableLocationSlice, this.kernelOwnedSharedExternEnginePtr);
                this.isKernelAllocated = true;

                // Pre-export the static add-files Arrow schema for reuse across commits.
                this.addFilesNativeSchema = Apache.Arrow.C.CArrowSchema.Create();
                Apache.Arrow.C.CArrowSchemaExporter.ExportSchema(
                    AddActionRecordBatchBuilder.AddFilesSchema, this.addFilesNativeSchema);
            }
        }

        #region Delta Kernel table operations

        internal async Task<OwnedArrowTable> ReadAsArrowTableAsync(
            ICancellationToken cancellationToken
        )
        {
            this.ThrowIfKernelNotSupported();

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        ArrowContextHandle handle = this.state.BuildArrowContextOwned();
                        try
                        {
                            Apache.Arrow.Table table = ArrowContextExtensions.BuildSanitizedTable(
                                handle.Schema,
                                handle.RecordBatchList);
                            return new OwnedArrowTable(table, handle);
                        }
                        catch
                        {
                            handle.Dispose();
                            throw;
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        internal async Task<OwnedDataFrame> ReadAsDataFrameAsync(ICancellationToken cancellationToken)
        {
            this.ThrowIfKernelNotSupported();

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        ArrowContextHandle handle = this.state.BuildArrowContextOwned();
                        try
                        {
                            Apache.Arrow.RecordBatch concatenated = ArrowContextExtensions.ConcatenateAndSanitize(
                                handle.Schema,
                                handle.RecordBatchList);
#pragma warning disable CA2000 // OwnedDataFrame owns the handle; the intermediate RecordBatch is consumed by FromArrowRecordBatch
                            DataFrame frame = DataFrame.FromArrowRecordBatch(concatenated);
                            return new OwnedDataFrame(frame, handle, concatenated);
#pragma warning restore CA2000
                        }
                        catch
                        {
                            handle.Dispose();
                            throw;
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        internal override long? Version()
        {
            if (this.isKernelAllocated)
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
            if (this.isKernelAllocated)
            {
                unsafe
                {
                    IntPtr tableRootPtr = IntPtr.Zero;
                    try
                    {
                        tableRootPtr = (IntPtr)Methods.snapshot_table_root(this.state.Snapshot(true), Marshal.GetFunctionPointerForDelegate<AllocateStringFn>(StringAllocatorCallbacks.AllocateString));

                        // Kernel returns an extra "/", delta-rs does not
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

        internal override async Task LoadVersionAsync(ulong version, ICancellationToken cancellationToken)
        {
            if (!this.tableStorageOptions.IsKernelSupported())
            {
                throw new NotSupportedException(
                    "LoadVersionAsync is not supported for memory:// tables. " +
                    "Use a file:// URI with a temp directory for in-process scenarios.");
            }

            await base.LoadVersionAsync(version, cancellationToken).ConfigureAwait(false);

            if (this.isKernelAllocated)
            {
                this.state.PinSnapshotTo((long)version);
            }
        }

        /// <remarks>
        /// Throws <see cref="NotSupportedException"/> on memory:// tables. On file:// tables,
        /// the bridge resolves the timestamp to a concrete version; the kernel snapshot is
        /// then pinned to that resolved version via <see cref="ISafeState.PinSnapshotTo(long)"/>.
        /// </remarks>
        internal override async Task LoadTimestampAsync(long timestampMilliseconds, ICancellationToken cancellationToken)
        {
            if (!this.tableStorageOptions.IsKernelSupported())
            {
                throw new NotSupportedException(
                    "LoadTimestampAsync is not supported for memory:// tables. " +
                    "Use a file:// URI with a temp directory for in-process scenarios.");
            }

            await base.LoadTimestampAsync(timestampMilliseconds, cancellationToken).ConfigureAwait(false);

            if (this.isKernelAllocated)
            {
                long? resolved = base.Version();
                if (resolved is long v)
                {
                    this.state.PinSnapshotTo(v);
                }
            }
        }

        /// <remarks>
        /// Mirrors the bridge's post-advance table version onto the kernel snapshot so
        /// subsequent kernel reads honor the new version rather than reading the latest log version.
        /// </remarks>
        internal override async Task UpdateIncrementalAsync(long? maxVersion, ICancellationToken cancellationToken)
        {
            await base.UpdateIncrementalAsync(maxVersion, cancellationToken).ConfigureAwait(false);

            if (this.isKernelAllocated)
            {
                long? advanced = base.Version();
                if (advanced is long v)
                {
                    this.state.PinSnapshotTo(v);
                }
            }
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
            if (this.isKernelAllocated)
            {
                unsafe
                {
                    metadata.PartitionColumns = PartitionColumns();
                }
            }
            return metadata;
        }

        internal IEnumerable<Apache.Arrow.RecordBatch> QueryTableChanges(TableChangesOptions options)
        {
            ThrowIfKernelNotSupported();
            return ExecuteTableChanges(options);
        }

        /// <summary>
        /// Commits add-file actions to the Delta log without writing data files.
        /// Returns the new table version after commit.
        /// </summary>
        /// <param name="actions">File metadata for pre-written Parquet files.</param>
        /// <param name="appId">Optional application identifier for idempotent writes.</param>
        /// <param name="txnVersion">Optional application-specific version for idempotent writes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The committed table version number.</returns>
        internal async Task<ulong> CommitAddActionsAsync(
            IReadOnlyList<AddAction> actions,
            string? appId,
            long? txnVersion,
            ICancellationToken cancellationToken)
        {
            this.ThrowIfKernelNotSupported();

            using Apache.Arrow.RecordBatch addFilesBatch = AddActionRecordBatchBuilder.Build(actions);

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        unsafe
                        {
                            return TransactionCommitter.Commit(
                                this.tableLocationSlice,
                                this.kernelOwnedSharedExternEnginePtr,
                                addFilesBatch,
                                this.addFilesNativeSchema,
                                appId,
                                txnVersion);
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the current transaction version for a given application ID.
        /// Returns null if no transaction has been recorded for this appId.
        /// </summary>
        /// <param name="appId">The application identifier to look up.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The last committed version for this appId, or null if none exists.</returns>
        internal async Task<long?> GetLatestTransactionVersionAsync(
            string appId,
            ICancellationToken cancellationToken)
        {
            this.ThrowIfKernelNotSupported();

            return await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        unsafe
                        {
                            return TransactionCommitter.GetLatestTransactionVersion(
                                this.state.Snapshot(true),
                                appId,
                                this.kernelOwnedSharedExternEnginePtr);
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        internal async Task CheckpointAsync(CheckpointOptions options, ICancellationToken cancellationToken)
        {
            if (!this.isKernelAllocated)
            {
                throw new NotSupportedException(
                    "CheckpointAsync requires a kernel-readable table. " +
                    "memory:// tables and tables opened with TableOptions.Version are not supported. " +
                    "Use a file:// URI with a temp directory for in-process scenarios.");
            }

            await SyncToAsyncShim
                .ExecuteAsync(
                    () =>
                    {
                        unsafe
                        {
                            ExternResultFfiCheckpointWriteResult result;
                            if (options.Format == CheckpointFormat.Auto)
                            {
                                // Pass null to let the kernel auto-pick V1/V2.
                                result = Methods.checkpoint_snapshot(
                                    this.state.Snapshot(refresh: true),
                                    this.kernelOwnedSharedExternEnginePtr,
                                    null);
                            }
                            else
                            {
                                FfiCheckpointSpec spec = BuildCheckpointSpec(options);
                                result = Methods.checkpoint_snapshot(
                                    this.state.Snapshot(refresh: true),
                                    this.kernelOwnedSharedExternEnginePtr,
                                    &spec);
                            }

                            if (result.tag != ExternResultFfiCheckpointWriteResult_Tag.OkFfiCheckpointWriteResult)
                            {
                                throw KernelException.FromEngineError(
                                    result.Anonymous.Anonymous2.err,
                                    "Failed to checkpoint snapshot via kernel FFI");
                            }

                            FfiCheckpointWriteResult writeResult = result.Anonymous.Anonymous1.ok;
                            bool checkpointWritten =
                                writeResult.tag == FfiCheckpointWriteResult_Tag.FfiCheckpointWriteResultWritten;

                            // Both "Written" and "AlreadyExists" carry one owned snapshot handle; free it
                            // regardless of which.
                            SharedSnapshot* returnedSnapshot = checkpointWritten
                                ? writeResult.Anonymous.Anonymous1.written
                                : writeResult.Anonymous.Anonymous2.already_exists;
                            if (returnedSnapshot != null)
                            {
                                Methods.free_snapshot(returnedSnapshot);
                            }

                            return checkpointWritten;
                        }
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Translates managed <see cref="CheckpointOptions"/> into the kernel FFI
        /// <see cref="FfiCheckpointSpec"/>. Only called for non-<see cref="CheckpointFormat.Auto"/>
        /// specs; the sidecar hint applies to <see cref="CheckpointFormat.V2WithSidecar"/> only.
        /// </summary>
        private static unsafe FfiCheckpointSpec BuildCheckpointSpec(CheckpointOptions options)
        {
            FfiCheckpointSpec spec = default;
            switch (options.Format)
            {
                case CheckpointFormat.V1:
                    spec.tag = FfiCheckpointSpec_Tag.FfiCheckpointSpecV1;
                    break;
                case CheckpointFormat.V2NoSidecar:
                    spec.tag = FfiCheckpointSpec_Tag.FfiCheckpointSpecV2NoSidecar;
                    break;
                case CheckpointFormat.V2WithSidecar:
                    spec.tag = FfiCheckpointSpec_Tag.FfiCheckpointSpecV2WithSidecar;
                    OptionalValueusize hint = default;
                    if (options.FileActionsPerSidecarHint is ulong hintValue)
                    {
                        hint.tag = OptionalValueusize_Tag.Someusize;
                        hint.Anonymous.Anonymous.some = hintValue;
                    }
                    else
                    {
                        hint.tag = OptionalValueusize_Tag.Noneusize;
                    }
                    spec.Anonymous.v2_with_sidecar.file_actions_per_sidecar_hint = hint;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(options),
                        options.Format,
                        "Unsupported checkpoint Format.");
            }

            return spec;
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
                if (this.addFilesNativeSchema != null) Apache.Arrow.C.CArrowSchema.Free(this.addFilesNativeSchema);
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

        // Acquires all three FFI handles needed for CDC iteration.
        // A regular (non-iterator) method so it can contain an unsafe { } block.
        private TableChangesContext CreateTableChangesContext(TableChangesOptions options)
        {
            unsafe
            {
                ExternResultHandleExclusiveTableChanges changesResult = options.EndVersion.HasValue
                    ? Methods.table_changes_between_versions(
                        this.tableLocationSlice,
                        this.kernelOwnedSharedExternEnginePtr,
                        options.StartVersion,
                        options.EndVersion.Value)
                    : Methods.table_changes_from_version(
                        this.tableLocationSlice,
                        this.kernelOwnedSharedExternEnginePtr,
                        options.StartVersion);

                if (changesResult.tag != ExternResultHandleExclusiveTableChanges_Tag.OkHandleExclusiveTableChanges)
                {
                    throw KernelException.FromEngineError(
                        changesResult.Anonymous.Anonymous2.err,
                        "Failed to acquire table changes handle from Delta Kernel.");
                }

                // CRITICAL: table_changes_scan CONSUMES this pointer on both success and failure.
                ExclusiveTableChanges* tableChangesPtr = changesResult.Anonymous.Anonymous1.ok;

                // TODO: expose predicate push-down via TableChangesOptions once EnginePredicate* is surfaced.
                ExternResultHandleSharedTableChangesScan scanResult =
                    Methods.table_changes_scan(tableChangesPtr, this.kernelOwnedSharedExternEnginePtr, predicate: null);
                tableChangesPtr = null;  // now invalid regardless of outcome

                if (scanResult.tag != ExternResultHandleSharedTableChangesScan_Tag.OkHandleSharedTableChangesScan)
                {
                    throw KernelException.FromEngineError(
                        scanResult.Anonymous.Anonymous2.err,
                        "Failed to create table changes scan from Delta Kernel.");
                }

                SharedTableChangesScan* scanPtr = scanResult.Anonymous.Anonymous1.ok;

                ExternResultHandleSharedScanTableChangesIterator iterResult =
                    Methods.table_changes_scan_execute(scanPtr, this.kernelOwnedSharedExternEnginePtr);

                if (iterResult.tag != ExternResultHandleSharedScanTableChangesIterator_Tag.OkHandleSharedScanTableChangesIterator)
                {
                    Methods.free_table_changes_scan(scanPtr);
                    throw KernelException.FromEngineError(
                        iterResult.Anonymous.Anonymous2.err,
                        "Failed to execute table changes scan from Delta Kernel.");
                }

                return new TableChangesContext(scanPtr, iterResult.Anonymous.Anonymous1.ok);
            }
        }

        // Iterator method: contains no unsafe code — all FFI is delegated to TableChangesContext.
        private IEnumerable<Apache.Arrow.RecordBatch> ExecuteTableChanges(TableChangesOptions options)
        {
            using (TableChangesContext context = CreateTableChangesContext(options))
            {
                for (; ; )
                {
                    Apache.Arrow.RecordBatch? batch = context.Next();
                    if (batch == null) yield break;
                    yield return batch;
                }
            }
        }

        // Holds the scan + iterator handles for a single CDC traversal.
        // Methods have safe signatures so they are callable from the non-unsafe iterator above.
        private sealed unsafe class TableChangesContext : IDisposable
        {
            private SharedTableChangesScan* scanPtr;
            private SharedScanTableChangesIterator* iterPtr;

            internal TableChangesContext(
                SharedTableChangesScan* scanPtr,
                SharedScanTableChangesIterator* iterPtr)
            {
                this.scanPtr = scanPtr;
                this.iterPtr = iterPtr;
            }

            internal Apache.Arrow.RecordBatch? Next()
            {
                ExternResultArrowFFIData batchResult = Methods.scan_table_changes_next(this.iterPtr);

                if (batchResult.tag == ExternResultArrowFFIData_Tag.ErrArrowFFIData)
                {
                    throw KernelException.FromEngineError(
                        batchResult.Anonymous.Anonymous2.err,
                        "Failed to advance table changes iterator.");
                }

                ArrowFFIData* arrowDataPtr = batchResult.Anonymous.Anonymous1.ok;
                if (arrowDataPtr == null)
                {
                    return null;
                }

                // Import via Arrow C Data Interface — ownership of the underlying buffers
                // transfers to the managed RecordBatch. free_arrow_ffi_data is called in
                // the finally block to release only the ArrowFFIData* wrapper struct allocated
                // by Rust; the Arrow buffer release callbacks are zeroed on successful import
                // so free_arrow_ffi_data becomes a cheap no-op for the buffer bytes.
                try
                {
                    Apache.Arrow.Schema schema =
                        Apache.Arrow.C.CArrowSchemaImporter.ImportSchema(
                            (Apache.Arrow.C.CArrowSchema*)&arrowDataPtr->schema);
                    return Apache.Arrow.C.CArrowArrayImporter.ImportRecordBatch(
                        (Apache.Arrow.C.CArrowArray*)&arrowDataPtr->array, schema);
                }
                finally
                {
                    Methods.free_arrow_ffi_data(arrowDataPtr);
                }
            }

            public void Dispose()
            {
                // Free iterator before scan (reverse acquisition order).
                if (this.iterPtr != null)
                {
                    Methods.free_scan_table_changes_iter(this.iterPtr);
                    this.iterPtr = null;
                }
                if (this.scanPtr != null)
                {
                    Methods.free_table_changes_scan(this.scanPtr);
                    this.scanPtr = null;
                }
            }
        }

        private void ThrowIfKernelNotSupported()
        {
            if (!this.isKernelAllocated)
            {
                // There's currently no direct equivalent to this in delta-rs,
                // so we throw if the Kernel is not being used (memory:// tables
                // or tables opened without kernel support).
                //
                throw new InvalidOperationException("This operation is not supported without using the Delta Kernel.");
            }
        }

        #endregion Private methods
    }
}
