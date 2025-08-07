// -----------------------------------------------------------------------------
// <summary>
// An engine context holds Kernel engine state passed to and from Kernel.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Kernel.Interop;

namespace DeltaLake.Kernel.State
{
    /// <summary>
    /// Delta Kernel Engine context.
    /// </summary>
    internal struct EngineContext
    {
        /// <summary>
        /// The schema represents the read schema of the table shared from the
        /// Kernel to us.
        /// </summary>
        internal unsafe SharedSchema* LogicalSchema;

        /// <summary>
        /// The schema represents the read schema of the table shared from the
        /// Kernel to us.
        /// </summary>
        internal unsafe SharedSchema* PhysicalSchema;

        /// <summary>
        /// A pointer to the root of the table.
        /// </summary>
        internal unsafe byte* TableRoot;

        /// <summary>
        /// The External Engine represents the external engine (us) we share
        /// with the Kernel for various callbacks.
        /// </summary>
        internal unsafe SharedExternEngine* Engine;

        /// <summary>
        /// Kernel reported list of partitions this Delta Table has.
        /// </summary>
        internal unsafe PartitionList* PartitionList;

        /// <summary>
        /// Kernel reported list of key:values in a partition this Delta Table has.
        /// To actually get a value, we need to invoke [get_from_map].
        /// </summary>
        internal unsafe CStringMap* PartitionKeyValueMap;

        /// <summary>
        /// The ArrowContext holds all state pointers as we load the table from
        /// Parquet throughout the scan, this is used to expose the actual data
        /// back to end-users by converting from Arrow to various APIs.
        /// </summary>
        internal unsafe ArrowContext* ArrowContext;

        public unsafe KernelStringSlice KernelTableRoot()
        {
            if (TableRoot == null)
            {
                return new KernelStringSlice
                {
                    ptr = null,
                    len = System.UIntPtr.Zero,
                };
            }

            uint length = 0;
            while (TableRoot[length] != '\0')
            {
                ++length;
            }

            return new KernelStringSlice
            {
                ptr = (byte*)TableRoot,
                len = new System.UIntPtr(length)
            };
        }
    }
}
