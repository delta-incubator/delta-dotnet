// -----------------------------------------------------------------------------
// <summary>
// Engine Visitor callbacks.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System.Runtime.InteropServices;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.State;

namespace DeltaLake.Kernel.Callbacks.Visit
{
    /// <summary>
    /// Callbacks when Kernel asks us to visit a particular context.
    /// </summary>
    internal static class VisitCallbacks
    {
        /// <summary>
        /// Visits the partition.
        /// </summary>
        /// <param name="context">The partition context.</param>
        /// <param name="partition">The partition kernel slice.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void VisitPartitionDelegate(void* context, KernelStringSlice partition);

        /// <remarks>
        /// Caller is responsible for freeing the memory allocated to the column pointer.
        /// </remarks>
        internal static unsafe VisitPartitionDelegate VisitPartition = new((void* context, KernelStringSlice partition) =>
            {
                PartitionList* list = (PartitionList*)context;
                char* col = (char*)StringAllocatorCallbacks.AllocateString(partition);
                list->Cols[list->Len] = col;
                list->Len++;
            }
        );
    }
}
