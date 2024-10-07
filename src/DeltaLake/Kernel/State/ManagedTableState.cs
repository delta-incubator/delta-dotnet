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
using DeltaLake.Kernel.Callbacks.Visit;
using DeltaLake.Kernel.Interop;

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
        private unsafe SharedGlobalScanState* managedGlobalScanState = null;
        private unsafe SharedSchema* managedSchema = null;
        private unsafe PartitionList* partitionList = null;

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
        public unsafe SharedGlobalScanState* GlobalScanState(bool refresh)
        {
            if (refresh || managedGlobalScanState == null) this.RefreshGlobalScanState();
            return managedGlobalScanState;
        }

        /// <inheritdoc/>
        public unsafe SharedSchema* Schema(bool refresh)
        {
            if (refresh || managedSchema == null) this.RefreshSchema();
            return managedSchema;
        }

        /// <inheritdoc/>
        public unsafe PartitionList* PartitionList(bool refresh)
        {
            if (refresh || partitionList == null) this.RefreshPartitionList();
            return partitionList;
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
                this.DisposeGlobalScanState();
                this.DisposeScan();

                disposed = true;
            }
        }

        ~ManagedTableState() => Dispose(false);

        #endregion IDisposable implementation

        #region Private Dispose methods

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
                if (this.managedSchema != null)
                {
                    Methods.free_global_read_schema(this.managedSchema);
                    this.managedSchema = null;
                }
            }
        }

        private void DisposeGlobalScanState()
        {
            unsafe
            {
                if (this.managedGlobalScanState != null)
                {
                    Methods.free_global_scan_state(this.managedGlobalScanState);
                    this.managedGlobalScanState = null;
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
            unsafe
            {
                this.DisposePartitionList();
                int partitionColumnCount = (int)Methods.get_partition_column_count(this.GlobalScanState(false));
                this.partitionList = (PartitionList*)Marshal.AllocHGlobal(sizeof(PartitionList));

                // We set the length to 0 here and use it to track how many
                // items we've added.
                //
                this.partitionList->Len = 0;
                this.partitionList->Cols = (char**)Marshal.AllocHGlobal(sizeof(char*) * partitionColumnCount);

                StringSliceIterator* partitionIterator = Methods.get_partition_columns(this.GlobalScanState(false));
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
            unsafe
            {
                this.DisposeScan();
                ExternResultHandleSharedScan scanRes = Methods.scan(this.Snapshot(false), this.sharedExternEnginePtr, null);
                if (scanRes.tag != ExternResultHandleSharedScan_Tag.OkHandleSharedScan)
                {
                    throw new InvalidOperationException("Failed to create table scan from Delta Kernel.");
                }
                this.managedScan = scanRes.Anonymous.Anonymous1.ok;
            }
        }

        private void RefreshSchema()
        {
            unsafe
            {
                this.DisposeSchema();
                this.managedSchema = Methods.get_global_read_schema(this.GlobalScanState(false));
            }
        }

        private void RefreshGlobalScanState()
        {
            unsafe
            {
                this.DisposeGlobalScanState();
                this.managedGlobalScanState = Methods.get_global_scan_state(this.Scan(false));
            }
        }

        private void RefreshSnapshot()
        {
            unsafe
            {
                this.DisposeSnapshot();
                ExternResultHandleSharedSnapshot snapshotRes = Methods.snapshot(this.tableLocationSlice, this.sharedExternEnginePtr);
                if (snapshotRes.tag != ExternResultHandleSharedSnapshot_Tag.OkHandleSharedSnapshot)
                {
                    throw new InvalidOperationException("Failed to retrieve table snapshot from Delta Kernel.");
                }
                this.managedPointInTimeSnapshot = snapshotRes.Anonymous.Anonymous1.ok;
            }
        }

        #endregion Private Refresh methods
    }
}
