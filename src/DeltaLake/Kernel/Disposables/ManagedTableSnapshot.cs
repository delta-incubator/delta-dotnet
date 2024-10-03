// -----------------------------------------------------------------------------
// <summary>
// A disposable handle for a shared, auto-refreshed snapshot.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.Disposables
{
    /// <summary>
    /// A disposable handle for a managed snapshot.
    /// </summary>
    internal class ManagedTableSnapshot : IManagedTableSnapshot
    {
        private bool disposed;
        private readonly KernelStringSlice tableLocationSlice;
        private readonly unsafe SharedExternEngine* sharedExternEnginePtr;
        private unsafe SharedSnapshot* managedPointInTimeSnapshot = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedTableSnapshot"/> class.
        /// </summary>
        /// <param name="tableLocationSlice">The table location slice.</param>
        /// <param name="sharedExternEnginePtr">The Shared External Engine Pointer, not managed in this class.</param>
        public unsafe ManagedTableSnapshot(
            KernelStringSlice tableLocationSlice,
            SharedExternEngine* sharedExternEnginePtr
        )
        {
            this.tableLocationSlice = tableLocationSlice;
            this.sharedExternEnginePtr = sharedExternEnginePtr;

            this.RefreshSnapshot();
        }

        #region IManagedTableSnapshot implementation

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

        #endregion IManagedTableSnapshot implementation

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
                disposed = true;
            }
        }

        ~ManagedTableSnapshot() => Dispose(false);

        #endregion IDisposable implementation

        #region Private methods

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

        private void RefreshSnapshot()
        {
            unsafe
            {
                this.DisposeSnapshot();
                ExternResultHandleSharedSnapshot snapshotRes = Methods.snapshot(
                    this.tableLocationSlice,
                    this.sharedExternEnginePtr
                );
                if (snapshotRes.tag != ExternResultHandleSharedSnapshot_Tag.OkHandleSharedSnapshot)
                {
                    throw new InvalidOperationException(
                        "Failed to retrieve table snapshot from Delta Kernel."
                    );
                }
                this.managedPointInTimeSnapshot = snapshotRes.Anonymous.Anonymous1.ok;
            }
        }

        #endregion Private methods
    }
}
