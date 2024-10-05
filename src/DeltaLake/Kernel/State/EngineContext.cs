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
        internal unsafe SharedGlobalScanState* GlobalScanState;
        internal unsafe SharedSchema* Schema;
        internal unsafe char* TableRoot;
        internal unsafe SharedExternEngine* Engine;
        internal unsafe PartitionList* PartitionList;
        internal unsafe CStringMap* PartitionValues;
        internal unsafe ArrowContext* ArrowContext;
    }
}
