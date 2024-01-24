using System;
using System.Runtime.InteropServices;

namespace DeltaLake.Bridge.Interop
{
    [NativeTypeName("unsigned int")]
    internal enum DeltaTableErrorCode : uint
    {
        Utf8 = 0,
        Protocol = 1,
        ObjectStore = 2,
        Parquet = 3,
        Arrow = 4,
        InvalidJsonLog = 5,
        InvalidStatsJson = 6,
        InvalidInvariantJson = 7,
        InvalidVersion = 8,
        MissingDataFile = 9,
        InvalidDateTimeString = 10,
        InvalidData = 11,
        NotATable = 12,
        NoMetadata = 13,
        NoSchema = 14,
        LoadPartitions = 15,
        SchemaMismatch = 16,
        PartitionError = 17,
        InvalidPartitionFilter = 18,
        ColumnsNotPartitioned = 19,
        Io = 20,
        Transaction = 21,
        VersionAlreadyExists = 22,
        VersionMismatch = 23,
        MissingFeature = 24,
        InvalidTableLocation = 25,
        SerializeLogJson = 26,
        SerializeSchemaJson = 27,
        Generic = 28,
        GenericError = 29,
        Kernel = 30,
        MetaDataError = 31,
        NotInitialized = 32,
    }

    internal partial struct CancellationToken
    {
    }

    internal partial struct Map
    {
    }

    internal partial struct RawDeltaTable
    {
    }

    internal partial struct Runtime
    {
    }

    internal unsafe partial struct ByteArrayRef
    {
        [NativeTypeName("const uint8_t *")]
        public byte* data;

        [NativeTypeName("size_t")]
        public UIntPtr size;
    }

    internal unsafe partial struct ByteArray
    {
        [NativeTypeName("const uint8_t *")]
        public byte* data;

        [NativeTypeName("size_t")]
        public UIntPtr size;

        [NativeTypeName("size_t")]
        public UIntPtr cap;

        [NativeTypeName("bool")]
        public byte disable_free;
    }

    internal partial struct DeltaTableError
    {
        [NativeTypeName("enum DeltaTableErrorCode")]
        public DeltaTableErrorCode code;

        [NativeTypeName("struct ByteArray")]
        public ByteArray error;
    }

    internal unsafe partial struct RuntimeOrFail
    {
        [NativeTypeName("struct Runtime *")]
        public Runtime* runtime;

        [NativeTypeName("const struct ByteArray *")]
        public ByteArray* fail;
    }

    internal partial struct RuntimeOptions
    {
    }

    internal unsafe partial struct DynamicArray
    {
        [NativeTypeName("const struct ByteArray *")]
        public ByteArray* data;

        [NativeTypeName("size_t")]
        public UIntPtr size;

        [NativeTypeName("size_t")]
        public UIntPtr cap;

        [NativeTypeName("bool")]
        public byte disable_free;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void TableNewCallback([NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* success, [NativeTypeName("const struct DeltaTableError *")] DeltaTableError* fail);

    internal unsafe partial struct TableOptions
    {
        [NativeTypeName("int64_t")]
        public long version;

        [NativeTypeName("struct Map *")]
        public Map* storage_options;

        [NativeTypeName("bool")]
        public byte without_files;

        [NativeTypeName("size_t")]
        public UIntPtr log_buffer_size;
    }

    internal unsafe partial struct GenericOrError
    {
        [NativeTypeName("const void *")]
        public void* bytes;

        [NativeTypeName("const struct DeltaTableError *")]
        public DeltaTableError* error;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void GenericErrorCallback([NativeTypeName("const void *")] void* success, [NativeTypeName("const struct DeltaTableError *")] DeltaTableError* fail);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void TableEmptyCallback([NativeTypeName("const struct DeltaTableError *")] DeltaTableError* fail);

    internal unsafe partial struct VacuumOptions
    {
        [NativeTypeName("bool")]
        public byte dry_run;

        [NativeTypeName("uint64_t")]
        public ulong retention_hours;

        [NativeTypeName("bool")]
        public byte enforce_retention_duration;

        [NativeTypeName("const struct Map *")]
        public Map* custom_metadata;
    }

    internal static unsafe partial class Methods
    {
        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("const struct Map *")]
        public static extern Map* map_new([NativeTypeName("const struct Runtime *")] Runtime* runtime, [NativeTypeName("uintptr_t")] UIntPtr capacity);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte map_add([NativeTypeName("struct Map *")] Map* map, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* key, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* value);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct CancellationToken *")]
        public static extern CancellationToken* cancellation_token_new();

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void cancellation_token_cancel([NativeTypeName("struct CancellationToken *")] CancellationToken* token);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void cancellation_token_free([NativeTypeName("struct CancellationToken *")] CancellationToken* token);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void error_free([NativeTypeName("struct Runtime *")] Runtime* _runtime, [NativeTypeName("const struct DeltaTableError *")] DeltaTableError* error);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct RuntimeOrFail")]
        public static extern RuntimeOrFail runtime_new([NativeTypeName("const struct RuntimeOptions *")] RuntimeOptions* options);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void runtime_free([NativeTypeName("struct Runtime *")] Runtime* runtime);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void byte_array_free([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("const struct ByteArray *")] ByteArray* bytes);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void map_free([NativeTypeName("struct Runtime *")] Runtime* _runtime, [NativeTypeName("const struct Map *")] Map* map);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void dynamic_array_free([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("const struct DynamicArray *")] DynamicArray* array);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ByteArray *")]
        public static extern ByteArray* table_uri([NativeTypeName("const struct RawDeltaTable *")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_free([NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void create_deltalake([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* table_uri, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* schema, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* partition_by, [NativeTypeName("int32_t")] int mode, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* name, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* description, [NativeTypeName("struct Map *")] Map* configuration, [NativeTypeName("struct Map *")] Map* storage_options, [NativeTypeName("struct Map *")] Map* custom_metadata, [NativeTypeName("TableNewCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_new([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* table_uri, [NativeTypeName("const struct TableOptions *")] TableOptions* table_options, [NativeTypeName("TableNewCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct GenericOrError")]
        public static extern GenericOrError table_file_uris([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct GenericOrError")]
        public static extern GenericOrError table_files([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void history([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("uintptr_t")] UIntPtr limit, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_update_incremental([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_load_version([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_load_with_datetime([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long ts_milliseconds, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_merge([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_protocol([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_restore([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_update([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_schema([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_checkpoint([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_vacuum([NativeTypeName("struct Runtime *")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table, [NativeTypeName("const struct VacuumOptions *")] VacuumOptions* options, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("int64_t")]
        public static extern long table_version([NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table_handle);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_metadata([NativeTypeName("struct RawDeltaTable *")] RawDeltaTable* table_handle, [NativeTypeName("TableEmptyCallback")] IntPtr callback);
    }
}
