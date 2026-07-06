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

using System;
using System.Threading.Tasks;
using DeltaLake.Bridge.Interop;
using DeltaLake.Table;
using DeltaRustBridge = DeltaLake.Bridge;

namespace DeltaLake.Kernel.Core
{
    /// <summary>
    /// Core-owned Delta Kernel runtime.
    ///
    /// The idea is, we prioritize the Kernel FFI implementations, and
    /// operations not supported by the FFI (such as create table, load version)
    /// yet falls back to Delta RS Runtime implementation.
    /// </summary>
    internal class Runtime : DeltaRustBridge.Runtime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Runtime"/> class.
        /// </summary>
        /// <param name="options">Engine options.</param>
        internal Runtime(EngineOptions options)
            : base(options) { }

        /// <summary>
        /// A thin wrapper around the Delta Rust Runtime to load a table.
        /// </summary>
        /// <param name="options">Table options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        internal async Task<Table> LoadTableAsync(
            DeltaLake.Table.TableOptions options,
            System.Threading.CancellationToken cancellationToken
        )
        {
            IntPtr tablePtr = await base.LoadTablePtrAsync(options, cancellationToken).ConfigureAwait(false);
            unsafe
            {
                return new Table(this, (RawDeltaTable*)tablePtr, options);
            }
        }

        /// <summary>
        /// A thin wrapper around the Delta Rust Runtime to create a table.
        /// </summary>
        /// <param name="options">Table creation options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        internal async Task<Table> CreateTableAsync(
            DeltaLake.Table.TableCreateOptions options,
            System.Threading.CancellationToken cancellationToken
        )
        {
            // NOTE: create-table runs through the delta-rs BRIDGE (create_deltalake), not the
            // kernel commit path, so there is no kernel ExclusiveCommittedTransaction / post-commit
            // snapshot to install here (unlike CommitAddActionsAsync, which advances the cached
            // kernel snapshot for free via committed_transaction_post_commit_snapshot). The kernel's
            // maintained snapshot is built lazily on the first kernel read/Version() via the
            // incremental RefreshSnapshot. Installing a post-commit snapshot at create time would
            // require routing create-table through the (currently unused) kernel create-table FFI
            // (get_create_table_builder / create_table_commit); the payoff is negligible because the
            // first snapshot build on a brand-new empty table is already cheap.
            IntPtr tablePtr = await base.CreateTablePtrAsync(options, cancellationToken).ConfigureAwait(false);
            unsafe
            {
                return new Table(this, (RawDeltaTable*)tablePtr, options);
            }
        }
    }
}
