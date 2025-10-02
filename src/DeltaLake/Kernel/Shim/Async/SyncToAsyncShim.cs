// -----------------------------------------------------------------------------
// <summary>
// A shim for turning synchronous operations to be asynchronous and cancellable.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaLake.Kernel.Shim.Async
{
    internal static class SyncToAsyncShim
    {
        /// <summary>
        /// Converts a synchronous operation to a cancellable asynchronous
        /// operation.
        /// </summary>
        /// <param name="action">Action to invoke.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of the result.</typeparam>
        internal static async Task<T> ExecuteAsync<T>(
            Func<T> action,
            CancellationToken cancellationToken
        )
        {
            return await Task.Run(() => action(), cancellationToken).ConfigureAwait(false);
        }
    }
}
