// -----------------------------------------------------------------------------
// <summary>
// The Delta Kernel FFI Runtime.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using DeltaLake.Table;
using DeltaRustBridge = DeltaLake.Bridge;

namespace DeltaLake.Kernel.Core
{
    /// <summary>
    /// Core-owned Delta Kernel runtime.
    ///
    /// The idea is, we prioritize the Kernel FFI implementations, and
    /// operations not supported by the FFI yet falls back to Delta RS Runtime
    /// implementation.
    /// </summary>
    /// <remarks>
    /// This class isn't really used today, because the Kernel's entry point is
    /// at per table scope, meaning, we essentially maintain a Kernel Runtime per
    /// table.
    ///
    /// In the future, if Kernel exposes a global FFI that spans table, we can overload
    /// this class to provide the global Kernel Runtime.
    /// </remarks>
    internal class Runtime : DeltaRustBridge.Runtime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Runtime"/> class.
        /// </summary>
        /// <param name="options">Engine options.</param>
        internal Runtime(EngineOptions options)
            : base(options) { }
    }
}
