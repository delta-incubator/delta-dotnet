// -----------------------------------------------------------------------------
// <summary>
// An IDisposable wrapper that owns a Microsoft.Data.Analysis.DataFrame
// together with the ArrowContextHandle that backs its imported RecordBatches
// and native allocations.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using DeltaLake.Kernel.State;
using Microsoft.Data.Analysis;

namespace DeltaLake.Kernel.Core
{
    /// <summary>
    /// <see cref="IDisposable"/> wrapper that holds a <see cref="DataFrame"/>
    /// together with the <see cref="ArrowContextHandle"/> that owns the
    /// imported <see cref="Apache.Arrow.RecordBatch"/> instances and native
    /// <see cref="ArrowContext"/> allocation backing it.
    /// </summary>
    /// <remarks>
    /// Disposing the wrapper releases the managed record batches first (firing
    /// the C-Data Interface release callbacks) and then frees the native
    /// allocation. The contained <see cref="Frame"/> reference is undefined
    /// after the wrapper is disposed; callers must not retain it past the
    /// <c>using</c> scope that produced the wrapper.
    /// </remarks>
    public sealed class OwnedDataFrame : IDisposable
    {
        private readonly ArrowContextHandle _handle;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedDataFrame"/> class.
        /// </summary>
        /// <param name="frame">The data frame built from the imported record batches.</param>
        /// <param name="handle">The handle that owns the underlying record batches and native allocation.</param>
        internal OwnedDataFrame(DataFrame frame, ArrowContextHandle handle)
        {
            Frame = frame;
            _handle = handle;
        }

        /// <summary>
        /// Gets the underlying data frame. The reference is undefined after the
        /// wrapper is disposed.
        /// </summary>
        public DataFrame Frame { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _handle.Dispose();
        }
    }
}
