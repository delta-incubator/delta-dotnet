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
    internal struct KernelReadError
    {
        public EngineError etype;
        public IntPtr msg;

        public string Message
        {
            get => Marshal.PtrToStringAnsi(msg);
            set => msg = Marshal.StringToHGlobalAnsi(value);
        }
    }
}
