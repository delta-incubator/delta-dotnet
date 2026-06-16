// -----------------------------------------------------------------------------
// <summary>
// An IDisposable handle that owns the native ArrowContext struct plus the
// managed RecordBatches and Schema imported from it. Disposing the handle
// fires the C-Data Interface release callbacks on the imported RecordBatches
// first, then frees the underlying ArrowContext native allocation.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Apache.Arrow;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// Owns the unmanaged <see cref="ArrowContext"/> struct allocated by
    /// <see cref="ManagedTableState"/> and the managed
    /// <see cref="Apache.Arrow.RecordBatch"/> instances imported from it via the
    /// C-Data Interface, alongside the <see cref="Apache.Arrow.Schema"/>.
    /// </summary>
    /// <remarks>
    /// Dispose order is load-bearing: the managed <see cref="RecordBatch"/>
    /// instances are disposed first so the per-array C-Data release callbacks
    /// fire while the backing <see cref="ArrowFFIData"/> pointers are still
    /// valid, then the native <see cref="ArrowContext"/> allocation is freed via
    /// <see cref="ManagedTableState.FreeArrowContextNative(ArrowContext*)"/>.
    /// </remarks>
    internal sealed class ArrowContextHandle : IDisposable
    {
        private unsafe ArrowContext* _native;
        private List<RecordBatch>? _recordBatches;
        private Schema? _schema;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrowContextHandle"/> class.
        /// Ownership of <paramref name="native"/> and every batch in
        /// <paramref name="recordBatches"/> transfers to this handle and they
        /// are released when <see cref="Dispose()"/> runs.
        /// </summary>
        /// <param name="native">The native <see cref="ArrowContext"/> struct to own.</param>
        /// <param name="schema">The Arrow schema describing the imported batches.</param>
        /// <param name="recordBatches">The managed record batches imported from <paramref name="native"/>.</param>
        internal unsafe ArrowContextHandle(
            ArrowContext* native,
            Schema schema,
            List<RecordBatch> recordBatches)
        {
            _native = native;
            _schema = schema;
            _recordBatches = recordBatches;
        }

        /// <summary>Gets the Arrow schema describing the owned record batches.</summary>
        /// <exception cref="ObjectDisposedException">Thrown when the handle has already been disposed.</exception>
        internal Schema Schema => _schema ?? throw new ObjectDisposedException(nameof(ArrowContextHandle));

        /// <summary>Gets the managed record batches owned by this handle.</summary>
        /// <exception cref="ObjectDisposedException">Thrown when the handle has already been disposed.</exception>
        internal IReadOnlyList<RecordBatch> RecordBatches =>
            _recordBatches ?? throw new ObjectDisposedException(nameof(ArrowContextHandle));

        /// <summary>
        /// Gets the backing record-batch list as <see cref="IList{T}"/> for internal Arrow
        /// interop callers that need the mutable-list interface required by
        /// <see cref="Apache.Arrow.Table.TableFromRecordBatches"/>. The handle still owns the
        /// returned list; callers must not mutate or retain a reference beyond the handle's
        /// lifetime.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the handle has already been disposed.</exception>
        internal IList<RecordBatch> RecordBatchList =>
            _recordBatches ?? throw new ObjectDisposedException(nameof(ArrowContextHandle));

        /// <inheritdoc/>
        public void Dispose() => Dispose(disposing: true);

        /// <summary>Finalizer that releases unmanaged resources when Dispose was not called.</summary>
        ~ArrowContextHandle() => Dispose(disposing: false);

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (_recordBatches is not null)
            {
                foreach (RecordBatch rb in _recordBatches)
                {
                    try { rb.Dispose(); } catch { /* drain — must continue to native free */ }
                }
                _recordBatches = null;
                _schema = null;
            }

            unsafe
            {
                if (_native != null)
                {
                    ManagedTableState.FreeArrowContextNative(_native);
                    _native = null;
                }
            }

            if (disposing) GC.SuppressFinalize(this);
        }
    }
}
