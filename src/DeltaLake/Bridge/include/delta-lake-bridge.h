#pragma once

#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>

typedef enum DeltaTableErrorCode {
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
  InvalidTimestamp = 36,
} DeltaTableErrorCode;

typedef enum PartitionFilterBinaryOp {
  Equal = 0,
  /**
   * The partition value with the not equal operator
   */
  NotEqual = 1,
  /**
   * The partition value with the greater than operator
   */
  GreaterThan = 2,
  /**
   * The partition value with the greater than or equal operator
   */
  GreaterThanOrEqual = 3,
  /**
   * The partition value with the less than operator
   */
  LessThan = 4,
  /**
   * The partition value with the less than or equal operator
   */
  LessThanOrEqual = 5,
} PartitionFilterBinaryOp;

typedef struct CancellationToken CancellationToken;

typedef struct Map Map;

typedef struct PartitionFilterList PartitionFilterList;

typedef struct RawDeltaTable RawDeltaTable;

typedef struct Runtime Runtime;

typedef struct ByteArrayRef {
  const uint8_t *data;
  size_t size;
} ByteArrayRef;

typedef struct ByteArray {
  const uint8_t *data;
  size_t size;
  /**
   * For internal use only.
   */
  size_t cap;
  /**
   * For internal use only.
   */
  bool disable_free;
} ByteArray;

typedef struct DeltaTableError {
  enum DeltaTableErrorCode code;
  struct ByteArray error;
} DeltaTableError;

/**
 * If fail is not null, it must be manually freed when done. Runtime is always
 * present, but it should never be used if fail is present, only freed after
 * fail is freed using it.
 */
typedef struct RuntimeOrFail {
  struct Runtime *runtime;
  const struct ByteArray *fail;
} RuntimeOrFail;

typedef struct RuntimeOptions {

} RuntimeOptions;

typedef struct DynamicArray {
  const struct ByteArray *data;
  size_t size;
  /**
   * For internal use only.
   */
  size_t cap;
  /**
   * For internal use only.
   */
  bool disable_free;
} DynamicArray;

typedef struct TableCreatOptions {
  struct ByteArrayRef table_uri;
  const void *schema;
  const struct ByteArrayRef *partition_by;
  uintptr_t partition_count;
  struct ByteArrayRef mode;
  struct ByteArrayRef name;
  struct ByteArrayRef description;
  struct Map *configuration;
  struct Map *storage_options;
  struct Map *custom_metadata;
} TableCreatOptions;

typedef void (*TableNewCallback)(struct RawDeltaTable *success, const struct DeltaTableError *fail);

typedef struct TableOptions {
  int64_t version;
  struct Map *storage_options;
  bool without_files;
  size_t log_buffer_size;
} TableOptions;

typedef struct GenericOrError {
  const void *bytes;
  const struct DeltaTableError *error;
} GenericOrError;

typedef void (*GenericErrorCallback)(const void *success, const struct DeltaTableError *fail);

typedef void (*TableEmptyCallback)(const struct DeltaTableError *fail);

typedef struct ProtocolResponse {
  int32_t min_reader_version;
  int32_t min_writer_version;
  const struct DeltaTableError *error;
} ProtocolResponse;

typedef struct OptimizeOptions {
  bool has_max_concurrent_tasks;
  uint32_t max_concurrent_tasks;
  bool has_max_spill_size;
  uint64_t max_spill_size;
  bool has_min_commit_interval;
  uint64_t min_commit_interval;
  bool has_preserve_insertion_order;
  bool preserve_insertion_order;
  bool has_target_size;
  uint64_t target_size;
  const struct ByteArrayRef *zorder_columns;
  uintptr_t zorder_columns_count;
  uint32_t optimize_type;
} OptimizeOptions;

typedef struct VacuumOptions {
  bool dry_run;
  uint64_t retention_hours;
  bool enforce_retention_duration;
  uint32_t vacuum_mode;
  struct Map *custom_metadata;
} VacuumOptions;

