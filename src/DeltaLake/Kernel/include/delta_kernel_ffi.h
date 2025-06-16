#pragma once

#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>

typedef enum KernelError {
  UnknownError,
  FFIError,
#if defined(DEFINE_DEFAULT_ENGINE)
  ArrowError,
#endif
  EngineDataTypeError,
  ExtractError,
  GenericError,
  IOErrorError,
#if defined(DEFINE_DEFAULT_ENGINE)
  ParquetError,
#endif
#if defined(DEFINE_DEFAULT_ENGINE)
  ObjectStoreError,
#endif
#if defined(DEFINE_DEFAULT_ENGINE)
  ObjectStorePathError,
#endif
#if defined(DEFINE_DEFAULT_ENGINE)
  ReqwestError,
#endif
  FileNotFoundError,
  MissingColumnError,
  UnexpectedColumnTypeError,
  MissingDataError,
  MissingVersionError,
  DeletionVectorError,
  InvalidUrlError,
  MalformedJsonError,
  MissingMetadataError,
  MissingProtocolError,
  InvalidProtocolError,
  MissingMetadataAndProtocolError,
  ParseError,
  JoinFailureError,
  Utf8Error,
  ParseIntError,
  InvalidColumnMappingModeError,
  InvalidTableLocationError,
  InvalidDecimalError,
  InvalidStructDataError,
  InternalError,
  InvalidExpression,
  InvalidLogPath,
  InvalidCommitInfo,
  FileAlreadyExists,
  MissingCommitInfo,
  UnsupportedError,
  ParseIntervalError,
  ChangeDataFeedUnsupported,
  ChangeDataFeedIncompatibleSchema,
  InvalidCheckpoint,
  LiteralExpressionTransformError,
  CheckpointWriteError,
  SchemaError,
} KernelError;

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
   * `{"timestamp":"2022-02-15T18:47:10.821315Z","level":"INFO","fields":{"message":"preparing to shave yaks","number_of_yaks":3},"target":"fmt_json"}`
   */
  JSON,
} LogLineFormat;

typedef struct CStringMap CStringMap;

/**
 * Transformation expressions that need to be applied to each row `i` in ScanMetadata. You can use
 * [`get_transform_for_row`] to get the transform for a particular row. If that returns an
 * associated expression, it _must_ be applied to the data read from the file specified by the
 * row. The resultant schema for this expression is guaranteed to be `Scan.schema()`. If
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

#if defined(DEFINE_DEFAULT_ENGINE)
/**
 * A builder that allows setting options on the `Engine` before actually building it
 */
typedef struct EngineBuilder EngineBuilder;
#endif

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
 * A SQL expression.
 *
 * These expressions do not track or validate data types, other than the type
 * of literals. It is up to the expression evaluator to validate the
 * expression against a schema and add appropriate casts as required.
 */
typedef struct Expression Expression;

typedef struct KernelExpressionVisitorState KernelExpressionVisitorState;

typedef struct OptionHandleSharedExpression OptionHandleSharedExpression;

/**
 * A SQL predicate.
 *
 * These predicates do not track or validate data types, other than the type
 * of literals. It is up to the predicate evaluator to validate the
 * predicate against a schema and add appropriate casts as required.
 */
typedef struct Predicate Predicate;

typedef struct SharedExpression SharedExpression;

typedef struct SharedExpressionEvaluator SharedExpressionEvaluator;

typedef struct SharedExternEngine SharedExternEngine;

typedef struct SharedPredicate SharedPredicate;

typedef struct SharedScan SharedScan;

typedef struct SharedScanMetadata SharedScanMetadata;

typedef struct SharedScanMetadataIterator SharedScanMetadataIterator;

typedef struct SharedSchema SharedSchema;

typedef struct SharedSnapshot SharedSnapshot;

typedef struct StringSliceIterator StringSliceIterator;

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
typedef struct SharedSnapshot *HandleSharedSnapshot;

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

typedef void *NullableCvoid;

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

#if defined(DEFINE_DEFAULT_ENGINE)
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

