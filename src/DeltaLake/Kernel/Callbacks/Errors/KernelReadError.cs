// -----------------------------------------------------------------------------
// <summary>
// Error handling for Kernel read operations.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.Callbacks.Errors
{
    /// <summary>
    /// Represents an error that occurred during reading operations via the Kernel.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct KernelReadError
    {
        public KernelReadError(EngineError engineError, KernelStringSlice kernelStringSlice)
        {
            unsafe
            {
                msg = kernelStringSlice.ptr == null ? "test message" : System.Text.Encoding.UTF8.GetString(kernelStringSlice.ptr, (int)kernelStringSlice.len);
            }

            etype = engineError;
        }

        // [FieldOffset(0)]
        public EngineError etype;

        // [FieldOffset(4)]
        public string msg;
    }
}
