using System;

namespace DeltaLake.Runtime
{

    /// <summary>
    /// Represents the runtime
    /// </summary>
    public sealed class DeltaRuntime : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="options"></param>
        public DeltaRuntime(RuntimeOptions options)
        {
            Runtime = new Bridge.Runtime(options);
        }

        /// <summary>
        /// Gets the runtime.
        /// </summary>
        internal Bridge.Runtime Runtime { get; private init; }

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
    }
}