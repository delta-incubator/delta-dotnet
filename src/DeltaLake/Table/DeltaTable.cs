using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
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
        /// <returns><see cref="long"/></returns>
        public long Version()
        {
            return _table.Version();
        }

        /// <summary>
        /// Loads table at specific version
        /// </summary>
        /// <param name="version">desired version</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns><see cref="Task"/></returns>
        public Task LoadVersionAsync(long version, CancellationToken cancellationToken)
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
            await _table.InsertAsync(records, schema, options, cancellationToken).ConfigureAwait(false);
        }

        public async Task HistoryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateIncrementalAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads table at a specific point in time
        /// </summary>
        /// <param name="timestamp">desired point in time</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns><see cref="Task"/></returns>
        public Task LoadDateTimeAsync(DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task MergeAsync(string query, IReadOnlyCollection<RecordBatch> records, Schema schema, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ProtocolInfo ProtocolVersions()
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(RestoreOptions options, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(string query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string predicate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<RecordBatch> QueryAsync(SelectQuery query, CancellationToken cancellationToken)
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

    public class SelectQuery
    {

        public SelectQuery(string query)
        {
            Query = query;
        }

        public string Query { get; }

        public string? TableAlias { get; init; }
    }
}