typedef struct KeyValuePair {
  uint8_t *key;
  uintptr_t key_length;
  uintptr_t key_capacity;
  uint8_t *value;
  uintptr_t value_length;
  uintptr_t value_capacity;
} KeyValuePair;

typedef struct Dictionary {
  struct KeyValuePair **values;
  uintptr_t length;
  uintptr_t capacity;
} Dictionary;

typedef struct TableMetadata {
  /**
   * Unique identifier for this table
   */
  const char *id;
  /**
   * User-provided identifier for this table
   */
  const char *name;
  /**
   * User-provided description for this table
   */
  const char *description;
  /**
   * Specification of the encoding for the files stored in the table
   */
  const char *format_provider;
  struct Dictionary format_options;
  /**
   * Schema of the table
   */
  const char *schema_string;
  /**
   * Column names by which the data should be partitioned
   */
  char **partition_columns;
  uintptr_t partition_columns_count;
  /**
   * The time when this metadata action is created, in milliseconds since the Unix epoch
   */
  int64_t created_time;
  /**
   * Configuration options for the metadata action
   */
  struct Dictionary configuration;
  void (*release)(struct TableMetadata *arg1);
} TableMetadata;

typedef struct MetadataOrError {
  const struct TableMetadata *metadata;
  const struct DeltaTableError *error;
} MetadataOrError;

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

const struct Map *map_new(const struct Runtime *runtime, uintptr_t capacity);

bool map_add(struct Map *map, const struct ByteArrayRef *key, const struct ByteArrayRef *value);

struct CancellationToken *cancellation_token_new(void);

void cancellation_token_cancel(struct CancellationToken *token);

void cancellation_token_free(struct CancellationToken *token);

void error_free(struct Runtime *_runtime, const struct DeltaTableError *error);

struct RuntimeOrFail runtime_new(const struct RuntimeOptions *options);

void runtime_free(struct Runtime *runtime);

void byte_array_free(struct Runtime *runtime, const struct ByteArray *bytes);

void map_free(struct Runtime *_runtime, const struct Map *map);

void dynamic_array_free(struct Runtime *runtime, const struct DynamicArray *array);

struct PartitionFilterList *partition_filter_list_new(uintptr_t capacity);

bool partition_filter_list_add_binary(struct PartitionFilterList *_list,
                                      const struct ByteArrayRef *_key,
                                      enum PartitionFilterBinaryOp _op,
                                      const struct ByteArrayRef *_value);

bool partition_filter_list_add_set(struct PartitionFilterList *_list,
                                   const struct ByteArrayRef *_key,
                                   enum PartitionFilterBinaryOp _op,
                                   const struct ByteArrayRef *_value,
                                   uintptr_t _value_count);

void partition_filter_list_free(struct PartitionFilterList *list);

struct ByteArray *table_uri(struct RawDeltaTable *_Nonnull table);

void table_free(struct RawDeltaTable *_Nonnull table);

void create_deltalake(struct Runtime *_Nonnull runtime,
                      struct TableCreatOptions *_Nonnull options,
                      const struct CancellationToken *cancellation_token,
                      TableNewCallback callback);

void table_new(struct Runtime *_Nonnull runtime,
               struct ByteArrayRef *_Nonnull table_uri,
               struct TableOptions *_Nonnull table_options,
               const struct CancellationToken *cancellation_token,
               TableNewCallback callback);

struct GenericOrError table_file_uris(struct Runtime *_Nonnull runtime,
                                      struct RawDeltaTable *_Nonnull table,
                                      struct PartitionFilterList *filters);

struct GenericOrError table_files(struct Runtime *_Nonnull runtime,
                                  struct RawDeltaTable *_Nonnull table,
                                  struct PartitionFilterList *filters);

void history(struct Runtime *_Nonnull runtime,
             struct RawDeltaTable *_Nonnull table,
             uintptr_t limit,
             const struct CancellationToken *cancellation_token,
             GenericErrorCallback callback);

void table_update_incremental(struct Runtime *_Nonnull runtime,
                              struct RawDeltaTable *_Nonnull table,
                              int64_t max_version,
                              const struct CancellationToken *cancellation_token,
                              TableEmptyCallback callback);

