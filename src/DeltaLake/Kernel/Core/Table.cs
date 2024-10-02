// -----------------------------------------------------------------------------
// <summary>
// The Kernel representation of a Delta Table.
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
    /// Reference to unmanaged delta table.
    ///
    /// The idea is, we prioritize the Kernel FFI implementations, and
    /// operations not supported by the FFI yet falls back to Delta RS Runtime
    /// implementation.
    /// </summary>
    internal class Table : DeltaRustBridge.Table
    {
        private readonly TableStorageOptions tableStorageOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="table">The Delta Rust table.</param>
        /// <param name="tableStorageOptions">The table storage options.</param>
        internal unsafe Table(DeltaRustBridge.Table table, TableStorageOptions tableStorageOptions)
            : base(table._runtime, table._ptr)
        {
            this.tableStorageOptions = tableStorageOptions;
        }
    }
}
