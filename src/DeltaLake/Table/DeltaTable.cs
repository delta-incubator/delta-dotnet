// -----------------------------------------------------------------------------
// <summary>
// A single delta table.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using DeltaLake.Errors;
using DeltaLake.Interfaces;
using Microsoft.Data.Analysis;
using Core = DeltaLake.Kernel.Core;

namespace DeltaLake.Table
{
    /// <summary>
    /// The primary Delta Table implementation.
    /// </summary>
    public sealed class DeltaTable : ITable
    {
        private readonly Core.Table table;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeltaTable"/> class.
        /// </summary>
        /// <param name="table">The Delta Kernel Table.</param>
        internal DeltaTable(Core.Table table) => this.table = table;

        #region ITable implementation

        /// <inheritdoc/>
        public string[] Files() => this.table.Files();

        /// <inheritdoc/>
        public string[] FileUris() => this.table.FileUris();

        /// <inheritdoc/>
        public string Location() => this.table.Uri();

        /// <inheritdoc/>
        public ulong Version() => (ulong)this.table.Version();

        /// <inheritdoc/>
        public Schema Schema() => this.table.Schema();

        /// <inheritdoc/>
        public TableMetadata Metadata() => this.table.Metadata();

        /// <inheritdoc/>
        public ProtocolInfo ProtocolVersions() => this.table.ProtocolVersions();

        /// <inheritdoc/>
        public async Task<CommitInfo[]> HistoryAsync(
            ulong? limit,
            CancellationToken cancellationToken
        ) =>
            JsonSerializer.Deserialize<CommitInfo[]>(
                await this.table.HistoryAsync(limit ?? 0, cancellationToken).ConfigureAwait(false)
            ) ?? System.Array.Empty<CommitInfo>();

        /// <inheritdoc/>
        public Task LoadVersionAsync(ulong version, CancellationToken cancellationToken) =>
            this.table.LoadVersionAsync(version, cancellationToken);

        /// <inheritdoc/>
        public Task LoadDateTimeAsync(
            DateTimeOffset timestamp,
            CancellationToken cancellationToken
        ) => this.LoadDateTimeAsync(timestamp.ToUnixTimeMilliseconds(), cancellationToken);

        /// <inheritdoc/>
        public Task LoadDateTimeAsync(
            long timestampMilliseconds,
            CancellationToken cancellationToken
        ) => this.table.LoadTimestampAsync(timestampMilliseconds, cancellationToken);

        /// <inheritdoc/>
        public Task UpdateIncrementalAsync(long? maxVersion, CancellationToken cancellationToken) =>
            this.table.UpdateIncrementalAsync(maxVersion, cancellationToken);

        /// <inheritdoc/>
        public Task RestoreAsync(RestoreOptions options, CancellationToken cancellationToken) =>
            this.table.RestoreAsync(options, cancellationToken);

        /// <inheritdoc/>
        public async IAsyncEnumerable<RecordBatch> QueryAsync(
            SelectQuery query,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var result = await this
                .table.QueryAsync(query.Query, query.TableAlias, cancellationToken)
                .ConfigureAwait(false);

            while (true)
            {
                var rb = await result
                    .ReadNextRecordBatchAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (rb == null)
                {
                    yield break;
                }

                yield return rb;
            }
        }

        /// <inheritdoc/>
        public async Task<Apache.Arrow.Table> ReadAsArrowTableAsync(
            CancellationToken cancellationToken
        ) => await this.table.ReadAsArrowTableAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<DataFrame> ReadAsDataFrameAsync(CancellationToken cancellationToken) =>
            await this.table.ReadAsDataFrameAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task InsertAsync(
            IReadOnlyCollection<RecordBatch> records,
            Schema schema,
            InsertOptions options,
            CancellationToken cancellationToken
        )
        {
            if (!options.IsValid)
            {
                throw new DeltaConfigurationException(
                    "Invalid InsertOptions",
                    new ArgumentException("configuration is invalid", nameof(options))
                );
            }

            _ = await this
                .table.InsertAsync(records, schema, options, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task InsertAsync(
            IArrowArrayStream records,
            InsertOptions options,
            CancellationToken cancellationToken
        )
        {
            if (!options.IsValid)
            {
                throw new DeltaConfigurationException(
                    "Invalid InsertOptions",
                    new ArgumentException("configuration is invalid", nameof(options))
                );
            }

            _ = await this
                .table.InsertAsync(records, options, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task MergeAsync(
            string query,
            IReadOnlyCollection<RecordBatch> records,
            Schema schema,
            CancellationToken cancellationToken
        ) =>
            await this
                .table.MergeAsync(query, records, schema, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public Task<string> UpdateAsync(string query, CancellationToken cancellationToken) =>
            this.table.UpdateAsync(query, cancellationToken);

        /// <inheritdoc/>
        public Task DeleteAsync(string predicate, CancellationToken cancellationToken) =>
            this.table.DeleteAsync(predicate, cancellationToken);

        /// <inheritdoc/>
        public Task DeleteAsync(CancellationToken cancellationToken) =>
            this.table.DeleteAsync(string.Empty, cancellationToken);

        /// <inheritdoc/>
        public async Task AddConstraintsAsync(
            IReadOnlyDictionary<string, string> constraints,
            IReadOnlyDictionary<string, string>? customMetadata,
            CancellationToken cancellationToken
        ) =>
            await this
                .table.AddConstraintAsync(constraints, customMetadata, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public Task AddConstraintsAsync(
            IReadOnlyDictionary<string, string> constraints,
            CancellationToken cancellationToken
        ) => this.AddConstraintsAsync(constraints, null, cancellationToken);

        #endregion ITable implementation

        #region IDisposable implementation

        /// <inheritdoc />
        public void Dispose() => this.table.Dispose();

        #endregion IDisposable implementation
    }
}