typedef void (*VisitBinaryFn)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);

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
 * itself is a list of schema elements, and every complex type (struct expression, array, junction, etc)
 * contains a list of "child" elements.
 *  1. Before visiting any complex expression type, the kernel asks the engine to allocate a list to
 *     hold its children
 *  2. When visiting any expression element, the kernel passes its parent's "child list" as the
 *     "sibling list" the element should be appended to:
 *      - For a struct literal, first visit each struct field and visit each value
 *      - For a struct expression, visit each sub expression.
 *      - For an array literal, visit each of the elements.
 *      - For a junction `and` or `or` expression, visit each sub-expression.
 *      - For a binary operator expression, visit the left and right operands.
 *      - For a unary `is null` or `not` expression, visit the sub-expression.
 *  3. When visiting a complex expression, the kernel also passes the "child list" containing
 *     that element's (already-visited) children.
 *  4. The [`visit_expression`] method returns the id of the list of top-level columns
 *
 * WARNING: The visitor MUST NOT retain internal references to string slices or binary data passed
 * to visitor methods
 * TODO: Visit type information in struct field and null. This will likely involve using the schema
 * visitor. Note that struct literals are currently in flux, and may change significantly. Here is
 * the relevant issue: <https://github.com/delta-io/delta-kernel-rs/issues/412>
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
   * Visit a 32bit integer `date` representing days since UNIX epoch 1970-01-01.  The `date` belongs
   * to the list identified by `sibling_list_id`.
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
   * Visits a null value belonging to the list identified by `sibling_list_id.
   */
  void (*visit_literal_null)(void *data, uintptr_t sibling_list_id);
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
   * Visits the `LessThan` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_lt;
  /**
   * Visits the `GreaterThan` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_gt;
  /**
   * Visits the `Equal` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_eq;
  /**
   * Visits the `Distinct` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
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
   * Visits the `Multiply` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_multiply;
  /**
   * Visits the `Divide` binary operator belonging to the list identified by `sibling_list_id`.
   * The operands will be in a _two_ item list identified by `child_list_id`
   */
  VisitBinaryFn visit_divide;
  /**
   * Visits the `column` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_column)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visits a `StructExpression` belonging to the list identified by `sibling_list_id`.
   * The sub-expressions of the `StructExpression` are in a list identified by `child_list_id`
   */
  void (*visit_struct_expr)(void *data, uintptr_t sibling_list_id, uintptr_t child_list_id);
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
   * file where the event occurred. If unknown the slice `ptr` will be null and the len will be 0
   */
  struct KernelStringSlice file;
} Event;

typedef void (*TracingEventFn)(struct Event event);

typedef void (*TracingLogLineFn)(struct KernelStringSlice line);

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
 * A predicate that can be used to skip data when scanning.
 *
 * When invoking [`scan`], The engine provides a pointer to the (engine's native) predicate, along
 * with a visitor function that can be invoked to recursively visit the predicate. This engine
 * state must be valid until the call to [`scan`] returns. Inside that method, the kernel allocates
 * visitor state, which becomes the second argument to the predicate visitor invocation along with
 * the engine-provided predicate pointer. The visitor state is valid for the lifetime of the
 * predicate visitor invocation. Thanks to this double indirection, engine and kernel each retain
 * ownership of their respective objects, with no need to coordinate memory lifetimes with the
 * other.
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
 * This callback will be invoked for each valid file that needs to be read for a scan.
 *
 * The arguments to the callback are:
 * * `context`: a `void*` context this can be anything that engine needs to pass through to each call
 * * `path`: a `KernelStringSlice` which is the path to the file
 * * `size`: an `i64` which is the size of the file
 * * `dv_info`: a [`DvInfo`] struct, which allows getting the selection vector for this file
 * * `transform`: An optional expression that, if not `NULL`, _must_ be applied to physical data to
 *   convert it to the correct logical format. If this is `NULL`, no transform is needed.
 * * `partition_values`: [DEPRECATED] a `HashMap<String, String>` which are partition values
 */
typedef void (*CScanCallback)(NullableCvoid engine_context,
                              struct KernelStringSlice path,
                              int64_t size,
                              const struct Stats *stats,
                              const struct DvInfo *dv_info,
                              const struct Expression *transform,
                              const struct CStringMap *partition_map);

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
} EngineSchemaVisitor;

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

#if defined(DEFINE_DEFAULT_ENGINE)
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

#if defined(DEFINE_DEFAULT_ENGINE)
/**
 * Set an option on the builder
 *
 * # Safety
 *
 * Caller must pass a valid EngineBuilder pointer, and valid slices for key and value
 */
void set_builder_option(struct EngineBuilder *builder,
                        struct KernelStringSlice key,
                        struct KernelStringSlice value);
#endif

#if defined(DEFINE_DEFAULT_ENGINE)
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

#if defined(DEFINE_DEFAULT_ENGINE)
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
 * Get the latest snapshot from the specified table
 *
 * # Safety
 *
 * Caller is responsible for passing valid handles and path pointer.
 */
struct ExternResultHandleSharedSnapshot snapshot(struct KernelStringSlice path,
                                                 HandleSharedExternEngine engine);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
void free_snapshot(HandleSharedSnapshot snapshot);

/**
 * Get the version of the specified snapshot
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
uint64_t version(HandleSharedSnapshot snapshot);

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
 * Get the number of rows in an engine data
 *
 * # Safety
 * `data_handle` must be a valid pointer to a kernel allocated `ExclusiveEngineData`
 */
uintptr_t engine_data_length(HandleExclusiveEngineData *data);

/**
 * Allow an engine to "unwrap" an [`ExclusiveEngineData`] into the raw pointer for the case it wants
 * to use its own engine data format
 *
 * # Safety
 *
 * `data_handle` must be a valid pointer to a kernel allocated `ExclusiveEngineData`. The Engine must
 * ensure the handle outlives the returned pointer.
 */
void *get_raw_engine_data(HandleExclusiveEngineData data);

#if defined(DEFINE_DEFAULT_ENGINE)
/**
 * Get an [`ArrowFFIData`] to allow binding to the arrow [C Data
 * Interface](https://arrow.apache.org/docs/format/CDataInterface.html). This includes the data and
 * the schema. If this function returns an `Ok` variant the _engine_ must free the returned struct.
 *
 * # Safety
 * data_handle must be a valid ExclusiveEngineData as read by the
 * [`delta_kernel::engine::default::DefaultEngine`] obtained from `get_default_engine`.
 */
struct ExternResultArrowFFIData get_raw_arrow_data(HandleExclusiveEngineData data,
                                                   HandleSharedExternEngine engine);
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
HandleSharedExpressionEvaluator new_expression_evaluator(HandleSharedExternEngine engine,
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
 * Enable getting called back for tracing (logging) events in the kernel. `max_level` specifies
 * that only events `<=` to the specified level should be reported.  More verbose Levels are "greater
 * than" less verbose ones. So Level::ERROR is the lowest, and Level::TRACE the highest.
 *
 * Note that setting up such a call back can only be done ONCE. Calling any of
 * `enable_event_tracing`, `enable_log_line_tracing`, or `enable_formatted_log_line_tracing` more
 * than once is a no-op.
 *
 * Returns `true` if the callback was setup successfully, false on failure (i.e. if called a second
 * time)
 *
 * Event-based tracing gives an engine maximal flexibility in formatting event log
 * lines. Kernel can also format events for the engine. If this is desired call
 * [`enable_log_line_tracing`] instead of this method.
 *
 * # Safety
 * Caller must pass a valid function pointer for the callback
 */
bool enable_event_tracing(TracingEventFn callback,
                          enum Level max_level);

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
 * Note that setting up such a call back can only be done ONCE. Calling any of
 * `enable_event_tracing`, `enable_log_line_tracing`, or `enable_formatted_log_line_tracing` more
 * than once is a no-op.
 *
 * Returns `true` if the callback was setup successfully, false on failure (i.e. if called a second
 * time)
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
 * Note that setting up such a call back can only be done ONCE. Calling any of
 * `enable_event_tracing`, `enable_log_line_tracing`, or `enable_formatted_log_line_tracing` more
 * than once is a no-op.
 *
 * Returns `true` if the callback was setup successfully, false on failure (i.e. if called a second
 * time)
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
                                         struct EnginePredicate *predicate);

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
 * Call the provided `engine_visitor` on the next scan metadata item. The visitor will be provided with
 * a [`SharedScanMetadata`], which contains the actual scan files and the associated selection vector. It is the
 * responsibility of the _engine_ to free the associated resources after use by calling
 * [`free_engine_data`] and [`free_bool_slice`] respectively.
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
NullableCvoid get_from_string_map(const struct CStringMap *map,
                                  struct KernelStringSlice key,
                                  AllocateStringFn allocate_fn);

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
struct OptionHandleSharedExpression get_transform_for_row(uintptr_t row,
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
 * Shim for ffi to call visit_scan_metadata. This will generally be called when iterating through scan
 * data which provides the [`SharedScanMetadata`] as each element in the iterator.
 *
 * # Safety
 * engine is responsible for passing a valid [`SharedScanMetadata`].
 */
void visit_scan_metadata(HandleSharedScanMetadata scan_metadata,
                         NullableCvoid engine_context,
                         CScanCallback callback);

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
 * Constructs a kernel expression that is passed back as a [`SharedExpression`] handle. The expected
 * output expression can be found in `ffi/tests/test_expression_visitor/expected.txt`.
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
