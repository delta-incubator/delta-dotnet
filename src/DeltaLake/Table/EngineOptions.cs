// -----------------------------------------------------------------------------
// <summary>
// Centralized configuration for Delta Engine.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

namespace DeltaLake.Table
{
    /// <summary>
    /// Represents the engine options.
    /// </summary>
    public sealed class EngineOptions
    {
        /// <summary>
        /// Default instance of engine options.
        /// </summary>
        public static EngineOptions Default { get; } = new();

        /// <summary>
        /// If non-zero, sets the batch size of DataFusion to this value.
        /// </summary>
        public ulong DataFusionExecutionBatchSize { get; init; } = 0;

        /// <summary>
        /// If non-zero, sets the maximum memory (in bytes) available to DataFusion before disk spilling occurs.
        /// </summary>
        public ulong DataFusionRuntimeMaxSpillSize { get; init; } = 0;

        /// <summary>
        /// If defined, sets the temp directory DataFusion will use for disk spilling.
        /// </summary>
        public string? DataFusionRuntimeTempDirectory { get; init; } = null;

        /// <summary>
        /// If non-zero, sets the maximum disk space (in bytes) that the DataFusion temp directory can use.
        /// </summary>
        /// <seealso cref="DataFusionRuntimeTempDirectory"/>
        public ulong DataFusionRuntimeMaxTempDirectorySize { get; init; } = 0;
    }
}