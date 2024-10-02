// -----------------------------------------------------------------------------
// <summary>
// Delta Engine manages one or more Delta Tables throughout the lifetime of an
// application.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DeltaLake.Interfaces;
using DeltaLake.Kernel.Core;
using CancellationToken = System.Threading.CancellationToken;
using Core = DeltaLake.Kernel.Core;

namespace DeltaLake.Table
{
    /// <summary>
    /// Represents the Delta Lake Engine.
    /// </summary>
    public sealed class DeltaEngine : IEngine
    {
        private Runtime Runtime { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaEngine"/> class.
        /// </summary>
        /// <param name="options">The engine options.</param>
        public DeltaEngine(EngineOptions options) => this.Runtime = new Runtime(options);

        #region IEngineRuntime implementation

        /// <inheritdoc/>
        public async Task<ITable> CreateTableAsync(
            TableCreateOptions options,
            CancellationToken cancellationToken
        ) =>
            new DeltaTable(
                new Core.Table(
                    await Runtime
                        .CreateTableAsync(options, cancellationToken)
                        .ConfigureAwait(false),
                    options
                )
            );

        /// <inheritdoc/>
        public async Task<ITable> LoadTableAsync(
            TableOptions options,
            CancellationToken cancellationToken
        ) =>
            new DeltaTable(
                new Core.Table(
                    await Runtime
                        .LoadTableAsync(options, cancellationToken)
                        .ConfigureAwait(false),
                    options
                )
            );

        #endregion IEngineRuntime implementation

        #region IDisposable implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                Runtime.Dispose();
            }
        }

        #endregion IDisposable implementation
    }
}
