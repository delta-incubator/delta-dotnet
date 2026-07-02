// -----------------------------------------------------------------------------
// <summary>
// A disposable handle for a shared, on-demand-refreshable Kernel Table state.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using DeltaLake.Kernel.Arrow.Extensions;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Callbacks.Visit;
using DeltaLake.Kernel.Interop;
using static DeltaLake.Kernel.Callbacks.Visit.VisitCallbacks;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// A disposable handle for managed table state.
    /// </summary>
    internal class ManagedTableState : ISafeState
    {
        private bool disposed;

        private readonly KernelStringSlice tableLocationSlice;
        private readonly unsafe SharedExternEngine* sharedExternEnginePtr;

        private unsafe SharedSnapshot* managedPointInTimeSnapshot = null;
        private unsafe SharedScan* managedScan = null;
        private unsafe SharedSchema* managedSchema = null;
        private unsafe SharedSchema* physicalSchema = null;
        private unsafe PartitionList* partitionList = null;

        // When set, RefreshSnapshot builds at this version instead of latest.
        // This keeps kernel reads aligned with bridge version-loading APIs.
        private long? pinnedVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedTableState"/> class.
        /// </summary>
        /// <param name="tableLocationSlice">The table location slice.</param>
        /// <param name="sharedExternEnginePtr">The Shared External Engine Pointer, not managed in this class.</param>
        public unsafe ManagedTableState(
            KernelStringSlice tableLocationSlice,
            SharedExternEngine* sharedExternEnginePtr
        )
        {
            this.tableLocationSlice = tableLocationSlice;
            this.sharedExternEnginePtr = sharedExternEnginePtr;
        }

#pragma warning disable CS1587
        /// <remarks>
        /// Refreshes state if requested or if not exists.
        /// </remarks>
#pragma warning restore CS1587
        #region ISafeState implementation

        /// <inheritdoc/>
        public unsafe SharedSnapshot* Snapshot(bool refresh)
        {
            if (refresh || managedPointInTimeSnapshot == null) this.RefreshSnapshot();
            return managedPointInTimeSnapshot;
        }

        /// <inheritdoc/>
        public unsafe SharedSnapshot* AdvanceSnapshot()
        {
            // No cached snapshot yet: a full from-path build is the only correct option.
            if (this.managedPointInTimeSnapshot == null)
            {
                this.RefreshSnapshot();
                return this.managedPointInTimeSnapshot;
            }

            // Backward-version guard: get_snapshot_builder_from advances forward only and REJECTS an
            // earlier target version. If pinned earlier than the cached snapshot, rebuild from path.
            if (this.pinnedVersion is long pinnedCheck)
            {
                ulong cachedVersion = Methods.version(this.managedPointInTimeSnapshot);
                if ((ulong)pinnedCheck < cachedVersion)
                {
                    this.RefreshSnapshot();
                    return this.managedPointInTimeSnapshot;
                }
            }

            SharedSnapshot* previous = this.managedPointInTimeSnapshot;

            // Borrow the previous snapshot to build an advanced one (reads only new commits).
            // get_snapshot_builder_from clone_as_arc's the handle, so `previous` remains ours to free.
            ExternResultHandleMutableFfiSnapshotBuilder builderRes =
                Methods.get_snapshot_builder_from(previous, this.sharedExternEnginePtr);
            if (builderRes.tag != ExternResultHandleMutableFfiSnapshotBuilder_Tag.OkHandleMutableFfiSnapshotBuilder)
            {
                // Could not create an incremental builder; fall back to a correct full rebuild.
                this.RefreshSnapshot();
                return this.managedPointInTimeSnapshot;
            }
            MutableFfiSnapshotBuilder* builderPtr = builderRes.Anonymous.Anonymous1.ok;

            if (this.pinnedVersion is long pinned)
            {
                Methods.snapshot_builder_set_version(&builderPtr, (ulong)pinned);
            }

            // snapshot_builder_build consumes/frees the builder on all paths.
            ExternResultHandleSharedSnapshot snapshotRes = Methods.snapshot_builder_build(builderPtr);
            if (snapshotRes.tag != ExternResultHandleSharedSnapshot_Tag.OkHandleSharedSnapshot)
            {
                // Build failed; `previous` is still valid. Fall back to a correct full rebuild.
                this.RefreshSnapshot();
                return this.managedPointInTimeSnapshot;
            }

            SharedSnapshot* advanced = snapshotRes.Anonymous.Anonymous1.ok;

            // The snapshot moved: invalidate dependents WITHOUT freeing the new snapshot, then swap
            // and free the previous handle. Do NOT call InvalidateSnapshotDependentCaches() here because
            // it also calls DisposeSnapshot(), which would free the handle we are about to install.
            this.DisposePartitionList();
            this.DisposeScan();
            this.DisposeSchema();
            this.DisposePhysicalSchema();

            Methods.free_snapshot(previous);
            this.managedPointInTimeSnapshot = advanced;
            return this.managedPointInTimeSnapshot;
        }

        /// <inheritdoc/>
        public void PinSnapshotTo(long version)
        {
            if (this.pinnedVersion == version)
            {
                return;
            }

            this.pinnedVersion = version;
            this.InvalidateSnapshotDependentCaches();
        }

        /// <inheritdoc/>
        public void UnpinSnapshot()
        {
            if (this.pinnedVersion is null)
            {
                return;
            }

            this.pinnedVersion = null;
            this.InvalidateSnapshotDependentCaches();
        }

        private void InvalidateSnapshotDependentCaches()
        {
            this.DisposePartitionList();
            this.DisposeScan();
            this.DisposeSchema();
            this.DisposePhysicalSchema();
            this.DisposeSnapshot();
        }

        /// <inheritdoc/>
        public unsafe SharedScan* Scan(bool refresh)
        {
            if (refresh || managedScan == null) this.RefreshScan();
            return managedScan;
        }

        /// <inheritdoc/>
        public unsafe SharedSchema* Schema(bool refresh)
        {
            if (refresh || managedSchema == null) this.RefreshSchema();
            return managedSchema;
        }

        /// <inheritdoc/>
        public unsafe SharedSchema* PhysicalSchema(bool refresh)
        {
            if (refresh || physicalSchema == null) this.RefreshPhysicalSchema();
            return physicalSchema;
        }

        /// <inheritdoc/>
        public unsafe PartitionList* PartitionList(bool refresh)
        {
            if (refresh || partitionList == null) this.RefreshPartitionList();
            return partitionList;
        }

        /// <summary>
        /// Builds a new caller-owned <see cref="ArrowContextHandle"/> for a single
        /// read operation. The returned handle owns a freshly-allocated native
        /// <see cref="ArrowContext"/> and the managed <see cref="Apache.Arrow.RecordBatch"/>
        /// instances imported from it. The caller (typically an
        /// <see cref="DeltaLake.Kernel.Core.OwnedArrowTable"/> or
        /// <see cref="DeltaLake.Kernel.Core.OwnedDataFrame"/>) must dispose the
        /// handle to release native memory.
        /// </summary>
        /// <returns>A fresh <see cref="ArrowContextHandle"/> owning the per-call native allocation and imported batches.</returns>
        public ArrowContextHandle BuildArrowContextOwned()
        {
            unsafe
            {
                ArrowContext* native = (ArrowContext*)Marshal.AllocHGlobal(Marshal.SizeOf<ArrowContext>());
                *native = new ArrowContext { NumBatches = 0 };

                try
                {
                    SharedSnapshot* snap = this.Snapshot(true);
                    SharedSchema* schema = this.Schema(true);
                    PartitionList* partList = this.PartitionList(true);
                    SharedScan* scan = this.Scan(true);
                    SharedSchema* physical = this.PhysicalSchema(true);

                    this.PopulateArrowContextViaScan(native, snap, schema, scan, partList, physical);

                    (Apache.Arrow.Schema importedSchema, System.Collections.Generic.List<Apache.Arrow.RecordBatch> batches) =
                        (*native).ToSchematizedBatches();
                    return new ArrowContextHandle(native, importedSchema, batches);
                }
                catch
                {
                    FreeArrowContextNative(native);
                    throw;
                }
            }
        }

        #endregion ISafeState implementation

        #region IDisposable implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.DisposePartitionList();
                this.DisposeSnapshot();
                this.DisposeSchema();
                this.DisposeScan();
                this.DisposePhysicalSchema();
                disposed = true;
            }
        }

        ~ManagedTableState() => Dispose(false);

        #endregion IDisposable implementation

        #region Private Dispose methods

        /// <summary>
        /// Frees the unmanaged memory backing an <see cref="ArrowContext"/> struct, including
        /// each per-batch <see cref="ArrowFFIData"/> and <see cref="ParquetStringPartitions"/>
        /// pointer, the outer <c>ArrowStructs</c> and <c>Partitions</c> pointer arrays, and the
        /// struct itself. Unlike the previous instance helper, this unconditionally frees the
        /// outer arrays and the struct when the pointer is non-null, eliminating the
        /// empty-batches outer-array leak path.
        /// </summary>
        /// <param name="native">Pointer to the <see cref="ArrowContext"/> to free. No-op when null.</param>
        internal static unsafe void FreeArrowContextNative(ArrowContext* native)
        {
            if (native == null) return;

            if (native->NumBatches != 0)
            {
                for (int i = 0; i < native->NumBatches; i++)
                {
                    ArrowFFIData* arrowStruct = native->ArrowStructs[i];
                    ParquetStringPartitions* partitions = native->Partitions[i];

                    if (arrowStruct != null)
                    {
                        Marshal.FreeHGlobal((IntPtr)arrowStruct);
                        native->ArrowStructs[i] = null;
                    }

                    if (partitions != null)
                    {
                        for (int j = 0; j < partitions->Len; j++)
                        {
                            Marshal.FreeHGlobal((IntPtr)partitions->ColNames[j]);
                            Marshal.FreeHGlobal((IntPtr)partitions->ColValues[j]);
                        }
                        Marshal.FreeHGlobal((IntPtr)partitions);
                        native->Partitions[i] = null;
                    }
                }
            }

            if (native->ArrowStructs != null) Marshal.FreeHGlobal((IntPtr)native->ArrowStructs);
            if (native->Partitions != null) Marshal.FreeHGlobal((IntPtr)native->Partitions);
            Marshal.FreeHGlobal((IntPtr)native);
        }

        private void DisposePartitionList()
        {
            unsafe
            {
                if (this.partitionList != null)
                {
                    for (int i = 0; i < this.partitionList->Len; i++) Marshal.FreeHGlobal((IntPtr)this.partitionList->Cols[i]);
                    Marshal.FreeHGlobal((IntPtr)this.partitionList->Cols);
                    Marshal.FreeHGlobal((IntPtr)this.partitionList);
                    this.partitionList = null;
                }
            }
        }

        private void DisposeScan()
        {
            unsafe
            {
                if (this.managedScan != null)
                {
                    Methods.free_scan(this.managedScan);
                    this.managedScan = null;
                }
            }
        }

        private void DisposeSchema()
        {
            unsafe
            {
                DisposeSchema(this.managedSchema);
                this.managedSchema = null;
            }
        }

        private void DisposePhysicalSchema()
        {
            unsafe
            {
                DisposeSchema(this.physicalSchema);
                this.physicalSchema = null;
            }
        }

        private unsafe static void DisposeSchema(SharedSchema* schema)
        {
            unsafe
            {
                if (schema != null)
                {
                    Methods.free_schema(schema);
                    schema = null;
                }
            }
        }

        private void DisposeSnapshot()
        {
            unsafe
            {
                if (this.managedPointInTimeSnapshot != null)
                {
                    Methods.free_snapshot(this.managedPointInTimeSnapshot);
                    this.managedPointInTimeSnapshot = null;
                }
            }
        }

        #endregion Private Dispose methods

        /// <remarks>
        /// The refresh methods use the public methods when calling on dependent
        /// state without requesting refresh, since the public methods are
        /// idempotent (and will refresh parent state if not exists).
        /// </remarks>
        #region Private Refresh methods

        private void RefreshPartitionList()
        {

            this.DisposePartitionList();

            unsafe
            {
                int partitionColumnCount = (int)Methods.get_partition_column_count(this.Snapshot(false));
                this.partitionList = (PartitionList*)Marshal.AllocHGlobal(sizeof(PartitionList));

                // We set the length to 0 here and use it to track how many
                // items we've added.
                //
                this.partitionList->Len = 0;
                this.partitionList->Cols = (char**)Marshal.AllocHGlobal(sizeof(char*) * partitionColumnCount);

                StringSliceIterator* partitionIterator = Methods.get_partition_columns(this.Snapshot(false));
                try
                {
                    for (; ; )
                    {
                        bool hasNext = Methods.string_slice_next(partitionIterator, this.partitionList, Marshal.GetFunctionPointerForDelegate(VisitCallbacks.VisitPartition));
                        if (!hasNext) break;
                    }

                    int receivedPartitionLen = this.partitionList->Len;
                    if (receivedPartitionLen != partitionColumnCount)
                    {
                        throw new InvalidOperationException(
                            $"Delta Kernel partition iterator did not return {partitionColumnCount} columns as reported by 'get_partition_column_count' after iterating, reported {receivedPartitionLen} instead."
                        );
                    }
                }
                finally
                {
                    Methods.free_string_slice_data(partitionIterator);
                }
            }
        }

        private void RefreshScan()
        {

            this.DisposeScan();

            unsafe
            {
                // predicate: null = no filter (read all rows)
                // schema: null = no column projection (read all columns)
                ExternResultHandleSharedScan scanRes = Methods.scan(
                    this.Snapshot(false),
                    this.sharedExternEnginePtr,
                    predicate: null,
                    schema: null);
                if (scanRes.tag != ExternResultHandleSharedScan_Tag.OkHandleSharedScan)
                {
                    throw KernelException.FromEngineError(scanRes.Anonymous.Anonymous2.err, "Failed to create table scan from Delta Kernel.");
                }

                this.managedScan = scanRes.Anonymous.Anonymous1.ok;
            }
        }

        private void RefreshSchema()
        {

            this.DisposeSchema();

            unsafe
            {
                this.managedSchema = Methods.scan_logical_schema(this.Scan(false));
            }
        }

        private void RefreshPhysicalSchema()
        {

            this.DisposePhysicalSchema();

            unsafe
            {
                this.physicalSchema = Methods.scan_physical_schema(this.Scan(false));
            }
        }

        // TODO: delta-kernel-rs upgrade coordination
        // This code was migrated from snapshot()/scan() to the builder pattern as part of the
        // v0.17.0 → v0.21.0 upgrade. For future kernel version bumps:
        // 1. Check https://github.com/delta-incubator/delta-dotnet/pulls for existing upgrade PRs
        // 2. Review delta-kernel-rs CHANGELOG for FFI breaking changes
        // 3. Regenerate bindings via 'make generate-kernel-bindings' after updating delta-kernel-rs.version.txt
        private void RefreshSnapshot()
        {

            this.DisposeSnapshot();

            unsafe
            {
                // Step 1: Create snapshot builder
                ExternResultHandleMutableFfiSnapshotBuilder builderRes = Methods.get_snapshot_builder(this.tableLocationSlice, this.sharedExternEnginePtr);
                if (builderRes.tag != ExternResultHandleMutableFfiSnapshotBuilder_Tag.OkHandleMutableFfiSnapshotBuilder)
                {
                    throw KernelException.FromEngineError(builderRes.Anonymous.Anonymous2.err, "Failed to create snapshot builder from Delta Kernel.");
                }
                MutableFfiSnapshotBuilder* builderPtr = builderRes.Anonymous.Anonymous1.ok;

                // Step 2: Apply pinned version (if any)
                if (this.pinnedVersion is long pinned)
                {
                    Methods.snapshot_builder_set_version(&builderPtr, (ulong)pinned);
                }

                // Step 3: Build snapshot (latest version, or pinned per Step 2)
                // snapshot_builder_build consumes the builder handle on both success and failure paths.
                ExternResultHandleSharedSnapshot snapshotRes = Methods.snapshot_builder_build(builderPtr);
                if (snapshotRes.tag != ExternResultHandleSharedSnapshot_Tag.OkHandleSharedSnapshot)
                {
                    throw KernelException.FromEngineError(snapshotRes.Anonymous.Anonymous2.err, "Failed to build table snapshot from Delta Kernel.");
                }

                this.managedPointInTimeSnapshot = snapshotRes.Anonymous.Anonymous1.ok;
            }
        }

        /// <summary>
        /// Drives the kernel scan-metadata iteration loop and populates
        /// <paramref name="native"/> via the <see cref="EngineContext"/> visitor
        /// callback. The caller owns <paramref name="native"/> and is responsible
        /// for freeing it; this helper only fills the struct's RecordBatches.
        /// </summary>
        /// <param name="native">The target <see cref="ArrowContext"/> to populate.</param>
        /// <param name="snap">The pinned snapshot for the scan.</param>
        /// <param name="schema">The logical schema for the scan.</param>
        /// <param name="scan">The kernel scan handle.</param>
        /// <param name="partList">The partition column list.</param>
        /// <param name="physical">The physical schema.</param>
        private unsafe void PopulateArrowContextViaScan(
            ArrowContext* native,
            SharedSnapshot* snap,
            SharedSchema* schema,
            SharedScan* scan,
            PartitionList* partList,
            SharedSchema* physical)
        {
            // Memory scoped to this scan
            //
            // TODO: scan_metadata_next_arrow
            // v0.21.0 exposes scan_metadata_next_arrow for batch-mode Arrow scan metadata,
            // which avoids per-file callback overhead. Consider migrating from the per-file
            // scan_metadata_next + CScanCallback pattern to the batch Arrow API.
            SharedScanMetadataIterator* iter = null;
            IntPtr tableRootPtr = (IntPtr)Methods.snapshot_table_root(snap, Marshal.GetFunctionPointerForDelegate<AllocateStringFn>(StringAllocatorCallbacks.AllocateString));
            EngineContext ctx = new()
            {
                LogicalSchema = schema,
                TableRoot = (byte*)tableRootPtr,
                Engine = this.sharedExternEnginePtr,
                PartitionList = partList,
                PartitionKeyValueMap = null,
                ArrowContext = native,
                PhysicalSchema = physical,
            };
            EngineContext* ctxPtr = &ctx;

            try
            {
                ExternResultHandleSharedScanMetadataIterator iterHandle = Methods.scan_metadata_iter_init(this.sharedExternEnginePtr, scan);
                if (iterHandle.tag != ExternResultHandleSharedScanMetadataIterator_Tag.OkHandleSharedScanMetadataIterator)
                {
                    throw KernelException.FromEngineError(iterHandle.Anonymous.Anonymous2.err, "Failed to construct kernel scan data iterator.");
                }
                iter = iterHandle.Anonymous.Anonymous1.ok;
                for (; ; )
                {
                    ExternResultbool ok = Methods.scan_metadata_next(
                        iter,
                        ctxPtr,
                        Marshal.GetFunctionPointerForDelegate<VisitScanDataDelegate>(VisitCallbacks.VisitScanData));
                    if (ok.tag != ExternResultbool_Tag.Okbool)
                    {
                        throw KernelException.FromEngineError(ok.Anonymous.Anonymous2.err, "Failed to iterate on table scan data.");
                    }
                    if (!ok.Anonymous.Anonymous1.ok) break;
                }
            }
            finally
            {
                if (iter != null) Methods.free_scan_metadata_iter(iter);
                if (tableRootPtr != IntPtr.Zero) Marshal.FreeHGlobal(tableRootPtr);
            }
        }

        #endregion Private Refresh methods
    }
}
