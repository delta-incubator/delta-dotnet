using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using DeltaLake.Errors;
using DeltaLake.Runtime;

namespace DeltaLake.Table
{
    /// <summary>
    /// Represents a deltalake table
    /// </summary>
    public sealed class DeltaTable : IDisposable
    {
        private readonly Bridge.Table _table;

        private DeltaTable(Bridge.Table table)
        {
            _table = table;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="location"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns>A new delta table loaded at the latest version</returns>
        public static async Task<DeltaTable> LoadAsync(
            DeltaRuntime runtime,
             string location,
              TableOptions options,
              CancellationToken cancellationToken)
        {
            var table = await runtime.Runtime.LoadTableAsync(location, options, cancellationToken).ConfigureAwait(false);
            return new DeltaTable(table);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns>A new delta table loaded at the latest version</returns>
        public static async Task<DeltaTable> LoadAsync(
            DeltaRuntime runtime,
             Memory<byte> uri,
              TableOptions options,
              CancellationToken cancellationToken)
        {
            var table = await runtime.Runtime.LoadTableAsync(uri, options, cancellationToken).ConfigureAwait(false);
            return new DeltaTable(table);
        }

        /// <summary>
        /// Creates a new delta table from the specified options
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns>New DeltaTable</returns>
        public static async Task<DeltaTable> CreateAsync(
            DeltaRuntime runtime,
             TableCreateOptions options,
              CancellationToken cancellationToken)
        {
            var table = await runtime.Runtime.CreateTableAsync(options, cancellationToken).ConfigureAwait(false);
            return new DeltaTable(table);
        }

        /// <summary>
        /// Retrieves the current table files
        /// </summary>
        /// <returns>list of files</returns>
        public string[] Files()
        {
            return _table.Files();
        }

        /// <summary>
        /// Retrieves the current table file uris
        /// </summary>
        /// <returns>list of paths</returns>
        public string[] FileUris()
        {
            return _table.FileUris();
        }

        /// <summary>
        /// Retrieves the current table location
        /// </summary>
        /// <returns>table location</returns>
        public string Location()
        {
            return _table.Uri();
        }

        /// <summary>
        /// Retrieves the current table version
        /// </summary>
        /// <returns><see cref="ulong"/></returns>
        public ulong Version()
        {
            return (ulong)_table.Version();
        }

        /// <summary>
        /// Loads table at specific version
        /// </summary>
        /// <param name="version">desired version</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns><see cref="Task"/></returns>
        public Task LoadVersionAsync(ulong version, CancellationToken cancellationToken)
        {
            return _table.LoadVersionAsync(version, cancellationToken);
        }

        /// <summary>
        /// Returns the table schema
        /// </summary>
        /// <returns><see cref="Schema"/></returns>
        public Apache.Arrow.Schema Schema()
        {
            return _table.Schema();
        }

        /// <summary>
        /// Inserts a record batch into the table based upon the provided options
        /// </summary>
        /// <param name="records">A collection of <see cref="RecordBatch"/> records to insert</param>
        /// <param name="schema">The associated <see cref="Schema"/> for the <paramref name="records"/></param>
        /// <param name="options"><see cref="InsertOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns><see cref="Task"/></returns>
        public async Task InsertAsync(
             IReadOnlyCollection<RecordBatch> records,
             Schema schema,
             InsertOptions options,
             CancellationToken cancellationToken)
        {
            if (!options.IsValid)
            {
                throw new DeltaConfigurationException(
                    "Invalid InsertOptions",
                    new ArgumentException("configuration is invalid", nameof(options)));
            }

            await _table.InsertAsync(records, schema, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts a record batch into the table based upon the provided options
        /// </summary>
        /// <param name="records">A collection of <see cref="IArrowArrayStream"/> records to insert</param>
        /// <param name="options"><see cref="InsertOptions"/> </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns><see cref="Task"/></returns>
        public async Task InsertAsync(
             IArrowArrayStream records,
             InsertOptions options,
             CancellationToken cancellationToken)
        {

            if (!options.IsValid)
            {
                throw new DeltaConfigurationException(
                    "Invalid InsertOptions",
                    new ArgumentException("configuration is invalid", nameof(options)));
            }

            await _table.InsertAsync(records, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the table history
        /// </summary>
        /// <param name="limit">Optional maximum amount of history to return</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns>An array of <see cref="CommitInfo"/> </returns>
        public async Task<CommitInfo[]> HistoryAsync(ulong? limit, CancellationToken cancellationToken)
        {
            var content = await _table.HistoryAsync(limit ?? 0, cancellationToken).ConfigureAwait(false);
            return System.Text.Json.JsonSerializer.Deserialize<CommitInfo[]>(content) ?? System.Array.Empty<CommitInfo>();
        }

        /// <summary>
        /// Adds constraints and custom metadata to a table
        /// </summary>
        /// <param name="constraints">A collection of key and values representing columns and constraints</param>
        /// <param name="customMetadata">A collection of key and values representing columns and metadata</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns><see cref="Task"/> </returns>
        public async Task AddConstraintsAsync(
            IReadOnlyDictionary<string, string> constraints,
            IReadOnlyDictionary<string, string>? customMetadata,
            CancellationToken cancellationToken)
        {
            await _table.AddConstraintAsync(constraints, customMetadata, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds constraints and custom metadata to a table
        /// </summary>
        /// <param name="constraints">A collection of key and values representing columns and constraints</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns><see cref="Task"/> </returns>
        public Task AddConstraintsAsync(
            IReadOnlyDictionary<string, string> constraints,
            CancellationToken cancellationToken)
        {
            return AddConstraintsAsync(constraints, null, cancellationToken);
        }

        /// <summary>
        /// Updates table to specific or latest version
        /// </summary>
        /// <param name="maxVersion">Optional maximum version</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        public Task UpdateIncrementalAsync(long? maxVersion, CancellationToken cancellationToken)
        {
            return _table.UpdateIncrementalAsync(maxVersion, cancellationToken);
        }

        /// <summary>
        /// Loads table at a specific point in time
        /// </summary>
        /// <param name="timestamp">desired point in time</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns><see cref="Task"/></returns>
        public Task LoadDateTimeAsync(DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            return LoadDateTimeAsync(timestamp.ToUnixTimeMilliseconds(), cancellationToken);
        }

        /// <summary>
        /// Loads table at a specific point in time
        /// </summary>
        /// <param name="timestampMilliseconds">desired point in time in unix milliseconds</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns><see cref="Task"/></returns>
        public Task LoadDateTimeAsync(long timestampMilliseconds, CancellationToken cancellationToken)
        {
            return _table.LoadTimestampAsync(timestampMilliseconds, cancellationToken);
        }

        /// <summary>
        /// Merges a collection of <see cref="RecordBatch"/> into the delta table
        /// </summary>
        /// <param name="query">A SQL MERGE statement</param>
        /// <param name="records">A collection of record batches</param>
        /// <param name="schema">The schema of the collection</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task MergeAsync(string query, IReadOnlyCollection<RecordBatch> records, Schema schema, CancellationToken cancellationToken)
        {
            await _table.MergeAsync(query, records, schema, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns table metadata
        /// </summary>
        /// <returns><see cref="DeltaLake.Table.TableMetadata"/></returns>
        public TableMetadata Metadata()
        {
            return _table.Metadata();
        }

        /// <summary>
        /// Returns minimum reader and writer versions
        /// </summary>
        /// <returns><see cref="DeltaLake.Table.ProtocolInfo"/></returns>
        public ProtocolInfo ProtocolVersions()
        {
            return _table.ProtocolVersions();
        }

        /// <summary>
        /// Restores a table using the options provided
        /// </summary>
        /// <param name="options">Restore options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        public Task RestoreAsync(RestoreOptions options, CancellationToken cancellationToken)
        {
            return _table.RestoreAsync(options, cancellationToken);
        }

        /// <summary>
        /// Returns json serialized statistics about the update issued against the table
        /// </summary>
        /// <param name="query">Sql UPDATE statement against the table</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns>json data</returns>
        public Task<string> UpdateAsync(string query, CancellationToken cancellationToken)
        {
            return _table.UpdateAsync(query, cancellationToken);
        }

        /// <summary>
        /// Accepts a predicate and deletes matching items from the table
        /// </summary>
        /// <param name="predicate">The criteria for deletion... ex color = 'Blue' AND ts > 10000</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns></returns>
        public Task DeleteAsync(string predicate, CancellationToken cancellationToken)
        {
            return _table.DeleteAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Accepts a predicate and deletes matching items from the table
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> </param>
        /// <returns></returns>
        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            return _table.DeleteAsync(string.Empty, cancellationToken);
        }

        /// <summary>
        /// Issue a select query against a delta table
        /// </summary>
        /// <param name="query">A select query</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IAsyncEnumerable{RecordBatch}"/>A collection of record batches representing the query results</returns>
        public async IAsyncEnumerable<RecordBatch> QueryAsync(
            SelectQuery query,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = await _table.QueryAsync(query.Query, query.TableAlias, cancellationToken).ConfigureAwait(false);
            while (true)
            {
                var rb = await result.ReadNextRecordBatchAsync(cancellationToken).ConfigureAwait(false);
                if (rb == null)
                {
                    yield break;
                }

                yield return rb;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _table.Dispose();
        }
    }
}