void table_load_version(struct Runtime *_Nonnull runtime,
                        struct RawDeltaTable *_Nonnull table,
                        int64_t version,
                        const struct CancellationToken *cancellation_token,
                        TableEmptyCallback callback);

void table_load_with_datetime(struct Runtime *_Nonnull runtime,
                              struct RawDeltaTable *_Nonnull table,
                              int64_t ts_milliseconds,
                              const struct CancellationToken *cancellation_token,
                              TableEmptyCallback callback);

void table_merge(struct Runtime *_Nonnull runtime,
                 struct RawDeltaTable *_Nonnull delta_table,
                 struct ByteArrayRef *_Nonnull query,
                 void *_Nonnull stream,
                 const struct CancellationToken *cancellation_token,
                 GenericErrorCallback callback);

struct ProtocolResponse table_protocol_versions(struct Runtime *_Nonnull runtime,
                                                struct RawDeltaTable *_Nonnull table);

void table_restore(struct Runtime *_Nonnull runtime,
                   struct RawDeltaTable *_Nonnull table,
                   int64_t version_or_timestamp,
                   bool is_timestamp,
                   bool ignore_missing_files,
                   bool protocol_downgrade_allowed,
                   struct Map *custom_metadata,
                   const struct CancellationToken *cancellation_token,
                   TableEmptyCallback callback);

void table_update(struct Runtime *_Nonnull runtime,
                  struct RawDeltaTable *_Nonnull table,
                  struct ByteArrayRef *_Nonnull query,
                  const struct CancellationToken *cancellation_token,
                  GenericErrorCallback callback);

void table_delete(struct Runtime *_Nonnull runtime,
                  struct RawDeltaTable *_Nonnull table,
                  const struct ByteArrayRef *predicate,
                  const struct CancellationToken *cancellation_token,
                  GenericErrorCallback callback);

void table_query(struct Runtime *_Nonnull runtime,
                 struct RawDeltaTable *_Nonnull table,
                 struct ByteArrayRef *_Nonnull query,
                 struct ByteArrayRef *_Nonnull table_name,
                 const struct CancellationToken *cancellation_token,
                 GenericErrorCallback callback);

void table_insert(struct Runtime *_Nonnull runtime,
                  struct RawDeltaTable *_Nonnull table,
                  void *_Nonnull stream,
                  const struct ByteArrayRef *predicate,
                  const struct ByteArrayRef *_Nonnull mode,
                  uintptr_t max_rows_per_group,
                  bool overwrite_schema,
                  const struct CancellationToken *cancellation_token,
                  GenericErrorCallback callback);

/**
 * Must free the error
 */
struct GenericOrError table_schema(struct Runtime *_Nonnull runtime,
                                   struct RawDeltaTable *_Nonnull table);

void table_checkpoint(struct Runtime *_Nonnull runtime,
                      struct RawDeltaTable *_Nonnull table,
                      const struct CancellationToken *cancellation_token,
                      TableEmptyCallback callback);

void table_optimize(struct Runtime *_Nonnull runtime,
                    struct RawDeltaTable *_Nonnull table,
                    struct OptimizeOptions *_Nonnull options,
                    GenericErrorCallback callback);

void table_vacuum(struct Runtime *_Nonnull runtime,
                  struct RawDeltaTable *_Nonnull table,
                  struct VacuumOptions *_Nonnull options,
                  GenericErrorCallback callback);

int64_t table_version(struct RawDeltaTable *_Nonnull table_handle);

struct MetadataOrError table_metadata(struct Runtime *_Nonnull runtime,
                                      struct RawDeltaTable *_Nonnull table_handle);

void table_add_constraints(struct Runtime *_Nonnull runtime,
                           struct RawDeltaTable *_Nonnull table,
                           struct Map *constraints,
                           struct Map *custom_metadata,
                           const struct CancellationToken *cancellation_token,
                           TableEmptyCallback callback);

#ifdef __cplusplus
}  // extern "C"
#endif  // __cplusplus
