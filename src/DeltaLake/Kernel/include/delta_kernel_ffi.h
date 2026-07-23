#pragma once

#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>

#ifdef __cplusplus
namespace ffi {
#endif  // __cplusplus

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Max `(name, value)` pairs in a [`CAuthHeaders`] struct.
 */
#define AUTH_MAX_NUM_HEADERS 8
#endif

typedef enum KernelError {
  UnknownError = 0,
  FFIError = 1,
#if defined(DEFINE_DEFAULT_ENGINE_BASE)
  ArrowError = 2,
#endif
  EngineDataTypeError = 3,
  ExtractError = 4,
  GenericError = 5,
  IOErrorError = 6,
#if defined(DEFINE_DEFAULT_ENGINE_BASE)
  ParquetError = 7,
#endif
#if defined(DEFINE_DEFAULT_ENGINE_BASE)
  ObjectStoreError = 8,
#endif
#if defined(DEFINE_DEFAULT_ENGINE_BASE)
  ObjectStorePathError = 9,
#endif
#if defined(DEFINE_DEFAULT_ENGINE_BASE)
  ReqwestError = 10,
#endif
  FileNotFoundError = 11,
  MissingColumnError = 12,
  UnexpectedColumnTypeError = 13,
  MissingDataError = 14,
  MissingVersionError = 15,
  DeletionVectorError = 16,
  InvalidUrlError = 17,
  MalformedJsonError = 18,
  MissingMetadataError = 19,
  MissingProtocolError = 20,
  InvalidProtocolError = 21,
  MissingMetadataAndProtocolError = 22,
  ParseError = 23,
  JoinFailureError = 24,
  Utf8Error = 25,
  ParseIntError = 26,
  InvalidColumnMappingModeError = 27,
  InvalidTableLocationError = 28,
  InvalidDecimalError = 29,
  InvalidStructDataError = 30,
  InternalError = 31,
  InvalidExpression = 32,
  InvalidLogPath = 33,
  FileAlreadyExists = 34,
  UnsupportedError = 35,
  ParseIntervalError = 36,
  ChangeDataFeedUnsupported = 37,
  ChangeDataFeedIncompatibleSchema = 38,
  InvalidCheckpoint = 39,
  LiteralExpressionTransformError = 40,
  CheckpointWriteError = 41,
  SchemaError = 42,
  LogHistoryError = 43,
} KernelError;

/**
 * Selects which commit type to return for the history_manager query. FFI-safe mirror of
 * [`HistoryCommitType`].
 */
typedef enum FfiHistoryCommitType {
  /**
   * Maps to [`HistoryCommitType::Published`].
   */
  Published = 0,
  /**
   * Maps to [`HistoryCommitType::Recreatable`].
   */
  Recreatable = 1,
} FfiHistoryCommitType;

/**
 * FFI-safe mirror of the kernel's [`KernelDeltaAction`]: the Delta log action kinds a caller can
 * request from [`commit_range_commits`].
 */
typedef enum DeltaAction {
  AddAction = 0,
  RemoveAction = 1,
  MetadataAction = 2,
  ProtocolAction = 3,
  CommitInfoAction = 4,
  CdcAction = 5,
  DomainMetadataAction = 6,
  SetTxnAction = 7,
  CheckpointMetadataAction = 8,
  SidecarAction = 9,
} DeltaAction;

/**
 * Definitions of level verbosity. Verbose Levels are "greater than" less verbose ones. So
 * Level::ERROR is the lowest, and Level::TRACE the highest.
 */
typedef enum Level {
  ERROR = 0,
  WARN = 1,
  INFO = 2,
  DEBUG = 3,
  TRACE = 4,
} Level;

/**
 * Format to use for log lines. These correspond to the formats from [`tracing_subscriber`
 * formats](https://docs.rs/tracing-subscriber/latest/tracing_subscriber/fmt/format/index.html).
 */
typedef enum LogLineFormat {
  /**
   * The default formatter. This emits human-readable, single-line logs for each event that
   * occurs, with the context displayed before the formatted representation of the event.
   * Example:
   * `2022-02-15T18:40:14.289898Z  INFO fmt: preparing to shave yaks number_of_yaks=3`
   */
  FULL,
  /**
   * A variant of the FULL formatter, optimized for short line lengths. Fields from the context
   * are appended to the fields of the formatted event, and targets are not shown.
   * Example:
   * `2022-02-17T19:51:05.809287Z  INFO fmt_compact: preparing to shave yaks number_of_yaks=3`
   */
  COMPACT,
  /**
   * Emits excessively pretty, multi-line logs, optimized for human readability. This is
   * primarily intended to be used in local development and debugging, or for command-line
   * applications, where automated analysis and compact storage of logs is less of a priority
   * than readability and visual appeal.
   * Example:
   * ```ignore
   *   2022-02-15T18:44:24.535324Z  INFO fmt_pretty: preparing to shave yaks, number_of_yaks: 3
   *   at examples/examples/fmt-pretty.rs:16 on main
   * ```
   */
  PRETTY,
  /**
   * Outputs newline-delimited JSON logs. This is intended for production use with systems where
   * structured logs are consumed as JSON by analysis and viewing tools. The JSON output is not
   * optimized for human readability.
   * Example:
   * `{"timestamp":"2022-02-15T18:47:10.821315Z","level":"INFO","fields":{"message":"preparing
   * to shave yaks","number_of_yaks":3},"target":"fmt_json"}`
   */
  JSON,
} LogLineFormat;

/**
 * Whether a table is path-based or catalog-managed.
 *
 */
typedef enum TableType {
  TableTypePathBased,
  TableTypeCatalogManaged,
} TableType;

/**
 * Why a transaction commit did not succeed.
 *
 */
typedef enum CommitFailureReason {
  CommitFailureReasonConflict,
  CommitFailureReasonRetryableIo,
  CommitFailureReasonError,
} CommitFailureReason;

/**
 * Which scan execution path produced a [`ScanMetadataCompleted`] event.
 *
 */
typedef enum ScanType {
  ScanTypeSequentialPhase,
  ScanTypeParallelPhase,
  ScanTypeFull,
} ScanType;

typedef struct CStringMap CStringMap;

/**
 * Transformation expressions that need to be applied to each row `i` in ScanMetadata. You can use
 * [`get_transform_for_row`] to get the transform for a particular row. If that returns an
 * associated expression, it _must_ be applied to the data read from the file specified by the
 * row. The resultant schema for this expression is guaranteed to be [`scan_logical_schema()`]. If
 * `get_transform_for_row` returns `NULL` no expression need be applied and the data read from disk
 * is already in the correct logical state.
 *
 * NB: If you are using `visit_scan_metadata` you don't need to worry about dealing with probing
 * `CTransforms`. The callback will be invoked with the correct transform for you.
 */
typedef struct CTransforms CTransforms;

/**
 * this struct can be used by an engine to materialize a selection vector
 */
typedef struct DvInfo DvInfo;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * A builder that allows setting options on the `Engine` before actually building it.
 *
 * For a normal object store backend, `url` is the table storage location (`s3://…`, `file://…`).
 * For REST, call [`set_builder_rest_object_store`] with a [`rest_engine::CRestEndpointConfig`]
 * and set `url` to the REST service base URL; see [`rest_engine`] for TLS and auth options.
 */
typedef struct EngineBuilder EngineBuilder;
#endif

/**
 * A handle for a [`CommittedTransaction`].
 *
 * Returned by [`commit`] and [`create_table_commit`]. Carries the committed version and,
 * when available, the post-commit snapshot. Use [`committed_transaction_version`] and
 * [`committed_transaction_post_commit_snapshot`] to read the contents, then release with
 * [`free_committed_transaction`].
 */
typedef struct ExclusiveCommittedTransaction ExclusiveCommittedTransaction;

/**
 * A handle representing an exclusive [`CreateTableTransactionBuilder`].
 *
 * The caller must eventually either call [`create_table_builder_build`] (which consumes the
 * handle and returns a transaction) or [`free_create_table_builder`] (which drops it without
 * creating anything).
 */
typedef struct ExclusiveCreateTableBuilder ExclusiveCreateTableBuilder;

/**
 * A handle for a create-table transaction (`Transaction<CreateTable>`).
 *
 * Returned by [`create_table_builder_build`]. Only supports operations valid during table
 * creation: adding files, setting data change, engine info, and committing. Operations like
 * file removal, blind append, and deletion vector updates are not available.
 */
typedef struct ExclusiveCreateTransaction ExclusiveCreateTransaction;

/**
 * Mutable handle for a [`DeletionVectorDescriptor`] crossing the FFI boundary.
 */
typedef struct ExclusiveDvDescriptor ExclusiveDvDescriptor;

/**
 * Mutable handle for a deletion vector descriptor map.
 */
typedef struct ExclusiveDvDescriptorMap ExclusiveDvDescriptorMap;

/**
 * an opaque struct that encapsulates data read by an engine. this handle can be passed back into
 * some kernel calls to operate on the data, or can be converted into the raw data as read by the
 * [`delta_kernel::Engine`] by calling [`get_raw_engine_data`]
 *
 * [`get_raw_engine_data`]: crate::engine_data::get_raw_engine_data
 */
typedef struct ExclusiveEngineData ExclusiveEngineData;

typedef struct ExclusiveFileReadResultIterator ExclusiveFileReadResultIterator;

/**
 * Mutable handle for a `PartitionValueMap`.
 */
typedef struct ExclusivePartitionValueMap ExclusivePartitionValueMap;

/**
 * A kernel-allocated type representing an owned byte buffer. This can be obtained by
 * calling [`allocate_kernel_bytes`] with a [`KernelBytesSlice`]. Kernel takes ownership of the
 * handle when it consumes the bytes and the engine must not use the handle afterwards.
 */
typedef struct ExclusiveRustBytes ExclusiveRustBytes;

/**
 * An opaque type that rust will understand as a string. This can be obtained by calling
 * [`allocate_kernel_string`] with a [`KernelStringSlice`]
 */
typedef struct ExclusiveRustString ExclusiveRustString;

/**
 * An opaque, exclusive handle owning a [`ScanBuilder`].
 *
 * The caller must eventually either call [`scan_builder_build`] (which consumes the handle
 * and produces a [`SharedScan`]) or [`free_scan_builder`] (which drops it without building).
 */
typedef struct ExclusiveScanBuilder ExclusiveScanBuilder;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
typedef struct ExclusiveTableChanges ExclusiveTableChanges;
#endif

/**
 * A handle for an existing-table transaction (`Transaction<ExistingTable>`).
 *
 * Returned by [`transaction`] and [`transaction_with_committer`]. Supports all transaction
 * operations including existing-table-only operations like blind append and file removal.
 */
typedef struct ExclusiveTransaction ExclusiveTransaction;

/**
 * A SQL expression.
 *
 * These expressions do not track or validate data types, other than the type
 * of literals. It is up to the expression evaluator to validate the
 * expression against a schema and add appropriate casts as required.
 */
typedef struct Expression Expression;

typedef struct KernelExpressionVisitorState KernelExpressionVisitorState;

typedef struct KernelSchemaVisitorState KernelSchemaVisitorState;

/**
 * Handle for a mutable boxed committer that can be passed across FFI
 */
typedef struct MutableCommitter MutableCommitter;

/**
 * An opaque handle with exclusive (Box-like) ownership of a [`FfiCommitRangeBuilder`].
 */
typedef struct MutableFfiCommitRangeBuilder MutableFfiCommitRangeBuilder;

/**
 * An opaque handle with exclusive (Box-like) ownership of a [`FfiSnapshotBuilder`].
 */
typedef struct MutableFfiSnapshotBuilder MutableFfiSnapshotBuilder;

typedef struct OptionCAuthHeaderCallback OptionCAuthHeaderCallback;

/**
 * A SQL predicate.
 *
 * These predicates do not track or validate data types, other than the type
 * of literals. It is up to the predicate evaluator to validate the
 * predicate against a schema and add appropriate casts as required.
 */
typedef struct Predicate Predicate;

/**
 * An opaque, shared handle to a single [`CommitAction`] yielded by [`commit_range_commits_next`].
 * The engine owns each handle passed to its visitor and must release it with
 * [`free_commit_action`].
 */
typedef struct SharedCommitAction SharedCommitAction;

typedef struct SharedCommitActionsIterator SharedCommitActionsIterator;

/**
 * An opaque, shared handle owning a [`CommitRange`] produced by
 * [`commit_range_builder_build`]. The caller owns the handle and must release it with
 * [`free_commit_range`].
 */
typedef struct SharedCommitRange SharedCommitRange;

typedef struct SharedExpression SharedExpression;

typedef struct SharedExpressionEvaluator SharedExpressionEvaluator;

typedef struct SharedExternEngine SharedExternEngine;

typedef struct SharedFfiUCCommitClient SharedFfiUCCommitClient;

typedef struct SharedMetadata SharedMetadata;

typedef struct SharedOpaqueExpressionOp SharedOpaqueExpressionOp;

typedef struct SharedOpaquePredicateOp SharedOpaquePredicateOp;

/**
 * A shared (`Arc`-like) handle to an [`PlanExecutor`].
 */
typedef struct SharedPlanExecutor SharedPlanExecutor;

typedef struct SharedPredicate SharedPredicate;

typedef struct SharedProtocol SharedProtocol;

typedef struct SharedScan SharedScan;

typedef struct SharedScanMetadata SharedScanMetadata;

typedef struct SharedScanMetadataIterator SharedScanMetadataIterator;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
typedef struct SharedScanTableChangesIterator SharedScanTableChangesIterator;
#endif

typedef struct SharedSchema SharedSchema;

typedef struct SharedSnapshot SharedSnapshot;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
typedef struct SharedTableChangesScan SharedTableChangesScan;
#endif

/**
 * A [`WriteContext`] that provides schema and path information needed for writing data.
 * This is a shared reference that can be cloned and used across multiple consumers.
 *
 * The [`WriteContext`] must be freed using [`free_write_context`] when no longer needed.
 */
typedef struct SharedWriteContext SharedWriteContext;

typedef struct StringSliceIterator StringSliceIterator;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveRustString *HandleExclusiveRustString;

/**
 * An error that can be returned to the engine. Engines that wish to associate additional
 * information can define and use any type that is [pointer
 * interconvertible](https://en.cppreference.com/w/cpp/language/static_cast#pointer-interconvertible)
 * with this one -- e.g. by subclassing this struct or by embedding this struct as the first member
 * of a [standard layout](https://en.cppreference.com/w/cpp/language/data_members#Standard-layout)
 * class.
 */
typedef struct EngineError {
  enum KernelError etype;
} EngineError;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveRustString_Tag {
  OkHandleExclusiveRustString,
  ErrHandleExclusiveRustString,
} ExternResultHandleExclusiveRustString_Tag;

typedef struct ExternResultHandleExclusiveRustString {
  ExternResultHandleExclusiveRustString_Tag tag;
  union {
    struct {
      HandleExclusiveRustString ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveRustString;

/**
 * A non-owned slice of a UTF8 string, intended for arg-passing between kernel and engine. The
 * slice is only valid until the function it was passed into returns, and should not be copied.
 *
 * # Safety
 *
 * Intentionally not Copy, Clone, Send, nor Sync.
 *
 * Whoever instantiates the struct must ensure it does not outlive the data it points to. The
 * compiler cannot help us here, because raw pointers don't have lifetimes. A good rule of thumb is
 * to always use the `kernel_string_slice` macro to create string slices, and to avoid returning
 * a string slice from a code block or function (since the move risks over-extending its lifetime):
 *
 * ```ignore
 * # // Ignored because this code is pub(crate) and doc tests cannot compile it
 * let dangling_slice = {
 *     let tmp = String::from("tmp");
 *     kernel_string_slice!(tmp)
 * }
 * ```
 *
 * Meanwhile, the callee must assume that the slice is only valid until the function returns, and
 * must not retain any references to the slice or its data that might outlive the function call.
 */
typedef struct KernelStringSlice {
  const char *ptr;
  uintptr_t len;
} KernelStringSlice;

typedef struct EngineError *(*AllocateErrorFn)(enum KernelError etype, struct KernelStringSlice msg);

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveRustBytes *HandleExclusiveRustBytes;

/**
 * A non-owned slice of raw bytes, intended for arg-passing between kernel and engine.
 *
 * Like [`KernelStringSlice`], the pointed-to data must outlive the slice itself, and the slice
 * must not be retained beyond the foreign function call it was passed into.
 */
typedef struct KernelBytesSlice {
  const uint8_t *ptr;
  uintptr_t len;
} KernelBytesSlice;

/**
 * Represents an owned slice of boolean values allocated by the kernel. Any time the engine
 * receives a `KernelBoolSlice` as a return value from a kernel method, engine is responsible
 * to free that slice, by calling [super::free_bool_slice] exactly once.
 */
typedef struct KernelBoolSlice {
  bool *ptr;
  uintptr_t len;
} KernelBoolSlice;

/**
 * An owned slice of u64 row indexes allocated by the kernel. The engine is responsible for
 * freeing this slice by calling [super::free_row_indexes] once.
 */
typedef struct KernelRowIndexArray {
  uint64_t *ptr;
  uintptr_t len;
} KernelRowIndexArray;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveEngineData *HandleExclusiveEngineData;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultEngineBuilder_Tag {
  OkEngineBuilder,
  ErrEngineBuilder,
} ExternResultEngineBuilder_Tag;

typedef struct ExternResultEngineBuilder {
  ExternResultEngineBuilder_Tag tag;
  union {
    struct {
      struct EngineBuilder *ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultEngineBuilder;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultbool_Tag {
  Okbool,
  Errbool,
} ExternResultbool_Tag;

typedef struct ExternResultbool {
  ExternResultbool_Tag tag;
  union {
    struct {
      bool ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultbool;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * REST file API dialect passed to
 * [`set_builder_rest_object_store`](crate::set_builder_rest_object_store).
 *
 * Each [`KernelStringSlice`] must remain valid for the duration of that call; the kernel copies
 * the strings into the built engine. Optional fields (the prefixes and `entry_strip_prefix`) may
 * be empty, which the kernel treats as unset.
 */
typedef struct CRestEndpointConfig {
  struct KernelStringSlice files_prefix;
  struct KernelStringSlice directories_prefix;
  struct KernelStringSlice page_token_param;
  struct KernelStringSlice start_from_param;
  struct KernelStringSlice recursive_param;
  struct KernelStringSlice overwrite_param;
  struct KernelStringSlice contents_field;
  struct KernelStringSlice next_page_token_field;
  struct KernelStringSlice entry_path_field;
  struct KernelStringSlice entry_size_field;
  struct KernelStringSlice entry_is_directory_field;
  struct KernelStringSlice entry_last_modified_field;
  struct KernelStringSlice entry_strip_prefix;
} CRestEndpointConfig;
#endif

typedef void *NullableCvoid;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedExternEngine *HandleSharedExternEngine;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedExternEngine_Tag {
  OkHandleSharedExternEngine,
  ErrHandleSharedExternEngine,
} ExternResultHandleSharedExternEngine_Tag;

typedef struct ExternResultHandleSharedExternEngine {
  ExternResultHandleSharedExternEngine_Tag tag;
  union {
    struct {
      HandleSharedExternEngine ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedExternEngine;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct MutableFfiSnapshotBuilder *HandleMutableFfiSnapshotBuilder;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleMutableFfiSnapshotBuilder_Tag {
  OkHandleMutableFfiSnapshotBuilder,
  ErrHandleMutableFfiSnapshotBuilder,
} ExternResultHandleMutableFfiSnapshotBuilder_Tag;

typedef struct ExternResultHandleMutableFfiSnapshotBuilder {
  ExternResultHandleMutableFfiSnapshotBuilder_Tag tag;
  union {
    struct {
      HandleMutableFfiSnapshotBuilder ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleMutableFfiSnapshotBuilder;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedSnapshot *HandleSharedSnapshot;

/**
 * Delta table version is 8 byte unsigned int
 */
typedef uint64_t Version;

/**
 * FFI-safe LogPath representation that can be passed from the engine
 */
typedef struct FfiLogPath {
  /**
   * URL location of the log file
   */
  struct KernelStringSlice location;
  /**
   * Last modified time as milliseconds since unix epoch
   */
  int64_t last_modified;
  /**
   * Size in bytes of the log file
   */
  uint64_t size;
} FfiLogPath;

/**
 * FFI-safe array of LogPaths. Note that we _explicitly_ do not implement `Copy` on this struct
 * despite all types being `Copy`, to avoid accidental misuse of the pointer.
 *
 * This struct is essentially a borrowed view into an array. The owner must ensure the underlying
 * array remains valid for the duration of its use.
 */
typedef struct LogPathArray {
  /**
   * Pointer to the first element of the FfiLogPath array. If len is 0, this pointer may be
   * null, otherwise it must be non-null.
   */
  const struct FfiLogPath *ptr;
  /**
   * Number of elements in the array
   */
  uintptr_t len;
} LogPathArray;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedSnapshot_Tag {
  OkHandleSharedSnapshot,
  ErrHandleSharedSnapshot,
} ExternResultHandleSharedSnapshot_Tag;

typedef struct ExternResultHandleSharedSnapshot {
  ExternResultHandleSharedSnapshot_Tag tag;
  union {
    struct {
      HandleSharedSnapshot ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedSnapshot;

/**
 * Outcome of a checkpoint write performed via [`checkpoint_snapshot`].
 *
 * `Written` indicates the kernel wrote a new checkpoint at this version and returns an
 * updated snapshot whose log segment reflects the new checkpoint. `AlreadyExists` indicates
 * a checkpoint at this version was already present (either pre-existed or was written by a
 * concurrent writer) and returns the original snapshot unchanged.
 *
 * Both variants carry an owned `Handle<SharedSnapshot>` that the caller must release via
 * [`free_snapshot`].
 *
 */
typedef enum FfiCheckpointWriteResult_Tag {
  FfiCheckpointWriteResultWritten,
  FfiCheckpointWriteResultAlreadyExists,
} FfiCheckpointWriteResult_Tag;

typedef struct FfiCheckpointWriteResult {
  FfiCheckpointWriteResult_Tag tag;
  union {
    struct {
      HandleSharedSnapshot written;
    };
    struct {
      HandleSharedSnapshot already_exists;
    };
  };
} FfiCheckpointWriteResult;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultFfiCheckpointWriteResult_Tag {
  OkFfiCheckpointWriteResult,
  ErrFfiCheckpointWriteResult,
} ExternResultFfiCheckpointWriteResult_Tag;

typedef struct ExternResultFfiCheckpointWriteResult {
  ExternResultFfiCheckpointWriteResult_Tag tag;
  union {
    struct {
      struct FfiCheckpointWriteResult ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultFfiCheckpointWriteResult;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueusize_Tag {
  Someusize,
  Noneusize,
} OptionalValueusize_Tag;

typedef struct OptionalValueusize {
  OptionalValueusize_Tag tag;
  union {
    struct {
      uintptr_t some;
    };
  };
} OptionalValueusize;

/**
 * Checkpoint write configuration for [`checkpoint_snapshot`]. Mirrors the kernel's
 * [`CheckpointSpec`] and [`V2CheckpointConfig`] across the C ABI; see those types for the
 * authoritative semantics and constraints.
 *
 * Pass `Option<&FfiCheckpointSpec>::None` to [`checkpoint_snapshot`] to let the kernel
 * auto-pick V1 or V2 based on the table's protocol features.
 *
 */
typedef enum FfiCheckpointSpec_Tag {
  /**
   * See [`CheckpointSpec::V1`].
   */
  FfiCheckpointSpecV1,
  /**
   * See [`V2CheckpointConfig::NoSidecar`].
   */
  FfiCheckpointSpecV2NoSidecar,
  /**
   * See [`V2CheckpointConfig::WithSidecar`].
   */
  FfiCheckpointSpecV2WithSidecar,
} FfiCheckpointSpec_Tag;

typedef struct FfiCheckpointSpecV2WithSidecar_Body {
  /**
   * See [`V2CheckpointConfig::WithSidecar`]. `None` selects the kernel default hint.
   */
  struct OptionalValueusize file_actions_per_sidecar_hint;
} FfiCheckpointSpecV2WithSidecar_Body;

typedef struct FfiCheckpointSpec {
  FfiCheckpointSpec_Tag tag;
  union {
    FfiCheckpointSpecV2WithSidecar_Body v2_with_sidecar;
  };
} FfiCheckpointSpec;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResulti64_Tag {
  Oki64,
  Erri64,
} ExternResulti64_Tag;

typedef struct ExternResulti64 {
  ExternResulti64_Tag tag;
  union {
    struct {
      int64_t ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResulti64;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultVersion_Tag {
  OkVersion,
  ErrVersion,
} ExternResultVersion_Tag;

typedef struct ExternResultVersion {
  ExternResultVersion_Tag tag;
  union {
    struct {
      Version ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultVersion;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueVersion_Tag {
  SomeVersion,
  NoneVersion,
} OptionalValueVersion_Tag;

typedef struct OptionalValueVersion {
  OptionalValueVersion_Tag tag;
  union {
    struct {
      Version some;
    };
  };
} OptionalValueVersion;

/**
 * A commit located by a timestamp query: a commit version paired with its timestamp. FFI-safe
 * mirror of [`CommitAt`].
 */
typedef struct FfiCommitAt {
  /**
   * The commit version.
   */
  Version version;
  /**
   * Timestamp (milliseconds since the Unix epoch) associated with this commit. This is the
   * commit's in-commit timestamp (ICT) when ICT is enabled, otherwise the commit's file
   * modification time.
   */
  int64_t timestamp;
} FfiCommitAt;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultFfiCommitAt_Tag {
  OkFfiCommitAt,
  ErrFfiCommitAt,
} ExternResultFfiCommitAt_Tag;

typedef struct ExternResultFfiCommitAt {
  ExternResultFfiCommitAt_Tag tag;
  union {
    struct {
      struct FfiCommitAt ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultFfiCommitAt;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedSchema *HandleSharedSchema;

/**
 * Allow engines to allocate strings of their own type. the contract of calling a passed allocate
 * function is that `kernel_str` is _only_ valid until the return from this function
 */
typedef NullableCvoid (*AllocateStringFn)(struct KernelStringSlice kernel_str);

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct StringSliceIterator *HandleStringSliceIterator;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedProtocol *HandleSharedProtocol;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedMetadata *HandleSharedMetadata;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueKernelStringSlice_Tag {
  SomeKernelStringSlice,
  NoneKernelStringSlice,
} OptionalValueKernelStringSlice_Tag;

typedef struct OptionalValueKernelStringSlice {
  OptionalValueKernelStringSlice_Tag tag;
  union {
    struct {
      struct KernelStringSlice some;
    };
  };
} OptionalValueKernelStringSlice;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct MutableFfiCommitRangeBuilder *HandleMutableFfiCommitRangeBuilder;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleMutableFfiCommitRangeBuilder_Tag {
  OkHandleMutableFfiCommitRangeBuilder,
  ErrHandleMutableFfiCommitRangeBuilder,
} ExternResultHandleMutableFfiCommitRangeBuilder_Tag;

typedef struct ExternResultHandleMutableFfiCommitRangeBuilder {
  ExternResultHandleMutableFfiCommitRangeBuilder_Tag tag;
  union {
    struct {
      HandleMutableFfiCommitRangeBuilder ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleMutableFfiCommitRangeBuilder;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedCommitRange *HandleSharedCommitRange;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedCommitRange_Tag {
  OkHandleSharedCommitRange,
  ErrHandleSharedCommitRange,
} ExternResultHandleSharedCommitRange_Tag;

typedef struct ExternResultHandleSharedCommitRange {
  ExternResultHandleSharedCommitRange_Tag tag;
  union {
    struct {
      HandleSharedCommitRange ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedCommitRange;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedCommitAction *HandleSharedCommitAction;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveFileReadResultIterator *HandleExclusiveFileReadResultIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveFileReadResultIterator_Tag {
  OkHandleExclusiveFileReadResultIterator,
  ErrHandleExclusiveFileReadResultIterator,
} ExternResultHandleExclusiveFileReadResultIterator_Tag;

typedef struct ExternResultHandleExclusiveFileReadResultIterator {
  ExternResultHandleExclusiveFileReadResultIterator_Tag tag;
  union {
    struct {
      HandleExclusiveFileReadResultIterator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveFileReadResultIterator;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedCommitActionsIterator *HandleSharedCommitActionsIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedCommitActionsIterator_Tag {
  OkHandleSharedCommitActionsIterator,
  ErrHandleSharedCommitActionsIterator,
} ExternResultHandleSharedCommitActionsIterator_Tag;

typedef struct ExternResultHandleSharedCommitActionsIterator {
  ExternResultHandleSharedCommitActionsIterator_Tag tag;
  union {
    struct {
      HandleSharedCommitActionsIterator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedCommitActionsIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultNullableCvoid_Tag {
  OkNullableCvoid,
  ErrNullableCvoid,
} ExternResultNullableCvoid_Tag;

typedef struct ExternResultNullableCvoid {
  ExternResultNullableCvoid_Tag tag;
  union {
    struct {
      NullableCvoid ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultNullableCvoid;

/**
 * ABI-compatible struct for ArrowArray from C Data Interface
 * See <https://arrow.apache.org/docs/format/CDataInterface.html#structure-definitions>
 *
 * ```
 * # use arrow_data::ArrayData;
 * # use arrow_data::ffi::FFI_ArrowArray;
 * fn export_array(array: &ArrayData) -> FFI_ArrowArray {
 *     FFI_ArrowArray::new(array)
 * }
 * ```
 */
typedef struct FFI_ArrowArray {
  int64_t length;
  int64_t null_count;
  int64_t offset;
  int64_t n_buffers;
  int64_t n_children;
  const void **buffers;
  struct FFI_ArrowArray **children;
  struct FFI_ArrowArray *dictionary;
  void (*release)(struct FFI_ArrowArray *arg1);
  void *private_data;
} FFI_ArrowArray;

/**
 * ABI-compatible struct for `ArrowSchema` from C Data Interface
 * See <https://arrow.apache.org/docs/format/CDataInterface.html#structure-definitions>
 *
 * ```
 * # use arrow_schema::DataType;
 * # use arrow_schema::ffi::FFI_ArrowSchema;
 * fn array_schema(data_type: &DataType) -> FFI_ArrowSchema {
 *     FFI_ArrowSchema::try_from(data_type).unwrap()
 * }
 * ```
 *
 */
typedef struct FFI_ArrowSchema {
  const char *format;
  const char *name;
  const char *metadata;
  /**
   * Refer to [Arrow Flags](https://arrow.apache.org/docs/format/CDataInterface.html#c.ArrowSchema.flags)
   */
  int64_t flags;
  int64_t n_children;
  struct FFI_ArrowSchema **children;
  struct FFI_ArrowSchema *dictionary;
  void (*release)(struct FFI_ArrowSchema *arg1);
  void *private_data;
} FFI_ArrowSchema;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Struct to allow binding to the arrow [C Data
 * Interface](https://arrow.apache.org/docs/format/CDataInterface.html). This includes the data and
 * the schema.
 */
typedef struct ArrowFFIData {
  struct FFI_ArrowArray array;
  struct FFI_ArrowSchema schema;
} ArrowFFIData;
#endif

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultArrowFFIData_Tag {
  OkArrowFFIData,
  ErrArrowFFIData,
} ExternResultArrowFFIData_Tag;

typedef struct ExternResultArrowFFIData {
  ExternResultArrowFFIData_Tag tag;
  union {
    struct {
      struct ArrowFFIData *ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultArrowFFIData;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveEngineData_Tag {
  OkHandleExclusiveEngineData,
  ErrHandleExclusiveEngineData,
} ExternResultHandleExclusiveEngineData_Tag;

typedef struct ExternResultHandleExclusiveEngineData {
  ExternResultHandleExclusiveEngineData_Tag tag;
  union {
    struct {
      HandleExclusiveEngineData ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveEngineData;

typedef struct FileMeta {
  struct KernelStringSlice path;
  int64_t last_modified;
  uintptr_t size;
} FileMeta;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedExpressionEvaluator *HandleSharedExpressionEvaluator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedExpressionEvaluator_Tag {
  OkHandleSharedExpressionEvaluator,
  ErrHandleSharedExpressionEvaluator,
} ExternResultHandleSharedExpressionEvaluator_Tag;

typedef struct ExternResultHandleSharedExpressionEvaluator {
  ExternResultHandleSharedExpressionEvaluator_Tag tag;
  union {
    struct {
      HandleSharedExpressionEvaluator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedExpressionEvaluator;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveTableChanges *HandleExclusiveTableChanges;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveTableChanges_Tag {
  OkHandleExclusiveTableChanges,
  ErrHandleExclusiveTableChanges,
} ExternResultHandleExclusiveTableChanges_Tag;

typedef struct ExternResultHandleExclusiveTableChanges {
  ExternResultHandleExclusiveTableChanges_Tag tag;
  union {
    struct {
      HandleExclusiveTableChanges ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveTableChanges;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedTableChangesScan *HandleSharedTableChangesScan;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedTableChangesScan_Tag {
  OkHandleSharedTableChangesScan,
  ErrHandleSharedTableChangesScan,
} ExternResultHandleSharedTableChangesScan_Tag;

typedef struct ExternResultHandleSharedTableChangesScan {
  ExternResultHandleSharedTableChangesScan_Tag tag;
  union {
    struct {
      HandleSharedTableChangesScan ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedTableChangesScan;

/**
 * A predicate that can be used to skip data when scanning.
 *
 * Used by [`scan`] and [`scan_builder_with_predicate`]. The engine provides a pointer to its
 * native predicate along with a visitor function that recursively visits it. This engine state
 * must remain valid for the duration of the call. The kernel allocates visitor state internally,
 * which becomes the second argument to the visitor invocation. Thanks to this double indirection,
 * engine and kernel each retain ownership of their respective objects with no need to coordinate
 * memory lifetimes.
 */
typedef struct EnginePredicate {
  void *predicate;
  uintptr_t (*visitor)(void *predicate, struct KernelExpressionVisitorState *state);
} EnginePredicate;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedScanTableChangesIterator *HandleSharedScanTableChangesIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedScanTableChangesIterator_Tag {
  OkHandleSharedScanTableChangesIterator,
  ErrHandleSharedScanTableChangesIterator,
} ExternResultHandleSharedScanTableChangesIterator_Tag;

typedef struct ExternResultHandleSharedScanTableChangesIterator {
  ExternResultHandleSharedScanTableChangesIterator_Tag tag;
  union {
    struct {
      HandleSharedScanTableChangesIterator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedScanTableChangesIterator;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedFfiUCCommitClient *HandleSharedFfiUCCommitClient;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueHandleExclusiveRustString_Tag {
  SomeHandleExclusiveRustString,
  NoneHandleExclusiveRustString,
} OptionalValueHandleExclusiveRustString_Tag;

typedef struct OptionalValueHandleExclusiveRustString {
  OptionalValueHandleExclusiveRustString_Tag tag;
  union {
    struct {
      HandleExclusiveRustString some;
    };
  };
} OptionalValueHandleExclusiveRustString;

/**
 * Data representing a commit.
 */
typedef struct Commit {
  int64_t version;
  int64_t timestamp;
  struct KernelStringSlice file_name;
  int64_t file_size;
  int64_t file_modification_timestamp;
} Commit;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueCommit_Tag {
  SomeCommit,
  NoneCommit,
} OptionalValueCommit_Tag;

typedef struct OptionalValueCommit {
  OptionalValueCommit_Tag tag;
  union {
    struct {
      struct Commit some;
    };
  };
} OptionalValueCommit;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValuei64_Tag {
  Somei64,
  Nonei64,
} OptionalValuei64_Tag;

typedef struct OptionalValuei64 {
  OptionalValuei64_Tag tag;
  union {
    struct {
      int64_t some;
    };
  };
} OptionalValuei64;

/**
 * Request to commit a new version to the table. It must include either a `commit_info` or
 * `latest_backfilled_version`.
 */
typedef struct CommitRequest {
  struct KernelStringSlice table_id;
  struct KernelStringSlice table_uri;
  struct OptionalValueCommit commit_info;
  struct OptionalValuei64 latest_backfilled_version;
  /**
   * json serialized version of the metadata
   */
  struct OptionalValueKernelStringSlice metadata;
  /**
   * json serialized version of the protocol
   */
  struct OptionalValueKernelStringSlice protocol;
} CommitRequest;

/**
 * The callback that will be called when the client wants to commit. Return `None` on success, or
 * `Some("error description")` if an error occured.
 */
typedef struct OptionalValueHandleExclusiveRustString (*CCommit)(NullableCvoid context,
                                                                 struct CommitRequest request);

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct MutableCommitter *HandleMutableCommitter;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleMutableCommitter_Tag {
  OkHandleMutableCommitter,
  ErrHandleMutableCommitter,
} ExternResultHandleMutableCommitter_Tag;

typedef struct ExternResultHandleMutableCommitter {
  ExternResultHandleMutableCommitter_Tag tag;
  union {
    struct {
      HandleMutableCommitter ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleMutableCommitter;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedExpression *HandleSharedExpression;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedPredicate *HandleSharedPredicate;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedOpaqueExpressionOp *HandleSharedOpaqueExpressionOp;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedOpaquePredicateOp *HandleSharedOpaquePredicateOp;

typedef void (*VisitLiteralFni32)(void *data, uintptr_t sibling_list_id, int32_t value);

typedef void (*VisitLiteralFni64)(void *data, uintptr_t sibling_list_id, int64_t value);

typedef void (*VisitLiteralFni16)(void *data, uintptr_t sibling_list_id, int16_t value);

typedef void (*VisitLiteralFni8)(void *data, uintptr_t sibling_list_id, int8_t value);

typedef void (*VisitLiteralFnf32)(void *data, uintptr_t sibling_list_id, float value);

typedef void (*VisitLiteralFnf64)(void *data, uintptr_t sibling_list_id, double value);

typedef void (*VisitLiteralFnKernelStringSlice)(void *data,
                                                uintptr_t sibling_list_id,
                                                struct KernelStringSlice value);

typedef void (*VisitLiteralFnbool)(void *data, uintptr_t sibling_list_id, bool value);

typedef void (*VisitJunctionFn)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);

typedef void (*VisitUnaryFn)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);

typedef void (*VisitParseJsonFn)(void *data,
                                 uintptr_t sibling_list_id,
                                 uintptr_t child_list_id,
                                 HandleSharedSchema output_schema);

typedef void (*VisitBinaryFn)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);

typedef void (*VisitVariadicFn)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);

/**
 * The [`EngineExpressionVisitor`] defines a visitor system to allow engines to build their own
 * representation of a kernel expression or predicate.
 *
 * The model is list based. When the kernel needs a list, it will ask engine to allocate one of a
 * particular size. Once allocated the engine returns an `id`, which can be any integer identifier
 * ([`usize`]) the engine wants, and will be passed back to the engine to identify the list in the
 * future.
 *
 * Every expression the kernel visits belongs to some list of "sibling" elements. The schema
 * itself is a list of schema elements, and every complex type (struct expression, array, junction,
 * etc) contains a list of "child" elements.
 *  1. Before visiting any complex expression type, the kernel asks the engine to allocate a list
 *     to hold its children
 *  2. When visiting any expression element, the kernel passes its parent's "child list" as the
 *     "sibling list" the element should be appended to:
 *      - For a struct literal, first visit each struct field and visit each value
 *      - For a struct expression, visit each sub expression.
 *      - For an array literal, visit each of the elements.
 *      - For a junction `and` or `or` expression, visit each sub-expression.
 *      - For a binary operator expression, visit the left and right operands.
 *      - For a unary `is null` or `not` expression, visit the sub-expression.
 *  3. When visiting a complex expression, the kernel also passes the "child list" containing that
 *     element's (already-visited) children.
 *  4. The [`visit_expression`] method returns the id of the list of top-level columns
 *
 * WARNING: The visitor MUST NOT retain internal references to string slices or binary data passed
 * to visitor methods
 * TODO: Visit type information in struct field. This will likely involve using the schema visitor.
 * Note that struct literals are currently in flux, and may change significantly. Here is the
 * relevant issue: <https://github.com/delta-io/delta-kernel-rs/issues/412>
 */
typedef struct EngineExpressionVisitor {
  /**
   * An opaque engine state pointer
   */
  void *data;
  /**
   * Creates a new expression list, optionally reserving capacity up front
   */
  uintptr_t (*make_field_list)(void *data, uintptr_t reserve);
  /**
   * Visit a 32bit `integer` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFni32 visit_literal_int;
  /**
   * Visit a 64bit `long`  belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFni64 visit_literal_long;
  /**
   * Visit a 16bit `short` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFni16 visit_literal_short;
  /**
   * Visit an 8bit `byte` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFni8 visit_literal_byte;
  /**
   * Visit a 32bit `float` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFnf32 visit_literal_float;
  /**
   * Visit a 64bit `double` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFnf64 visit_literal_double;
  /**
   * Visit a `string` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFnKernelStringSlice visit_literal_string;
  /**
   * Visit a `boolean` belonging to the list identified by `sibling_list_id`.
   */
  VisitLiteralFnbool visit_literal_bool;
  /**
   * Visit a 64bit timestamp belonging to the list identified by `sibling_list_id`.
   * The timestamp is microsecond precision and adjusted to UTC.
   */
  VisitLiteralFni64 visit_literal_timestamp;
  /**
   * Visit a 64bit timestamp belonging to the list identified by `sibling_list_id`.
   * The timestamp is microsecond precision with no timezone.
   */
  VisitLiteralFni64 visit_literal_timestamp_ntz;
  /**
   * Visit a 32bit integer `date` representing days since UNIX epoch 1970-01-01.  The `date`
   * belongs to the list identified by `sibling_list_id`.
   */
  VisitLiteralFni32 visit_literal_date;
  /**
   * Visit binary data at the `buffer` with length `len` belonging to the list identified by
   * `sibling_list_id`.
   */
  void (*visit_literal_binary)(void *data,
                               uintptr_t sibling_list_id,
                               const uint8_t *buffer,
                               uintptr_t len);
  /**
   * Visit a 128bit `decimal` value with the given precision and scale. The 128bit integer
   * is split into the most significant 64 bits in `value_ms`, and the least significant 64
   * bits in `value_ls`. The `decimal` belongs to the list identified by `sibling_list_id`.
   */
  void (*visit_literal_decimal)(void *data,
                                uintptr_t sibling_list_id,
                                int64_t value_ms,
                                uint64_t value_ls,
                                uint8_t precision,
                                uint8_t scale);
  /**
   * Visit a struct literal belonging to the list identified by `sibling_list_id`.
   * The field names of the struct are in a list identified by `child_field_list_id`.
   * The values of the struct are in a list identified by `child_value_list_id`.
   */
  void (*visit_literal_struct)(void *data,
                               uintptr_t sibling_list_id,
                               uintptr_t child_field_list_id,
                               uintptr_t child_value_list_id);
  /**
   * Visit an array literal belonging to the list identified by `sibling_list_id`.
   * The values of the array are in a list identified by `child_list_id`.
   */
  void (*visit_literal_array)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);
  /**
   * Visit a map literal belonging to the list identified by `sibling_list_id`.
   * The keys of the map are in order in a list identified by `key_list_id`. The values of the
   * map are in order in a list identified by `value_list_id`.
   */
  void (*visit_literal_map)(void *data,
                            uintptr_t sibling_list_id,
                            uintptr_t key_list_id,
                            uintptr_t value_list_id);
  /**
   * Visits a typed null value belonging to the list identified by `sibling_list_id`.
   *
   * The `type_tag` identifies the data type using the `NullTypeTag` encoding. For decimal
   * nulls (`type_tag == 12`), `precision` and `scale` carry the decimal type parameters;
   * for all other types, they are zero. Non-primitive types (struct, array, map, variant)
   * use `type_tag == 255`.
   */
  void (*visit_literal_null)(void *data,
                             uintptr_t sibling_list_id,
                             uint8_t type_tag,
                             uint8_t precision,
                             uint8_t scale);
  /**
   * Visits an `and` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expressions of the array are in a list identified by `child_list_id`
   */
  VisitJunctionFn visit_and;
  /**
   * Visits an `or` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expressions of the array are in a list identified by `child_list_id`
   */
  VisitJunctionFn visit_or;
  /**
   * Visits a `not` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expression will be in a _one_ item list identified by `child_list_id`
   */
  VisitUnaryFn visit_not;
  /**
   * Visits a `is_null` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expression will be in a _one_ item list identified by `child_list_id`
   */
  VisitUnaryFn visit_is_null;
  /**
   * Visits the `ToJson` unary operator belonging to the list identified by `sibling_list_id`.
   * The sub-expression will be in a _one_ item list identified by `child_list_id`
   */
  VisitUnaryFn visit_to_json;
  /**
   * Visits the `ParseJson` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expression (JSON string) will be in a _one_ item list identified by
   * `child_list_id`. The `output_schema` handle specifies the schema to parse the JSON
   * into.
   */
  VisitParseJsonFn visit_parse_json;
  /**
   * Visits the `MapToStruct` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expression (map column) will be in a _one_ item list identified by `child_list_id`.
   * The output struct schema is determined by the evaluator's result type.
   */
  VisitUnaryFn visit_map_to_struct;
  /**
   * Visits the `LessThan` binary operator belonging to the list identified by
   * `sibling_list_id`. The operands will be in a _two_ item list identified by
   * `child_list_id`
   */
  VisitBinaryFn visit_lt;
  /**
   * Visits the `GreaterThan` binary operator belonging to the list identified by
   * `sibling_list_id`. The operands will be in a _two_ item list identified by
   * `child_list_id`
   */
  VisitBinaryFn visit_gt;
  /**
   * Visits the `Equal` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_eq;
  /**
   * Visits the `Distinct` binary operator belonging to the list identified by
   * `sibling_list_id`. The operands will be in a _two_ item list identified by
   * `child_list_id`
   */
  VisitBinaryFn visit_distinct;
  /**
   * Visits the `In` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_in;
  /**
   * Visits the `Add` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_add;
  /**
   * Visits the `Minus` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_minus;
  /**
   * Visits the `Multiply` binary operator belonging to the list identified by
   * `sibling_list_id`. The operands will be in a _two_ item list identified by
   * `child_list_id`
   */
  VisitBinaryFn visit_multiply;
  /**
   * Visits the `Divide` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_divide;
  /**
   * Visits the `Coalesce` variadic operator belonging to the list identified by
   * `sibling_list_id`. The operands will be in a list identified by `child_list_id`
   */
  VisitVariadicFn visit_coalesce;
  /**
   * Visits the `Array` variadic constructor belonging to the list identified by
   * `sibling_list_id`. The element expressions will be in a list identified by
   * `child_list_id`.
   */
  VisitVariadicFn visit_array;
  /**
   * Visits the `column` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_column)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visits a `Struct` expression belonging to the list identified by `sibling_list_id`.
   * The sub-expressions (fields) of the struct are in a list identified by `child_list_id`
   */
  void (*visit_struct_expr)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);
  /**
   * Visits a `StructPatch` expression belonging to the list identified by `sibling_list_id`.
   * The `input_path_list_id` is a zero-or-one item list containing the patch's input path as a
   * column reference. The `prepended_field_list_id` and `appended_field_list_id` identify
   * expression lists to emit before and after the named input fields. The
   * `field_patch_list_id` identifies the list of named field patches to apply. See also
   * [`Self::visit_field_patch`].
   */
  void (*visit_struct_patch_expr)(void *data,
                                  uintptr_t sibling_list_id,
                                  uintptr_t input_path_list_id,
                                  uintptr_t prepended_field_list_id,
                                  uintptr_t field_patch_list_id,
                                  uintptr_t appended_field_list_id);
  /**
   * Visits one named field patch of a `StructPatch` expression that owns the list identified by
   * `sibling_list_id`.
   *
   * The `insertion_expr_list_id` identifies expressions to emit after this field's output
   * position. If `keep_input` is true, the original input field is emitted before these
   * insertions. If `keep_input` is false, the original input field is omitted and the first
   * insertion, if present, occupies the input field's output position. The `optional` flag
   * indicates that the patch is silently ignored when the input field does not exist.
   */
  void (*visit_field_patch)(void *data,
                            uintptr_t sibling_list_id,
                            struct KernelStringSlice field_name,
                            uintptr_t insertion_expr_list_id,
                            bool keep_input,
                            bool optional);
  /**
   * Visits the operator (`op`) and children (`child_list_id`) of an opaque expression belonging
   * to the list identified by `sibling_list_id`.
   */
  void (*visit_opaque_expr)(void *data,
                            uintptr_t sibling_list_id,
                            HandleSharedOpaqueExpressionOp op,
                            uintptr_t child_list_id);
  /**
   * Visits the operator (`op`) and children (`child_list_id`) of an opaque predicate belonging
   * to the list identified by `sibling_list_id`.
   */
  void (*visit_opaque_pred)(void *data,
                            uintptr_t sibling_list_id,
                            HandleSharedOpaquePredicateOp op,
                            uintptr_t child_list_id);
  /**
   * Visits the name of an `Expression::Unknown` or `Predicate::Unknown` belonging to the
   * list identified by `sibling_list_id`.
   */
  void (*visit_unknown)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
} EngineExpressionVisitor;

/**
 * Model iterators. This allows an engine to specify iteration however it likes, and we simply wrap
 * the engine functions. The engine retains ownership of the iterator.
 */
typedef struct EngineIterator {
  /**
   * Opaque data that will be iterated over. This data will be passed to the get_next function
   * each time a next item is requested from the iterator
   */
  void *data;
  /**
   * A function that should advance the iterator and return the next time from the data
   * If the iterator is complete, it should return null. It should be safe to
   * call `get_next()` multiple times if it returns null.
   */
  const void *(*get_next)(void *data);
} EngineIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultusize_Tag {
  Okusize,
  Errusize,
} ExternResultusize_Tag;

typedef struct ExternResultusize {
  ExternResultusize_Tag tag;
  union {
    struct {
      uintptr_t ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultusize;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedExpression_Tag {
  OkHandleSharedExpression,
  ErrHandleSharedExpression,
} ExternResultHandleSharedExpression_Tag;

typedef struct ExternResultHandleSharedExpression {
  ExternResultHandleSharedExpression_Tag tag;
  union {
    struct {
      HandleSharedExpression ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedExpression;

/**
 * An engine-provided expression along with a visitor function to convert
 * it to a kernel expression.
 *
 * The engine provides a pointer to its own expression representation, along
 * with a visitor function that can convert it to a kernel expression by
 * calling the appropriate visitor methods on the kernel's
 * `KernelExpressionVisitorState`. The visitor function returns an expression
 * ID that can be converted to a kernel expression handle.
 */
typedef struct EngineExpression {
  void *expression;
  uintptr_t (*visitor)(void *expression, struct KernelExpressionVisitorState *state);
} EngineExpression;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedPredicate_Tag {
  OkHandleSharedPredicate,
  ErrHandleSharedPredicate,
} ExternResultHandleSharedPredicate_Tag;

typedef struct ExternResultHandleSharedPredicate {
  ExternResultHandleSharedPredicate_Tag tag;
  union {
    struct {
      HandleSharedPredicate ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedPredicate;

/**
 * An error that can be returned from engine-side execution (e.g during an upcall).
 *
 * This is intended to be a kernel-allocated error which Engines can return TO kernel. It is the
 * inverse of [`EngineError`] (which is engine-allocated, and returned FROM kernel).
 *
 * The message is an [`ExclusiveRustString`] handle, which means the engine must
 * downcall to [`allocate_kernel_string`](crate::allocate_kernel_string) to construct it. Kernel
 * can then take ownership and free it appropriately after receiving the error.
 */
typedef struct EngineExecError {
  enum KernelError etype;
  HandleExclusiveRustString message;
} EngineExecError;

/**
 * Generic wrapper around an EngineExecError, representing the result of an engine upcall.
 *
 * Typically, engines will populate an out pointer with this result type. We include an `Uninit`
 * variant to signal that the engine returned without writing to the out pointer. Kernel should
 * always initialize such an out pointer to `Uninit` before handing it to an engine upcall.
 *
 * The variants are deliberately named `Success`/`Failure` rather than `Ok`/`Err` to avoid a
 * conflict with [`ExternResult`]. This is due to an issue in cbindgen, where generic types sharing
 * the same variant names causes failures during monomorphization (<https://github.com/mozilla/cbindgen/issues/1166>).
 */
typedef enum EngineExecResultArrowFFIData_Tag {
  SuccessArrowFFIData,
  FailureArrowFFIData,
  UninitArrowFFIData,
} EngineExecResultArrowFFIData_Tag;

typedef struct EngineExecResultArrowFFIData {
  EngineExecResultArrowFFIData_Tag tag;
  union {
    struct {
      struct ArrowFFIData success;
    };
    struct {
      struct EngineExecError failure;
    };
  };
} EngineExecResultArrowFFIData;

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Engine callback for **row-time** evaluation of an opaque predicate.
 *
 * Kernel pre-evaluates the predicate's args into a `RecordBatch` (one field per arg) and passes it
 * by value as `args_in` (an `ArrowFFIData`); the engine owns it and must release both Arrow C Data
 * Interface handles on every path (including `Failure`/`Uninit`). The batch's columns line up with
 * the predicate's args by position:
 * - a `Column` arg arrives as its evaluated values
 * - a `Literal` arg as the constant repeated for every row
 * - a `Predicate` arg as a `BooleanArray` of its verdicts
 *
 * The engine returns one bool per row.
 *
 * The result uses the out-pointer convention: kernel pre-initializes `*out` to
 * `EngineExecResult::Uninit`. On success the engine writes `EngineExecResult::Success` holding the
 * result `BooleanArray` as Arrow C Data Interface structs, transferring their ownership to kernel.
 * On failure it writes `EngineExecResult::Failure` carrying a `KernelError` code and a message
 * handle (built via `allocate_kernel_string`); leaving `*out` as `Uninit` is also treated as an
 * error. When `inverted`, evaluate `NOT op`.
 *
 * # Safety
 * `out` is valid only for the duration of the call; the engine must not retain it. The callback
 * must not panic or unwind across the FFI boundary.
 */
typedef void (*EngineEvalRowsFn)(void *engine_state,
                                 struct KernelStringSlice op_name,
                                 struct ArrowFFIData args_in,
                                 bool inverted,
                                 struct EngineExecResultArrowFFIData *out);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Engine callback for **stats-based** evaluation of an opaque predicate, for file data skipping.
 *
 * Like [`EngineEvalRowsFn`], but each `Column` arg arrives as a per-file 4-field struct indexed by
 * position: `0`=min, `1`=max (column type; null if not collected), `2`=nullcount, `3`=rowcount
 * (`Int64`; null if not collected). `Literal`/`Predicate` args are unchanged. The engine returns
 * one bool per file: `false` = skip, `true`/`null` = keep.
 *
 * Stats are *conservative*: `min`/`max` only bound the values and `nullcount`/`rowcount` may
 * overcount, so skip a file only when the predicate *cannot* hold for any value in `[min, max]`.
 * Keep (`true`/`null`) whenever the stats can't prove the predicate impossible -- in particular
 * on null/absent bounds, where skipping silently drops live files from the scan. Log-replay
 * soundness does not rest on the engine, though: kernel wraps the rewritten predicate with an
 * `OR(NOT is_add, ...)` guard, so Remove rows (which carry null stats) are kept regardless of
 * the verdict and a misbehaving callback cannot resurrect deleted files.
 * When `inverted`, evaluate `NOT op` -- not `!verdict`; if you can't reason soundly about the
 * negated op, keep every file. Result/out-pointer/panic contract matches [`EngineEvalRowsFn`].
 */
typedef void (*EngineEvalStatsFn)(void *engine_state,
                                  struct KernelStringSlice op_name,
                                  struct ArrowFFIData args_in,
                                  bool inverted,
                                  struct EngineExecResultArrowFFIData *out);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Destructor for the engine's state pointer.
 *
 * Must not panic or unwind across the FFI boundary.
 */
typedef void (*EngineFreeStateFn)(void *engine_state);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Bundle of engine callbacks for opaque-predicate evaluation, passed by value to
 * [`visit_predicate_opaque_with_eval`]. Ownership of `engine_state` transfers to kernel with the
 * call: `free_state` is invoked exactly once when the predicate built from it (including any
 * data-skipping clones kernel derives) is dropped. Engines attaching the same logical state to
 * multiple opaque ops must pass independently freeable state per call.
 *
 * # Thread safety
 *
 * Kernel may invoke the eval callbacks -- and ultimately `free_state` -- from any thread,
 * potentially concurrently: the predicate is shared across whatever threads drive the scan, and
 * the last reference may drop on any of them. The engine must hand over only an `engine_state`
 * and callbacks that tolerate this; any synchronization is the engine's responsibility.
 *
 * [`visit_predicate_opaque_with_eval`]: crate::expressions::kernel_visitor::visit_predicate_opaque_with_eval
 */
typedef struct COpaqueEvalCallbacks {
  /**
   * Opaque engine state; passed back as the first argument to each
   * callback.
   */
  void *engine_state;
  /**
   * Row-time evaluation: one verdict per data row.
   */
  EngineEvalRowsFn eval_pred_rows;
  /**
   * Stats-based evaluation for file data skipping: one verdict per file.
   */
  EngineEvalStatsFn eval_pred_stats;
  /**
   * Destructor for `engine_state`. Called exactly once; may run on any
   * kernel thread.
   */
  EngineFreeStateFn free_state;
} COpaqueEvalCallbacks;
#endif

/**
 * An `Event` can generally be thought of a "log message". It contains all the relevant bits such
 * that an engine can generate a log message in its format
 */
typedef struct Event {
  /**
   * The log message associated with the event
   */
  struct KernelStringSlice message;
  /**
   * Level that the event was emitted at
   */
  enum Level level;
  /**
   * A string that specifies in what part of the system the event occurred
   */
  struct KernelStringSlice target;
  /**
   * source file line number where the event occurred, or 0 (zero) if unknown
   */
  uint32_t line;
  /**
   * file where the event occurred. If unknown the slice `ptr` will be null and the len will be
   * 0
   */
  struct KernelStringSlice file;
} Event;

typedef void (*TracingEventFn)(struct Event event);

typedef void (*TracingLogLineFn)(struct KernelStringSlice line);

/**
 * The 16 raw bytes of an operation's UUID, used to correlate events from the same operation.
 */
typedef struct MetricId {
  uint8_t bytes[16];
} MetricId;

/**
 * A log segment was loaded.
 */
typedef struct LogSegmentLoadSuccess {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  uint64_t duration_ns;
  uint64_t num_commit_files;
  uint64_t num_checkpoint_files;
  uint64_t num_compaction_files;
  bool has_latest_crc_file;
} LogSegmentLoadSuccess;

/**
 * Listing the log segment failed.
 */
typedef struct LogSegmentLoadFailure {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
} LogSegmentLoadFailure;

/**
 * Protocol and metadata actions were read from the log.
 */
typedef struct ProtocolMetadataLoadSuccess {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  uint64_t duration_ns;
} ProtocolMetadataLoadSuccess;

/**
 * Reading protocol and metadata from the log failed.
 */
typedef struct ProtocolMetadataLoadFailure {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
} ProtocolMetadataLoadFailure;

/**
 * A snapshot was built successfully.
 */
typedef struct SnapshotBuildSuccess {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  uint64_t version;
  uint64_t duration_ns;
} SnapshotBuildSuccess;

/**
 * Building a snapshot failed.
 */
typedef struct SnapshotBuildFailure {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
} SnapshotBuildFailure;

/**
 * A transaction was committed successfully. `operation` and `correlation_id` are slices into
 * engine-supplied data that are only valid for the duration of the callback; an empty slice means
 * the value was unset.
 */
typedef struct TransactionCommitSuccess {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  uint64_t commit_version;
  uint64_t num_add_files;
  uint64_t num_remove_files;
  uint64_t num_dv_updates;
  uint64_t add_files_bytes;
  uint64_t remove_files_bytes;
  bool is_blind_append;
  bool data_change;
  struct KernelStringSlice operation;
  uint64_t prepare_duration_ns;
  uint64_t committer_duration_ns;
  uint64_t total_duration_ns;
} TransactionCommitSuccess;

/**
 * A transaction commit did not succeed; `reason` distinguishes conflict, retryable IO, and
 * terminal errors.
 */
typedef struct TransactionCommitFailure {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  enum CommitFailureReason reason;
} TransactionCommitFailure;

/**
 * A domain metadata load completed.
 */
typedef struct DomainMetadataLoadSuccess {
  bool from_cache;
  uint64_t num_domains_returned;
  uint64_t duration_ns;
} DomainMetadataLoadSuccess;

/**
 * A `SetTransaction` (app id) load completed.
 */
typedef struct SetTransactionLoadSuccess {
  bool from_cache;
  bool found;
  uint64_t duration_ns;
} SetTransactionLoadSuccess;

/**
 * A CRC file was read and parsed successfully.
 */
typedef struct CrcReadSuccess {
  uint64_t bytes_read;
  uint64_t duration_ns;
} CrcReadSuccess;

/**
 * A `JsonHandler::read_json_files` call completed.
 */
typedef struct JsonReadCompleted {
  uint64_t num_files;
  uint64_t bytes_read;
} JsonReadCompleted;

/**
 * A `ParquetHandler::read_parquet_files` call completed.
 */
typedef struct ParquetReadCompleted {
  uint64_t num_files;
  uint64_t bytes_read;
} ParquetReadCompleted;

/**
 * A scan metadata operation completed. See [`delta_kernel::metrics::ScanMetadataCompleted`] for
 * field semantics.
 */
typedef struct ScanMetadataCompleted {
  struct MetricId operation_id;
  struct KernelStringSlice correlation_id;
  enum TableType table_type;
  enum ScanType scan_type;
  uint64_t duration_ns;
  uint64_t num_add_files_seen;
  uint64_t num_active_add_files;
  uint64_t active_add_files_bytes;
  uint64_t num_remove_files_seen;
  uint64_t num_non_file_actions;
  uint64_t num_predicate_filtered;
  uint64_t peak_hash_set_size;
  uint64_t dedup_visitor_time_ns;
  uint64_t predicate_eval_time_ns;
} ScanMetadataCompleted;

/**
 * A storage list operation completed.
 */
typedef struct StorageListCompleted {
  uint64_t duration_ns;
  uint64_t num_files;
} StorageListCompleted;

/**
 * A storage read operation completed.
 */
typedef struct StorageReadCompleted {
  uint64_t duration_ns;
  uint64_t num_files;
  uint64_t bytes_read;
} StorageReadCompleted;

/**
 * A storage copy or rename operation completed.
 */
typedef struct StorageCopyCompleted {
  uint64_t duration_ns;
} StorageCopyCompleted;

/**
 * C version of [`delta_kernel::metrics::MetricEvent`]. Passed by value to a callback registered
 * via [`crate::ffi_tracing::enable_metrics_reporting`].
 *
 */
typedef enum MetricEvent_Tag {
  MetricEventLogSegmentLoadSuccess,
  MetricEventLogSegmentLoadFailure,
  MetricEventProtocolMetadataLoadSuccess,
  MetricEventProtocolMetadataLoadFailure,
  MetricEventSnapshotBuildSuccess,
  MetricEventSnapshotBuildFailure,
  MetricEventTransactionCommitSuccess,
  MetricEventTransactionCommitFailure,
  MetricEventDomainMetadataLoadSuccess,
  MetricEventDomainMetadataLoadFailure,
  MetricEventSetTransactionLoadSuccess,
  MetricEventSetTransactionLoadFailure,
  MetricEventCrcReadSuccess,
  MetricEventCrcReadFailure,
  MetricEventJsonReadCompleted,
  MetricEventParquetReadCompleted,
  MetricEventScanMetadataCompleted,
  MetricEventStorageListCompleted,
  MetricEventStorageReadCompleted,
  MetricEventStorageCopyCompleted,
} MetricEvent_Tag;

typedef struct MetricEvent {
  MetricEvent_Tag tag;
  union {
    struct {
      struct LogSegmentLoadSuccess log_segment_load_success;
    };
    struct {
      struct LogSegmentLoadFailure log_segment_load_failure;
    };
    struct {
      struct ProtocolMetadataLoadSuccess protocol_metadata_load_success;
    };
    struct {
      struct ProtocolMetadataLoadFailure protocol_metadata_load_failure;
    };
    struct {
      struct SnapshotBuildSuccess snapshot_build_success;
    };
    struct {
      struct SnapshotBuildFailure snapshot_build_failure;
    };
    struct {
      struct TransactionCommitSuccess transaction_commit_success;
    };
    struct {
      struct TransactionCommitFailure transaction_commit_failure;
    };
    struct {
      struct DomainMetadataLoadSuccess domain_metadata_load_success;
    };
    struct {
      struct SetTransactionLoadSuccess set_transaction_load_success;
    };
    struct {
      struct CrcReadSuccess crc_read_success;
    };
    struct {
      struct JsonReadCompleted json_read_completed;
    };
    struct {
      struct ParquetReadCompleted parquet_read_completed;
    };
    struct {
      struct ScanMetadataCompleted scan_metadata_completed;
    };
    struct {
      struct StorageListCompleted storage_list_completed;
    };
    struct {
      struct StorageReadCompleted storage_read_completed;
    };
    struct {
      struct StorageCopyCompleted storage_copy_completed;
    };
  };
} MetricEvent;

/**
 * Callback an engine registers via [`enable_metrics_reporting`] to receive kernel
 * [`MetricEvent`]s. This is invoked synchronously on the thread that emitted the event. Note that
 * the `event`, and any [`KernelStringSlice`] it carries, are only valid for the duration of the
 * call.
 */
typedef void (*MetricsEventFn)(struct MetricEvent event);

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedPlanExecutor *HandleSharedPlanExecutor;

/**
 * Generic wrapper around an EngineExecError, representing the result of an engine upcall.
 *
 * Typically, engines will populate an out pointer with this result type. We include an `Uninit`
 * variant to signal that the engine returned without writing to the out pointer. Kernel should
 * always initialize such an out pointer to `Uninit` before handing it to an engine upcall.
 *
 * The variants are deliberately named `Success`/`Failure` rather than `Ok`/`Err` to avoid a
 * conflict with [`ExternResult`]. This is due to an issue in cbindgen, where generic types sharing
 * the same variant names causes failures during monomorphization (<https://github.com/mozilla/cbindgen/issues/1166>).
 */
typedef enum EngineExecResultHandleExclusiveEngineData_Tag {
  SuccessHandleExclusiveEngineData,
  FailureHandleExclusiveEngineData,
  UninitHandleExclusiveEngineData,
} EngineExecResultHandleExclusiveEngineData_Tag;

typedef struct EngineExecResultHandleExclusiveEngineData {
  EngineExecResultHandleExclusiveEngineData_Tag tag;
  union {
    struct {
      HandleExclusiveEngineData success;
    };
    struct {
      struct EngineExecError failure;
    };
  };
} EngineExecResultHandleExclusiveEngineData;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueEngineExecResultHandleExclusiveEngineData_Tag {
  SomeEngineExecResultHandleExclusiveEngineData,
  NoneEngineExecResultHandleExclusiveEngineData,
} OptionalValueEngineExecResultHandleExclusiveEngineData_Tag;

typedef struct OptionalValueEngineExecResultHandleExclusiveEngineData {
  OptionalValueEngineExecResultHandleExclusiveEngineData_Tag tag;
  union {
    struct {
      struct EngineExecResultHandleExclusiveEngineData some;
    };
  };
} OptionalValueEngineExecResultHandleExclusiveEngineData;

/**
 * Function pointer an engine iterator uses to yield its next item into `out`.
 *
 * See "`next()` protocol" in the module docs.
 */
typedef void (*CIterNextFnHandleExclusiveEngineData)(NullableCvoid state,
                                                     struct OptionalValueEngineExecResultHandleExclusiveEngineData *out);

/**
 * Function pointer that releases an engine iterator's `state`. Invoked exactly once by kernel once
 * iteration is complete.
 */
typedef void (*CIterFreeFn)(NullableCvoid state);

/**
 * An engine-owned iterator of [`EngineData`] batches.
 *
 * See module level docs for memory management and safety requirements.
 */
typedef struct CEngineDataIterator {
  NullableCvoid state;
  CIterNextFnHandleExclusiveEngineData next;
  CIterFreeFn free;
} CEngineDataIterator;

/**
 * Generic wrapper around an EngineExecError, representing the result of an engine upcall.
 *
 * Typically, engines will populate an out pointer with this result type. We include an `Uninit`
 * variant to signal that the engine returned without writing to the out pointer. Kernel should
 * always initialize such an out pointer to `Uninit` before handing it to an engine upcall.
 *
 * The variants are deliberately named `Success`/`Failure` rather than `Ok`/`Err` to avoid a
 * conflict with [`ExternResult`]. This is due to an issue in cbindgen, where generic types sharing
 * the same variant names causes failures during monomorphization (<https://github.com/mozilla/cbindgen/issues/1166>).
 */
typedef enum EngineExecResultFFI_ArrowArray_Tag {
  SuccessFFI_ArrowArray,
  FailureFFI_ArrowArray,
  UninitFFI_ArrowArray,
} EngineExecResultFFI_ArrowArray_Tag;

typedef struct EngineExecResultFFI_ArrowArray {
  EngineExecResultFFI_ArrowArray_Tag tag;
  union {
    struct {
      struct FFI_ArrowArray success;
    };
    struct {
      struct EngineExecError failure;
    };
  };
} EngineExecResultFFI_ArrowArray;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueEngineExecResultFFI_ArrowArray_Tag {
  SomeEngineExecResultFFI_ArrowArray,
  NoneEngineExecResultFFI_ArrowArray,
} OptionalValueEngineExecResultFFI_ArrowArray_Tag;

typedef struct OptionalValueEngineExecResultFFI_ArrowArray {
  OptionalValueEngineExecResultFFI_ArrowArray_Tag tag;
  union {
    struct {
      struct EngineExecResultFFI_ArrowArray some;
    };
  };
} OptionalValueEngineExecResultFFI_ArrowArray;

/**
 * Function pointer an engine iterator uses to yield its next item into `out`.
 *
 * See "`next()` protocol" in the module docs.
 */
typedef void (*CIterNextFnFFI_ArrowArray)(NullableCvoid state,
                                          struct OptionalValueEngineExecResultFFI_ArrowArray *out);

/**
 * An engine-owned iterator of [`FileMeta`](delta_kernel::FileMeta) batches.
 *
 * See module level docs for memory management and safety requirements.
 *
 * Similar to `CBytesIterator`, CFileMetaIterator uses `FFI_ArrowArray` as a convenient container
 * for passing FileMeta entries across the FFI boundary. Each `next` invocation MUST yield a batch
 * of one or more FileMeta rows as a `StructArray` whose fields are
 * `{location: Utf8, last_modified: Int64, size: UInt64}`, all non-null. Kernel assumes this fixed
 * schema when converting into FileMeta (the schema is NOT passed along through FFI).
 * Empty or otherwise invalid batches surface as errors and permanently terminate iteration
 * (kernel will not call `next` again).
 */
typedef struct CFileMetaIterator {
  NullableCvoid state;
  CIterNextFnFFI_ArrowArray next;
  CIterFreeFn free;
} CFileMetaIterator;

/**
 * An engine-owned iterator of byte buffers.
 *
 * See module level docs for memory management and safety requirements.
 *
 * Each byte array is transferred across the FFI boundary as an [`FFI_ArrowArray`].
 * Invocation of `next` MUST yield an Arrow array that is a `BinaryArray` containing a single
 * row of non-null bytes. Kernel assumes the schema is [`ArrowDataType::Binary`] (the schema is NOT
 * passed along through FFI).
 *
 * FFI_ArrowArray serves as a convenient container for receiving bytes because it internally
 * manages its own memory release callback.
 *
 * TODO: ArrowDataType::Binary uses i32 offsets, so each byte array is limited to 2 GiB. If this is
 * a problem, we need to support ArrowDataType::LargeBinary instead.
 */
typedef struct CBytesIterator {
  NullableCvoid state;
  CIterNextFnFFI_ArrowArray next;
  CIterFreeFn free;
} CBytesIterator;

/**
 * C-compatible equivalent of the kernel's `ParquetFooter` struct.
 *
 * The schema is delivered as a proto-serialized `delta.kernel.schema.StructType` message. The
 * engine should call [`allocate_kernel_bytes`](crate::allocate_kernel_bytes) to copy those bytes
 * into a kernel-owned [`ExclusiveRustBytes`] handle that can then be embedded into this struct.
 */
typedef struct CParquetFooter {
  HandleExclusiveRustBytes schema_proto;
} CParquetFooter;

/**
 * C-compatible equivalent of the kernel's `PlanResult` enum.
 *
 * We instruct cbindgen to prefix enum variants with enum name (e.g. `CPlanResult_Unit`)
 * so they don't collide with other identifiers (e.g. with the `FileMeta` struct)
 *
 */
typedef enum CPlanResult_Tag {
  CPlanResultUnit,
  CPlanResultData,
  CPlanResultFileMeta,
  CPlanResultBytes,
  CPlanResultParquetFooter,
} CPlanResult_Tag;

typedef struct CPlanResult {
  CPlanResult_Tag tag;
  union {
    struct {
      struct CEngineDataIterator data;
    };
    struct {
      struct CFileMetaIterator file_meta;
    };
    struct {
      struct CBytesIterator bytes;
    };
    struct {
      struct CParquetFooter parquet_footer;
    };
  };
} CPlanResult;

/**
 * Generic wrapper around an EngineExecError, representing the result of an engine upcall.
 *
 * Typically, engines will populate an out pointer with this result type. We include an `Uninit`
 * variant to signal that the engine returned without writing to the out pointer. Kernel should
 * always initialize such an out pointer to `Uninit` before handing it to an engine upcall.
 *
 * The variants are deliberately named `Success`/`Failure` rather than `Ok`/`Err` to avoid a
 * conflict with [`ExternResult`]. This is due to an issue in cbindgen, where generic types sharing
 * the same variant names causes failures during monomorphization (<https://github.com/mozilla/cbindgen/issues/1166>).
 */
typedef enum EngineExecResultCPlanResult_Tag {
  SuccessCPlanResult,
  FailureCPlanResult,
  UninitCPlanResult,
} EngineExecResultCPlanResult_Tag;

typedef struct EngineExecResultCPlanResult {
  EngineExecResultCPlanResult_Tag tag;
  union {
    struct {
      struct CPlanResult success;
    };
    struct {
      struct EngineExecError failure;
    };
  };
} EngineExecResultCPlanResult;

/**
 * C callback, provided by the Engine, for executing an [`Operation`].
 *
 * `context` - an opaque pointer, originally passed to
 * [`get_plan_executor`](super::get_plan_executor).
 * `plan_proto` - a byte slice containing the proto-serialized representation of an [`Operation`]
 * `out` - an out pointer into which the engine writes the result.
 *
 * Since the out result is written to caller (Kernel) provided memory, the kernel will also be
 * responsible for freeing it. Kernel will pre-initialize the out pointer to
 * [`EngineExecResult::Uninit`] before handing it to the engine upcall.
 */
typedef void (*CExecuteOpFn)(NullableCvoid context,
                             struct KernelBytesSlice plan_proto,
                             struct EngineExecResultCPlanResult *out);

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedScanMetadata *HandleSharedScanMetadata;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultKernelBoolSlice_Tag {
  OkKernelBoolSlice,
  ErrKernelBoolSlice,
} ExternResultKernelBoolSlice_Tag;

typedef struct ExternResultKernelBoolSlice {
  ExternResultKernelBoolSlice_Tag tag;
  union {
    struct {
      struct KernelBoolSlice ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultKernelBoolSlice;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedScan *HandleSharedScan;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedScan_Tag {
  OkHandleSharedScan,
  ErrHandleSharedScan,
} ExternResultHandleSharedScan_Tag;

typedef struct ExternResultHandleSharedScan {
  ExternResultHandleSharedScan_Tag tag;
  union {
    struct {
      HandleSharedScan ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedScan;

/**
 * An engine-provided schema along with a visitor function to convert it to a kernel schema.
 *
 * Used by [`scan`] and [`scan_builder_with_schema`] for projection pushdown, and by
 * [`get_create_table_builder`] to specify the table schema at creation time. The engine
 * provides a pointer to its native schema representation along with a visitor function. The
 * kernel allocates visitor state internally, which becomes the second argument to the schema
 * visitor invocation. Thanks to this double indirection, engine and kernel each retain
 * ownership of their respective objects with no need to coordinate memory lifetimes.
 *
 * [`get_create_table_builder`]: crate::transaction::get_create_table_builder
 */
typedef struct EngineSchema {
  void *schema;
  uintptr_t (*visitor)(void *schema, struct KernelSchemaVisitorState *state);
} EngineSchema;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveScanBuilder *HandleExclusiveScanBuilder;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveScanBuilder_Tag {
  OkHandleExclusiveScanBuilder,
  ErrHandleExclusiveScanBuilder,
} ExternResultHandleExclusiveScanBuilder_Tag;

typedef struct ExternResultHandleExclusiveScanBuilder {
  ExternResultHandleExclusiveScanBuilder_Tag tag;
  union {
    struct {
      HandleExclusiveScanBuilder ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveScanBuilder;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedScanMetadataIterator *HandleSharedScanMetadataIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedScanMetadataIterator_Tag {
  OkHandleSharedScanMetadataIterator,
  ErrHandleSharedScanMetadataIterator,
} ExternResultHandleSharedScanMetadataIterator_Tag;

typedef struct ExternResultHandleSharedScanMetadataIterator {
  ExternResultHandleSharedScanMetadataIterator_Tag tag;
  union {
    struct {
      HandleSharedScanMetadataIterator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedScanMetadataIterator;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueHandleSharedExpression_Tag {
  SomeHandleSharedExpression,
  NoneHandleSharedExpression,
} OptionalValueHandleSharedExpression_Tag;

typedef struct OptionalValueHandleSharedExpression {
  OptionalValueHandleSharedExpression_Tag tag;
  union {
    struct {
      HandleSharedExpression some;
    };
  };
} OptionalValueHandleSharedExpression;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultKernelRowIndexArray_Tag {
  OkKernelRowIndexArray,
  ErrKernelRowIndexArray,
} ExternResultKernelRowIndexArray_Tag;

typedef struct ExternResultKernelRowIndexArray {
  ExternResultKernelRowIndexArray_Tag tag;
  union {
    struct {
      struct KernelRowIndexArray ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultKernelRowIndexArray;

/**
 * Give engines an easy way to consume stats
 */
typedef struct Stats {
  /**
   * For any file where the deletion vector is not present (see [`DvInfo::has_vector`]), the
   * `num_records` statistic must be present and accurate, and must equal the number of records
   * in the data file. In the presence of Deletion Vectors the statistics may be somewhat
   * outdated, i.e. not reflecting deleted rows yet.
   */
  uint64_t num_records;
} Stats;

/**
 * Contains information that can be used to get a selection vector. If `has_vector` is false, that
 * indicates there is no selection vector to consider. It is always possible to get a vector out of
 * a `DvInfo`, but if `has_vector` is false it will just be an empty vector (indicating all
 * selected). Without this there's no way for a connector using ffi to know if a &DvInfo actually
 * has a vector in it. We have has_vector() on the rust side, but this isn't exposed via ffi. So
 * this just wraps the &DvInfo in another struct which includes a boolean that says if there is a
 * dv to consider or not.  This allows engines to ignore dv info if there isn't any without needing
 * to make another ffi call at all.
 */
typedef struct CDvInfo {
  const struct DvInfo *info;
  bool has_vector;
} CDvInfo;

/**
 * This callback will be invoked for each valid file that needs to be read for a scan.
 *
 * The arguments to the callback are:
 * * `context`: a `void*` context this can be anything that engine needs to pass through to each
 *   call
 * * `path`: a `KernelStringSlice` which is the path to the file
 * * `size`: an `i64` which is the size of the file
 * * `mod_time`: an `i64` which is the time the file was created, as milliseconds since the epoch
 * * `dv_info`: a [`CDvInfo`] struct, which allows getting the selection vector for this file
 * * `transform`: An optional expression that, if not `NULL`, _must_ be applied to physical data to
 *   convert it to the correct logical format. If this is `NULL`, no transform is needed.
 * * `partition_values`: [DEPRECATED] a `HashMap<String, String>` which are partition values
 */
typedef void (*CScanCallback)(NullableCvoid engine_context,
                              struct KernelStringSlice path,
                              int64_t size,
                              int64_t mod_time,
                              const struct Stats *stats,
                              const struct CDvInfo *dv_info,
                              const struct Expression *transform,
                              const struct CStringMap *partition_map);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Result of [`scan_metadata_next_arrow`]: an Arrow C Data Interface batch, a selection
 * vector, and per-row transformation expressions.
 *
 * The engine must free this by calling [`free_scan_metadata_arrow_result`] exactly once.
 */
typedef struct ScanMetadataArrowResult {
  /**
   * Arrow C Data Interface batch containing scan file metadata (path, size, stats, etc.).
   */
  struct ArrowFFIData arrow_data;
  /**
   * Boolean selection vector indicating active rows. Length equals the batch row count;
   * `true` at index `i` means row `i` should be processed.
   */
  struct KernelBoolSlice selection_vector;
  /**
   * Opaque pointer to per-row transformation expressions. Use [`get_transform_for_row`]
   * with a row index to retrieve the transform for that row. If non-null, the transform
   * must be applied to data read from the file to produce the correct logical schema.
   * Owned by this struct and freed by [`free_scan_metadata_arrow_result`].
   */
  struct CTransforms *transforms;
} ScanMetadataArrowResult;
#endif

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultScanMetadataArrowResult_Tag {
  OkScanMetadataArrowResult,
  ErrScanMetadataArrowResult,
} ExternResultScanMetadataArrowResult_Tag;

typedef struct ExternResultScanMetadataArrowResult {
  ExternResultScanMetadataArrowResult_Tag tag;
  union {
    struct {
      struct ScanMetadataArrowResult *ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultScanMetadataArrowResult;

/**
 * The `EngineSchemaVisitor` defines a visitor system to allow engines to build their own
 * representation of a schema from a particular schema within kernel.
 *
 * The model is list based. When the kernel needs a list, it will ask engine to allocate one of a
 * particular size. Once allocated the engine returns an `id`, which can be any integer identifier
 * ([`usize`]) the engine wants, and will be passed back to the engine to identify the list in the
 * future.
 *
 * Every schema element the kernel visits belongs to some list of "sibling" elements. The schema
 * itself is a list of schema elements, and every complex type (struct, map, array) contains a list
 * of "child" elements.
 *  1. Before visiting schema or any complex type, the kernel asks the engine to allocate a list to
 *     hold its children
 *  2. When visiting any schema element, the kernel passes its parent's "child list" as the
 *     "sibling list" the element should be appended to:
 *      - For the top-level schema, visit each top-level column, passing the column's name and type
 *      - For a struct, first visit each struct field, passing the field's name, type, nullability,
 *        and metadata
 *      - For a map, visit the key and value, passing its special name ("map_key" or "map_value"),
 *        type, and value nullability (keys are never nullable)
 *      - For a list, visit the element, passing its special name ("array_element"), type, and
 *        nullability
 *  3. When visiting a complex schema element, the kernel also passes the "child list" containing
 *     that element's (already-visited) children.
 *  4. The [`visit_schema`] method returns the id of the list of top-level columns
 */
typedef struct EngineSchemaVisitor {
  /**
   * opaque state pointer
   */
  void *data;
  /**
   * Creates a new field list, optionally reserving capacity up front
   */
  uintptr_t (*make_field_list)(void *data, uintptr_t reserve);
  /**
   * Indicate that the schema contains a `Struct` type. The top level of a Schema is always a
   * `Struct`. The fields of the `Struct` are in the list identified by `child_list_id`.
   */
  void (*visit_struct)(void *data,
                       uintptr_t sibling_list_id,
                       struct KernelStringSlice name,
                       bool is_nullable,
                       const struct CStringMap *metadata,
                       uintptr_t child_list_id);
  /**
   * Indicate that the schema contains an Array type. `child_list_id` will be a _one_ item list
   * with the array's element type
   */
  void (*visit_array)(void *data,
                      uintptr_t sibling_list_id,
                      struct KernelStringSlice name,
                      bool is_nullable,
                      const struct CStringMap *metadata,
                      uintptr_t child_list_id);
  /**
   * Indicate that the schema contains an Map type. `child_list_id` will be a _two_ item list
   * where the first element is the map's key type and the second element is the
   * map's value type
   */
  void (*visit_map)(void *data,
                    uintptr_t sibling_list_id,
                    struct KernelStringSlice name,
                    bool is_nullable,
                    const struct CStringMap *metadata,
                    uintptr_t child_list_id);
  /**
   * visit a `decimal` with the specified `precision` and `scale`
   */
  void (*visit_decimal)(void *data,
                        uintptr_t sibling_list_id,
                        struct KernelStringSlice name,
                        bool is_nullable,
                        const struct CStringMap *metadata,
                        uint8_t precision,
                        uint8_t scale);
  /**
   * Visit a `string` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_string)(void *data,
                       uintptr_t sibling_list_id,
                       struct KernelStringSlice name,
                       bool is_nullable,
                       const struct CStringMap *metadata);
  /**
   * Visit a `long` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_long)(void *data,
                     uintptr_t sibling_list_id,
                     struct KernelStringSlice name,
                     bool is_nullable,
                     const struct CStringMap *metadata);
  /**
   * Visit an `integer` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_integer)(void *data,
                        uintptr_t sibling_list_id,
                        struct KernelStringSlice name,
                        bool is_nullable,
                        const struct CStringMap *metadata);
  /**
   * Visit a `short` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_short)(void *data,
                      uintptr_t sibling_list_id,
                      struct KernelStringSlice name,
                      bool is_nullable,
                      const struct CStringMap *metadata);
  /**
   * Visit a `byte` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_byte)(void *data,
                     uintptr_t sibling_list_id,
                     struct KernelStringSlice name,
                     bool is_nullable,
                     const struct CStringMap *metadata);
  /**
   * Visit a `float` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_float)(void *data,
                      uintptr_t sibling_list_id,
                      struct KernelStringSlice name,
                      bool is_nullable,
                      const struct CStringMap *metadata);
  /**
   * Visit a `double` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_double)(void *data,
                       uintptr_t sibling_list_id,
                       struct KernelStringSlice name,
                       bool is_nullable,
                       const struct CStringMap *metadata);
  /**
   * Visit a `boolean` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_boolean)(void *data,
                        uintptr_t sibling_list_id,
                        struct KernelStringSlice name,
                        bool is_nullable,
                        const struct CStringMap *metadata);
  /**
   * Visit `binary` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_binary)(void *data,
                       uintptr_t sibling_list_id,
                       struct KernelStringSlice name,
                       bool is_nullable,
                       const struct CStringMap *metadata);
  /**
   * Visit a `date` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_date)(void *data,
                     uintptr_t sibling_list_id,
                     struct KernelStringSlice name,
                     bool is_nullable,
                     const struct CStringMap *metadata);
  /**
   * Visit a `timestamp` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_timestamp)(void *data,
                          uintptr_t sibling_list_id,
                          struct KernelStringSlice name,
                          bool is_nullable,
                          const struct CStringMap *metadata);
  /**
   * Visit a `timestamp` with no timezone belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_timestamp_ntz)(void *data,
                              uintptr_t sibling_list_id,
                              struct KernelStringSlice name,
                              bool is_nullable,
                              const struct CStringMap *metadata);
  /**
   * Visit a `void` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_void)(void *data,
                     uintptr_t sibling_list_id,
                     struct KernelStringSlice name,
                     bool is_nullable,
                     const struct CStringMap *metadata);
  /**
   * Visit a `variant` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_variant)(void *data,
                        uintptr_t sibling_list_id,
                        struct KernelStringSlice name,
                        bool is_nullable,
                        const struct CStringMap *metadata);
} EngineSchemaVisitor;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveTransaction *HandleExclusiveTransaction;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveTransaction_Tag {
  OkHandleExclusiveTransaction,
  ErrHandleExclusiveTransaction,
} ExternResultHandleExclusiveTransaction_Tag;

typedef struct ExternResultHandleExclusiveTransaction {
  ExternResultHandleExclusiveTransaction_Tag tag;
  union {
    struct {
      HandleExclusiveTransaction ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveTransaction;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveCommittedTransaction *HandleExclusiveCommittedTransaction;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveCommittedTransaction_Tag {
  OkHandleExclusiveCommittedTransaction,
  ErrHandleExclusiveCommittedTransaction,
} ExternResultHandleExclusiveCommittedTransaction_Tag;

typedef struct ExternResultHandleExclusiveCommittedTransaction {
  ExternResultHandleExclusiveCommittedTransaction_Tag tag;
  union {
    struct {
      HandleExclusiveCommittedTransaction ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveCommittedTransaction;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveCreateTransaction *HandleExclusiveCreateTransaction;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveCreateTransaction_Tag {
  OkHandleExclusiveCreateTransaction,
  ErrHandleExclusiveCreateTransaction,
} ExternResultHandleExclusiveCreateTransaction_Tag;

typedef struct ExternResultHandleExclusiveCreateTransaction {
  ExternResultHandleExclusiveCreateTransaction_Tag tag;
  union {
    struct {
      HandleExclusiveCreateTransaction ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveCreateTransaction;

/**
 * FFI-safe implementation for Rust's `Option<T>`
 */
typedef enum OptionalValueHandleSharedSnapshot_Tag {
  SomeHandleSharedSnapshot,
  NoneHandleSharedSnapshot,
} OptionalValueHandleSharedSnapshot_Tag;

typedef struct OptionalValueHandleSharedSnapshot {
  OptionalValueHandleSharedSnapshot_Tag tag;
  union {
    struct {
      HandleSharedSnapshot some;
    };
  };
} OptionalValueHandleSharedSnapshot;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveCreateTableBuilder *HandleExclusiveCreateTableBuilder;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveCreateTableBuilder_Tag {
  OkHandleExclusiveCreateTableBuilder,
  ErrHandleExclusiveCreateTableBuilder,
} ExternResultHandleExclusiveCreateTableBuilder_Tag;

typedef struct ExternResultHandleExclusiveCreateTableBuilder {
  ExternResultHandleExclusiveCreateTableBuilder_Tag tag;
  union {
    struct {
      HandleExclusiveCreateTableBuilder ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveCreateTableBuilder;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveDvDescriptorMap *HandleExclusiveDvDescriptorMap;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusiveDvDescriptor *HandleExclusiveDvDescriptor;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleExclusiveDvDescriptor_Tag {
  OkHandleExclusiveDvDescriptor,
  ErrHandleExclusiveDvDescriptor,
} ExternResultHandleExclusiveDvDescriptor_Tag;

typedef struct ExternResultHandleExclusiveDvDescriptor {
  ExternResultHandleExclusiveDvDescriptor_Tag tag;
  union {
    struct {
      HandleExclusiveDvDescriptor ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleExclusiveDvDescriptor;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct ExclusivePartitionValueMap *HandleExclusivePartitionValueMap;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultOptionalValuei64_Tag {
  OkOptionalValuei64,
  ErrOptionalValuei64,
} ExternResultOptionalValuei64_Tag;

typedef struct ExternResultOptionalValuei64 {
  ExternResultOptionalValuei64_Tag tag;
  union {
    struct {
      struct OptionalValuei64 ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultOptionalValuei64;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 *   must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 *   shared handle must always be [`Send`]+[`Sync`].
 *
 * * Sized vs. unsized. Sized types allow handle operations to be implemented more efficiently.
 *
 * # Validity
 *
 * A `Handle` is _valid_ if all of the following hold:
 *
 * * It was created by a call to [`Handle::from`]
 * * Not yet dropped by a call to [`Handle::drop_handle`]
 * * Not yet consumed by a call to [`Handle::into_inner`]
 *
 * Additionally, in keeping with the [`Send`] contract, multi-threaded external code must
 * enforce mutual exclusion -- no mutable handle should ever be passed to more than one kernel
 * API call at a time. If thread races are possible, the handle should be protected with a
 * mutex. Due to Rust [reference rules], this requirement applies even for API calls that
 * appear to be read-only (because Rust code always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 * [reference rules]:
 * https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references
 */
typedef struct SharedWriteContext *HandleSharedWriteContext;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedWriteContext_Tag {
  OkHandleSharedWriteContext,
  ErrHandleSharedWriteContext,
} ExternResultHandleSharedWriteContext_Tag;

typedef struct ExternResultHandleSharedWriteContext {
  ExternResultHandleSharedWriteContext_Tag tag;
  union {
    struct {
      HandleSharedWriteContext ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedWriteContext;

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_HEADER_PREFIX_KEY[8];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_TLS_CERT_PATH_KEY[14];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_TLS_KEY_PATH_KEY[13];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_TLS_CA_PATH_KEY[12];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_TLS_DNS_OVERRIDE_KEY[17];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_TLS_TIMEOUT_SECS_KEY[17];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_RETRY_MAX_RETRIES_KEY[18];
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
extern const uint8_t REST_BUILDER_OPTION_PUT_VERIFY_ON_AMBIGUOUS_KEY[24];
#endif

/**
 * Allow engines to create an opaque pointer that Rust will understand as a String. Returns an
 * error if the slice contains invalid utf-8 data.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid KernelStringSlice
 */
struct ExternResultHandleExclusiveRustString allocate_kernel_string(struct KernelStringSlice kernel_str,
                                                                    AllocateErrorFn error_fn);

/**
 * Allow engines to create an opaque pointer to [`ExclusiveRustBytes`] by copying the provided
 * `bytes` into kernel-owned memory.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid [`KernelBytesSlice`] whose pointer references at least
 * `len` readable bytes. The slice only needs to remain valid until after this call returns.
 */
HandleExclusiveRustBytes allocate_kernel_bytes(struct KernelBytesSlice bytes);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_bool_slice(struct KernelBoolSlice slice);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_row_indexes(struct KernelRowIndexArray slice);

/**
 * Drop an `ExclusiveEngineData`.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle as engine_data
 */
void free_engine_data(HandleExclusiveEngineData engine_data);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get a "builder" that can be used to construct an engine. The function
 * [`set_builder_option`] can be used to set options on the builder prior to constructing the
 * actual engine
 *
 * # Safety
 * Caller is responsible for passing a valid path pointer.
 */
struct ExternResultEngineBuilder get_engine_builder(struct KernelStringSlice path,
                                                    AllocateErrorFn allocate_error);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Set an option on the builder
 *
 * # Safety
 *
 * Caller must pass a valid EngineBuilder pointer, and valid slices for key and value
 */
struct ExternResultbool set_builder_option(struct EngineBuilder *builder,
                                           struct KernelStringSlice key,
                                           struct KernelStringSlice value);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Configure the builder to use a multi-threaded executor instead of the default
 * single-threaded background executor.
 *
 * # Parameters
 * - `builder`: The engine builder to configure.
 * - `worker_threads`: Number of worker threads. Pass 0 to use Tokio's default.
 * - `max_blocking_threads`: Maximum number of blocking threads. Pass 0 to use Tokio's default.
 *
 * # Safety
 *
 * Caller must pass a valid EngineBuilder pointer.
 */
void set_builder_with_multithreaded_executor(struct EngineBuilder *builder,
                                             uintptr_t worker_threads,
                                             uintptr_t max_blocking_threads);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Configure read-path I/O concurrency for the engine's JSON and Parquet handlers.
 *
 * These control the `read_*_files` paths used during log replay and scans. Returned data ordering
 * is preserved regardless of these values.
 *
 * # Parameters
 * - `builder`: The engine builder to configure.
 * - `buffer_size`: Maximum number of files read concurrently (file-level readahead depth). Higher
 *   values overlap more object-store requests to hide latency, at the cost of more in-flight
 *   memory. Pass 0 to use the engine default.
 * - `batch_size`: Maximum number of rows per yielded batch. Pass 0 to use the engine default.
 *   Overall read memory usage is roughly proportional to `buffer_size * batch_size`.
 *
 * # Safety
 *
 * Caller must pass a valid EngineBuilder pointer.
 */
void set_builder_with_io_concurrency(struct EngineBuilder *builder,
                                     uintptr_t buffer_size,
                                     uintptr_t batch_size);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Select a REST-backed object store. See [`rest_engine`] for setup, option keys, and callbacks.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer and a non-null `endpoint_config`. When `callback` is
 * non-null, `context` must remain valid for the engine lifetime and the callback must be safe to
 * invoke from any thread concurrently (see [`rest_engine::CAuthHeaderCallback`]).
 */
struct ExternResultbool set_builder_rest_object_store(struct EngineBuilder *builder,
                                                      const struct CRestEndpointConfig *endpoint_config,
                                                      struct OptionCAuthHeaderCallback callback,
                                                      NullableCvoid context);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Consume the builder and return a `default` engine. After calling, the passed pointer is _no
 * longer valid_. Note that this _consumes_ and frees the builder, so there is no need to
 * drop/free it afterwards.
 *
 *
 * # Safety
 *
 * Caller is responsible to pass a valid EngineBuilder pointer, and to not use it again afterwards
 */
struct ExternResultHandleSharedExternEngine builder_build(struct EngineBuilder *builder);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * # Safety
 *
 * Caller is responsible for passing a valid path pointer.
 */
struct ExternResultHandleSharedExternEngine get_default_engine(struct KernelStringSlice path,
                                                               AllocateErrorFn allocate_error);
#endif

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_engine(HandleSharedExternEngine engine);

/**
 * Get a builder for creating a [`SharedSnapshot`] from a table path.
 *
 * Use [`snapshot_builder_set_version`] to pin a specific version, then call
 * [`snapshot_builder_build`] to obtain the snapshot. The caller owns the returned handle and must
 * eventually call either [`snapshot_builder_build`] to produce a [`SharedSnapshot`], or
 * [`free_snapshot_builder`] to drop it without building.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid path and engine handle.
 */
struct ExternResultHandleMutableFfiSnapshotBuilder get_snapshot_builder(struct KernelStringSlice path,
                                                                        HandleSharedExternEngine engine);

/**
 * Get a builder for incrementally updating an existing snapshot.
 *
 * This avoids re-reading the full log. Use [`snapshot_builder_set_version`] to target a specific
 * version, then call [`snapshot_builder_build`] to obtain the updated snapshot. The caller owns
 * the returned handle and must eventually call either [`snapshot_builder_build`] to produce a
 * [`SharedSnapshot`], or [`free_snapshot_builder`] to drop it without building.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles.
 */
struct ExternResultHandleMutableFfiSnapshotBuilder get_snapshot_builder_from(HandleSharedSnapshot prev_snapshot,
                                                                             HandleSharedExternEngine engine);

/**
 * Set the target version on a snapshot builder. When omitted, the snapshot is created at the
 * latest version of the table.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer.
 */
void snapshot_builder_set_version(HandleMutableFfiSnapshotBuilder *builder, Version version);

/**
 * Set the log tail on a snapshot builder for catalog-managed tables.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer. The log_tail array and its contents must remain valid
 * for the duration of this call.
 */
struct ExternResultbool snapshot_builder_set_log_tail(HandleMutableFfiSnapshotBuilder *builder,
                                                      struct LogPathArray log_tail);

/**
 * Set the max catalog version on a snapshot builder for catalog-managed tables. This bounds the
 * snapshot version to what the catalog has ratified.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer.
 */
void snapshot_builder_set_max_catalog_version(HandleMutableFfiSnapshotBuilder *builder,
                                              Version max_catalog_version);

/**
 * Consume the builder and return a snapshot. After calling, the builder pointer is _no longer
 * valid_. The builder is always freed by this call, whether or not it succeeds.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer and must not use it again after this call.
 */
struct ExternResultHandleSharedSnapshot snapshot_builder_build(HandleMutableFfiSnapshotBuilder builder);

/**
 * Free a snapshot builder without building a snapshot (e.g. on an error path).
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer and must not use it again after this call.
 */
void free_snapshot_builder(HandleMutableFfiSnapshotBuilder builder);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_snapshot(HandleSharedSnapshot snapshot);

/**
 * Perform a checkpoint write on the given snapshot. Mirrors the kernel's
 * [`delta_kernel::snapshot::Snapshot::checkpoint`] API across the C ABI.
 *
 * Pass `Option<&FfiCheckpointSpec>::None` to let the kernel auto-pick V1 or V2 based on the
 * table's protocol features and emit an inline checkpoint with no sidecars. Pass `Some(&spec)`
 * to force a specific shape; see [`FfiCheckpointSpec`] for the available options.
 *
 * Returns [`FfiCheckpointWriteResult::Written`] with the post-checkpoint snapshot (whose log
 * segment records the new checkpoint), or [`FfiCheckpointWriteResult::AlreadyExists`] with the
 * original snapshot when a checkpoint at this version was already present. In both branches the
 * caller owns the returned snapshot handle and must release it via [`free_snapshot`].
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles. The `spec` pointer, if non-null, must point
 * to a valid `FfiCheckpointSpec` for the duration of the call.
 */
struct ExternResultFfiCheckpointWriteResult checkpoint_snapshot(HandleSharedSnapshot snapshot,
                                                                HandleSharedExternEngine engine,
                                                                const struct FfiCheckpointSpec *spec);

/**
 * Get the version of the specified snapshot
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
uint64_t version(HandleSharedSnapshot snapshot);

/**
 * Get the timestamp of the specified snapshot in milliseconds since the Unix epoch.
 *
 * When In-Commit Timestamp (ICT) is enabled, returns the ICT value from the commit's
 * `CommitInfo` action. Otherwise, falls back to the filesystem last-modified time of
 * the latest commit file.
 *
 * Returns an error if the commit file is missing, the ICT configuration is invalid, or the
 * ICT value cannot be read.
 *
 * # Safety
 *
 * Caller is responsible for passing valid snapshot handle and engine handle.
 */
struct ExternResulti64 snapshot_timestamp(HandleSharedSnapshot snapshot,
                                          HandleSharedExternEngine engine);

/**
 * Get the earliest commit version available in the table's `_delta_log/` directory.
 *
 * # Parameters
 * - `engine`: engine handle used to list the log directory.
 * - `log_root`: URL of the table's `_delta_log/` directory (must end with `/`).
 * - `earliest_ratified_commit_version`: for catalog-managed tables, the earliest version the
 *   catalog has ratified a commit at; pass `OptionalValue::None` for filesystem-only tables.
 * - `commit_type`: selects the query. [`FfiHistoryCommitType::Published`] returns the earliest
 *   commit; [`FfiHistoryCommitType::Recreatable`] returns the earliest fully reconstructable
 *   version.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid `log_root` string slice and a valid engine handle.
 */
struct ExternResultVersion get_earliest_commit(HandleSharedExternEngine engine,
                                               struct KernelStringSlice log_root,
                                               struct OptionalValueVersion earliest_ratified_commit_version,
                                               enum FfiHistoryCommitType commit_type);

/**
 * Get the latest commit (version and timestamp) within `snapshot`'s version range with a
 * timestamp at or before `timestamp` (milliseconds since the Unix epoch). `commit_type` selects
 * whether to return any version present in the log ([`FfiHistoryCommitType::Published`]) or only a
 * version whose table can be fully reconstructed ([`FfiHistoryCommitType::Recreatable`]).
 *
 * Returns an error if no version exists at or before `timestamp`,
 * or when there is an error resolving the commit based on the [`FfiHistoryCommitType`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle and a valid engine handle.
 */
struct ExternResultFfiCommitAt latest_version_as_of(HandleSharedSnapshot snapshot,
                                                    HandleSharedExternEngine engine,
                                                    int64_t timestamp,
                                                    enum FfiHistoryCommitType commit_type);

/**
 * Get the first commit (version and timestamp) within `snapshot`'s version range with a
 * timestamp at or after `timestamp` (milliseconds since the Unix epoch). `commit_type` selects
 * whether to return any version present in the log ([`FfiHistoryCommitType::Published`]) or only a
 * version whose table can be fully reconstructed ([`FfiHistoryCommitType::Recreatable`]).
 *
 * Returns an error if no version exists at or after `timestamp`,
 * or when there is an error resolving the commit based on the [`FfiHistoryCommitType`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle and a valid engine handle.
 */
struct ExternResultFfiCommitAt first_version_after(HandleSharedSnapshot snapshot,
                                                   HandleSharedExternEngine engine,
                                                   int64_t timestamp,
                                                   enum FfiHistoryCommitType commit_type);

/**
 * Get the logical schema of the specified snapshot
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle.
 */
HandleSharedSchema logical_schema(HandleSharedSnapshot snapshot);

/**
 * Free a schema
 *
 * # Safety
 * Engine is responsible for providing a valid schema handle.
 */
void free_schema(HandleSharedSchema schema);

/**
 * Get the resolved root of the table. This should be used in any future calls that require
 * constructing a path
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle.
 */
NullableCvoid snapshot_table_root(HandleSharedSnapshot snapshot, AllocateStringFn allocate_fn);

/**
 * Get a count of the number of partition columns for this snapshot
 *
 * # Safety
 * Caller is responsible for passing a valid snapshot handle
 */
uintptr_t get_partition_column_count(HandleSharedSnapshot snapshot);

/**
 * Get an iterator of the list of partition columns for this snapshot.
 *
 * # Safety
 * Caller is responsible for passing a valid snapshot handle.
 */
HandleStringSliceIterator get_partition_columns(HandleSharedSnapshot snapshot);

/**
 * Visit each metadata configuration (key/value pair) for the specified snapshot by invoking the
 * provided `visitor` callback once per entry.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle, a valid `engine_context` as an
 * opaque pointer passed to each `visitor` invocation, and a valid `visitor` function pointer.
 */
void visit_metadata_configuration(HandleSharedSnapshot snapshot,
                                  NullableCvoid engine_context,
                                  void (*visitor)(NullableCvoid engine_context,
                                                  struct KernelStringSlice key,
                                                  struct KernelStringSlice value));

/**
 * Get the protocol for this snapshot. The returned handle must be freed with [`free_protocol`].
 *
 * # Safety
 * Caller is responsible for providing a valid snapshot handle.
 */
HandleSharedProtocol snapshot_get_protocol(HandleSharedSnapshot snapshot);

/**
 * Free a protocol handle obtained from [`snapshot_get_protocol`].
 *
 * # Safety
 * Caller is responsible for providing a valid, non-freed protocol handle.
 */
void free_protocol(HandleSharedProtocol protocol);

/**
 * Visit all fields of the protocol in a single FFI call. The caller provides:
 * - `visit_versions`: called once with `(context, min_reader_version, min_writer_version)`
 * - `visit_feature`: called once per feature with `(context, is_reader, feature_name)`.
 *   `is_reader` is `true` for reader features, `false` for writer features. If the protocol uses
 *   legacy versioning (no explicit feature lists), the `visit_feature` callback will not fire.
 *
 * # Safety
 * Caller is responsible for providing a valid protocol handle, a valid `context` pointer, and
 * valid function pointers for `visit_versions` and `visit_feature`.
 */
void visit_protocol(HandleSharedProtocol protocol,
                    NullableCvoid context,
                    void (*visit_versions)(NullableCvoid context,
                                           int32_t min_reader,
                                           int32_t min_writer),
                    void (*visit_feature)(NullableCvoid context,
                                          bool is_reader,
                                          struct KernelStringSlice feature));

/**
 * Get the metadata for this snapshot. The returned handle must be freed with [`free_metadata`].
 *
 * # Safety
 * Caller is responsible for providing a valid snapshot handle.
 */
HandleSharedMetadata snapshot_get_metadata(HandleSharedSnapshot snapshot);

/**
 * Free a metadata handle obtained from [`snapshot_get_metadata`].
 *
 * # Safety
 * Caller is responsible for providing a valid, non-freed metadata handle.
 */
void free_metadata(HandleSharedMetadata metadata);

/**
 * Visit all fields of the metadata in a single FFI call. String fields are passed as
 * [`KernelStringSlice`] references that borrow from the metadata handle -- they are only valid
 * for the duration of the callback.
 *
 * The visitor receives:
 * - `id`: always present
 * - `name`: `OptionalValue::None` if not set
 * - `description`: `OptionalValue::None` if not set
 * - `format_provider`: always present
 * - `has_created_time`: whether `created_time_ms` is meaningful
 * - `created_time_ms`: milliseconds since epoch (only valid when `has_created_time` is true)
 *
 * # Safety
 * Caller is responsible for providing a valid metadata handle, a valid `context` pointer, and
 * a valid `visit_metadata_fields` function pointer. String slices must not be retained past
 * the callback return.
 */
void visit_metadata(HandleSharedMetadata metadata,
                    NullableCvoid context,
                    void (*visit_metadata_fields)(NullableCvoid context,
                                                  struct KernelStringSlice id,
                                                  struct OptionalValueKernelStringSlice name,
                                                  struct OptionalValueKernelStringSlice description,
                                                  struct KernelStringSlice format_provider,
                                                  bool has_created_time,
                                                  int64_t created_time_ms));

/**
 * # Safety
 *
 * The iterator must be valid (returned by [`scan_metadata_iter_init`]) and not yet freed by
 * [`free_scan_metadata_iter`]. The visitor function pointer must be non-null.
 *
 * [`scan_metadata_iter_init`]: crate::scan::scan_metadata_iter_init
 * [`free_scan_metadata_iter`]: crate::scan::free_scan_metadata_iter
 */
bool string_slice_next(HandleStringSliceIterator data,
                       NullableCvoid engine_context,
                       void (*engine_visitor)(NullableCvoid engine_context,
                                              struct KernelStringSlice slice));

/**
 * # Safety
 *
 * Caller is responsible for (at most once) passing a valid pointer to a [`StringSliceIterator`]
 */
void free_string_slice_data(HandleStringSliceIterator data);

/**
 * Get a builder for constructing a [`CommitRange`] from a table path, starting at
 * `start_version`.
 *
 * The caller owns the returned handle and must eventually call either
 * [`commit_range_builder_build`] to produce a [`CommitRange`], or [`free_commit_range_builder`]
 * to drop it without building.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid path and engine handle.
 */
struct ExternResultHandleMutableFfiCommitRangeBuilder commit_range_builder_for(struct KernelStringSlice path,
                                                                               Version start_version,
                                                                               HandleSharedExternEngine engine);

/**
 * Pin the end version of the range. When omitted, the range extends to the latest committed
 * version observed at build time.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer.
 */
void commit_range_builder_set_end_version(HandleMutableFfiCommitRangeBuilder *builder,
                                          Version end_version);

/**
 * Consume the builder and return a [`CommitRange`]. After calling, the builder pointer is no
 * longer valid. The builder is always freed by this call, whether or not it succeeds.
 *
 * Returns an error if the resolved version range is invalid (start > end), the listed commits
 * are non-contiguous, or the requested start version is not present.
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer and must not use it again after this call.
 */
struct ExternResultHandleSharedCommitRange commit_range_builder_build(HandleMutableFfiCommitRangeBuilder builder);

/**
 * Free a commit range builder without building a range (e.g. on an error path).
 *
 * # Safety
 *
 * Caller must pass a valid builder pointer and must not use it again after this call.
 */
void free_commit_range_builder(HandleMutableFfiCommitRangeBuilder builder);

/**
 * Free a [`CommitRange`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_commit_range(HandleSharedCommitRange commit_range);

/**
 * The commit version of this commit action.
 *
 * # Safety
 *
 * Caller must pass a valid commit action handle.
 */
uint64_t commit_action_version(HandleSharedCommitAction commit_action);

/**
 * The commit timestamp (milliseconds since epoch) of this commit action.
 *
 * # Safety
 *
 * Caller must pass a valid commit action handle.
 */
int64_t commit_action_timestamp(HandleSharedCommitAction commit_action);

/**
 * Get an iterator over this commit's action batches, projected to the read schema requested when
 * the iterator was created (the `actions` passed to [`commit_range_commits`]). Each batch is the
 * raw actions recorded in the commit JSON, with no column-mapping translation applied.
 *
 * The caller owns the returned iterator: drain it with [`read_result_next`] and release it with
 * [`free_read_result_iter`].
 *
 * # Safety
 *
 * Caller is responsible for passing valid commit action and engine handles.
 *
 * [`read_result_next`]: crate::engine_funcs::read_result_next
 * [`free_read_result_iter`]: crate::engine_funcs::free_read_result_iter
 */
struct ExternResultHandleExclusiveFileReadResultIterator commit_action_get_actions(HandleSharedCommitAction commit_action,
                                                                                   HandleSharedExternEngine engine);

/**
 * Free a [`CommitAction`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_commit_action(HandleSharedCommitAction commit_action);

/**
 * Get an iterator over the commits in `commit_range`, yielding one [`CommitAction`] per commit.
 * Use [`commit_range_commits_with_snapshot`] to seed the iterator from a start snapshot instead.
 *
 * - `engine`: performs the per-commit JSON reads and allocates errors.
 * - `actions` / `actions_len`: the action kinds to project into each commit's read schema.
 *
 * Returns an error if `actions` is empty or contains duplicate kinds. The caller owns the
 * returned iterator and must release it with [`free_commit_actions_iter`].
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles, and an `actions` pointer to `actions_len`
 * valid [`DeltaAction`] values.
 */
struct ExternResultHandleSharedCommitActionsIterator commit_range_commits(HandleSharedCommitRange commit_range,
                                                                          HandleSharedExternEngine engine,
                                                                          const enum DeltaAction *actions,
                                                                          uintptr_t actions_len);

/**
 * Like [`commit_range_commits`], but seeds the iterator's protocol/metadata from `start_snapshot`.
 *
 * The snapshot's version must match the range's anchor (the start version for ascending ranges,
 * the end version for descending ranges).
 *
 * Returns an error if `actions` is empty or contains duplicate kinds, or if `start_snapshot`
 * belongs to a different table, its version does not match the range anchor, or its table does
 * not support scanning. The caller owns the returned iterator and must release it with
 * [`free_commit_actions_iter`].
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles, and an `actions` pointer to `actions_len`
 * valid [`DeltaAction`] values.
 */
struct ExternResultHandleSharedCommitActionsIterator commit_range_commits_with_snapshot(HandleSharedCommitRange commit_range,
                                                                                        HandleSharedExternEngine engine,
                                                                                        HandleSharedSnapshot start_snapshot,
                                                                                        const enum DeltaAction *actions,
                                                                                        uintptr_t actions_len);

/**
 * Call `engine_visitor` with the next [`CommitAction`] in the range, returning `true` if an item
 * was yielded and `false` once the iterator is exhausted. The visitor receives a
 * [`SharedCommitAction`] must free via [`free_commit_action`]. Per-commit protocol validation
 * runs here, so a malformed commit surfaces as an error from this call.
 *
 * # Safety
 *
 * The iterator must be valid (returned by [`commit_range_commits`]) and not yet freed by
 * [`free_commit_actions_iter`]. The visitor function pointer must be non-null.
 */
struct ExternResultbool commit_range_commits_next(HandleSharedCommitActionsIterator data,
                                                  NullableCvoid engine_context,
                                                  void (*engine_visitor)(NullableCvoid engine_context,
                                                                         HandleSharedCommitAction commit_action));

/**
 * Free a commit-actions iterator.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid pointer.
 */
void free_commit_actions_iter(HandleSharedCommitActionsIterator iter);

/**
 * Get the domain metadata as an optional string allocated by `AllocatedStringFn` for a specific
 * domain in this snapshot
 *
 * # Safety
 *
 * Caller is responsible for passing in a valid handle
 */
struct ExternResultNullableCvoid get_domain_metadata(HandleSharedSnapshot snapshot,
                                                     struct KernelStringSlice domain,
                                                     HandleSharedExternEngine engine,
                                                     AllocateStringFn allocate_fn);

/**
 * Get the domain metadata as an optional string allocated by `AllocatedStringFn` for a specific
 * domain in this snapshot
 *
 * # Safety
 *
 * Caller is responsible for passing in a valid handle
 */
struct ExternResultbool visit_domain_metadata(HandleSharedSnapshot snapshot,
                                              HandleSharedExternEngine engine,
                                              NullableCvoid engine_context,
                                              void (*visitor)(NullableCvoid engine_context,
                                                              struct KernelStringSlice domain,
                                                              struct KernelStringSlice configuration));

/**
 * Get the number of rows in an engine data
 *
 * # Safety
 * `data_handle` must be a valid pointer to a kernel allocated `ExclusiveEngineData`
 */
uintptr_t engine_data_length(HandleExclusiveEngineData *data);

/**
 * Allow an engine to "unwrap" an [`ExclusiveEngineData`] into the raw pointer for the case it
 * wants to use its own engine data format
 *
 * # Safety
 *
 * `data_handle` must be a valid pointer to a kernel allocated `ExclusiveEngineData`. The Engine
 * must ensure the handle outlives the returned pointer.
 */
void *get_raw_engine_data(HandleExclusiveEngineData data);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get an [`ArrowFFIData`] to allow binding to the arrow [C Data
 * Interface](https://arrow.apache.org/docs/format/CDataInterface.html). This includes the data and
 * the schema. If this function returns an `Ok` variant the _engine_ must free the returned struct
 * via [`free_arrow_ffi_data`] exactly once.
 *
 * # Safety
 * data_handle must be a valid ExclusiveEngineData as read by the
 * [`delta_kernel_default_engine::DefaultEngine`] obtained from `get_default_engine`.
 */
struct ExternResultArrowFFIData get_raw_arrow_data(HandleExclusiveEngineData data,
                                                   HandleSharedExternEngine engine);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Free an [`ArrowFFIData`] pointer produced by a kernel FFI function (e.g.
 * [`get_raw_arrow_data`] or [`crate::table_changes::scan_table_changes_next`]).
 *
 * If the consumer has already imported the inner `FFI_ArrowArray` / `FFI_ArrowSchema` via a
 * foreign Arrow layer (e.g. arrow-glib's `garrow_record_batch_import`), that import has
 * moved ownership of the release callbacks out of the structs; dropping the `Box` here is
 * then a cheap no-op on the arrays. If the consumer has not imported them, the structs'
 * `Drop` impls will call their release callbacks so no memory is leaked.
 *
 * A null pointer is a no-op, matching the convention used by
 * [`crate::scan::free_scan_metadata_arrow_result`].
 *
 * # Safety
 *
 * `result` must be either null, or a pointer returned by a kernel FFI function that produces
 * `*mut ArrowFFIData`. Must be called at most once per non-null pointer.
 */
void free_arrow_ffi_data(struct ArrowFFIData *result);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Creates engine data from Arrow C Data Interface array and schema.
 *
 * Converts the provided Arrow C Data Interface array and schema into delta-kernel's internal
 * engine data format. Note that ownership of the array is transferred to the kernel, whereas the
 * ownership of the schema stays the engine's.
 *
 * # Safety
 * - `array` must be a valid FFI_ArrowArray
 * - `schema` must be a valid pointer to a FFI_ArrowSchema
 * - `engine` must be a valid Handle to a SharedExternEngine
 */
struct ExternResultHandleExclusiveEngineData get_engine_data(struct FFI_ArrowArray array,
                                                             const struct FFI_ArrowSchema *schema,
                                                             AllocateErrorFn allocate_error);
#endif

/**
 * Call the engine back with the next `EngineData` batch read by Parquet/Json handler. The
 * _engine_ "owns" the data that is passed into the `engine_visitor`, since it is allocated by the
 * `Engine` being used for log-replay. If the engine wants the kernel to free this data, it _must_
 * call [`free_engine_data`] on it.
 *
 * # Safety
 *
 * The iterator must be valid (returned by [`read_parquet_file`]) and not yet freed by
 * [`free_read_result_iter`]. The visitor function pointer must be non-null.
 *
 * [`free_engine_data`]: crate::free_engine_data
 */
struct ExternResultbool read_result_next(HandleExclusiveFileReadResultIterator data,
                                         NullableCvoid engine_context,
                                         void (*engine_visitor)(NullableCvoid engine_context,
                                                                HandleExclusiveEngineData engine_data));

/**
 * Free the memory from the passed read result iterator
 * # Safety
 *
 * Caller is responsible for (at most once) passing a valid pointer returned by a call to
 * [`read_parquet_file`].
 */
void free_read_result_iter(HandleExclusiveFileReadResultIterator data);

/**
 * Use the specified engine's [`delta_kernel::ParquetHandler`] to read the specified file.
 *
 * # Safety
 * Caller is responsible for calling with a valid `ExternEngineHandle` and `FileMeta`
 */
struct ExternResultHandleExclusiveFileReadResultIterator read_parquet_file(HandleSharedExternEngine engine,
                                                                           const struct FileMeta *file,
                                                                           HandleSharedSchema physical_schema);

/**
 * Creates a new expression evaluator as provided by the passed engines `EvaluationHandler`.
 *
 * # Safety
 * Caller is responsible for calling with a valid `Engine`, `Expression`, and `SharedSchema`s
 */
struct ExternResultHandleSharedExpressionEvaluator new_expression_evaluator(HandleSharedExternEngine engine,
                                                                            HandleSharedSchema input_schema,
                                                                            const struct Expression *expression,
                                                                            HandleSharedSchema output_type);

/**
 * Free an expression evaluator
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_expression_evaluator(HandleSharedExpressionEvaluator evaluator);

/**
 * Use the passed `evaluator` to evaluate its expression against the passed `batch` data.
 *
 * # Safety
 * Caller is responsible for calling with a valid `Engine`, `ExclusiveEngineData`, and `Evaluator`
 */
struct ExternResultHandleExclusiveEngineData evaluate_expression(HandleSharedExternEngine engine,
                                                                 HandleExclusiveEngineData *batch,
                                                                 HandleSharedExpressionEvaluator evaluator);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the table changes from the specified table at a specific version
 *
 * - `table_root`: url pointing at the table root (where `_delta_log` folder is located)
 * - `engine`: Implementation of `Engine` apis.
 * - `start_version`: The start version of the change data feed End version will be the newest
 *   table version.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles and path pointer.
 */
struct ExternResultHandleExclusiveTableChanges table_changes_from_version(struct KernelStringSlice path,
                                                                          HandleSharedExternEngine engine,
                                                                          Version start_version);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the table changes from the specified table between two versions
 *
 * - `table_root`: url pointing at the table root (where `_delta_log` folder is located)
 * - `engine`: Implementation of `Engine` apis.
 * - `start_version`: The start version of the change data feed
 * - `end_version`: The end version (inclusive) of the change data feed.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles and path pointer.
 */
struct ExternResultHandleExclusiveTableChanges table_changes_between_versions(struct KernelStringSlice path,
                                                                              HandleSharedExternEngine engine,
                                                                              Version start_version,
                                                                              Version end_version);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Drops table changes.
 *
 * # Safety
 * Caller is responsible for passing a valid table changes handle.
 */
void free_table_changes(HandleExclusiveTableChanges table_changes);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get schema from the specified TableChanges.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid table changes handle.
 */
HandleSharedSchema table_changes_schema(HandleExclusiveTableChanges table_changes);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get table root from the specified TableChanges.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid table changes handle.
 */
NullableCvoid table_changes_table_root(HandleExclusiveTableChanges table_changes,
                                       AllocateStringFn allocate_fn);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get start version from the specified TableChanges.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid table changes handle.
 */
uint64_t table_changes_start_version(HandleExclusiveTableChanges table_changes);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get end version from the specified TableChanges.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid table changes handle.
 */
uint64_t table_changes_end_version(HandleExclusiveTableChanges table_changes);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get a [`TableChangesScan`] over the table specified by the passed table changes.
 * It is the responsibility of the _engine_ to free this scan when complete by calling
 * [`free_table_changes_scan`]. Consumes TableChanges.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid table changes pointer, and engine pointer
 */
struct ExternResultHandleSharedTableChangesScan table_changes_scan(HandleExclusiveTableChanges table_changes,
                                                                   HandleSharedExternEngine engine,
                                                                   struct EnginePredicate *predicate);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Drops a table changes scan.
 *
 * # Safety
 * Caller is responsible for passing a valid scan handle.
 */
void free_table_changes_scan(HandleSharedTableChangesScan table_changes_scan);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the table root of a table changes scan.
 *
 * # Safety
 * Engine is responsible for providing a valid scan pointer and allocate_fn (for allocating the
 * string)
 */
NullableCvoid table_changes_scan_table_root(HandleSharedTableChangesScan table_changes_scan,
                                            AllocateStringFn allocate_fn);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the logical schema of the specified table changes scan.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle.
 */
HandleSharedSchema table_changes_scan_logical_schema(HandleSharedTableChangesScan table_changes_scan);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the physical schema of the specified table changes scan.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle.
 */
HandleSharedSchema table_changes_scan_physical_schema(HandleSharedTableChangesScan table_changes_scan);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get an iterator over the data needed to perform a table changes scan. This will return a
 * [`ScanTableChangesIterator`] which can be passed to [`scan_table_changes_next`] to get the
 * actual data in the iterator.
 *
 * # Safety
 *
 * Engine is responsible for passing a valid [`SharedExternEngine`] and [`SharedTableChangesScan`]
 */
struct ExternResultHandleSharedScanTableChangesIterator table_changes_scan_execute(HandleSharedTableChangesScan table_changes_scan,
                                                                                   HandleSharedExternEngine engine);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * # Safety
 *
 * Drops table changes iterator.
 * Caller is responsible for (at most once) passing a valid pointer returned by a call to
 * [`table_changes_scan_execute`].
 */
void free_scan_table_changes_iter(HandleSharedScanTableChangesIterator data);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get next batch of data from the table changes iterator.
 *
 * Returns `Ok(non-null)` with a heap-allocated [`ArrowFFIData`] containing the next batch,
 * `Ok(null)` when the iterator is exhausted, or `Err` on failure. A non-null pointer must
 * be freed by the engine via [`crate::engine_data::free_arrow_ffi_data`] exactly once.
 *
 * # Safety
 *
 * The iterator must be valid (returned by [`table_changes_scan_execute`]) and not yet freed
 * by [`free_scan_table_changes_iter`].
 */
struct ExternResultArrowFFIData scan_table_changes_next(HandleSharedScanTableChangesIterator data);
#endif

/**
 * Get a commit client that will call the passed callbacks when it wants to make a commit. The
 * context will be passed back to the callback when called.
 *
 * IMPORTANT: The pointer passed for the context MUST be thread-safe (i.e. be able to be sent
 * between threads safely) and MUST remain valid for as long as the client is used. It is valid to
 * pass NULL as the context.
 *
 * # Safety
 *
 *  Caller is responsible for passing a valid pointer for the callback and a valid context pointer
 */
HandleSharedFfiUCCommitClient get_uc_commit_client(NullableCvoid context, CCommit commit_callback);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_uc_commit_client(HandleSharedFfiUCCommitClient commit_client);

/**
 * Get a commit client that will call the passed callbacks when it wants to make a commit.
 *
 * # Safety
 *
 *  Caller is responsible for passing a valid pointer to a SharedFfiUCCommitClient, obtained via
 *  calling [`get_uc_commit_client`], a valid KernelStringSlice as the table_id, and a valid error
 *  function pointer.
 */
struct ExternResultHandleMutableCommitter get_uc_committer(HandleSharedFfiUCCommitClient commit_client,
                                                           struct KernelStringSlice table_id,
                                                           AllocateErrorFn error_fn);

/**
 * Free a committer obtained via get_uc_committer. Warning! Normally the value returned here will
 * be consumed when creating a transaction via [`crate::transaction::transaction_with_committer`]
 * and will NOT need to be freed.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle obtained via `get_uc_committer`
 */
void free_uc_committer(HandleMutableCommitter commit_client);

/**
 * Free the memory the passed SharedExpression
 *
 * # Safety
 * Engine is responsible for passing a valid SharedExpression
 */
void free_kernel_expression(HandleSharedExpression data);

/**
 * Free the memory the passed SharedPredicate
 *
 * # Safety
 * Engine is responsible for passing a valid SharedPredicate
 */
void free_kernel_predicate(HandleSharedPredicate data);

/**
 * Free the passed SharedOpaqueExpressionOp
 *
 * # Safety
 * Engine is responsible for passing a valid SharedOpaqueExpressionOp
 */
void free_kernel_opaque_expression_op(HandleSharedOpaqueExpressionOp data);

/**
 * Free the passed SharedOpaquePredicateOp
 *
 * # Safety
 * Engine is responsible for passing a valid SharedOpaquePredicateOp
 */
void free_kernel_opaque_predicate_op(HandleSharedOpaquePredicateOp data);

/**
 * Visits the name of a SharedOpaqueExpressionOp
 *
 * # Safety
 * Engine is responsible for passing a valid SharedOpaqueExpressionOp
 */
void visit_kernel_opaque_expression_op_name(HandleSharedOpaqueExpressionOp op,
                                            void *data,
                                            void (*visit)(void *data, struct KernelStringSlice name));

/**
 * Visits the name of a SharedOpaquePredicateOp
 *
 * # Safety
 * Engine is responsible for passing a valid SharedOpaquePredicateOp
 */
void visit_kernel_opaque_predicate_op_name(HandleSharedOpaquePredicateOp op,
                                           void *data,
                                           void (*visit)(void *data, struct KernelStringSlice name));

/**
 * Visit the expression of the passed [`SharedExpression`] Handle using the provided `visitor`.
 * See the documentation of [`EngineExpressionVisitor`] for a description of how this visitor
 * works.
 *
 * This method returns the id that the engine generated for the top level expression
 *
 * # Safety
 *
 * The caller must pass a valid SharedExpression Handle and expression visitor
 */
uintptr_t visit_expression(const HandleSharedExpression *expression,
                           struct EngineExpressionVisitor *visitor);

/**
 * Visit the expression of the passed [`Expression`] pointer using the provided `visitor`.  See the
 * documentation of [`EngineExpressionVisitor`] for a description of how this visitor works.
 *
 * This method returns the id that the engine generated for the top level expression
 *
 * # Safety
 *
 * The caller must pass a valid Expression pointer and expression visitor
 */
uintptr_t visit_expression_ref(const struct Expression *expression,
                               struct EngineExpressionVisitor *visitor);

/**
 * Visit the predicate of the passed [`SharedPredicate`] Handle using the provided `visitor`.
 * See the documentation of [`EngineExpressionVisitor`] for a description of how this visitor
 * works.
 *
 * This method returns the id that the engine generated for the top level predicate
 *
 * # Safety
 *
 * The caller must pass a valid SharedPredicate Handle and expression visitor
 */
uintptr_t visit_predicate(const HandleSharedPredicate *predicate,
                          struct EngineExpressionVisitor *visitor);

/**
 * Visit the predicate of the passed [`Predicate`] pointer using the provided `visitor`.  See the
 * documentation of [`EngineExpressionVisitor`] for a description of how this visitor works.
 *
 * This method returns the id that the engine generated for the top level predicate
 *
 * # Safety
 *
 * The caller must pass a valid Predicate pointer and expression visitor
 */
uintptr_t visit_predicate_ref(const struct Predicate *predicate,
                              struct EngineExpressionVisitor *visitor);

uintptr_t visit_predicate_and(struct KernelExpressionVisitorState *state,
                              struct EngineIterator *children);

uintptr_t visit_expression_plus(struct KernelExpressionVisitorState *state,
                                uintptr_t a,
                                uintptr_t b);

uintptr_t visit_expression_minus(struct KernelExpressionVisitorState *state,
                                 uintptr_t a,
                                 uintptr_t b);

uintptr_t visit_expression_multiply(struct KernelExpressionVisitorState *state,
                                    uintptr_t a,
                                    uintptr_t b);

uintptr_t visit_expression_divide(struct KernelExpressionVisitorState *state,
                                  uintptr_t a,
                                  uintptr_t b);

uintptr_t visit_predicate_lt(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_le(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_gt(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_ge(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_eq(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_ne(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_unknown(struct KernelExpressionVisitorState *state,
                                  struct KernelStringSlice name);

uintptr_t visit_expression_unknown(struct KernelExpressionVisitorState *state,
                                   struct KernelStringSlice name);

/**
 * # Safety
 * The string slice must be valid
 */
struct ExternResultusize visit_expression_column(struct KernelExpressionVisitorState *state,
                                                 struct KernelStringSlice name,
                                                 AllocateErrorFn allocate_error);

uintptr_t visit_predicate_not(struct KernelExpressionVisitorState *state, uintptr_t inner_pred);

uintptr_t visit_predicate_is_null(struct KernelExpressionVisitorState *state, uintptr_t inner_expr);

/**
 * # Safety
 * The string slice must be valid
 */
struct ExternResultusize visit_expression_literal_string(struct KernelExpressionVisitorState *state,
                                                         struct KernelStringSlice value,
                                                         AllocateErrorFn allocate_error);

uintptr_t visit_expression_literal_int(struct KernelExpressionVisitorState *state, int32_t value);

uintptr_t visit_expression_literal_long(struct KernelExpressionVisitorState *state, int64_t value);

uintptr_t visit_expression_literal_short(struct KernelExpressionVisitorState *state, int16_t value);

uintptr_t visit_expression_literal_byte(struct KernelExpressionVisitorState *state, int8_t value);

uintptr_t visit_expression_literal_float(struct KernelExpressionVisitorState *state, float value);

uintptr_t visit_expression_literal_double(struct KernelExpressionVisitorState *state, double value);

uintptr_t visit_expression_literal_bool(struct KernelExpressionVisitorState *state, bool value);

/**
 * visit a date literal expression 'value' (i32 representing days since unix epoch)
 */
uintptr_t visit_expression_literal_date(struct KernelExpressionVisitorState *state, int32_t value);

/**
 * visit a timestamp literal expression 'value' (i64 representing microseconds since unix epoch)
 */
uintptr_t visit_expression_literal_timestamp(struct KernelExpressionVisitorState *state,
                                             int64_t value);

/**
 * visit a timestamp_ntz literal expression 'value' (i64 representing microseconds since unix
 * epoch)
 */
uintptr_t visit_expression_literal_timestamp_ntz(struct KernelExpressionVisitorState *state,
                                                 int64_t value);

/**
 * visit a binary literal expression
 *
 * # Safety
 * The caller must ensure that `value` points to a valid array of at least `len` bytes.
 */
uintptr_t visit_expression_literal_binary(struct KernelExpressionVisitorState *state,
                                          const uint8_t *value,
                                          uintptr_t len);

/**
 * visit a decimal literal expression
 *
 * Returns an error if the precision/scale combination is invalid.
 */
struct ExternResultusize visit_expression_literal_decimal(struct KernelExpressionVisitorState *state,
                                                          uint64_t value_hi,
                                                          uint64_t value_lo,
                                                          uint8_t precision,
                                                          uint8_t scale,
                                                          AllocateErrorFn allocate_error);

/**
 * Visit a typed null literal expression.
 *
 * The `type_tag` identifies the data type using the `NullTypeTag` encoding. For decimal nulls
 * (`type_tag == 12`), `precision` and `scale` specify the decimal type parameters; for all
 * other types, callers should pass 0 for both.
 *
 * Returns an error if the type tag is unrecognized, if the tag is `NonPrimitive` (255), or
 * if the decimal precision/scale is invalid.
 */
struct ExternResultusize visit_expression_literal_null(struct KernelExpressionVisitorState *state,
                                                       uint8_t type_tag,
                                                       uint8_t precision,
                                                       uint8_t scale,
                                                       AllocateErrorFn allocate_error);

uintptr_t visit_predicate_distinct(struct KernelExpressionVisitorState *state,
                                   uintptr_t a,
                                   uintptr_t b);

uintptr_t visit_predicate_in(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_predicate_or(struct KernelExpressionVisitorState *state,
                             struct EngineIterator *children);

uintptr_t visit_expression_struct(struct KernelExpressionVisitorState *state,
                                  struct EngineIterator *children);

/**
 * Visit a MapToStruct expression. The `child_expr` is the map expression.
 */
uintptr_t visit_expression_map_to_struct(struct KernelExpressionVisitorState *state,
                                         uintptr_t child_expr);

/**
 * Convert an engine expression to a kernel expression using the visitor
 * pattern.
 *
 * # Safety
 *
 * Caller must ensure that `engine_expression` points to a valid
 * `EngineExpression` with a valid visitor function and expression pointer.
 */
struct ExternResultHandleSharedExpression visit_engine_expression(struct EngineExpression *engine_expression,
                                                                  AllocateErrorFn allocate_error);

/**
 * Convert an engine predicate to a kernel predicate using the visitor
 * pattern.
 *
 * # Safety
 *
 * Caller must ensure that `engine_predicate` points to a valid
 * `EnginePredicate` with a valid visitor function and predicate pointer.
 */
struct ExternResultHandleSharedPredicate visit_engine_predicate(struct EnginePredicate *engine_predicate,
                                                                AllocateErrorFn allocate_error);

/**
 * Build a placeholder for an engine-defined predicate that kernel cannot evaluate: a NULL
 * boolean literal, which abstains from all pruning (even under `NOT`) while sibling predicates
 * prune normally. `children` are drained and discarded.
 *
 * Use [`visit_predicate_opaque_with_eval`] to attach engine eval callbacks so the node itself
 * can participate in file pruning.
 *
 * Returns 0 if any child ID is invalid.
 *
 * # Safety
 * `name` must be valid UTF-8 for the duration of the call.
 */
struct ExternResultusize visit_predicate_opaque(struct KernelExpressionVisitorState *state,
                                                struct KernelStringSlice name,
                                                struct EngineIterator *children,
                                                AllocateErrorFn allocate_error);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Build an opaque predicate over `FfiOpaquePredicateOp(name, callbacks)` and
 * `children`, routed through the default engine's Arrow batch evaluator (via
 * `Predicate::arrow_opaque`). Kernel pre-evaluates each child arg recursively
 * via its standard `evaluate_expression`, exports the resulting columns as a
 * single `RecordBatch` over Arrow C Data Interface, and invokes the engine's
 * eval callback. Engine never walks the AST.
 *
 * `callbacks` is passed by value; ownership of its `engine_state` transfers to
 * kernel, which invokes `free_state` exactly once -- even when this call fails
 * or returns 0. Engines attaching the same logical state to multiple opaque
 * ops must pass independently freeable state per call.
 *
 * Returns 0 if any child ID is invalid.
 *
 * # Safety
 * `name` must be valid UTF-8 for the duration of the call; the function
 * pointers in `callbacks` must remain valid for the lifetime of the built
 * predicate.
 */
struct ExternResultusize visit_predicate_opaque_with_eval(struct KernelExpressionVisitorState *state,
                                                          struct KernelStringSlice name,
                                                          struct EngineIterator *children,
                                                          struct COpaqueEvalCallbacks callbacks,
                                                          AllocateErrorFn allocate_error);
#endif

/**
 * Enable getting called back for tracing (logging) events in the kernel. `max_level` specifies
 * that only events `<=` to the specified level should be reported.  More verbose Levels are
 * "greater than" less verbose ones. So Level::ERROR is the lowest, and Level::TRACE the highest.
 *
 * Calling `enable_event_tracing`, `enable_log_line_tracing`, or
 * `enable_formatted_log_line_tracing` again replaces the active logging layer and its level,
 * including switching between event-based and log-line formats.
 *
 * Returns `true` if the callback was setup successfully, `false` on failure.
 *
 * Event-based tracing gives an engine maximal flexibility in formatting event log
 * lines. Kernel can also format events for the engine. If this is desired call
 * [`enable_log_line_tracing`] instead of this method.
 *
 * # Safety
 * Caller must pass a valid function pointer for the callback
 */
bool enable_event_tracing(TracingEventFn callback, enum Level max_level);

/**
 * Enable getting called back with log lines in the kernel using default settings:
 * - FULL format
 * - include ansi color
 * - include timestamps
 * - include level
 * - include target
 *
 * `max_level` specifies that only logs `<=` to the specified level should be reported.  More
 * verbose Levels are "greater than" less verbose ones. So Level::ERROR is the lowest, and
 * Level::TRACE the highest.
 *
 * Log lines passed to the callback will already have a newline at the end.
 *
 * Calling `enable_event_tracing`, `enable_log_line_tracing`, or
 * `enable_formatted_log_line_tracing` again replaces the active logging layer and its level,
 * including switching between event-based and log-line formats.
 *
 * Returns `true` if the callback was setup successfully, `false` on failure.
 *
 * Log line based tracing is simple for an engine as it can just log the passed string, but does
 * not provide flexibility for an engine to format events. If the engine wants to use a specific
 * format for events it should call [`enable_event_tracing`] instead of this function.
 *
 * # Safety
 * Caller must pass a valid function pointer for the callback
 */
bool enable_log_line_tracing(TracingLogLineFn callback, enum Level max_level);

/**
 * Enable getting called back with log lines in the kernel. This variant allows specifying
 * formatting options for the log lines. See [`enable_log_line_tracing`] for general info on
 * getting called back for log lines.
 *
 * Calling `enable_event_tracing`, `enable_log_line_tracing`, or
 * `enable_formatted_log_line_tracing` again replaces the active logging layer and its level,
 * including switching between event-based and log-line formats.
 *
 * Returns `true` if the callback was setup successfully, `false` on failure.
 *
 * Options that can be set:
 * - `format`: see [`LogLineFormat`]
 * - `ansi`: should the formatter use ansi escapes for color
 * - `with_time`: should the formatter include a timestamp in the log message
 * - `with_level`: should the formatter include the level in the log message
 * - `with_target`: should the formatter include what part of the system the event occurred
 *
 * # Safety
 * Caller must pass a valid function pointer for the callback
 */
bool enable_formatted_log_line_tracing(TracingLogLineFn callback,
                                       enum Level max_level,
                                       enum LogLineFormat format,
                                       bool ansi,
                                       bool with_time,
                                       bool with_level,
                                       bool with_target);

/**
 * Enable getting called back with structured kernel metric events. `callback` receives a
 * [`MetricEvent`] each time kernel emits a report. (See the [`delta_kernel::metrics`] module).
 *
 * Calling this replaces any previously set callback.
 *
 * Returns `true` if reporting was enabled successfully, `false` on failure.
 *
 * # Safety
 * Caller must pass a valid function pointer for the callback.
 */
bool enable_metrics_reporting(MetricsEventFn callback);

/**
 * Build a [`PlanExecutor`] backed by an engine-provided C callback.
 *
 * # Safety
 * The `context` pointer MUST be thread-safe (Send + Sync) and MUST remain valid for as long as the
 * executor is used. It is valid to pass NULL as the context.
 */
HandleSharedPlanExecutor get_plan_executor(NullableCvoid context, CExecuteOpFn callback);

/**
 * Free a plan executor obtained from [`get_plan_executor`].
 *
 * Normally the handle is consumed by [`get_plan_based_engine`] and need not be explicitly freed by
 * the caller. Use this only when discarding the executor without wrapping it in PlanBasedEngine.
 *
 * # Safety
 *
 * Caller must pass a valid handle previously obtained from [`get_plan_executor`] and must not use
 * it again afterwards.
 */
void free_plan_executor(HandleSharedPlanExecutor executor);

/**
 * Construct a [`PlanBasedEngine`] from the given [`SharedPlanExecutor`].
 *
 * This method consumes the [`SharedPlanExecutor`] handle, which should be freed when the engine
 * is dropped via `free_engine` (caller responsibility).
 *
 * # Safety
 *
 * Caller must pass a valid [`SharedPlanExecutor`] handle obtained from [`get_plan_executor`] and
 * a valid [`AllocateErrorFn`].
 */
HandleSharedExternEngine get_plan_based_engine(HandleSharedPlanExecutor plan_executor,
                                               AllocateErrorFn allocate_error);

/**
 * Drop a `SharedScanMetadata`.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid scan data handle.
 */
void free_scan_metadata(HandleSharedScanMetadata scan_metadata);

/**
 * Get a selection vector out of a [`SharedScanMetadata`] struct
 *
 * # Safety
 * Engine is responsible for providing valid pointers for each argument
 */
struct ExternResultKernelBoolSlice selection_vector_from_scan_metadata(HandleSharedScanMetadata scan_metadata,
                                                                       HandleSharedExternEngine engine);

/**
 * Drops a scan.
 *
 * # Safety
 * Caller is responsible for passing a valid scan handle.
 */
void free_scan(HandleSharedScan scan);

/**
 * Get a [`Scan`] over the table specified by the passed snapshot. It is the responsibility of the
 * _engine_ to free this scan when complete by calling [`free_scan`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot pointer, and engine pointer
 */
struct ExternResultHandleSharedScan scan(HandleSharedSnapshot snapshot,
                                         HandleSharedExternEngine engine,
                                         struct EnginePredicate *predicate,
                                         const struct EngineSchema *schema);

/**
 * Create a [`ScanBuilder`] for the given snapshot.
 *
 * The caller owns the returned handle and must eventually call either
 * [`scan_builder_build`] to produce a [`SharedScan`], or [`free_scan_builder`] to drop it
 * without building.
 *
 * This function is infallible; constructing a [`ScanBuilder`] from a snapshot always succeeds.
 *
 * # Safety
 *
 * `snapshot` must be a valid [`SharedSnapshot`] handle.
 */
HandleExclusiveScanBuilder scan_builder(HandleSharedSnapshot snapshot);

/**
 * Apply a predicate to an [`ExclusiveScanBuilder`] for data skipping and row-level filtering.
 *
 * Consumes the `builder` handle and returns a new handle with the predicate applied. The
 * `builder` handle must not be used after this call. Returns an error if the engine's predicate
 * visitor fails to produce a valid predicate (i.e. returns an invalid expression ID). On error,
 * the builder is dropped.
 *
 * # Safety
 *
 * `builder` and `engine` must be valid handles. The `builder` handle must not be used after this
 * call. `predicate` must be a valid, non-null [`EnginePredicate`] whose `visitor` and `predicate`
 * fields are safe to call and read.
 */
struct ExternResultHandleExclusiveScanBuilder scan_builder_with_predicate(HandleExclusiveScanBuilder builder,
                                                                          HandleSharedExternEngine engine,
                                                                          struct EnginePredicate *predicate);

/**
 * Apply a column projection schema to an [`ExclusiveScanBuilder`].
 *
 * Consumes the `builder` handle and returns a new handle with the schema applied. The `builder`
 * handle must not be used after this call. Returns an error if the schema visitor produces an
 * invalid schema, such as a non-struct root or unconsumed field IDs. On error, the builder is
 * dropped.
 *
 * # Safety
 *
 * `builder` and `engine` must be valid handles. The `builder` handle must not be used after this
 * call. `schema` must be a valid, non-null [`EngineSchema`] whose `visitor` and `schema` fields
 * are safe to call and read.
 */
struct ExternResultHandleExclusiveScanBuilder scan_builder_with_schema(HandleExclusiveScanBuilder builder,
                                                                       HandleSharedExternEngine engine,
                                                                       const struct EngineSchema *schema);

/**
 * Consume an [`ExclusiveScanBuilder`] and produce a [`SharedScan`].
 *
 * The `builder` handle is consumed and must not be used afterward. On error, the builder is
 * dropped and an error is returned. It is the responsibility of the caller to free the returned
 * scan handle by calling [`free_scan`].
 *
 * # Safety
 *
 * `builder` and `engine` must be valid handles. The `builder` handle must not be used after
 * this call.
 */
struct ExternResultHandleSharedScan scan_builder_build(HandleExclusiveScanBuilder builder,
                                                       HandleSharedExternEngine engine);

/**
 * Free an [`ExclusiveScanBuilder`] without building a scan.
 *
 * Only call this if you will not call [`scan_builder_build`]. If you have already called
 * [`scan_builder_build`], the builder handle was consumed and this must not be called.
 *
 * # Safety
 *
 * `builder` must be a valid handle that has not been previously consumed or freed.
 */
void free_scan_builder(HandleExclusiveScanBuilder builder);

/**
 * Get the table root of a scan.
 *
 * # Safety
 * Engine is responsible for providing a valid scan pointer and allocate_fn (for allocating the
 * string)
 */
NullableCvoid scan_table_root(HandleSharedScan scan, AllocateStringFn allocate_fn);

/**
 * Get the logical (i.e. output) schema of a scan.
 *
 * # Safety
 * Engine is responsible for providing a valid `SharedScan` handle
 */
HandleSharedSchema scan_logical_schema(HandleSharedScan scan);

/**
 * Get the kernel view of the physical read schema that an engine should read from parquet file in
 * a scan
 *
 * # Safety
 * Engine is responsible for providing a valid `SharedScan` handle
 */
HandleSharedSchema scan_physical_schema(HandleSharedScan scan);

/**
 * Get an iterator over the data needed to perform a scan. This will return a
 * [`ScanMetadataIterator`] which can be passed to [`scan_metadata_next`] to get the
 * actual data in the iterator.
 *
 * # Safety
 *
 * Engine is responsible for passing a valid [`SharedExternEngine`] and [`SharedScan`]
 */
struct ExternResultHandleSharedScanMetadataIterator scan_metadata_iter_init(HandleSharedExternEngine engine,
                                                                            HandleSharedScan scan);

/**
 * Call the provided `engine_visitor` on the next scan metadata item. The visitor will be provided
 * with a [`SharedScanMetadata`], which contains the actual scan files and the associated selection
 * vector. It is the responsibility of the _engine_ to free the associated resources after use by
 * calling [`free_engine_data`] and [`free_bool_slice`] respectively.
 *
 * # Safety
 *
 * The iterator must be valid (returned by [scan_metadata_iter_init]) and not yet freed by
 * [`free_scan_metadata_iter`]. The visitor function pointer must be non-null.
 *
 * [`free_bool_slice`]: crate::free_bool_slice
 * [`free_engine_data`]: crate::free_engine_data
 */
struct ExternResultbool scan_metadata_next(HandleSharedScanMetadataIterator data,
                                           NullableCvoid engine_context,
                                           void (*engine_visitor)(NullableCvoid engine_context,
                                                                  HandleSharedScanMetadata scan_metadata));

/**
 * # Safety
 *
 * Caller is responsible for (at most once) passing a valid pointer returned by a call to
 * [`scan_metadata_iter_init`].
 */
void free_scan_metadata_iter(HandleSharedScanMetadataIterator data);

/**
 * allow probing into a CStringMap. If the specified key is in the map, kernel will call
 * allocate_fn with the value associated with the key and return the value returned from that
 * function. If the key is not in the map, this will return NULL
 *
 * # Safety
 *
 * The engine is responsible for providing a valid [`CStringMap`] pointer and [`KernelStringSlice`]
 */
struct ExternResultNullableCvoid get_from_string_map(const struct CStringMap *map,
                                                     struct KernelStringSlice key,
                                                     AllocateStringFn allocate_fn,
                                                     HandleSharedExternEngine engine);

/**
 * Visit all values in a CStringMap. The callback will be called once for each element of the map
 *
 * # Safety
 *
 * The engine is responsible for providing a valid [`CStringMap`] pointer and callback
 */
void visit_string_map(const struct CStringMap *map,
                      NullableCvoid engine_context,
                      void (*visitor)(NullableCvoid engine_context,
                                      struct KernelStringSlice key,
                                      struct KernelStringSlice value));

/**
 * Allow getting the transform for a particular row. If the requested row is outside the range of
 * the passed `CTransforms` returns `NULL`, otherwise returns the element at the index of the
 * specified row. See also [`CTransforms`] above.
 *
 * # Safety
 *
 * The engine is responsible for providing a valid [`CTransforms`] pointer, and for checking if the
 * return value is `NULL` or not.
 */
struct OptionalValueHandleSharedExpression get_transform_for_row(uintptr_t row,
                                                                 const struct CTransforms *transforms);

/**
 * Get a selection vector out of a [`DvInfo`] struct
 *
 * # Safety
 * Engine is responsible for providing valid pointers for each argument
 */
struct ExternResultKernelBoolSlice selection_vector_from_dv(const struct DvInfo *dv_info,
                                                            HandleSharedExternEngine engine,
                                                            struct KernelStringSlice root_url);

/**
 * Get a vector of row indexes out of a [`DvInfo`] struct
 *
 * # Safety
 * Engine is responsible for providing valid pointers for each argument
 */
struct ExternResultKernelRowIndexArray row_indexes_from_dv(const struct DvInfo *dv_info,
                                                           HandleSharedExternEngine engine,
                                                           struct KernelStringSlice root_url);

/**
 * Shim for ffi to call visit_scan_metadata. This will generally be called when iterating through
 * scan data which provides the [`SharedScanMetadata`] as each element in the iterator.
 *
 * # Safety
 * engine is responsible for passing a valid [`SharedScanMetadata`].
 */
struct ExternResultbool visit_scan_metadata(HandleSharedScanMetadata scan_metadata,
                                            HandleSharedExternEngine engine,
                                            NullableCvoid engine_context,
                                            CScanCallback callback);

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Get the next scan metadata batch as Arrow via the C Data Interface.
 *
 * Advances the iterator by one batch and returns a [`ScanMetadataArrowResult`] containing:
 * - An Arrow RecordBatch with scan row schema columns (path, size, modificationTime, stats,
 *   deletionVector, fileConstantValues)
 * - A boolean selection vector indicating active rows (true = selected)
 * - Per-row transformation expressions (use [`get_transform_for_row`] to access)
 *
 * Returns `Ok(non-null)` with the next batch, `Ok(null)` when the iterator is exhausted,
 * or `Err` if an error occurred during iteration.
 *
 * This is an alternative to the callback-based [`scan_metadata_next`] +
 * [`visit_scan_metadata`] path, avoiding per-row FFI overhead.
 *
 * # Safety
 *
 * `data` must be a valid [`SharedScanMetadataIterator`] handle.
 * `engine` must be a valid [`SharedExternEngine`] handle.
 */
struct ExternResultScanMetadataArrowResult scan_metadata_next_arrow(HandleSharedScanMetadataIterator data,
                                                                    HandleSharedExternEngine engine);
#endif

#if defined(DEFINE_DEFAULT_ENGINE_BASE)
/**
 * Free a [`ScanMetadataArrowResult`] returned by [`scan_metadata_next_arrow`].
 *
 * # Safety
 *
 * `result` must be a valid pointer returned by [`scan_metadata_next_arrow`], or null.
 * Must be called at most once per result.
 */
void free_scan_metadata_arrow_result(struct ScanMetadataArrowResult *result);
#endif

/**
 * Visit the given `schema` using the provided `visitor`. See the documentation of
 * [`EngineSchemaVisitor`] for a description of how this visitor works.
 *
 * This method returns the id of the list allocated to hold the top level schema columns.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid schema handle and schema visitor.
 */
uintptr_t visit_schema(HandleSharedSchema schema, struct EngineSchemaVisitor *visitor);

/**
 * Visit a string field. Strings can hold arbitrary UTF-8 text data.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_string(struct KernelSchemaVisitorState *state,
                                            struct KernelStringSlice name,
                                            bool nullable,
                                            AllocateErrorFn allocate_error);

/**
 * Visit a long field. Long fields store 64-bit signed integers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_long(struct KernelSchemaVisitorState *state,
                                          struct KernelStringSlice name,
                                          bool nullable,
                                          AllocateErrorFn allocate_error);

/**
 * Visit an integer field. Integer fields store 32-bit signed integers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_integer(struct KernelSchemaVisitorState *state,
                                             struct KernelStringSlice name,
                                             bool nullable,
                                             AllocateErrorFn allocate_error);

/**
 * Visit a short field. Short fields store 16-bit signed integers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_short(struct KernelSchemaVisitorState *state,
                                           struct KernelStringSlice name,
                                           bool nullable,
                                           AllocateErrorFn allocate_error);

/**
 * Visit a byte field. Byte fields store 8-bit signed integers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_byte(struct KernelSchemaVisitorState *state,
                                          struct KernelStringSlice name,
                                          bool nullable,
                                          AllocateErrorFn allocate_error);

/**
 * Visit a float field. Float fields store 32-bit floating point numbers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_float(struct KernelSchemaVisitorState *state,
                                           struct KernelStringSlice name,
                                           bool nullable,
                                           AllocateErrorFn allocate_error);

/**
 * Visit a double field. Double fields store 64-bit floating point numbers.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_double(struct KernelSchemaVisitorState *state,
                                            struct KernelStringSlice name,
                                            bool nullable,
                                            AllocateErrorFn allocate_error);

/**
 * Visit a boolean field. Boolean fields store true/false values.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_boolean(struct KernelSchemaVisitorState *state,
                                             struct KernelStringSlice name,
                                             bool nullable,
                                             AllocateErrorFn allocate_error);

/**
 * Visit a binary field. Binary fields store arbitrary byte arrays.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_binary(struct KernelSchemaVisitorState *state,
                                            struct KernelStringSlice name,
                                            bool nullable,
                                            AllocateErrorFn allocate_error);

/**
 * Visit a date field. Date fields store calendar dates without time information.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_date(struct KernelSchemaVisitorState *state,
                                          struct KernelStringSlice name,
                                          bool nullable,
                                          AllocateErrorFn allocate_error);

/**
 * Visit a timestamp field. Timestamp fields store date and time with microsecond precision in UTC.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_timestamp(struct KernelSchemaVisitorState *state,
                                               struct KernelStringSlice name,
                                               bool nullable,
                                               AllocateErrorFn allocate_error);

/**
 * Visit a timestamp_ntz field. Similar to timestamp but without timezone information.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_timestamp_ntz(struct KernelSchemaVisitorState *state,
                                                   struct KernelStringSlice name,
                                                   bool nullable,
                                                   AllocateErrorFn allocate_error);

/**
 * Visit a decimal field. Decimal fields store fixed-precision decimal numbers with specified
 * precision and scale.
 *
 * # Safety
 *
 * Caller is responsible for providing a valid `state`, `name` slice with valid UTF-8 data,
 * and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_decimal(struct KernelSchemaVisitorState *state,
                                             struct KernelStringSlice name,
                                             uint8_t precision,
                                             uint8_t scale,
                                             bool nullable,
                                             AllocateErrorFn allocate_error);

/**
 * Visit a struct field. Struct fields contain nested fields organized as ordered key-value pairs.
 *
 * Note: This creates a named struct field (e.g. `address: struct<street, city>`). This function
 * should _also_ be used to create the final schema element, where the field IDs of the top-level
 * fields should be passed as `field_ids`. The name for the final schema element is ignored.
 *
 * The `field_ids` array must contain IDs from previous `visit_field_*` field creation calls.
 *
 * # Safety
 *
 * Caller is responsible for providing valid `state`, `name` slice, `field_ids` array pointing
 * to valid field IDs previously returned by this visitor, and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_struct(struct KernelSchemaVisitorState *state,
                                            struct KernelStringSlice name,
                                            const uintptr_t *field_ids,
                                            uintptr_t field_count,
                                            bool nullable,
                                            AllocateErrorFn allocate_error);

/**
 * Visit an array field. Array fields store ordered sequences of elements of the same type.
 *
 * The `element_type_id` must reference a field created by a previous `visit_field_*`. Elements of
 * the array can be null if and only if the field referenced by `element_type_id` is nullable.
 *
 * # Safety
 *
 * Caller is responsible for providing valid `state`, `name` slice, `element_type_id` from
 * previous `visit_data_type_*` call, and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_array(struct KernelSchemaVisitorState *state,
                                           struct KernelStringSlice name,
                                           uintptr_t element_type_id,
                                           bool nullable,
                                           AllocateErrorFn allocate_error);

/**
 * Visit a map field. Map fields store key-value pairs where all keys have the same type and all
 * values have the same type.
 *
 * Both `key_type_id` and `value_type_id` must reference fields created by previous `visit_field_*`
 * calls. The map can contain null values if and only if the field referenced by `value_type_id` is
 * nullable.
 *
 * # Safety
 *
 * Caller is responsible for providing valid `state`, `name` slice, `key_type_id` and
 * `value_type_id` from previous `visit_data_type_*` calls, and `allocate_error` function pointer.
 */
struct ExternResultusize visit_field_map(struct KernelSchemaVisitorState *state,
                                         struct KernelStringSlice name,
                                         uintptr_t key_type_id,
                                         uintptr_t value_type_id,
                                         bool nullable,
                                         AllocateErrorFn allocate_error);

/**
 * Visit a variant field.
 *
 * Takes a struct type ID that defines the variant schema. This must reference a field created by
 * previous `visit_field_struct` call.
 *
 * # Safety
 *
 * Caller must ensure:
 * - All base parameters are valid as per visit_field_string
 * - `variant_struct_id` is a valid struct type ID from a previous visitor call
 */
struct ExternResultusize visit_field_variant(struct KernelSchemaVisitorState *state,
                                             struct KernelStringSlice name,
                                             uintptr_t variant_struct_id,
                                             bool nullable,
                                             AllocateErrorFn allocate_error);

/**
 * Constructs a kernel expression that is passed back as a [`SharedExpression`] handle. The
 * expected output expression can be found in `ffi/tests/test_expression_visitor/expected.txt`.
 *
 * # Safety
 * The caller is responsible for freeing the returned memory, either by calling
 * [`crate::expressions::free_kernel_expression`], or [`crate::handle::Handle::drop_handle`].
 */
HandleSharedExpression get_testing_kernel_expression(void);

/**
 * Constructs a kernel predicate that is passed back as a [`SharedPredicate`] handle. The expected
 * output predicate can be found in `ffi/tests/test_predicate_visitor/expected.txt`.
 *
 * # Safety
 * The caller is responsible for freeing the returned memory, either by calling
 * [`crate::expressions::free_kernel_predicate`], or [`crate::handle::Handle::drop_handle`].
 */
HandleSharedPredicate get_testing_kernel_predicate(void);

/**
 * Constructs a simple kernel expression using only primitive types for round-trip testing.
 * This expression only uses types that have full visitor support.
 *
 * # Safety
 * The caller is responsible for freeing the returned memory.
 */
HandleSharedExpression get_simple_testing_kernel_expression(void);

/**
 * Constructs a simple kernel predicate using only primitive types for round-trip testing.
 * This predicate only uses types that have full visitor support.
 *
 * # Safety
 * The caller is responsible for freeing the returned memory.
 */
HandleSharedPredicate get_simple_testing_kernel_predicate(void);

/**
 * Compare two kernel expressions for equality. Returns true if they are
 * structurally equal, false otherwise.
 *
 * # Safety
 * Both expr1 and expr2 must be valid SharedExpression handles.
 */
bool expressions_are_equal(const HandleSharedExpression *expr1,
                           const HandleSharedExpression *expr2);

/**
 * Compare two kernel predicates for equality. Returns true if they are
 * structurally equal, false otherwise.
 *
 * # Safety
 * Both pred1 and pred2 must be valid SharedPredicate handles.
 */
bool predicates_are_equal(const HandleSharedPredicate *pred1, const HandleSharedPredicate *pred2);

/**
 * Start a transaction on the latest snapshot of the table.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles and path pointer.
 */
struct ExternResultHandleExclusiveTransaction transaction(struct KernelStringSlice path,
                                                          HandleSharedExternEngine engine);

/**
 * Start a transaction with a custom committer
 * NOTE: This consumes the committer handle
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles
 */
struct ExternResultHandleExclusiveTransaction transaction_with_committer(HandleSharedSnapshot snapshot,
                                                                         HandleSharedExternEngine engine,
                                                                         HandleMutableCommitter committer);

/**
 * Free an existing-table transaction handle without committing.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_transaction(HandleExclusiveTransaction txn);

/**
 * Attaches engine info to an existing-table transaction.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. CONSUMES the transaction handle.
 */
struct ExternResultHandleExclusiveTransaction with_engine_info(HandleExclusiveTransaction txn,
                                                               struct KernelStringSlice engine_info,
                                                               HandleSharedExternEngine engine);

/**
 * Set the operation that this transaction is performing. This string will be persisted in the
 * commit and visible to anyone who describes the table history. This CONSUMES the transaction
 * handle and returns a new handle for the updated transaction.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. CONSUMES the transaction handle.
 */
struct ExternResultHandleExclusiveTransaction with_operation(HandleExclusiveTransaction txn,
                                                             struct KernelStringSlice operation,
                                                             HandleSharedExternEngine engine);

/**
 * Add domain metadata to the transaction. The domain metadata will be written to the Delta log
 * as a `domainMetadata` action when the transaction is committed.
 *
 * `domain` identifies the metadata domain (e.g. `"myApp"`). `configuration` is an arbitrary
 * string value associated with the domain (typically JSON).
 *
 * Each domain can only appear once per transaction. Setting metadata for multiple distinct
 * domains is allowed. Duplicate domains or setting and removing the same domain in a single
 * transaction will cause the commit to fail.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles. CONSUMES the transaction handle and returns
 * a new one.
 */
struct ExternResultHandleExclusiveTransaction with_domain_metadata(HandleExclusiveTransaction txn,
                                                                   struct KernelStringSlice domain,
                                                                   struct KernelStringSlice configuration,
                                                                   HandleSharedExternEngine engine);

/**
 * Remove domain metadata from the table in this transaction. A tombstone action with
 * `removed: true` will be written to the Delta log when the transaction is committed.
 *
 * The caller does not need to provide a configuration value -- the existing value is
 * automatically preserved in the tombstone.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles. CONSUMES the transaction handle and returns
 * a new one.
 */
struct ExternResultHandleExclusiveTransaction with_domain_metadata_removed(HandleExclusiveTransaction txn,
                                                                           struct KernelStringSlice domain,
                                                                           HandleSharedExternEngine engine);

/**
 * Add file metadata to the transaction for files that have been written. The metadata contains
 * information about files written during the transaction that will be added to the Delta log
 * during commit.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. Consumes write_metadata.
 */
void add_files(HandleExclusiveTransaction txn, HandleExclusiveEngineData write_metadata);

/**
 * Mark the transaction as having data changes or not (these are recorded at the file level).
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void set_data_change(HandleExclusiveTransaction txn, bool data_change);

/**
 * Attempt to commit a transaction to the table. On success, returns a handle to the
 * [`CommittedTransaction`] from which the caller can read the version and the optional
 * post-commit snapshot. The returned handle must be freed with [`free_committed_transaction`].
 *
 * Returns an error if the commit fails. The FFI surfaces conflicted and retryable
 * `CommitResult` variants as errors today (see TODO on `commit_result_to_committed_handle`).
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. And MUST NOT USE transaction after this
 * method is called.
 */
struct ExternResultHandleExclusiveCommittedTransaction commit(HandleExclusiveTransaction txn,
                                                              HandleSharedExternEngine engine);

/**
 * Free a create-table transaction handle without committing.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void create_table_free_transaction(HandleExclusiveCreateTransaction txn);

/**
 * Attaches engine info to a create-table transaction.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. CONSUMES the transaction handle.
 */
struct ExternResultHandleExclusiveCreateTransaction create_table_with_engine_info(HandleExclusiveCreateTransaction txn,
                                                                                  struct KernelStringSlice engine_info,
                                                                                  HandleSharedExternEngine engine);

/**
 * Add file metadata to a create-table transaction for files that have been written. The metadata
 * contains information about files written during the transaction that will be added to the
 * Delta log during commit.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. Consumes write_metadata.
 */
void create_table_add_files(HandleExclusiveCreateTransaction txn,
                            HandleExclusiveEngineData write_metadata);

/**
 * Mark the create-table transaction as having data changes or not (these are recorded at the
 * file level).
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void create_table_set_data_change(HandleExclusiveCreateTransaction txn, bool data_change);

/**
 * Attempt to commit a create-table transaction. On success, returns a handle to the
 * [`CommittedTransaction`] from which the caller can read the version and the optional
 * post-commit snapshot. The returned handle must be freed with [`free_committed_transaction`].
 *
 * Returns an error if the commit fails.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle. And MUST NOT USE transaction after this
 * method is called.
 */
struct ExternResultHandleExclusiveCommittedTransaction create_table_commit(HandleExclusiveCreateTransaction txn,
                                                                           HandleSharedExternEngine engine);

/**
 * Free a [`CommittedTransaction`] handle.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle and must not use it after this call.
 */
void free_committed_transaction(HandleExclusiveCommittedTransaction txn);

/**
 * Read the committed version from a [`CommittedTransaction`] handle.
 *
 * Does not consume the handle; the caller still owns it and must eventually pass it to
 * [`free_committed_transaction`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
uint64_t committed_transaction_version(const HandleExclusiveCommittedTransaction *txn);

/**
 * Reads the post-commit snapshot, if available.
 *
 * Returns `Some` with a fresh [`SharedSnapshot`] handle if the committed transaction has an
 * associated post-commit snapshot. Returns `None` otherwise.
 *
 * Not every commit path produces a post-commit snapshot (see
 * [`CommittedTransaction::post_commit_snapshot`] for the kernel-side rationale); callers
 * can fall back to building a snapshot via [`get_snapshot_builder`](crate::get_snapshot_builder)
 * in that case.
 *
 * Each `Some` result contains an independent handle that the caller must eventually free with
 * [`free_snapshot`](crate::free_snapshot). Does not consume the input handle; the caller must
 * eventually pass it to [`free_committed_transaction`].
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
struct OptionalValueHandleSharedSnapshot committed_transaction_post_commit_snapshot(const HandleExclusiveCommittedTransaction *txn);

/**
 * Set a clustered data layout on a [`CreateTableTransactionBuilder`] from an array of top-level
 * clustering column names (in order). Clustering and partitioning are mutually exclusive; the
 * last data-layout call wins. Column validation (existence, stats-eligible types, duplicates)
 * happens later at [`create_table_builder_build`].
 *
 * This consumes the builder handle and returns a new one. The caller MUST replace their handle
 * pointer with the returned handle. On error, the old builder handle is consumed and gone --
 * do not free or reuse it. There is no new handle to free either.
 *
 * Only top-level columns are supported through this entry point (each slice is one column name);
 * nested clustering columns must be set on the Rust builder directly.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid builder handle and a valid `engine`. When
 * `num_columns > 0`, `columns` must point to `num_columns` contiguous, valid `KernelStringSlice`
 * values whose backing bytes are readable for the duration of the call; `columns` may be null
 * when `num_columns == 0`. CONSUMES the builder handle unconditionally (even on error).
 */
struct ExternResultHandleExclusiveCreateTableBuilder create_table_builder_with_clustering_columns(HandleExclusiveCreateTableBuilder builder,
                                                                                                  const struct KernelStringSlice *columns,
                                                                                                  uintptr_t num_columns,
                                                                                                  HandleSharedExternEngine engine);

/**
 * Set a partitioned data layout on a [`CreateTableTransactionBuilder`] from an array of top-level
 * partition column names (in order). Clustering and partitioning are mutually exclusive; the last
 * data-layout call wins. Column validation (existence, primitive types, subset of schema) happens
 * later at [`create_table_builder_build`].
 *
 * This consumes the builder handle and returns a new one. The caller MUST replace their handle
 * pointer with the returned handle. On error, the old builder handle is consumed and gone --
 * do not free or reuse it. There is no new handle to free either.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid builder handle and a valid `engine`. When
 * `num_columns > 0`, `columns` must point to `num_columns` contiguous, valid `KernelStringSlice`
 * values whose backing bytes are readable for the duration of the call; `columns` may be null
 * when `num_columns == 0`. CONSUMES the builder handle unconditionally (even on error).
 */
struct ExternResultHandleExclusiveCreateTableBuilder create_table_builder_with_partition_columns(HandleExclusiveCreateTableBuilder builder,
                                                                                                 const struct KernelStringSlice *columns,
                                                                                                 uintptr_t num_columns,
                                                                                                 HandleSharedExternEngine engine);

/**
 * Create a new [`CreateTableTransactionBuilder`] for creating a Delta table at the given path.
 *
 * The schema is provided via the engine's visitor callback pattern ([`EngineSchema`]): the
 * kernel allocates a [`KernelSchemaVisitorState`], calls the engine's visitor function to
 * populate it via `visit_field_*` downcalls, then extracts the final schema.
 *
 * The returned builder can be configured with [`create_table_builder_with_table_property`]
 * before building with [`create_table_builder_build`]. The engine is only used for error
 * reporting at this stage.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid `path`, `schema`, `engine_info`, and `engine`.
 */
struct ExternResultHandleExclusiveCreateTableBuilder get_create_table_builder(struct KernelStringSlice path,
                                                                              const struct EngineSchema *schema,
                                                                              struct KernelStringSlice engine_info,
                                                                              HandleSharedExternEngine engine);

/**
 * Add a single table property to a [`CreateTableTransactionBuilder`].
 *
 * This consumes the builder handle and returns a new one. The caller MUST replace their handle
 * pointer with the returned handle. On error, the old builder handle is consumed and gone --
 * do not free or reuse it. There is no new handle to free either.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid builder handle, `key`, `value`, and `engine`.
 * CONSUMES the builder handle unconditionally (even on error).
 */
struct ExternResultHandleExclusiveCreateTableBuilder create_table_builder_with_table_property(HandleExclusiveCreateTableBuilder builder,
                                                                                              struct KernelStringSlice key,
                                                                                              struct KernelStringSlice value,
                                                                                              HandleSharedExternEngine engine);

/**
 * Build a create-table transaction using the default [`FileSystemCommitter`]. Returns a
 * create-table transaction handle that can be used with [`create_table_add_files`],
 * [`create_table_set_data_change`], [`create_table_with_engine_info`], and
 * [`create_table_commit`] to optionally stage initial data before committing.
 *
 * # Safety
 *
 * Caller is responsible for passing valid builder and engine handles.
 * CONSUMES the builder handle -- caller must not use it after this call.
 */
struct ExternResultHandleExclusiveCreateTransaction create_table_builder_build(HandleExclusiveCreateTableBuilder builder,
                                                                               HandleSharedExternEngine engine);

/**
 * Build a create-table transaction with a custom committer. Same as
 * [`create_table_builder_build`] but uses the provided committer instead of the default.
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles.
 * CONSUMES both the builder and committer handles -- caller must not use them after this call.
 */
struct ExternResultHandleExclusiveCreateTransaction create_table_builder_build_with_committer(HandleExclusiveCreateTableBuilder builder,
                                                                                              HandleMutableCommitter committer,
                                                                                              HandleSharedExternEngine engine);

/**
 * Free a [`CreateTableTransactionBuilder`] without building.
 *
 * Use this on failure paths when the builder will not be built.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_create_table_builder(HandleExclusiveCreateTableBuilder builder);

/**
 * Remove files from a transaction using engine data and a selection vector.
 *
 * The `data` handle is consumed. The selection vector indicates which rows in `data` represent
 * files to remove: nonzero means the row is selected for removal, `0` means it is skipped.
 * If `selection_vector` is null or `selection_vector_len` is 0, all rows are selected. When
 * `selection_vector_len` is 0, the `selection_vector` pointer is not accessed and may be null
 * or any arbitrary value.
 *
 * The `data` and `selection_vector` should be derived from
 * [`scan_metadata_next`](crate::scan::scan_metadata_next): `data` is the engine data batch and
 * `selection_vector` is the scan's selection vector, modified to select only the rows (files) to
 * remove. Selecting rows that were not active in the original scan selection vector produces
 * invalid Remove actions in the commit log.
 *
 * Note: Unlike [`add_files`], this function takes an `engine` handle and returns
 * [`ExternResult`] because the selection vector validation can fail. Returns `true` on
 * success (the value itself is not meaningful).
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles. The `selection_vector` pointer must be valid
 * for `selection_vector_len` bytes, or null. Consumes the `data` handle. Does NOT consume
 * the `txn` handle.
 */
struct ExternResultbool remove_files(HandleExclusiveTransaction txn,
                                     HandleExclusiveEngineData data,
                                     const uint8_t *selection_vector,
                                     uintptr_t selection_vector_len,
                                     HandleSharedExternEngine engine);

/**
 * Allocate an empty deletion vector descriptor map. The returned handle must be released
 * either by [`free_dv_descriptor_map`] or by [`transaction_update_deletion_vectors`]
 * (which consumes the map).
 */
HandleExclusiveDvDescriptorMap dv_descriptor_map_new(void);

/**
 * Free a deletion vector descriptor map handle.
 *
 * # Safety
 *
 * Caller must pass a valid handle previously returned by [`dv_descriptor_map_new`].
 */
void free_dv_descriptor_map(HandleExclusiveDvDescriptorMap map);

/**
 * Free a deletion vector descriptor handle. Only call this if the descriptor has not
 * been moved into a map via [`dv_descriptor_map_insert`].
 *
 * # Safety
 *
 * Caller must pass a valid descriptor handle.
 */
void free_dv_descriptor(HandleExclusiveDvDescriptor descriptor);

/**
 * Construct a [`DeletionVectorDescriptor`] from raw fields, for engines that author DV
 * files themselves and want to install them via [`transaction_update_deletion_vectors`].
 *
 * Field validation (storage-type rules, non-negative size/cardinality/offset, etc.) is
 * performed by [`DeletionVectorDescriptor::try_new`]; see its docs for the full contract.
 * Pass `has_offset = false` to omit the offset.
 *
 * For persisted DVs, `offset` is the byte offset within the DV file at which the DV's
 * 4-byte big-endian size prefix begins. For a single-DV file, this is usually `1`
 * (skipping the version byte). Omitting the offset is only appropriate for single-DV files
 * where the size prefix begins immediately after the version byte.
 *
 * # Safety
 *
 * Caller must pass valid string slice and engine handle.
 */
struct ExternResultHandleExclusiveDvDescriptor dv_descriptor_new(int storage_type,
                                                                 struct KernelStringSlice path_or_inline_dv,
                                                                 bool has_offset,
                                                                 int32_t offset,
                                                                 int32_t size_in_bytes,
                                                                 int64_t cardinality,
                                                                 HandleSharedExternEngine engine);

/**
 * Insert a deletion vector descriptor into the map under the given data file path.
 * Consumes the descriptor handle on success. On error (e.g. invalid `data_file_path`),
 * the descriptor handle is left untouched and must still be released by the caller via
 * [`free_dv_descriptor`].
 *
 * `data_file_path` must be the data-file path exactly as it appears in the scan
 * metadata produced by the kernel (the Add file action's `path` field). The kernel
 * matches against this string when applying the DV update; a typo causes
 * [`transaction_update_deletion_vectors`] to return an error.
 * Re-inserting a descriptor for an existing path replaces the previous descriptor.
 *
 * # Safety
 *
 * Caller must pass valid handles. The descriptor handle is consumed only on success.
 */
struct ExternResultbool dv_descriptor_map_insert(HandleExclusiveDvDescriptorMap map,
                                                 struct KernelStringSlice data_file_path,
                                                 HandleExclusiveDvDescriptor descriptor,
                                                 HandleSharedExternEngine engine);

/**
 * Stage deletion-vector update actions on the transaction. For every entry in `dv_map`
 * the kernel emits a Remove + Add action pair on commit (the Add carries the new DV
 * descriptor; row-level statistics from the original Add are preserved).
 * Matched scan metadata must include an accurate `numRecords` statistic because the Delta
 * protocol requires it for files with deletion vectors.
 *
 * Consumes both `dv_map` and `scan_iter`. The engine should pass an iterator that covers
 * at least every file path mentioned in the map; extra files are ignored. If the map
 * references a path that does not appear in the iterator, the call returns an error and
 * leaves the transaction unchanged.
 *
 * This stages data-changing DV updates by default. Call
 * [`crate::transaction::set_data_change`] first for maintenance operations that should commit
 * with `dataChange = false`.
 *
 * # Safety
 *
 * Caller must pass valid handles. The transaction handle is borrowed in place and remains
 * valid after this call; the caller is expected to follow with `commit` (or
 * `free_transaction`) on the same handle. The DV map and scan iterator handles are
 * consumed and must not be used or freed after this call.
 */
struct ExternResultbool transaction_update_deletion_vectors(HandleExclusiveTransaction txn,
                                                            HandleExclusiveDvDescriptorMap dv_map,
                                                            HandleSharedScanMetadataIterator scan_iter,
                                                            HandleSharedExternEngine engine);

/**
 * Allocate an empty partition value map. The returned handle must be released either by
 * [`free_partition_value_map`] or by `get_partitioned_write_context` (which consumes the map).
 */
HandleExclusivePartitionValueMap partition_value_map_new(void);

/**
 * Free a partition value map handle.
 *
 * # Safety
 *
 * Caller must pass a valid handle previously returned by [`partition_value_map_new`] that has
 * not already been consumed by a write-context call.
 */
void free_partition_value_map(HandleExclusivePartitionValueMap map);

/**
 * Insert a `string` partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and string slices.
 */
struct ExternResultbool partition_value_map_insert_string(HandleExclusivePartitionValueMap map,
                                                          struct KernelStringSlice name,
                                                          struct KernelStringSlice value,
                                                          HandleSharedExternEngine engine);

/**
 * Insert an `integer` (32-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_int(HandleExclusivePartitionValueMap map,
                                                       struct KernelStringSlice name,
                                                       int32_t value,
                                                       HandleSharedExternEngine engine);

/**
 * Insert a `long` (64-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_long(HandleExclusivePartitionValueMap map,
                                                        struct KernelStringSlice name,
                                                        int64_t value,
                                                        HandleSharedExternEngine engine);

/**
 * Insert a `short` (16-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_short(HandleExclusivePartitionValueMap map,
                                                         struct KernelStringSlice name,
                                                         int16_t value,
                                                         HandleSharedExternEngine engine);

/**
 * Insert a `byte` (8-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_byte(HandleExclusivePartitionValueMap map,
                                                        struct KernelStringSlice name,
                                                        int8_t value,
                                                        HandleSharedExternEngine engine);

/**
 * Insert a `float` (32-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_float(HandleExclusivePartitionValueMap map,
                                                         struct KernelStringSlice name,
                                                         float value,
                                                         HandleSharedExternEngine engine);

/**
 * Insert a `double` (64-bit) partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_double(HandleExclusivePartitionValueMap map,
                                                          struct KernelStringSlice name,
                                                          double value,
                                                          HandleSharedExternEngine engine);

/**
 * Insert a `boolean` partition value under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_bool(HandleExclusivePartitionValueMap map,
                                                        struct KernelStringSlice name,
                                                        bool value,
                                                        HandleSharedExternEngine engine);

/**
 * Insert a `date` partition value (`value` = days since the Unix epoch) under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_date(HandleExclusivePartitionValueMap map,
                                                        struct KernelStringSlice name,
                                                        int32_t value,
                                                        HandleSharedExternEngine engine);

/**
 * Insert a `timestamp` partition value (`value` = microseconds since the Unix epoch, UTC) under
 * `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_timestamp(HandleExclusivePartitionValueMap map,
                                                             struct KernelStringSlice name,
                                                             int64_t value,
                                                             HandleSharedExternEngine engine);

/**
 * Insert a `timestamp_ntz` partition value (`value` = microseconds since the Unix epoch, no
 * timezone) under `name`.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_timestamp_ntz(HandleExclusivePartitionValueMap map,
                                                                 struct KernelStringSlice name,
                                                                 int64_t value,
                                                                 HandleSharedExternEngine engine);

/**
 * Insert a `binary` partition value under `name`, copying `len` bytes from `value`.
 *
 * # Safety
 *
 * Caller must pass valid handles, a valid `name` slice, and (when `len > 0`) a `value` pointer to
 * at least `len` readable bytes. An empty value may be passed as `(null, 0)`.
 */
struct ExternResultbool partition_value_map_insert_binary(HandleExclusivePartitionValueMap map,
                                                          struct KernelStringSlice name,
                                                          const uint8_t *value,
                                                          uintptr_t len,
                                                          HandleSharedExternEngine engine);

/**
 * Insert a `decimal` partition value under `name`. The unscaled 128-bit value is supplied as two
 * 64-bit halves (`value_hi << 64 | value_lo`). Returns an error if the precision/scale
 * combination is invalid.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_decimal(HandleExclusivePartitionValueMap map,
                                                           struct KernelStringSlice name,
                                                           uint64_t value_hi,
                                                           uint64_t value_lo,
                                                           uint8_t precision,
                                                           uint8_t scale,
                                                           HandleSharedExternEngine engine);

/**
 * Insert a typed `null` partition value under `name`. The `type_tag` identifies the column's
 * data type using the same `NullTypeTag` encoding as `visit_expression_literal_null` in
 * [`kernel_visitor`](crate::expressions::kernel_visitor). For the decimal tag, `precision` and
 * `scale` specify the decimal parameters; for all other types, pass 0 for both.
 *
 * Returns an error if the tag is unrecognized, is the non-primitive sentinel (255), or carries an
 * invalid decimal precision/scale. A null partition value is only legal for a nullable partition
 * column; the kernel rejects it otherwise when the write context is built.
 *
 * # Safety
 *
 * Caller must pass valid handles and a valid `name` slice.
 */
struct ExternResultbool partition_value_map_insert_null(HandleExclusivePartitionValueMap map,
                                                        struct KernelStringSlice name,
                                                        uint8_t type_tag,
                                                        uint8_t precision,
                                                        uint8_t scale,
                                                        HandleSharedExternEngine engine);

/**
 * Associates an app_id and version with a transaction. These will be applied to the table on
 * commit.
 *
 * # Returns
 * A new handle to the transaction that will set the `app_id` version to `version` on commit
 *
 * # Safety
 * Caller is responsible for passing [valid][Handle#Validity] handles. The `app_id` string slice
 * must be valid. CONSUMES TRANSACTION
 */
struct ExternResultHandleExclusiveTransaction with_transaction_id(HandleExclusiveTransaction txn,
                                                                  struct KernelStringSlice app_id,
                                                                  int64_t version,
                                                                  HandleSharedExternEngine engine);

/**
 * Retrieves the version associated with an app_id from a snapshot.
 *
 * # Returns
 * The version number if found, or an error of type `MissingDataError` when the app_id was not set
 *
 * # Safety
 * Caller must ensure [valid][Handle#Validity] handles are provided for snapshot and engine. The
 * `app_id` string slice must be valid.
 */
struct ExternResultOptionalValuei64 get_app_id_version(HandleSharedSnapshot snapshot,
                                                       struct KernelStringSlice app_id,
                                                       HandleSharedExternEngine engine);

/**
 * Gets the write context from a transaction for an unpartitioned table. The write context
 * provides schema and path information needed for writing data.
 *
 * For partitioned tables, use [`get_partitioned_write_context`] instead. Returns an error if the
 * table is partitioned.
 *
 * # Safety
 *
 * Caller is responsible for passing a [valid][Handle#Validity] transaction handle and engine.
 */
struct ExternResultHandleSharedWriteContext get_unpartitioned_write_context(HandleExclusiveTransaction txn,
                                                                            HandleSharedExternEngine engine);

/**
 * Gets the write context from a create-table transaction for an unpartitioned table.
 *
 * For partitioned tables, use [`create_table_get_partitioned_write_context`] instead. Returns an
 * error if the table is partitioned.
 *
 * # Safety
 *
 * Caller is responsible for passing a [valid][Handle#Validity] transaction handle and engine.
 */
struct ExternResultHandleSharedWriteContext create_table_get_unpartitioned_write_context(HandleExclusiveCreateTransaction txn,
                                                                                         HandleSharedExternEngine engine);

/**
 * Gets the write context from a transaction for a partitioned table, for the partition described
 * by `partition_values`. A separate write context (and write directory) is needed per partition,
 * so call this once per distinct set of partition values.
 *
 * `partition_values` maps each partition column's logical name to its value; build it with
 * [`partition_value_map_new`](super::partition_value::partition_value_map_new) and the
 * `partition_value_map_insert_*` functions. The map must contain exactly the table's partition
 * columns (the kernel validates completeness and value types and rejects extras). This function
 * consumes the map handle on both success and error; do not use or free it afterward.
 *
 * Returns an error if the table is not partitioned (use [`get_unpartitioned_write_context`]
 * instead) or if the partition values are invalid for the table's partition schema.
 *
 * # Safety
 *
 * Caller is responsible for passing a [valid][Handle#Validity] transaction handle, partition
 * value map handle, and engine.
 */
struct ExternResultHandleSharedWriteContext get_partitioned_write_context(HandleExclusiveTransaction txn,
                                                                          HandleExclusivePartitionValueMap partition_values,
                                                                          HandleSharedExternEngine engine);

/**
 * Gets the write context from a create-table transaction for a partitioned table. See
 * [`get_partitioned_write_context`] for the contract; this is the create-table counterpart.
 *
 * # Safety
 *
 * Caller is responsible for passing a [valid][Handle#Validity] transaction handle, partition
 * value map handle, and engine.
 */
struct ExternResultHandleSharedWriteContext create_table_get_partitioned_write_context(HandleExclusiveCreateTransaction txn,
                                                                                       HandleExclusivePartitionValueMap partition_values,
                                                                                       HandleSharedExternEngine engine);

void free_write_context(HandleSharedWriteContext write_context);

/**
 * Returns the logical (user-facing) write schema from a [`WriteContext`] handle. For
 * column-mapping-enabled writes, pair with [`get_physical_write_schema`] and
 * [`get_logical_to_physical`].
 *
 * The returned schema must be freed via [`crate::free_schema`].
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer
 */
HandleSharedSchema get_write_schema(HandleSharedWriteContext write_context);

/**
 * Returns the physical write schema from a [`WriteContext`] handle: the schema of the data
 * written to parquet files. With column mapping enabled, field names are physical
 * (e.g. `col-<uuid>`) and each field has a `parquet.field.id` metadata entry per the Delta
 * column-mapping spec; otherwise it matches the logical schema. Partition columns are
 * excluded unless the `materializePartitionColumns` writer feature or `IcebergCompatV3` is
 * enabled.
 *
 * Use this as the parquet writer schema and as the output schema of the evaluator built
 * from [`get_logical_to_physical`].
 *
 * The returned schema must be freed via [`crate::free_schema`].
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer
 */
HandleSharedSchema get_physical_write_schema(HandleSharedWriteContext write_context);

/**
 * Returns the logical-to-physical expression from a [`WriteContext`] handle. Engines apply
 * it via an [`ExpressionEvaluator`] to each batch of logical data before writing parquet.
 * The logical data batches must not contain partition columns. The column rename itself is encoded
 * in the physical schema (the evaluator matches input columns to output fields by position), not
 * in this expression.
 *
 * To build the evaluator, pass the schema of the partition-free input data as the input, this
 * value as the expression to evaluate, and [`get_physical_write_schema`] as the output. See
 * [`crate::engine_funcs::new_expression_evaluator`].
 *
 * The returned expression must be freed via [`crate::expressions::free_kernel_expression`].
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer
 *
 * [`ExpressionEvaluator`]: delta_kernel::ExpressionEvaluator
 */
HandleSharedExpression get_logical_to_physical(HandleSharedWriteContext write_context);

/**
 * Get the table root URL from a WriteContext handle. Returns the table root, not the
 * recommended write directory (which may include Hive-style partition paths or random
 * prefixes); use [`get_write_dir`] for the latter.
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer
 */
NullableCvoid get_write_path(HandleSharedWriteContext write_context, AllocateStringFn allocate_fn);

/**
 * Get the recommended directory URL for writing data files from a WriteContext handle.
 * Connectors should write files as `<write_dir>/<uuid>.parquet`. For a partitioned write context
 * this includes the Hive-style partition prefix (e.g. `year=2024/`) when column mapping is off, or
 * a random prefix when column mapping or `delta.randomizeFilePrefixes` is on.
 *
 * The returned URL is URI-encoded. Engines that write to a local filesystem must URI-decode it
 * once before using it as a path; the still-encoded URL (plus the file name) is what
 * [`resolve_file_path`] expects to produce the `add.path` recorded in the Delta log.
 *
 * A fresh random prefix is generated on each call when column mapping or random prefixes are
 * enabled, so call this once per file batch and reuse the result.
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer
 */
NullableCvoid get_write_dir(HandleSharedWriteContext write_context, AllocateStringFn allocate_fn);

/**
 * Visit the serialized partition values of a WriteContext handle by invoking `visitor` once per
 * partition column. Keys are *physical* column names (column-mapping applied) and values are the
 * protocol-serialized strings the engine must record in each Add action's `partitionValues`. When
 * a partition value is null, `is_null` is `true` and `value` is an empty slice. For an
 * unpartitioned write context, `visitor` is never called. Entries are visited in sorted key order
 * so the callback sequence is deterministic across runs.
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer, a valid `engine_context`
 * pointer passed through to each `visitor` invocation, and a valid `visitor` function pointer.
 */
void visit_partition_values(HandleSharedWriteContext write_context,
                            NullableCvoid engine_context,
                            void (*visitor)(NullableCvoid engine_context,
                                            struct KernelStringSlice key,
                                            struct KernelStringSlice value,
                                            bool is_null));

/**
 * Compute the relative `add.path` for the Delta log from the absolute URL of a data file the
 * engine has written. `file_url` is the full (URI-encoded) URL of the written file, typically
 * formed by appending the file name to [`get_write_dir`]'s result.
 *
 * Returns an error if `file_url` is not a valid URL or does not live under the table root.
 *
 * # Safety
 * Engine is responsible for providing a valid WriteContext pointer, a valid `file_url` slice, and
 * a valid engine handle.
 */
struct ExternResultNullableCvoid resolve_file_path(HandleSharedWriteContext write_context,
                                                   struct KernelStringSlice file_url,
                                                   AllocateStringFn allocate_fn,
                                                   HandleSharedExternEngine engine);

#ifdef __cplusplus
}  // extern "C"
#endif  // __cplusplus

#ifdef __cplusplus
}  // namespace ffi
#endif  // __cplusplus
