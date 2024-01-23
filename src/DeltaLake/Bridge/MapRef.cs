using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeltaLake.Bridge
{
    internal sealed class Map : IDisposable
    {
        private readonly Runtime _runtime;

        private unsafe Map(Interop.Map* inner, Runtime runtime)
        {
            Ref = inner;
            _runtime = runtime;
        }

        ~Map()
        {
            Dispose(false);
        }

        public unsafe Interop.Map* Ref { get; }

        public static unsafe Map FromDictionary(Runtime runtime, IDictionary<string, string> source)
        {
            var map = Interop.Methods.map_new(runtime.Ptr, (nuint)source.Count);
            foreach (var (key, value) in source)
            {
                using var keyRef = ByteArrayRef.RentUtf8(key);
                using var valueRef = ByteArrayRef.RentUtf8(value);
                WriteTuple(map, keyRef.Ref, valueRef.Ref);
            }

            return new Map(map, runtime);
        }

        private static unsafe void WriteTuple(Interop.Map* map, ByteArrayRef keyRef, ByteArrayRef valueRef)
        {

            var keyRefHandle = GCHandle.Alloc(keyRef.Ref, GCHandleType.Pinned);
            try
            {
                var valueRefHandle = GCHandle.Alloc(valueRef.Ref, GCHandleType.Pinned);
                try
                {
                    Interop.Methods.map_add(
                        map,
                        (Interop.ByteArrayRef*)keyRefHandle.AddrOfPinnedObject(),
                        (Interop.ByteArrayRef*)valueRefHandle.AddrOfPinnedObject());
                }
                finally
                {
                    valueRefHandle.Free();
                }
            }
            finally
            {
                keyRefHandle.Free();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (Ref != null)
            {
                Interop.Methods.map_free(_runtime.Ptr, Ref);
            }
        }
    }
}