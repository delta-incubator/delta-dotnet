// -----------------------------------------------------------------------------
// <summary>
// A disposable arrow context holds Delta table data in arrow format.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Apache.Arrow;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// Delta table Arrow context.
    /// </summary>
    internal class ArrowContext : IDisposable
    {
#pragma warning disable IDE0044 // Add readonly modifier
        private bool disposed;
#pragma warning restore IDE0044

        public int NumBatches;
        public Schema Schema;
        public unsafe RecordBatch** Batches;
        public unsafe BooleanArray* CurFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrowContext"/> class.
        /// </summary>
        public ArrowContext()
        {
#pragma warning disable CS8625
            NumBatches = 0;
            Schema = null;

            unsafe
            {
                Batches = null;
                CurFilter = null;
            }

            disposed = false;
        }
#pragma warning restore CS8625

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
            unsafe
            {
                for (int i = 0; i < NumBatches; i++)
                {
                    RecordBatch* batch = Batches[i];
                    if (batch != null)
                    {
                        Marshal.FreeHGlobal((nint)batch);
                        Batches[i] = null;
                    }
                }
                Marshal.FreeHGlobal((nint)Batches);
            }
        }

        #endregion IDisposable implementation
    }
}
