// -----------------------------------------------------------------------------
// <summary>
// Extension methods for transforming strings.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaLake.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="String"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a pinned sbyte pointer.
        /// </summary>
        /// <remarks>
        /// Caller is responsible for freeing the GCHandle when done with the pointer.
        /// </remarks>
        /// <param name="str">The string to convert.</param>
        /// <returns>A tuple containing the pinned handle and the sbyte pointer.</returns>
        public static unsafe (GCHandle handle, IntPtr pointer) ToPinnedBytePointer(this string str)
        {
            int byteCount = Encoding.UTF8.GetByteCount(str);
            byte[] bytes = new byte[byteCount];
            fixed (char* strPtr = str)
            fixed (byte* bytesPtr = bytes)
            {
                _ = Encoding.UTF8.GetBytes(strPtr, str.Length, (byte*)bytesPtr, byteCount);
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            return (handle, handle.AddrOfPinnedObject());
        }
    }
}
