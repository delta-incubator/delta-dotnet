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
        public static EngineOptions Default { get; } = new EngineOptions();
    }
}