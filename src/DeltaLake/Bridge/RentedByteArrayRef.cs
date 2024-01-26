using System;
using System.Buffers;

namespace DeltaLake.Bridge
{
    internal readonly struct RentedByteArrayRef : IDisposable
    {
        private readonly byte[] _array;
        private readonly ArrayPool<byte>? _pool;

        public RentedByteArrayRef(ByteArrayRef byteArrayRef, byte[] array, ArrayPool<byte>? pool)
        {
            Ref = byteArrayRef;
            _array = array;
            _pool = pool;
        }

        public static RentedByteArrayRef Empty { get; } = new(ByteArrayRef.Empty, Array.Empty<byte>(), null);

        public ByteArrayRef Ref { get; init; }


        public readonly void Dispose()
        {
            Dispose(true);
        }

        private readonly void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pool?.Return(_array);
            }
        }
    }
}