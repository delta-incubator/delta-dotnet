using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DeltaLake.Bridge
{
    /// <summary>
    /// Core-owned runtime.
    /// </summary>
    internal class Runtime : SafeHandle
    {
        /*
        private static readonly Func<ForwardedLog, Exception?, string> ForwardLogMessageFormatter =
            LogMessageFormatter;

        private readonly bool forwardLoggerIncludeFields;*/
        private readonly GCHandle? forwardLoggerCallback;

        private ILogger? forwardLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Runtime"/> class.
        /// </summary>
        /// <param name="options">Runtime options.</param>
        /// <exception cref="InvalidOperationException">Any internal core error.</exception>
        public Runtime(DeltaLake.Runtime.RuntimeOptions options)
            : base(IntPtr.Zero, true)
        {
            unsafe
            {
                var res = Interop.Methods.runtime_new(null);
                // If it failed, copy byte array, free runtime and byte array. Otherwise just
                // return runtime.
                if (res.fail != null)
                {
                    var message = ByteArrayRef.StrictUTF8.GetString(
                        res.fail->data,
                        (int)res.fail->size);
                    Interop.Methods.byte_array_free(res.runtime, res.fail);
                    Interop.Methods.runtime_free(res.runtime);
                    throw new InvalidOperationException(message);
                }
                Ptr = res.runtime;
                SetHandle((IntPtr)Ptr);
            }
            // MetricMeter = new(() => new(this));
        }

        /// <inheritdoc />
        public override unsafe bool IsInvalid => false;

        public async Task<Table> NewTableAsync(string tableUri, DeltaLake.Table.TableOptions options)
        {
            var tsc = new TaskCompletionSource<Table>();
            var buffer = ArrayPool<byte>.Shared.Rent(System.Text.Encoding.UTF8.GetByteCount(tableUri));
            var encodedLength = System.Text.Encoding.UTF8.GetBytes(tableUri, buffer);
            try
            {
                unsafe
                {
                    var byteArrayRef = new ByteArrayRef(buffer.AsMemory(0, encodedLength));
                    var handle = GCHandle.Alloc(byteArrayRef.Ref, GCHandleType.Pinned);
                    var funcHandle = default(GCHandle);
                    object? funcPointer = null;
                    var (nativeOptions, map) = MakeNativeTableOptions(options);
                    var optionsHandle = GCHandle.Alloc(nativeOptions);
                    (funcHandle, funcPointer) = FunctionPointer<Interop.TableNewCallback>((success, fail) =>
                    {
                        try
                        {
                            if (fail != null)
                            {
                                tsc.TrySetException(new InvalidOperationException());
                                Interop.Methods.error_free(Ptr, fail);
                            }
                            else
                            {
                                tsc.TrySetResult(new Table(this, success));
                            }
                        }
                        finally
                        {
                            optionsHandle.Free();
                            handle.Free();
                            map?.Dispose();
                            if (funcHandle.IsAllocated)
                            {
                                funcHandle.Free();
                            }
                        }
                    });

                    Interop.Methods.table_new(
                        Ptr,
                        (Interop.ByteArrayRef*)handle.AddrOfPinnedObject(),
                        (Interop.TableOptions*)optionsHandle.AddrOfPinnedObject(),
                        funcHandle.AddrOfPinnedObject());
                }

                return await tsc.Task.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Gets the pointer to the runtime.
        /// </summary>
        internal unsafe Interop.Runtime* Ptr { get; private init; }

        /*
                /// <summary>
                /// Gets the lazy metric meter for this runtime.
                /// </summary>
                // internal Lazy<MetricMeter> MetricMeter { get; private init; }*/

        /// <summary>
        /// Read a JSON object into string keys and raw JSON values.
        /// </summary>
        /// <param name="bytes">Byte span.</param>
        /// <returns>Keys and raw values or null.</returns>
        internal static unsafe IReadOnlyDictionary<string, string>? ReadJsonObjectToRawValues(
            ReadOnlySpan<byte> bytes)
        {
            var reader = new Utf8JsonReader(bytes);
            // Expect start object
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                return null;
            }
            // Property names one at a time
            var ret = new Dictionary<string, string>();
            fixed (byte* ptr = bytes)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        return null;
                    }
                    var propertyName = reader.GetString()!;
                    // Read and skip and capture
                    if (!reader.Read())
                    {
                        return null;
                    }
                    var beginIndex = (int)reader.TokenStartIndex;
                    reader.Skip();
                    ret[propertyName] = ByteArrayRef.StrictUTF8.GetString(
                        ptr + beginIndex, (int)reader.BytesConsumed - beginIndex);
                }
            }
            return ret;
        }

        /// <summary>
        /// Free a byte array.
        /// </summary>
        /// <param name="byteArray">Byte array to free.</param>
        internal unsafe void FreeByteArray(Interop.ByteArray* byteArray)
        {
            Interop.Methods.byte_array_free(Ptr, byteArray);
        }

        /// <inheritdoc />
        protected override unsafe bool ReleaseHandle()
        {
            forwardLogger = null;
            forwardLoggerCallback?.Free();
            Interop.Methods.runtime_free(Ptr);
            return true;
        }

        internal static (GCHandle, object) FunctionPointer<T>(T func)
        where T : Delegate
        {
            var handle = GCHandle.Alloc(func);
            return (handle, Marshal.GetFunctionPointerForDelegate(handle.Target!));
        }

        private static string LogMessageFormatter(ForwardedLog state, Exception? error) =>
            state.ToString();


        private (Interop.TableOptions, Map?) MakeNativeTableOptions(DeltaLake.Table.TableOptions? options)
        {
            if (options == null)
            {
                return (new Interop.TableOptions()
                {
                    version = -1,
                    storage_options = null,
                    without_files = 0,
                    log_buffer_size = UIntPtr.Zero,
                }, null);
            }

            unsafe
            {
                var map = Map.FromDictionary(this, options.StorageOptions);
                return (new Interop.TableOptions()
                {
                    version = options.Version ?? -1,
                    storage_options = map.Ref,
                    without_files = (byte)(options.WithoutFiles ? 1 : 0),
                    log_buffer_size = options.LogBufferSize ?? (nuint)0,
                }, map);
            }
        }

        /*        private unsafe void OnLog(Interop.ForwardedLogLevel coreLevel, Interop.ForwardedLog* coreLog)
                {
                    if (forwardLogger is not { } logger)
                    {
                        return;
                    }
                    // Fortunately the Core log levels integers match .NET ones
                    var level = (LogLevel)coreLevel;
                    // Go no further if not enabled
                    if (!logger.IsEnabled(level))
                    {
                        return;
                    }
                    // If the fields are requested, we will try to convert from JSON
                    IReadOnlyDictionary<string, string>? jsonFields = null;
                    if (forwardLoggerIncludeFields)
                    {
                        try
                        {
                            var fieldBytes = Interop.Methods.forwarded_log_fields_json(coreLog);
                            jsonFields = ReadJsonObjectToRawValues(new(fieldBytes.data, (int)fieldBytes.size));
                        }
        #pragma warning disable CA1031 // We are ok swallowing all exceptions
                        catch
                        {
                        }
        #pragma warning restore CA1031
                    }
                    var log = new ForwardedLog(
                        Level: level,
                        Target: ByteArrayRef.ToUtf8(Interop.Methods.forwarded_log_target(coreLog)),
                        Message: ByteArrayRef.ToUtf8(Interop.Methods.forwarded_log_message(coreLog)),
                        TimestampMilliseconds: Interop.Methods.forwarded_log_timestamp_millis(coreLog),
                        JsonFields: jsonFields);
                    logger.Log(level, 0, log, null, ForwardLogMessageFormatter);
                }*/
    }
}