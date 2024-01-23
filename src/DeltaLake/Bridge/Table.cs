using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DeltaLake.Bridge.Interop;

namespace DeltaLake.Bridge
{
    /// <summary>
    ///
    /// </summary>
    internal class Table : SafeHandle
    {
        private readonly unsafe Interop.RawDeltaTable* _ptr;

        private readonly Runtime _runtime;

        /// <summary>
        /// Creates a table
        /// </summary>
        internal unsafe Table(Runtime runtime, Interop.RawDeltaTable* inner)
            : base(IntPtr.Zero, true)
        {
            _ptr = inner;
            _runtime = runtime;
            SetHandle((IntPtr)_ptr);
        }

        /// <inheritdoc />
        public override bool IsInvalid => false;

        /// <summary>
        /// Returns the current version of the table
        /// </summary>
        /// <returns></returns>
        public async Task LoadVersionAsync(long version)
        {
            var tsc = new TaskCompletionSource<bool>();
            unsafe
            {
                var funcHandle = default(GCHandle);
                object? funcPointer = null;
                (funcHandle, funcPointer) = Runtime.FunctionPointer<Interop.TableEmptyCallback>((fail) =>
                {
                    try
                    {
                        if (fail != null)
                        {
                            tsc.TrySetException(new InvalidOperationException());
                            Interop.Methods.error_free(_runtime.Ptr, fail);
                        }
                        else
                        {
                            tsc.TrySetResult(true);
                        }
                    }
                    finally
                    {
                        if (funcHandle.IsAllocated)
                        {
                            funcHandle.Free();
                        }
                    }
                });
                Interop.Methods.table_load_version(_runtime.Ptr, _ptr, version, funcHandle.AddrOfPinnedObject());
            }

            await tsc.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the current version of the table
        /// </summary>
        /// <returns></returns>
        public long Version()
        {
            unsafe
            {
                return Interop.Methods.table_version(_ptr);
            }
        }

        /// <summary>
        /// Returns the current version of the table
        /// </summary>
        /// <returns></returns>
        public string Uri()
        {
            unsafe
            {
                var uri = Interop.Methods.table_uri(_ptr);
                try
                {
                    if (uri == null)
                    {
                        return string.Empty;
                    }


                    return ByteArrayRef.StrictUTF8.GetString(uri->data, (int)uri->size);
                }
                finally
                {
                    Interop.Methods.byte_array_free(_runtime.Ptr, uri);
                }
            }
        }

        public string[] FileUris()
        {
            unsafe
            {
                return GetStringArray(Interop.Methods.table_file_uris(_runtime.Ptr, _ptr));
            }
        }

        public string[] Files()
        {
            unsafe
            {
                return GetStringArray(Interop.Methods.table_files(_runtime.Ptr, _ptr));
            }
        }

        /// <inheritdoc />
        protected override unsafe bool ReleaseHandle()
        {
            Interop.Methods.table_free(_ptr);
            return true;
        }

        private unsafe string[] GetStringArray(GenericOrError genericOrError)
        {
            try
            {
                if (genericOrError.error != null)
                {
                    try
                    {
                        throw new InvalidOperationException();
                    }
                    finally
                    {
                        Interop.Methods.error_free(_runtime.Ptr, genericOrError.error);
                    }
                }

                if (genericOrError.bytes == null)
                {
                    return Array.Empty<string>();
                }

                var dynamicArray = (DynamicArray*)genericOrError.bytes;
                var uris = new string[(int)dynamicArray->size];
                for (var i = 0; i < uris.Length; i++)
                {
                    var instance = dynamicArray->data + i;
                    uris[i] = ByteArrayRef.StrictUTF8.GetString(instance->data, (int)instance->size);
                }

                return uris;
            }
            finally
            {
                Interop.Methods.dynamic_array_free(_runtime.Ptr, (DynamicArray*)genericOrError.bytes);
            }
        }
    }
}