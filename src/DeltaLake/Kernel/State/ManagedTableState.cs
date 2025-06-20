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
        private unsafe ArrowContext* arrowContext = null;

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

        /// <inheritdoc/>
        public unsafe ArrowContext* ArrowContext(bool refresh)
        {
            if (refresh || arrowContext == null) this.RefreshArrowContext();
            return arrowContext;
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
                this.DisposeArrowContext();
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

        private void DisposeArrowContext()
        {
            unsafe
            {
                if (this.arrowContext != null && this.arrowContext->NumBatches != 0)
                {
                    for (int i = 0; i < this.arrowContext->NumBatches; i++)
                    {
                        ArrowFFIData* arrowStruct = this.arrowContext->ArrowStructs[i];
                        ParquetStringPartitions* partitions = this.arrowContext->Partitions[i];

                        if (arrowStruct != null)
                        {
                            Marshal.FreeHGlobal((IntPtr)arrowStruct);
                            this.arrowContext->ArrowStructs[i] = null;
                        }

                        if (partitions != null)
                        {
                            for (int j = 0; j < partitions->Len; j++)
                            {
                                Marshal.FreeHGlobal((IntPtr)partitions->ColNames[j]);
                                Marshal.FreeHGlobal((IntPtr)partitions->ColValues[j]);
                            }
                            Marshal.FreeHGlobal((IntPtr)partitions);
                            this.arrowContext->Partitions[i] = null;
                        }
                    }
                    Marshal.FreeHGlobal((IntPtr)this.arrowContext->ArrowStructs);
                    Marshal.FreeHGlobal((IntPtr)this.arrowContext->Partitions);

                    Marshal.FreeHGlobal((IntPtr)this.arrowContext);
                }
                this.arrowContext = null;
            }
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
                ExternResultHandleSharedScan scanRes = Methods.scan(this.Snapshot(false), this.sharedExternEnginePtr, null);
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

        private void RefreshSnapshot()
        {

            this.DisposeSnapshot();

            unsafe
            {
                ExternResultHandleSharedSnapshot snapshotRes = Methods.snapshot(this.tableLocationSlice, this.sharedExternEnginePtr);
                if (snapshotRes.tag != ExternResultHandleSharedSnapshot_Tag.OkHandleSharedSnapshot)
                {
                    throw KernelException.FromEngineError(snapshotRes.Anonymous.Anonymous2.err, "Failed to retrieve table snapshot from Delta Kernel.");
                }

                this.managedPointInTimeSnapshot = snapshotRes.Anonymous.Anonymous1.ok;
            }
        }

        private void RefreshArrowContext()
        {

            this.DisposeArrowContext();

            unsafe
            {
                // Generate a fresh arrow context and pin it. The pattern here
                // is a little bit different, because we're the ones
                // pre-allocating the struct, and not the Kernel. We pass the
                // Kernel the struct pointer and it will pass it right back to
                // us via a callback, alongside the actual Arrow data, so we can
                // fill up the struct with on-the-fly allocated RecordBatches.
                //
                // ...If only the Kernel had a method like delta-rs to return Arrow
                // Stream...
                //
                this.arrowContext = (ArrowContext*)Marshal.AllocHGlobal(Marshal.SizeOf<ArrowContext>());
                *arrowContext = new ArrowContext()
                {
                    NumBatches = 0
                };

                // Refresh the necessary Kernel state together before initiating
                // the fresh scan - no stale reads allowed!
                //
                SharedSnapshot* managedSnapshotPtr = this.Snapshot(true);
                SharedSchema* managedSchemaPtr = this.Schema(true);
                PartitionList* managedPartitionListPtr = this.PartitionList(true);
                SharedScan* managedScanPtr = this.Scan(true);
                SharedSchema* physicalSchema = this.PhysicalSchema(true);
                // SharedGlobalScanState* managedGlobalScanStatePtr = this.GlobalScanState(true);

                // Memory scoped to this scan
                //
                SharedScanMetadataIterator* kernelOwnedScanDataIteratorPtr = null;
                IntPtr tableRootPtr = (IntPtr)Methods.snapshot_table_root(managedSnapshotPtr, Marshal.GetFunctionPointerForDelegate<AllocateStringFn>(StringAllocatorCallbacks.AllocateString));
                EngineContext scanScopedEngineContext = new()
                {
                    LogicalSchema = managedSchemaPtr,
                    TableRoot = (byte*)tableRootPtr,
                    Engine = this.sharedExternEnginePtr,
                    PartitionList = managedPartitionListPtr,
                    PartitionKeyValueMap = null,
                    ArrowContext = this.arrowContext,
                    PhysicalSchema = physicalSchema,
                };
                EngineContext* scanScopedEngineContextPtr = &scanScopedEngineContext;

                try
                {
                    ExternResultHandleSharedScanMetadataIterator dataIteratorHandle = Methods.scan_metadata_iter_init(this.sharedExternEnginePtr, managedScanPtr);
                    if (dataIteratorHandle.tag != ExternResultHandleSharedScanMetadataIterator_Tag.OkHandleSharedScanMetadataIterator)
                    {
                        throw KernelException.FromEngineError(dataIteratorHandle.Anonymous.Anonymous2.err, "Failed to construct kernel scan data iterator.");
                    }
                    kernelOwnedScanDataIteratorPtr = dataIteratorHandle.Anonymous.Anonymous1.ok;
                    for (; ; )
                    {
                        ExternResultbool isScanOk = Methods.scan_metadata_next(
                            kernelOwnedScanDataIteratorPtr,
                            scanScopedEngineContextPtr,
                            Marshal.GetFunctionPointerForDelegate<VisitScanDataDelegate>(VisitCallbacks.VisitScanData));
                        if (isScanOk.tag != ExternResultbool_Tag.Okbool) throw KernelException.FromEngineError(isScanOk.Anonymous.Anonymous2.err, "Failed to iterate on table scan data.");
                        else if (!isScanOk.Anonymous.Anonymous1.ok) break;
                        else continue;
                    }
                }
                finally
                {
                    if (kernelOwnedScanDataIteratorPtr != null) Methods.free_scan_metadata_iter(kernelOwnedScanDataIteratorPtr);
                    if (tableRootPtr != IntPtr.Zero) Marshal.FreeHGlobal(tableRootPtr);
                }
            }
        }

        #endregion Private Refresh methods
    }
}
