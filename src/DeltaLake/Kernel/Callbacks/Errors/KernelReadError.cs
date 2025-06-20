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
    internal unsafe delegate void KernelReadErrorHandler(KernelReadError* error);
    internal unsafe delegate T KernelReadErrorProcessor<T>(KernelReadError* error);
    /// <summary>
    /// Represents an error that occurred during reading operations via the Kernel.
    /// It's memory layout matches the initial layout of <see cref="EngineError"/>
    /// so that we can pass it to the kernel as the same type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct KernelReadError
    {
        public KernelReadError(EngineError engineError, KernelStringSlice kernelStringSlice)
        {
            unsafe
            {
                if (kernelStringSlice.len.ToUInt64() == 0UL)
                {
                    ptr = null;
                }
                else
                {
                    ptr = (byte*)Marshal.AllocHGlobal((int)kernelStringSlice.len);
                    for (nuint i = 0; i < kernelStringSlice.len; i++)
                    {
                        *ptr++ = *kernelStringSlice.ptr++;
                    }
                }

                len = kernelStringSlice.len;
            }

            etype = engineError;
        }

        public EngineError etype;

        private byte* ptr;

        private readonly nuint len;

        public string Message => GetMessage();

        private string GetMessage()
        {
            if (len == 0)
            {
                return string.Empty;
            }

            return System.Text.Encoding.UTF8.GetString(ptr, (int)len);
        }


        public static void HandleEngineError(EngineError* source, KernelReadErrorHandler work)
        {
            var error = (KernelReadError*)source;
            try
            {
                work(error);
            }
            finally
            {
                Marshal.FreeHGlobal((IntPtr)error->ptr);
                Marshal.FreeHGlobal((IntPtr)error);
            }
        }

        public static T ProcessEngineError<T>(EngineError* source, KernelReadErrorProcessor<T> work)
        {
            var error = (KernelReadError*)source;
            try
            {
                return work(error);
            }
            finally
            {
                Marshal.FreeHGlobal((IntPtr)error->ptr);
                Marshal.FreeHGlobal((IntPtr)error);
            }
        }
    }
}
