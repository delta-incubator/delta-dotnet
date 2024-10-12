#pragma once

#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>

typedef enum KernelError {
  UnknownError,
  FFIError,
// #if (defined(DEFINE_DEFAULT_ENGINE) || defined(DEFINE_SYNC_ENGINE)) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
  ArrowError,
// #endif
  EngineDataTypeError,
  ExtractError,
  GenericError,
  IOErrorError,
// #if (defined(DEFINE_DEFAULT_ENGINE) || defined(DEFINE_SYNC_ENGINE)) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
  ParquetError,
// #endif
// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
  ObjectStoreError,
// #endif
// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
  ObjectStorePathError,
// #endif
// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
  ReqwestError,
// #endif
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
} KernelError;

typedef struct CStringMap CStringMap;

/**
 * this struct can be used by an engine to materialize a selection vector
 */
typedef struct DvInfo DvInfo;

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * A builder that allows setting options on the `Engine` before actually building it
 */
typedef struct EngineBuilder EngineBuilder;
// #endif

/**
 * an opaque struct that encapsulates data read by an engine. this handle can be passed back into
 * some kernel calls to operate on the data, or can be converted into the raw data as read by the
 * [`delta_kernel::Engine`] by calling [`get_raw_engine_data`]
 */
typedef struct ExclusiveEngineData ExclusiveEngineData;

typedef struct ExclusiveFileReadResultIterator ExclusiveFileReadResultIterator;

typedef struct KernelExpressionVisitorState KernelExpressionVisitorState;

typedef struct SharedExternEngine SharedExternEngine;

typedef struct SharedGlobalScanState SharedGlobalScanState;

typedef struct SharedScan SharedScan;

typedef struct SharedScanDataIterator SharedScanDataIterator;

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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
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
 * compiler cannot help us here, because raw pointers don't have lifetimes. To reduce the risk of
 * accidental misuse, it is recommended to only instantiate this struct as a function arg, by
 * converting a string slice `Into` a `KernelStringSlice`. That way, the borrowed reference at call
 * site protects the `KernelStringSlice` until the function returns. Meanwhile, the callee should
 * assume that the slice is only valid until the function returns, and must not retain any
 * references to the slice or its data that could outlive the function call.
 *
 * ```
 * # use delta_kernel_ffi::KernelStringSlice;
 * fn wants_slice(slice: KernelStringSlice) { }
 * let msg = String::from("hello");
 * wants_slice(msg.into());
 * ```
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 */
typedef struct StringSliceIterator *HandleStringSliceIterator;

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
                       uintptr_t child_list_id);
  /**
   * Indicate that the schema contains an Array type. `child_list_id` will be a _one_ item list
   * with the array's element type
   */
  void (*visit_array)(void *data,
                      uintptr_t sibling_list_id,
                      struct KernelStringSlice name,
                      bool contains_null,
                      uintptr_t child_list_id);
  /**
   * Indicate that the schema contains an Map type. `child_list_id` will be a _two_ item list
   * where the first element is the map's key type and the second element is the
   * map's value type
   */
  void (*visit_map)(void *data,
                    uintptr_t sibling_list_id,
                    struct KernelStringSlice name,
                    bool value_contains_null,
                    uintptr_t child_list_id);
  /**
   * visit a `decimal` with the specified `precision` and `scale`
   */
  void (*visit_decimal)(void *data,
                        uintptr_t sibling_list_id,
                        struct KernelStringSlice name,
                        uint8_t precision,
                        uint8_t scale);
  /**
   * Visit a `string` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_string)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `long` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_long)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit an `integer` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_integer)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `short` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_short)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `byte` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_byte)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `float` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_float)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `double` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_double)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `boolean` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_boolean)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit `binary` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_binary)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `date` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_date)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `timestamp` belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_timestamp)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
  /**
   * Visit a `timestamp` with no timezone belonging to the list identified by `sibling_list_id`.
   */
  void (*visit_timestamp_ntz)(void *data, uintptr_t sibling_list_id, struct KernelStringSlice name);
} EngineSchemaVisitor;

