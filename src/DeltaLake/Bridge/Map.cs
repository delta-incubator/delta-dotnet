using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeltaLake.Bridge
{
    internal sealed class Map
    {
        private readonly Runtime _runtime;

        private unsafe Map(Interop.Map* inner, Runtime runtime)
        {
            Ref = inner;
            _runtime = runtime;
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

        public static unsafe Map FromOptionalDictionary(Runtime runtime, IDictionary<string, string?> source)
        {
            var map = Interop.Methods.map_new(runtime.Ptr, (nuint)source.Count);
            foreach (var (key, value) in source)
            {
                using var keyRef = ByteArrayRef.RentUtf8(key);
                if (value != null)
                {
                    using var valueRef = ByteArrayRef.RentUtf8(value);
                    WriteTuple(map, keyRef.Ref, valueRef.Ref);
                }
                else
                {
                    WriteSingle(map, keyRef.Ref);
                }

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

        private static unsafe void WriteSingle(Interop.Map* map, ByteArrayRef keyRef)
        {

            var keyRefHandle = GCHandle.Alloc(keyRef.Ref, GCHandleType.Pinned);
            try
            {
                Interop.Methods.map_add(
                    map,
                    (Interop.ByteArrayRef*)keyRefHandle.AddrOfPinnedObject(),
                    null);

            }
            finally
            {
                keyRefHandle.Free();
            }
        }
    }
}