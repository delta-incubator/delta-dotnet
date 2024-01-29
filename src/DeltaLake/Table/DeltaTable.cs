using System;
using System.Threading;
using System.Threading.Tasks;
using DeltaLake.Runtime;

namespace DeltaLake.Table
{
    /// <summary>
    ///
    /// </summary>
    public sealed class DeltaTable : IDisposable
    {
        private readonly DeltaRuntime _runtime;
        private readonly Bridge.Table _table;

        private DeltaTable(DeltaRuntime runtime, Bridge.Table table)
        {
            _runtime = runtime;
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
            return new DeltaTable(runtime, table);
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
            return new DeltaTable(runtime, table);
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
            return new DeltaTable(runtime, table);
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
        /// <returns>current table version</returns>
        public long Version()
        {
            return _table.Version();
        }

        /// <summary>
        /// Loads table at specific version
        /// </summary>
        /// <param name="version">desired version</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken">cancellation token</see>  </param>
        /// <returns></returns>
        public Task LoadVersionAsync(long version, CancellationToken cancellationToken)
        {
            return _table.LoadVersionAsync(version, cancellationToken);
        }

        /// <summary>
        /// Returns the table schema
        /// </summary>
        /// <returns></returns>
        public Apache.Arrow.Schema Schema()
        {
            return _table.Schema();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _table.Dispose();
        }
    }
}