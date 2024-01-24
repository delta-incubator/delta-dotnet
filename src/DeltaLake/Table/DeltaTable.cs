using System;
using System.Threading.Tasks;
using DeltaLake.Runtime;

namespace DeltaLake.Table
{
    /// <summary>
    ///
    /// </summary>
    public sealed class DeltaTable
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
        public static async Task<DeltaTable> CreateAsync(DeltaRuntime runtime, string uri, TableOptions options)
        {
            var table = await runtime.Runtime.NewTableAsync(uri, options);
            return new DeltaTable(runtime, table);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="uri"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<DeltaTable> CreateAsync(DeltaRuntime runtime, Memory<byte> uri, TableOptions options)
        {
            var table = await runtime.Runtime.NewTableAsync(uri, options);
            return new DeltaTable(runtime, table);
        }
    }
}