// -----------------------------------------------------------------------------
// <summary>
// A disposable handle for a shared, auto-refreshed Kernel Table state.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
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

        #region ISafeState implementation

        /// <inheritdoc/>
        public unsafe SharedSnapshot* Snapshot
        {
            get
            {
                this.RefreshSnapshot();
                return managedPointInTimeSnapshot;
            }
            private set => managedPointInTimeSnapshot = value;
        }

        /// <inheritdoc/>
        public unsafe SharedScan* Scan
        {
            get
            {
                this.RefreshScan();
                return managedScan;
            }
            private set => managedScan = value;
        }

        /// <inheritdoc/>
        public unsafe SharedGlobalScanState* GlobalScanState
        {
            get
            {
                this.RefreshGlobalScanState();
                return managedGlobalScanState;
            }
            private set => managedGlobalScanState = value;
        }

        public unsafe SharedSchema* Schema
        {
            get
            {
                this.RefreshSchema();
                return managedSchema;
            }
            private set => managedSchema = value;
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
                this.DisposeSnapshot();
                this.DisposeSchema();
                this.DisposeGlobalScanState();
                this.DisposeScan();

                disposed = true;
            }
        }

        ~ManagedTableState() => Dispose(false);

        #endregion IDisposable implementation

        #region Private methods

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

        private void RefreshScan()
        {
            unsafe
            {
                this.DisposeScan();
                ExternResultHandleSharedScan scanRes = Methods.scan(this.Snapshot, this.sharedExternEnginePtr, null);
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
                this.managedSchema = Methods.get_global_read_schema(this.GlobalScanState);
            }
        }

        private void RefreshGlobalScanState()
        {
            unsafe
            {
                this.DisposeGlobalScanState();
                this.managedGlobalScanState = Methods.get_global_scan_state(this.Scan);
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

        #endregion Private methods
    }
}
