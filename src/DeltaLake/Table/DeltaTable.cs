using System;
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
        /// <param name="uri"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<DeltaTable> GetAsync(DeltaRuntime runtime, string uri, TableOptions options)
        {
            var table = await runtime.Runtime.NewTableAsync(uri, options).ConfigureAwait(false);
            return new DeltaTable(runtime, table);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Task<DeltaTable> CreateAsync(DeltaRuntime runtime, System.Uri uri, TableOptions options)
        {
            return GetAsync(runtime, uri.ToString(), options);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<DeltaTable> GetAsync(DeltaRuntime runtime, Memory<byte> uri, TableOptions options)
        {
            var table = await runtime.Runtime.NewTableAsync(uri, options).ConfigureAwait(false);
            return new DeltaTable(runtime, table);
        }

        /// <summary>
        /// Creates a new delta table from the specified options
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<DeltaTable> CreateAsync(DeltaRuntime runtime, TableCreateOptions options)
        {
            var table = await runtime.Runtime.CreateTableAsync(options).ConfigureAwait(false);
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

        /// <inheritdoc />
        public void Dispose()
        {
            _table.Dispose();
        }
    }
}