/**
 * Model iterators. This allows an engine to specify iteration however it likes, and we simply wrap
 * the engine functions. The engine retains ownership of the iterator.
 */
typedef struct EngineIterator {
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 */
typedef struct SharedSchema *HandleSharedSchema;

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
  int64_t flags;
  int64_t n_children;
  struct FFI_ArrowSchema **children;
  struct FFI_ArrowSchema *dictionary;
  void (*release)(struct FFI_ArrowSchema *arg1);
  void *private_data;
} FFI_ArrowSchema;

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * Struct to allow binding to the arrow [C Data
 * Interface](https://arrow.apache.org/docs/format/CDataInterface.html). This includes the data and
 * the schema.
 */
typedef struct ArrowFFIData {
  struct FFI_ArrowArray array;
  struct FFI_ArrowSchema schema;
} ArrowFFIData;
// #endif

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
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
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
 * When invoking [`scan::scan`], The engine provides a pointer to the (engine's native) predicate,
 * along with a visitor function that can be invoked to recursively visit the predicate. This
 * engine state must be valid until the call to `scan::scan` returns. Inside that method, the
 * kernel allocates visitor state, which becomes the second argument to the predicate visitor
 * invocation along with the engine-provided predicate pointer. The visitor state is valid for the
 * lifetime of the predicate visitor invocation. Thanks to this double indirection, engine and
 * kernel each retain ownership of their respective objects, with no need to coordinate memory
 * lifetimes with the other.
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
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 */
typedef struct SharedGlobalScanState *HandleSharedGlobalScanState;

/**
 * Represents an object that crosses the FFI boundary and which outlives the scope that created
 * it. It can be passed freely between rust code and external code. The
 *
 * An accompanying [`HandleDescriptor`] trait defines the behavior of each handle type:
 *
 * * The true underlying ("target") type the handle represents. For safety reasons, target type
 * must always be [`Send`].
 *
 * * Mutable (`Box`-like) vs. shared (`Arc`-like). For safety reasons, the target type of a
 * shared handle must always be [`Send`]+[`Sync`].
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
 * mutex. Due to Rust [reference
 * rules](https://doc.rust-lang.org/book/ch04-02-references-and-borrowing.html#the-rules-of-references),
 * this requirement applies even for API calls that appear to be read-only (because Rust code
 * always receives the handle as mutable).
 *
 * NOTE: Because the underlying type is always [`Sync`], multi-threaded external code can
 * freely access shared (non-mutable) handles.
 *
 */
typedef struct SharedScanDataIterator *HandleSharedScanDataIterator;

/**
 * Semantics: Kernel will always immediately return the leaked engine error to the engine (if it
 * allocated one at all), and engine is responsible for freeing it.
 */
typedef enum ExternResultHandleSharedScanDataIterator_Tag {
  OkHandleSharedScanDataIterator,
  ErrHandleSharedScanDataIterator,
} ExternResultHandleSharedScanDataIterator_Tag;

typedef struct ExternResultHandleSharedScanDataIterator {
  ExternResultHandleSharedScanDataIterator_Tag tag;
  union {
    struct {
      HandleSharedScanDataIterator ok;
    };
    struct {
      struct EngineError *err;
    };
  };
} ExternResultHandleSharedScanDataIterator;

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

typedef void (*CScanCallback)(NullableCvoid engine_context,
                              struct KernelStringSlice path,
                              int64_t size,
                              const struct Stats *stats,
                              const struct DvInfo *dv_info,
                              const struct CStringMap *partition_map);

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

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
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
// #endif

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
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
// #endif

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * Consume the builder and return a `default` engine. After calling, the passed pointer is _no
 * longer valid_.
 *
 *
 * # Safety
 *
 * Caller is responsible to pass a valid EngineBuilder pointer, and to not use it again afterwards
 */
struct ExternResultHandleSharedExternEngine builder_build(struct EngineBuilder *builder);
// #endif

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * # Safety
 *
 * Caller is responsible for passing a valid path pointer.
 */
struct ExternResultHandleSharedExternEngine get_default_engine(struct KernelStringSlice path,
                                                               AllocateErrorFn allocate_error);
// #endif

// #if defined(DEFINE_SYNC_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * # Safety
 *
 * Caller is responsible for passing a valid path pointer.
 */
struct ExternResultHandleSharedExternEngine get_sync_engine(AllocateErrorFn allocate_error);
// #endif

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
 * Get the resolved root of the table. This should be used in any future calls that require
 * constructing a path
 *
 * # Safety
 *
 * Caller is responsible for passing a valid handle.
 */
NullableCvoid snapshot_table_root(HandleSharedSnapshot snapshot, AllocateStringFn allocate_fn);

/**
 * # Safety
 *
 * The iterator must be valid (returned by [kernel_scan_data_init]) and not yet freed by
 * [kernel_scan_data_free]. The visitor function pointer must be non-null.
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
 * Visit the schema of the passed `SnapshotHandle`, using the provided `visitor`. See the
 * documentation of [`EngineSchemaVisitor`] for a description of how this visitor works.
 *
 * This method returns the id of the list allocated to hold the top level schema columns.
 *
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot handle and schema visitor.
 */
uintptr_t visit_schema(HandleSharedSnapshot snapshot, struct EngineSchemaVisitor *visitor);

uintptr_t visit_expression_and(struct KernelExpressionVisitorState *state,
                               struct EngineIterator *children);

uintptr_t visit_expression_lt(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_expression_le(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_expression_gt(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_expression_ge(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

uintptr_t visit_expression_eq(struct KernelExpressionVisitorState *state, uintptr_t a, uintptr_t b);

/**
 * # Safety
 * The string slice must be valid
 */
struct ExternResultusize visit_expression_column(struct KernelExpressionVisitorState *state,
                                                 struct KernelStringSlice name,
                                                 AllocateErrorFn allocate_error);

uintptr_t visit_expression_not(struct KernelExpressionVisitorState *state, uintptr_t inner_expr);

uintptr_t visit_expression_is_null(struct KernelExpressionVisitorState *state,
                                   uintptr_t inner_expr);

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
 * Call the engine back with the next `EngingeData` batch read by Parquet/Json handler. The
 * _engine_ "owns" the data that is passed into the `engine_visitor`, since it is allocated by the
 * `Engine` being used for log-replay. If the engine wants the kernel to free this data, it _must_
 * call [`free_engine_data`] on it.
 *
 * # Safety
 *
 * The iterator must be valid (returned by [`read_parquet_file`]) and not yet freed by
 * [`free_read_result_iter`]. The visitor function pointer must be non-null.
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

// #if defined(DEFINE_DEFAULT_ENGINE) // CLangSharp does not support build time feature flags, meaning C# won`t have the class, so we comment them out
/**
 * Get an [`ArrowFFIData`] to allow binding to the arrow [C Data
 * Interface](https://arrow.apache.org/docs/format/CDataInterface.html). This includes the data and
 * the schema.
 *
 * # Safety
 * data_handle must be a valid ExclusiveEngineData as read by the
 * [`delta_kernel::engine::default::DefaultEngine`] obtained from `get_default_engine`.
 */
struct ExternResultArrowFFIData get_raw_arrow_data(HandleExclusiveEngineData data,
                                                   HandleSharedExternEngine engine);
// #endif

/**
 * Drops a scan.
 * # Safety
 * Caller is responsible for passing a [valid][Handle#Validity] scan handle.
 */
void free_scan(HandleSharedScan scan);

/**
 * Get a [`Scan`] over the table specified by the passed snapshot.
 * # Safety
 *
 * Caller is responsible for passing a valid snapshot pointer, and engine pointer
 */
struct ExternResultHandleSharedScan scan(HandleSharedSnapshot snapshot,
                                         HandleSharedExternEngine engine,
                                         struct EnginePredicate *predicate);

/**
 * Get the global state for a scan. See the docs for [`delta_kernel::scan::state::GlobalScanState`]
 * for more information.
 *
 * # Safety
 * Engine is responsible for providing a valid scan pointer
 */
HandleSharedGlobalScanState get_global_scan_state(HandleSharedScan scan);

/**
 * Get the kernel view of the physical read schema that an engine should read from parquet file in
 * a scan
 *
 * # Safety
 * Engine is responsible for providing a valid GlobalScanState pointer
 */
HandleSharedSchema get_global_read_schema(HandleSharedGlobalScanState state);

/**
 * Free a global read schema
 *
 * # Safety
 * Engine is responsible for providing a valid schema obtained via [`get_global_read_schema`]
 */
void free_global_read_schema(HandleSharedSchema schema);

/**
 * Get a count of the number of partition columns for this scan
 *
 * # Safety
 * Caller is responsible for passing a valid global scan pointer.
 */
uintptr_t get_partition_column_count(HandleSharedGlobalScanState state);

/**
 * Get an iterator of the list of partition columns for this scan.
 *
 * # Safety
 * Caller is responsible for passing a valid global scan pointer.
 */
HandleStringSliceIterator get_partition_columns(HandleSharedGlobalScanState state);

/**
 * # Safety
 *
 * Caller is responsible for passing a valid global scan state pointer.
 */
void free_global_scan_state(HandleSharedGlobalScanState state);

/**
 * Get an iterator over the data needed to perform a scan. This will return a
 * [`KernelScanDataIterator`] which can be passed to [`kernel_scan_data_next`] to get the actual
 * data in the iterator.
 *
 * # Safety
 *
 * Engine is responsible for passing a valid [`SharedExternEngine`] and [`SharedScan`]
 */
struct ExternResultHandleSharedScanDataIterator kernel_scan_data_init(HandleSharedExternEngine engine,
                                                                      HandleSharedScan scan);

/**
 * # Safety
 *
 * The iterator must be valid (returned by [kernel_scan_data_init]) and not yet freed by
 * [`free_kernel_scan_data`]. The visitor function pointer must be non-null.
 */
struct ExternResultbool kernel_scan_data_next(HandleSharedScanDataIterator data,
                                              NullableCvoid engine_context,
                                              void (*engine_visitor)(NullableCvoid engine_context,
                                                                     HandleExclusiveEngineData engine_data,
                                                                     struct KernelBoolSlice selection_vector));

/**
 * # Safety
 *
 * Caller is responsible for (at most once) passing a valid pointer returned by a call to
 * [`kernel_scan_data_init`].
 */
void free_kernel_scan_data(HandleSharedScanDataIterator data);

/**
 * allow probing into a CStringMap. If the specified key is in the map, kernel will call
 * allocate_fn with the value associated with the key and return the value returned from that
 * function. If the key is not in the map, this will return NULL
 *
 * # Safety
 *
 * The engine is responsible for providing a valid [`CStringMap`] pointer and [`KernelStringSlice`]
 */
NullableCvoid get_from_map(const struct CStringMap *map,
                           struct KernelStringSlice key,
                           AllocateStringFn allocate_fn);

/**
 * Get a selection vector out of a [`DvInfo`] struct
 *
 * # Safety
 * Engine is responsible for providing valid pointers for each argument
 */
struct ExternResultKernelBoolSlice selection_vector_from_dv(const struct DvInfo *dv_info,
                                                            HandleSharedExternEngine engine,
                                                            HandleSharedGlobalScanState state);

/**
 * Get a vector of row indexes out of a [`DvInfo`] struct
 *
 * # Safety
 * Engine is responsible for providing valid pointers for each argument
 */
struct ExternResultKernelRowIndexArray row_indexes_from_dv(const struct DvInfo *dv_info,
                                                           HandleSharedExternEngine engine,
                                                           HandleSharedGlobalScanState state);

/**
 * Shim for ffi to call visit_scan_data. This will generally be called when iterating through scan
 * data which provides the data handle and selection vector as each element in the iterator.
 *
 * # Safety
 * engine is responsbile for passing a valid [`ExclusiveEngineData`] and selection vector.
 */
void visit_scan_data(HandleExclusiveEngineData data,
                     struct KernelBoolSlice selection_vec,
                     NullableCvoid engine_context,
                     CScanCallback callback);
