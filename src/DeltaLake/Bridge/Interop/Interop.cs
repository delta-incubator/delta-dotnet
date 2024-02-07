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
        OperationCanceled = 33,
        DataFusion = 34,
        SqlParser = 35,
    }

    [NativeTypeName("unsigned int")]
    internal enum PartitionFilterBinaryOp : uint
    {
        Equal = 0,
        NotEqual = 1,
        GreaterThan = 2,
        GreaterThanOrEqual = 3,
        LessThan = 4,
        LessThanOrEqual = 5,
    }

    internal partial struct CancellationToken
    {
    }

    internal partial struct Map
    {
    }

    internal partial struct PartitionFilterList
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

    internal unsafe partial struct TableCreatOptions
    {
        [NativeTypeName("struct ByteArrayRef")]
        public ByteArrayRef table_uri;

        [NativeTypeName("const void *")]
        public void* schema;

        [NativeTypeName("const struct ByteArrayRef *")]
        public ByteArrayRef* partition_by;

        [NativeTypeName("uintptr_t")]
        public UIntPtr partition_count;

        [NativeTypeName("struct ByteArrayRef")]
        public ByteArrayRef mode;

        [NativeTypeName("struct ByteArrayRef")]
        public ByteArrayRef name;

        [NativeTypeName("struct ByteArrayRef")]
        public ByteArrayRef description;

        [NativeTypeName("struct Map *")]
        public Map* configuration;

        [NativeTypeName("struct Map *")]
        public Map* storage_options;

        [NativeTypeName("struct Map *")]
        public Map* custom_metadata;
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

    internal unsafe partial struct ProtocolResponse
    {
        [NativeTypeName("int32_t")]
        public int min_reader_version;

        [NativeTypeName("int32_t")]
        public int min_writer_version;

        [NativeTypeName("const struct DeltaTableError *")]
        public DeltaTableError* error;
    }

    internal unsafe partial struct VacuumOptions
    {
        [NativeTypeName("bool")]
        public byte dry_run;

        [NativeTypeName("uint64_t")]
        public ulong retention_hours;

        [NativeTypeName("bool")]
        public byte enforce_retention_duration;

        [NativeTypeName("struct Map *")]
        public Map* custom_metadata;
    }

    internal unsafe partial struct KeyValuePair
    {
        [NativeTypeName("uint8_t *")]
        public byte* key;

        [NativeTypeName("uintptr_t")]
        public UIntPtr key_length;

        [NativeTypeName("uintptr_t")]
        public UIntPtr key_capacity;

        [NativeTypeName("uint8_t *")]
        public byte* value;

        [NativeTypeName("uintptr_t")]
        public UIntPtr value_length;

        [NativeTypeName("uintptr_t")]
        public UIntPtr value_capacity;
    }

    internal unsafe partial struct Dictionary
    {
        [NativeTypeName("struct KeyValuePair **")]
        public KeyValuePair** values;

        [NativeTypeName("uintptr_t")]
        public UIntPtr length;

        [NativeTypeName("uintptr_t")]
        public UIntPtr capacity;
    }

    internal unsafe partial struct TableMetadata
    {
        [NativeTypeName("const char *")]
        public sbyte* id;

        [NativeTypeName("const char *")]
        public sbyte* name;

        [NativeTypeName("const char *")]
        public sbyte* description;

        [NativeTypeName("const char *")]
        public sbyte* format_provider;

        [NativeTypeName("struct Dictionary")]
        public Dictionary format_options;

        [NativeTypeName("const char *")]
        public sbyte* schema_string;

        [NativeTypeName("char **")]
        public sbyte** partition_columns;

        [NativeTypeName("uintptr_t")]
        public UIntPtr partition_columns_count;

        [NativeTypeName("int64_t")]
        public long created_time;

        [NativeTypeName("struct Dictionary")]
        public Dictionary configuration;

        [NativeTypeName("void (*)(struct TableMetadata *)")]
        public IntPtr release;
    }

    internal unsafe partial struct MetadataOrError
    {
        [NativeTypeName("const struct TableMetadata *")]
        public TableMetadata* metadata;

        [NativeTypeName("const struct DeltaTableError *")]
        public DeltaTableError* error;
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
        [return: NativeTypeName("struct PartitionFilterList *")]
        public static extern PartitionFilterList* partition_filter_list_new([NativeTypeName("uintptr_t")] UIntPtr capacity);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte partition_filter_list_add_binary([NativeTypeName("struct PartitionFilterList *")] PartitionFilterList* _list, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* _key, [NativeTypeName("enum PartitionFilterBinaryOp")] PartitionFilterBinaryOp _op, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* _value);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte partition_filter_list_add_set([NativeTypeName("struct PartitionFilterList *")] PartitionFilterList* _list, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* _key, [NativeTypeName("enum PartitionFilterBinaryOp")] PartitionFilterBinaryOp _op, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* _value, [NativeTypeName("uintptr_t")] UIntPtr _value_count);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void partition_filter_list_free([NativeTypeName("struct PartitionFilterList *")] PartitionFilterList* list);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ByteArray *")]
        public static extern ByteArray* table_uri([NativeTypeName("const struct RawDeltaTable *")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_free([NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void create_deltalake([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct TableCreatOptions * _Nonnull")] TableCreatOptions* options, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableNewCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_new([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct ByteArrayRef * _Nonnull")] ByteArrayRef* table_uri, [NativeTypeName("struct TableOptions * _Nonnull")] TableOptions* table_options, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableNewCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct GenericOrError")]
        public static extern GenericOrError table_file_uris([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("struct PartitionFilterList *")] PartitionFilterList* filters);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct GenericOrError")]
        public static extern GenericOrError table_files([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("struct PartitionFilterList *")] PartitionFilterList* filters);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void history([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("uintptr_t")] UIntPtr limit, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_update_incremental([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_load_version([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte table_load_with_datetime([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("int64_t")] long ts_milliseconds, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_merge([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* delta_table, [NativeTypeName("struct ByteArrayRef * _Nonnull")] ByteArrayRef* query, void* stream, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ProtocolResponse")]
        public static extern ProtocolResponse table_protocol_versions([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_restore([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("int64_t")] long version_or_timestamp, [NativeTypeName("bool")] byte is_timestamp, [NativeTypeName("bool")] byte ignore_missing_files, [NativeTypeName("bool")] byte protocol_downgrade_allowed, [NativeTypeName("struct Map *")] Map* custom_metadata, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_update([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* query, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_delete([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* predicate, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_query([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("struct ByteArrayRef * _Nonnull")] ByteArrayRef* query, [NativeTypeName("struct ByteArrayRef * _Nonnull")] ByteArrayRef* table_name, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_insert([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("void * _Nonnull")] void* stream, [NativeTypeName("const struct ByteArrayRef *")] ByteArrayRef* predicate, [NativeTypeName("const struct ByteArrayRef * _Nonnull")] ByteArrayRef* mode, [NativeTypeName("uintptr_t")] UIntPtr max_rows_per_group, [NativeTypeName("bool")] byte overwrite_schema, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct GenericOrError")]
        public static extern GenericOrError table_schema([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_checkpoint([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_vacuum([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("const struct VacuumOptions *")] VacuumOptions* options, [NativeTypeName("GenericErrorCallback")] IntPtr callback);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("int64_t")]
        public static extern long table_version([NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table_handle);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct MetadataOrError")]
        public static extern MetadataOrError table_metadata([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table_handle);

        [DllImport("delta_rs_bridge", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void table_add_constraints([NativeTypeName("struct Runtime * _Nonnull")] Runtime* runtime, [NativeTypeName("struct RawDeltaTable * _Nonnull")] RawDeltaTable* table, [NativeTypeName("struct Map *")] Map* constraints, [NativeTypeName("struct Map *")] Map* custom_metadata, [NativeTypeName("const struct CancellationToken *")] CancellationToken* cancellation_token, [NativeTypeName("TableEmptyCallback")] IntPtr callback);
    }
}
