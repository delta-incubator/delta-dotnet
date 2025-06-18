// -----------------------------------------------------------------------------
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace DeltaLake.Kernel.Interop
{
    [NativeTypeName("unsigned int")]
    internal enum KernelError : uint
    {
        UnknownError,
        FFIError,
        ArrowError,
        EngineDataTypeError,
        ExtractError,
        GenericError,
        IOErrorError,
        ParquetError,
        ObjectStoreError,
        ObjectStorePathError,
        ReqwestError,
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
    }

    [NativeTypeName("unsigned int")]
    internal enum Level : uint
    {
        ERROR = 0,
        WARN = 1,
        INFO = 2,
        DEBUG = 3,
        TRACE = 4,
    }

    [NativeTypeName("unsigned int")]
    internal enum LogLineFormat : uint
    {
        FULL,
        COMPACT,
        PRETTY,
        JSON,
    }

    internal partial struct CStringMap
    {
    }

    internal partial struct CTransforms
    {
    }

    internal partial struct DvInfo
    {
    }

    internal partial struct EngineBuilder
    {
    }

    internal partial struct ExclusiveEngineData
    {
    }

    internal partial struct ExclusiveFileReadResultIterator
    {
    }

    internal partial struct Expression
    {
    }

    internal partial struct KernelExpressionVisitorState
    {
    }

    internal partial struct OptionHandleSharedExpression
    {
    }

    internal partial struct Predicate
    {
    }

    internal partial struct SharedExpression
    {
    }

    internal partial struct SharedExpressionEvaluator
    {
    }

    internal partial struct SharedExternEngine
    {
    }

    internal partial struct SharedPredicate
    {
    }

    internal partial struct SharedScan
    {
    }

    internal partial struct SharedScanMetadata
    {
    }

    internal partial struct SharedScanMetadataIterator
    {
    }

    internal partial struct SharedSchema
    {
    }

    internal partial struct SharedSnapshot
    {
    }

    internal partial struct StringSliceIterator
    {
    }

    internal unsafe partial struct KernelBoolSlice
    {
        [NativeTypeName("bool*")]
        public byte* ptr;

        [NativeTypeName("uintptr_t")]
        public UIntPtr len;
    }

    internal unsafe partial struct KernelRowIndexArray
    {
        [NativeTypeName("uint64_t *")]
        public UIntPtr* ptr;

        [NativeTypeName("uintptr_t")]
        public UIntPtr len;
    }

    internal partial struct EngineError
    {
        [NativeTypeName("enum KernelError")]
        public KernelError etype;
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultEngineBuilder_Tag : uint
    {
        OkEngineBuilder,
        ErrEngineBuilder,
    }

    internal unsafe partial struct ExternResultEngineBuilder
    {
        [NativeTypeName("ffi::ExternResultEngineBuilder_Tag")]
        public ExternResultEngineBuilder_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L281_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L282_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L285_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("struct EngineBuilder *")]
                public EngineBuilder* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal unsafe partial struct KernelStringSlice
    {
        [NativeTypeName("const char *")]
        public byte* ptr;

        [NativeTypeName("uintptr_t")]
        public UIntPtr len;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("struct EngineError *")]
    internal unsafe delegate EngineError* AllocateErrorFn([NativeTypeName("enum KernelError")] KernelError etype, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice msg);

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleSharedExternEngine_Tag : uint
    {
        OkHandleSharedExternEngine,
        ErrHandleSharedExternEngine,
    }

    internal unsafe partial struct ExternResultHandleSharedExternEngine
    {
        [NativeTypeName("ffi::ExternResultHandleSharedExternEngine_Tag")]
        public ExternResultHandleSharedExternEngine_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L369_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L370_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L373_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedExternEngine")]
                public SharedExternEngine* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleSharedSnapshot_Tag : uint
    {
        OkHandleSharedSnapshot,
        ErrHandleSharedSnapshot,
    }

    internal unsafe partial struct ExternResultHandleSharedSnapshot
    {
        [NativeTypeName("ffi::ExternResultHandleSharedSnapshot_Tag")]
        public ExternResultHandleSharedSnapshot_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L426_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L427_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L430_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedSnapshot")]
                public SharedSnapshot* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("ffi::NullableCvoid")]
    internal unsafe delegate void* AllocateStringFn([NativeTypeName("struct KernelStringSlice")] KernelStringSlice kernel_str);

    internal unsafe partial struct FFI_ArrowArray
    {
        [NativeTypeName("int64_t")]
        public IntPtr length;

        [NativeTypeName("int64_t")]
        public IntPtr null_count;

        [NativeTypeName("int64_t")]
        public IntPtr offset;

        [NativeTypeName("int64_t")]
        public IntPtr n_buffers;

        [NativeTypeName("int64_t")]
        public IntPtr n_children;

        [NativeTypeName("const void **")]
        public void** buffers;

        [NativeTypeName("struct FFI_ArrowArray **")]
        public FFI_ArrowArray** children;

        [NativeTypeName("struct FFI_ArrowArray *")]
        public FFI_ArrowArray* dictionary;

        [NativeTypeName("void (*)(struct FFI_ArrowArray *)")]
        public IntPtr release;

        public void* private_data;
    }

    internal unsafe partial struct FFI_ArrowSchema
    {
        [NativeTypeName("const char *")]
        public byte* format;

        [NativeTypeName("const char *")]
        public byte* name;

        [NativeTypeName("const char *")]
        public byte* metadata;

        [NativeTypeName("int64_t")]
        public IntPtr flags;

        [NativeTypeName("int64_t")]
        public IntPtr n_children;

        [NativeTypeName("struct FFI_ArrowSchema **")]
        public FFI_ArrowSchema** children;

        [NativeTypeName("struct FFI_ArrowSchema *")]
        public FFI_ArrowSchema* dictionary;

        [NativeTypeName("void (*)(struct FFI_ArrowSchema *)")]
        public IntPtr release;

        public void* private_data;
    }

    internal partial struct ArrowFFIData
    {
        [NativeTypeName("struct FFI_ArrowArray")]
        public FFI_ArrowArray array;

        [NativeTypeName("struct FFI_ArrowSchema")]
        public FFI_ArrowSchema schema;
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultArrowFFIData_Tag : uint
    {
        OkArrowFFIData,
        ErrArrowFFIData,
    }

    internal unsafe partial struct ExternResultArrowFFIData
    {
        [NativeTypeName("ffi::ExternResultArrowFFIData_Tag")]
        public ExternResultArrowFFIData_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L592_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L593_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L596_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("struct ArrowFFIData *")]
                public ArrowFFIData* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultbool_Tag : uint
    {
        Okbool,
        Errbool,
    }

    internal unsafe partial struct ExternResultbool
    {
        [NativeTypeName("ffi::ExternResultbool_Tag")]
        public ExternResultbool_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L613_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L614_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L617_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                public bool ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleExclusiveFileReadResultIterator_Tag : uint
    {
        OkHandleExclusiveFileReadResultIterator,
        ErrHandleExclusiveFileReadResultIterator,
    }

    internal unsafe partial struct ExternResultHandleExclusiveFileReadResultIterator
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveFileReadResultIterator_Tag")]
        public ExternResultHandleExclusiveFileReadResultIterator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L670_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L671_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L674_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveFileReadResultIterator")]
                public ExclusiveFileReadResultIterator* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal partial struct FileMeta
    {
        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice path;

        [NativeTypeName("int64_t")]
        public IntPtr last_modified;

        [NativeTypeName("uintptr_t")]
        public UIntPtr size;
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleExclusiveEngineData_Tag : uint
    {
        OkHandleExclusiveEngineData,
        ErrHandleExclusiveEngineData,
    }

    internal unsafe partial struct ExternResultHandleExclusiveEngineData
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveEngineData_Tag")]
        public ExternResultHandleExclusiveEngineData_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L733_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L734_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L737_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveEngineData")]
                public ExclusiveEngineData* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni32(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("int32_t")] int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni64(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("int64_t")] IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni16(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("int16_t")] short value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni8(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("int8_t")] sbyte value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnf32(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnf64(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, double value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnKernelStringSlice(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnbool(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, bool value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitJunctionFn(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("uintptr_t")] UIntPtr child_list_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitUnaryFn(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("uintptr_t")] UIntPtr child_list_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitBinaryFn(void* data, [NativeTypeName("uintptr_t")] UIntPtr sibling_list_id, [NativeTypeName("uintptr_t")] UIntPtr child_list_id);

    internal unsafe partial struct EngineExpressionVisitor
    {
        public void* data;

        [NativeTypeName("uintptr_t (*)(void *, uintptr_t)")]
        public IntPtr make_field_list;

        [NativeTypeName("ffi::VisitLiteralFni32")]
        public IntPtr visit_literal_int;

        [NativeTypeName("ffi::VisitLiteralFni64")]
        public IntPtr visit_literal_long;

        [NativeTypeName("ffi::VisitLiteralFni16")]
        public IntPtr visit_literal_short;

        [NativeTypeName("ffi::VisitLiteralFni8")]
        public IntPtr visit_literal_byte;

        [NativeTypeName("ffi::VisitLiteralFnf32")]
        public IntPtr visit_literal_float;

        [NativeTypeName("ffi::VisitLiteralFnf64")]
        public IntPtr visit_literal_double;

        [NativeTypeName("ffi::VisitLiteralFnKernelStringSlice")]
        public IntPtr visit_literal_string;

        [NativeTypeName("ffi::VisitLiteralFnbool")]
        public IntPtr visit_literal_bool;

        [NativeTypeName("ffi::VisitLiteralFni64")]
        public IntPtr visit_literal_timestamp;

        [NativeTypeName("ffi::VisitLiteralFni64")]
        public IntPtr visit_literal_timestamp_ntz;

        [NativeTypeName("ffi::VisitLiteralFni32")]
        public IntPtr visit_literal_date;

        [NativeTypeName("void (*)(void *, uintptr_t, const uint8_t *, uintptr_t)")]
        public IntPtr visit_literal_binary;

        [NativeTypeName("void (*)(void *, uintptr_t, int64_t, uint64_t, uint8_t, uint8_t)")]
        public IntPtr visit_literal_decimal;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t, uintptr_t)")]
        public IntPtr visit_literal_struct;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t)")]
        public IntPtr visit_literal_array;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t, uintptr_t)")]
        public IntPtr visit_literal_map;

        [NativeTypeName("void (*)(void *, uintptr_t)")]
        public IntPtr visit_literal_null;

        [NativeTypeName("ffi::VisitJunctionFn")]
        public IntPtr visit_and;

        [NativeTypeName("ffi::VisitJunctionFn")]
        public IntPtr visit_or;

        [NativeTypeName("ffi::VisitUnaryFn")]
        public IntPtr visit_not;

        [NativeTypeName("ffi::VisitUnaryFn")]
        public IntPtr visit_is_null;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_lt;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_gt;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_eq;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_distinct;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_in;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_add;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_minus;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_multiply;

        [NativeTypeName("ffi::VisitBinaryFn")]
        public IntPtr visit_divide;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice)")]
        public IntPtr visit_column;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t)")]
        public IntPtr visit_struct_expr;
    }

    internal unsafe partial struct EngineIterator
    {
        public void* data;

        [NativeTypeName("const void *(*)(void *)")]
        public IntPtr get_next;
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultusize_Tag : uint
    {
        Okusize,
        Errusize,
    }

    internal unsafe partial struct ExternResultusize
    {
        [NativeTypeName("ffi::ExternResultusize_Tag")]
        public ExternResultusize_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1078_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1079_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1082_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("uintptr_t")]
                public UIntPtr ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal partial struct Event
    {
        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice message;

        [NativeTypeName("enum Level")]
        public Level level;

        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice target;

        [NativeTypeName("uint32_t")]
        public uint line;

        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice file;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void TracingEventFn([NativeTypeName("struct Event")] Event @event);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void TracingLogLineFn([NativeTypeName("struct KernelStringSlice")] KernelStringSlice line);

    [NativeTypeName("unsigned int")]
    internal enum ExternResultKernelBoolSlice_Tag : uint
    {
        OkKernelBoolSlice,
        ErrKernelBoolSlice,
    }

    internal unsafe partial struct ExternResultKernelBoolSlice
    {
        [NativeTypeName("ffi::ExternResultKernelBoolSlice_Tag")]
        public ExternResultKernelBoolSlice_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1166_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1167_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1170_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("struct KernelBoolSlice")]
                public KernelBoolSlice ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleSharedScan_Tag : uint
    {
        OkHandleSharedScan,
        ErrHandleSharedScan,
    }

    internal unsafe partial struct ExternResultHandleSharedScan
    {
        [NativeTypeName("ffi::ExternResultHandleSharedScan_Tag")]
        public ExternResultHandleSharedScan_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1223_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1224_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1227_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedScan")]
                public SharedScan* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal unsafe partial struct EnginePredicate
    {
        public void* predicate;

        [NativeTypeName("uintptr_t (*)(void *, struct KernelExpressionVisitorState *)")]
        public IntPtr visitor;
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultHandleSharedScanMetadataIterator_Tag : uint
    {
        OkHandleSharedScanMetadataIterator,
        ErrHandleSharedScanMetadataIterator,
    }

    internal unsafe partial struct ExternResultHandleSharedScanMetadataIterator
    {
        [NativeTypeName("ffi::ExternResultHandleSharedScanMetadataIterator_Tag")]
        public ExternResultHandleSharedScanMetadataIterator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1297_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1298_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1301_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedScanMetadataIterator")]
                public SharedScanMetadataIterator* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [NativeTypeName("unsigned int")]
    internal enum ExternResultKernelRowIndexArray_Tag : uint
    {
        OkKernelRowIndexArray,
        ErrKernelRowIndexArray,
    }

    internal unsafe partial struct ExternResultKernelRowIndexArray
    {
        [NativeTypeName("ffi::ExternResultKernelRowIndexArray_Tag")]
        public ExternResultKernelRowIndexArray_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1318_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1319_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1322_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("struct KernelRowIndexArray")]
                public KernelRowIndexArray ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal partial struct Stats
    {
        [NativeTypeName("uint64_t")]
        public UIntPtr num_records;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void CScanCallback([NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("int64_t")] IntPtr size, [NativeTypeName("const struct Stats *")] Stats* stats, [NativeTypeName("const struct DvInfo *")] DvInfo* dv_info, [NativeTypeName("const struct Expression *")] Expression* transform, [NativeTypeName("const struct CStringMap *")] CStringMap* partition_map);

    internal unsafe partial struct EngineSchemaVisitor
    {
        public void* data;

        [NativeTypeName("uintptr_t (*)(void *, uintptr_t)")]
        public IntPtr make_field_list;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *, uintptr_t)")]
        public IntPtr visit_struct;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *, uintptr_t)")]
        public IntPtr visit_array;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *, uintptr_t)")]
        public IntPtr visit_map;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *, uint8_t, uint8_t)")]
        public IntPtr visit_decimal;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_string;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_long;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_integer;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_short;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_byte;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_float;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_double;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_boolean;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_binary;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_date;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_timestamp;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_timestamp_ntz;
    }

    internal static unsafe partial class Methods
    {
        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_bool_slice([NativeTypeName("struct KernelBoolSlice")] KernelBoolSlice slice);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_row_indexes([NativeTypeName("struct KernelRowIndexArray")] KernelRowIndexArray slice);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_engine_data([NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* engine_data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultEngineBuilder")]
        public static extern ExternResultEngineBuilder get_engine_builder([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void set_builder_option([NativeTypeName("struct EngineBuilder *")] EngineBuilder* builder, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice key, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExternEngine")]
        public static extern ExternResultHandleSharedExternEngine builder_build([NativeTypeName("struct EngineBuilder *")] EngineBuilder* builder);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExternEngine")]
        public static extern ExternResultHandleSharedExternEngine get_default_engine([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_engine([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedSnapshot")]
        public static extern ExternResultHandleSharedSnapshot snapshot([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_snapshot([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern UIntPtr version([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* logical_schema([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_schema([NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* schema);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* snapshot_table_root([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr get_partition_column_count([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleStringSliceIterator")]
        public static extern StringSliceIterator* get_partition_columns([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool string_slice_next([NativeTypeName("ffi::HandleStringSliceIterator")] StringSliceIterator* data, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice)")] IntPtr engine_visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_string_slice_data([NativeTypeName("ffi::HandleStringSliceIterator")] StringSliceIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr engine_data_length([NativeTypeName("ffi::HandleExclusiveEngineData *")] ExclusiveEngineData** data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void* get_raw_engine_data([NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultArrowFFIData")]
        public static extern ExternResultArrowFFIData get_raw_arrow_data([NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* data, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool read_result_next([NativeTypeName("ffi::HandleExclusiveFileReadResultIterator")] ExclusiveFileReadResultIterator* data, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, HandleExclusiveEngineData)")] IntPtr engine_visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_read_result_iter([NativeTypeName("ffi::HandleExclusiveFileReadResultIterator")] ExclusiveFileReadResultIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveFileReadResultIterator")]
        public static extern ExternResultHandleExclusiveFileReadResultIterator read_parquet_file([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("const struct FileMeta *")] FileMeta* file, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* physical_schema);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedExpressionEvaluator")]
        public static extern SharedExpressionEvaluator* new_expression_evaluator([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* input_schema, [NativeTypeName("const struct Expression *")] Expression* expression, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* output_type);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_expression_evaluator([NativeTypeName("ffi::HandleSharedExpressionEvaluator")] SharedExpressionEvaluator* evaluator);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveEngineData")]
        public static extern ExternResultHandleExclusiveEngineData evaluate_expression([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleExclusiveEngineData *")] ExclusiveEngineData** batch, [NativeTypeName("ffi::HandleSharedExpressionEvaluator")] SharedExpressionEvaluator* evaluator);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_expression([NativeTypeName("ffi::HandleSharedExpression")] SharedExpression* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_predicate([NativeTypeName("ffi::HandleSharedPredicate")] SharedPredicate* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression([NativeTypeName("const HandleSharedExpression *")] SharedExpression** expression, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_ref([NativeTypeName("const struct Expression *")] Expression* expression, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate([NativeTypeName("const HandleSharedPredicate *")] SharedPredicate** predicate, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_ref([NativeTypeName("const struct Predicate *")] Predicate* predicate, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_and([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct EngineIterator *")] EngineIterator* children);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_plus([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_minus([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_multiply([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_divide([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_lt([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_le([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_gt([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_ge([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_eq([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_ne([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr a, [NativeTypeName("uintptr_t")] UIntPtr b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_column([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_not([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr inner_pred);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_predicate_is_null([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] UIntPtr inner_expr);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_literal_string([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_int([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int32_t")] int value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_long([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int64_t")] IntPtr value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_short([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int16_t")] short value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_byte([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int8_t")] sbyte value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_float([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, float value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_double([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, double value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_expression_literal_bool([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, bool value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool enable_event_tracing([NativeTypeName("ffi::TracingEventFn")] IntPtr callback, [NativeTypeName("enum Level")] Level max_level);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool enable_log_line_tracing([NativeTypeName("ffi::TracingLogLineFn")] IntPtr callback, [NativeTypeName("enum Level")] Level max_level);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool enable_formatted_log_line_tracing([NativeTypeName("ffi::TracingLogLineFn")] IntPtr callback, [NativeTypeName("enum Level")] Level max_level, [NativeTypeName("enum LogLineFormat")] LogLineFormat format, bool ansi, bool with_time, bool with_level, bool with_target);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_scan_metadata([NativeTypeName("ffi::HandleSharedScanMetadata")] SharedScanMetadata* scan_metadata);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultKernelBoolSlice")]
        public static extern ExternResultKernelBoolSlice selection_vector_from_scan_metadata([NativeTypeName("ffi::HandleSharedScanMetadata")] SharedScanMetadata* scan_metadata, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_scan([NativeTypeName("ffi::HandleSharedScan")] SharedScan* scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedScan")]
        public static extern ExternResultHandleSharedScan scan([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct EnginePredicate *")] EnginePredicate* predicate);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* scan_table_root([NativeTypeName("ffi::HandleSharedScan")] SharedScan* scan, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* scan_logical_schema([NativeTypeName("ffi::HandleSharedScan")] SharedScan* scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* scan_physical_schema([NativeTypeName("ffi::HandleSharedScan")] SharedScan* scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedScanMetadataIterator")]
        public static extern ExternResultHandleSharedScanMetadataIterator scan_metadata_iter_init([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleSharedScan")] SharedScan* scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool scan_metadata_next([NativeTypeName("ffi::HandleSharedScanMetadataIterator")] SharedScanMetadataIterator* data, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, HandleSharedScanMetadata)")] IntPtr engine_visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_scan_metadata_iter([NativeTypeName("ffi::HandleSharedScanMetadataIterator")] SharedScanMetadataIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* get_from_string_map([NativeTypeName("const struct CStringMap *")] CStringMap* map, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice key, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct OptionHandleSharedExpression")]
        public static extern OptionHandleSharedExpression get_transform_for_row([NativeTypeName("uintptr_t")] UIntPtr row, [NativeTypeName("const struct CTransforms *")] CTransforms* transforms);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultKernelBoolSlice")]
        public static extern ExternResultKernelBoolSlice selection_vector_from_dv([NativeTypeName("const struct DvInfo *")] DvInfo* dv_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice root_url);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultKernelRowIndexArray")]
        public static extern ExternResultKernelRowIndexArray row_indexes_from_dv([NativeTypeName("const struct DvInfo *")] DvInfo* dv_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice root_url);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_scan_metadata([NativeTypeName("ffi::HandleSharedScanMetadata")] SharedScanMetadata* scan_metadata, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("ffi::CScanCallback")] IntPtr callback);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern UIntPtr visit_schema([NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* schema, [NativeTypeName("struct EngineSchemaVisitor *")] EngineSchemaVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedExpression")]
        public static extern SharedExpression* get_testing_kernel_expression();

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedPredicate")]
        public static extern SharedPredicate* get_testing_kernel_predicate();
    }
}
