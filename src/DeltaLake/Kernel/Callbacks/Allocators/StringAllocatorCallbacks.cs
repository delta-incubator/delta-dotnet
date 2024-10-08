// -----------------------------------------------------------------------------
// <summary>
// String allocation callback hooks.
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

namespace DeltaLake.Kernel.Callbacks.Allocators
{
    /// <summary>
    /// The Kernel String Allocation Callback method.
    /// </summary>
    internal static unsafe class StringAllocatorCallbacks
    {
        /// <summary>
        /// Allocates a string from a slice. Caller is responsible for freeing
        /// the memory.
        /// </summary>
        /// <param name="slice">The Kernel Slice.</param>
        /// <returns>The allocated string.</returns>
        internal static void* AllocateString(KernelStringSlice slice)
        {
            if (slice.ptr == null || (int)slice.len == 0)
                return null;

            // Allocate unmanaged memory for the string, adding 1 for the null
            // terminator.
            //
            int len = (int)slice.len;
            IntPtr unmanagedMemory = Marshal.AllocHGlobal(len + 1);

            // Copy the string into the allocated unmanaged memory.
            //
            for (int i = 0; i < len; i++)
            {
                Marshal.WriteByte(unmanagedMemory, i, Marshal.ReadByte((IntPtr)slice.ptr, i));
            }

            // Set the null terminator.
            //
            Marshal.WriteByte(unmanagedMemory, len, 0);

            return unmanagedMemory.ToPointer();
        }
    }
}
