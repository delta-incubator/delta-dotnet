using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Apache.Arrow.C;
using DeltaLake.Bridge.Interop;
using DeltaLake.Table;
using ICancellationToken = System.Threading.CancellationToken;

namespace DeltaLake.Bridge
{
    /// <summary>
    /// Reference to unmanaged delta table
    /// </summary>
    internal sealed class Table : SafeHandle
    {
        internal static readonly ByteArrayRef SaveModeAppend = ByteArrayRef.FromUTF8("append");

        internal static readonly ByteArrayRef SaveModeOverwrite = ByteArrayRef.FromUTF8("overwrite");

        internal static readonly ByteArrayRef SaveModeError = ByteArrayRef.FromUTF8("error");

        internal static readonly ByteArrayRef SaveModeIfgnore = ByteArrayRef.FromUTF8("ignore");

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
        public async Task LoadVersionAsync(long version, ICancellationToken cancellationToken)
        {
            var tsc = new TaskCompletionSource<bool>();
            using (var scope = new Scope())
            {
                unsafe
                {
                    Interop.Methods.table_load_version(
                        _runtime.Ptr,
                         _ptr,
                          version,
                          scope.CancellationToken(cancellationToken),
                          scope.FunctionPointer<Interop.TableEmptyCallback>((fail) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tsc.TrySetCanceled(cancellationToken);
                            return;
                        }

                        if (fail != null)
                        {
                            tsc.TrySetException(DeltaLakeException.FromDeltaTableError(_runtime.Ptr, fail));
                        }
                        else
                        {
                            tsc.TrySetResult(true);
                        }
                    }));
                }


                await tsc.Task.ConfigureAwait(false);
            }
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
                return GetStringArray(Interop.Methods.table_file_uris(_runtime.Ptr, _ptr, null));
            }
        }

        public string[] Files()
        {
            unsafe
            {
                return GetStringArray(Interop.Methods.table_files(_runtime.Ptr, _ptr, null));
            }
        }

        public Apache.Arrow.Schema Schema()
        {
            unsafe
            {
                var result = Methods.table_schema(_runtime.Ptr, _ptr);
                if (result.error != null)
                {
                    throw DeltaLakeException.FromDeltaTableError(_runtime.Ptr, result.error);
                }

                var schemaPointer = (CArrowSchema*)result.bytes;
                try
                {
                    return CArrowSchemaImporter.ImportSchema(schemaPointer);
                }
                finally
                {
                    CArrowSchema.Free(schemaPointer);
                }
            }
        }

        internal static ByteArrayRef ConvertSaveMode(SaveMode saveMode)
        {
            return saveMode switch
            {
                SaveMode.Append => SaveModeAppend,
                SaveMode.Overwrite => SaveModeOverwrite,
                SaveMode.ErrorIfExists => SaveModeError,
                SaveMode.Ignore => SaveModeIfgnore,
                _ => throw new ArgumentOutOfRangeException(nameof(saveMode)),
            };
        }

        /// <inheritdoc />
        protected override unsafe bool ReleaseHandle()
        {
            Interop.Methods.table_free(_ptr);
            return true;
        }

        private unsafe string[] GetStringArray(GenericOrError genericOrError)
        {
            if (genericOrError.error != null)
            {
                throw DeltaLakeException.FromDeltaTableError(_runtime.Ptr, genericOrError.error);
            }

            try
            {
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