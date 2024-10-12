// -----------------------------------------------------------------------------
// <summary>
// The end-user engine interface to reuse across multiple table operations.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using DeltaLake.Table;
using CancellationToken = System.Threading.CancellationToken;

namespace DeltaLake.Interfaces
{
    /// <summary>
    /// Engine can be reused across multiple table operations.
    /// </summary>
    public interface IEngine : IDisposable
    {
        /// <summary>
        /// Creates a new table from the specified options.
        /// </summary>
        /// <param name="options">The options for creating the table.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A new table.</returns>
        Task<ITable> CreateTableAsync(TableCreateOptions options, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously loads a table at the latest version.
        /// </summary>
        /// <param name="options">The options for loading the table.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>.</param>
        /// <returns>A new table loaded at the latest version.</returns>
        Task<ITable> LoadTableAsync(TableOptions options, CancellationToken cancellationToken);
    }
}
