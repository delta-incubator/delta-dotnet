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
    public enum KernelError
    {
        UnknownError = 0,
        FFIError = 1,
        ArrowError = 2,
        EngineDataTypeError = 3,
        ExtractError = 4,
        GenericError = 5,
        IOErrorError = 6,
        ParquetError = 7,
        ObjectStoreError = 8,
        ObjectStorePathError = 9,
        ReqwestError = 10,
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
    }

    internal enum Level
    {
        ERROR = 0,
        WARN = 1,
        INFO = 2,
        DEBUG = 3,
        TRACE = 4,
    }

    internal enum LogLineFormat
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

    internal partial struct ExclusiveCreateTableBuilder
    {
    }

    internal partial struct ExclusiveCreateTransaction
    {
    }

    internal partial struct ExclusiveEngineData
    {
    }

    internal partial struct ExclusiveFileReadResultIterator
    {
    }

    internal partial struct ExclusiveRustString
    {
    }

    internal partial struct ExclusiveScanBuilder
    {
    }

    internal partial struct ExclusiveTableChanges
    {
    }

    internal partial struct ExclusiveTransaction
    {
    }

    internal partial struct Expression
    {
    }

    internal partial struct KernelExpressionVisitorState
    {
    }

    internal partial struct KernelSchemaVisitorState
    {
    }

    internal partial struct MutableCommitter
    {
    }

    internal partial struct MutableFfiSnapshotBuilder
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

    internal partial struct SharedFfiUCCommitClient
    {
    }

    internal partial struct SharedMetadata
    {
    }

    internal partial struct SharedOpaqueExpressionOp
    {
    }

    internal partial struct SharedOpaquePredicateOp
    {
    }

    internal partial struct SharedPredicate
    {
    }

    internal partial struct SharedProtocol
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

    internal partial struct SharedScanTableChangesIterator
    {
    }

    internal partial struct SharedSchema
    {
    }

    internal partial struct SharedSnapshot
    {
    }

    internal partial struct SharedTableChangesScan
    {
    }

    internal partial struct SharedWriteContext
    {
    }

    internal partial struct StringSliceIterator
    {
    }

    internal partial struct EngineError
    {
        [NativeTypeName("enum KernelError")]
        public KernelError etype;
    }

    internal enum ExternResultHandleExclusiveRustString_Tag
    {
        OkHandleExclusiveRustString,
        ErrHandleExclusiveRustString,
    }

    internal unsafe partial struct ExternResultHandleExclusiveRustString
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveRustString_Tag")]
        public ExternResultHandleExclusiveRustString_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L340_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L341_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L344_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveRustString")]
                public ExclusiveRustString* ok;
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
        public sbyte* ptr;

        [NativeTypeName("uintptr_t")]
        public ulong len;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("struct EngineError *")]
    internal unsafe delegate EngineError* AllocateErrorFn([NativeTypeName("enum KernelError")] KernelError etype, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice msg);

    internal unsafe partial struct KernelBoolSlice
    {
        [NativeTypeName("bool*")]
        public byte* ptr;

        [NativeTypeName("uintptr_t")]
        public ulong len;
    }

    internal unsafe partial struct KernelRowIndexArray
    {
        [NativeTypeName("uint64_t *")]
        public ulong* ptr;

        [NativeTypeName("uintptr_t")]
        public ulong len;
    }

    internal enum ExternResultEngineBuilder_Tag
    {
        OkEngineBuilder,
        ErrEngineBuilder,
    }

    internal unsafe partial struct ExternResultEngineBuilder
    {
        [NativeTypeName("ffi::ExternResultEngineBuilder_Tag")]
        public ExternResultEngineBuilder_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L447_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L448_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L451_C5")]
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

    internal enum ExternResultbool_Tag
    {
        Okbool,
        Errbool,
    }

    internal unsafe partial struct ExternResultbool
    {
        [NativeTypeName("ffi::ExternResultbool_Tag")]
        public ExternResultbool_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L468_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L469_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L472_C5")]
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

    internal enum ExternResultHandleSharedExternEngine_Tag
    {
        OkHandleSharedExternEngine,
        ErrHandleSharedExternEngine,
    }

    internal unsafe partial struct ExternResultHandleSharedExternEngine
    {
        [NativeTypeName("ffi::ExternResultHandleSharedExternEngine_Tag")]
        public ExternResultHandleSharedExternEngine_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L525_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L526_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L529_C5")]
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

    internal enum ExternResultHandleMutableFfiSnapshotBuilder_Tag
    {
        OkHandleMutableFfiSnapshotBuilder,
        ErrHandleMutableFfiSnapshotBuilder,
    }

    internal unsafe partial struct ExternResultHandleMutableFfiSnapshotBuilder
    {
        [NativeTypeName("ffi::ExternResultHandleMutableFfiSnapshotBuilder_Tag")]
        public ExternResultHandleMutableFfiSnapshotBuilder_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L582_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L583_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L586_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder")]
                public MutableFfiSnapshotBuilder* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal partial struct FfiLogPath
    {
        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice location;

        [NativeTypeName("int64_t")]
        public long last_modified;

        [NativeTypeName("uint64_t")]
        public ulong size;
    }

    internal unsafe partial struct LogPathArray
    {
        [NativeTypeName("const struct FfiLogPath *")]
        public FfiLogPath* ptr;

        [NativeTypeName("uintptr_t")]
        public ulong len;
    }

    internal enum ExternResultHandleSharedSnapshot_Tag
    {
        OkHandleSharedSnapshot,
        ErrHandleSharedSnapshot,
    }

    internal unsafe partial struct ExternResultHandleSharedSnapshot
    {
        [NativeTypeName("ffi::ExternResultHandleSharedSnapshot_Tag")]
        public ExternResultHandleSharedSnapshot_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L681_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L682_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L685_C5")]
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

    internal enum ExternResulti64_Tag
    {
        Oki64,
        Erri64,
    }

    internal unsafe partial struct ExternResulti64
    {
        [NativeTypeName("ffi::ExternResulti64_Tag")]
        public ExternResulti64_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L702_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L703_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L706_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("int64_t")]
                public long ok;
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

    internal enum OptionalValueKernelStringSlice_Tag
    {
        SomeKernelStringSlice,
        NoneKernelStringSlice,
    }

    internal unsafe partial struct OptionalValueKernelStringSlice
    {
        [NativeTypeName("ffi::OptionalValueKernelStringSlice_Tag")]
        public OptionalValueKernelStringSlice_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L874_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L875_C5")]
            public _Anonymous_e__Struct Anonymous;

            internal partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("struct KernelStringSlice")]
                public KernelStringSlice some;
            }
        }
    }

    internal enum ExternResultNullableCvoid_Tag
    {
        OkNullableCvoid,
        ErrNullableCvoid,
    }

    internal unsafe partial struct ExternResultNullableCvoid
    {
        [NativeTypeName("ffi::ExternResultNullableCvoid_Tag")]
        public ExternResultNullableCvoid_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L892_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L893_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L896_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::NullableCvoid")]
                public void* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal unsafe partial struct FFI_ArrowArray
    {
        [NativeTypeName("int64_t")]
        public long length;

        [NativeTypeName("int64_t")]
        public long null_count;

        [NativeTypeName("int64_t")]
        public long offset;

        [NativeTypeName("int64_t")]
        public long n_buffers;

        [NativeTypeName("int64_t")]
        public long n_children;

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
        public sbyte* format;

        [NativeTypeName("const char *")]
        public sbyte* name;

        [NativeTypeName("const char *")]
        public sbyte* metadata;

        [NativeTypeName("int64_t")]
        public long flags;

        [NativeTypeName("int64_t")]
        public long n_children;

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

    internal enum ExternResultArrowFFIData_Tag
    {
        OkArrowFFIData,
        ErrArrowFFIData,
    }

    internal unsafe partial struct ExternResultArrowFFIData
    {
        [NativeTypeName("ffi::ExternResultArrowFFIData_Tag")]
        public ExternResultArrowFFIData_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L978_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L979_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L982_C5")]
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

    internal enum ExternResultHandleExclusiveEngineData_Tag
    {
        OkHandleExclusiveEngineData,
        ErrHandleExclusiveEngineData,
    }

    internal unsafe partial struct ExternResultHandleExclusiveEngineData
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveEngineData_Tag")]
        public ExternResultHandleExclusiveEngineData_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L999_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1000_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1003_C5")]
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

    internal enum ExternResultHandleExclusiveFileReadResultIterator_Tag
    {
        OkHandleExclusiveFileReadResultIterator,
        ErrHandleExclusiveFileReadResultIterator,
    }

    internal unsafe partial struct ExternResultHandleExclusiveFileReadResultIterator
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveFileReadResultIterator_Tag")]
        public ExternResultHandleExclusiveFileReadResultIterator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1056_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1057_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1060_C5")]
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
        public long last_modified;

        [NativeTypeName("uintptr_t")]
        public ulong size;
    }

    internal enum ExternResultHandleSharedExpressionEvaluator_Tag
    {
        OkHandleSharedExpressionEvaluator,
        ErrHandleSharedExpressionEvaluator,
    }

    internal unsafe partial struct ExternResultHandleSharedExpressionEvaluator
    {
        [NativeTypeName("ffi::ExternResultHandleSharedExpressionEvaluator_Tag")]
        public ExternResultHandleSharedExpressionEvaluator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1119_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1120_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1123_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedExpressionEvaluator")]
                public SharedExpressionEvaluator* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleExclusiveTableChanges_Tag
    {
        OkHandleExclusiveTableChanges,
        ErrHandleExclusiveTableChanges,
    }

    internal unsafe partial struct ExternResultHandleExclusiveTableChanges
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveTableChanges_Tag")]
        public ExternResultHandleExclusiveTableChanges_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1176_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1177_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1180_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveTableChanges")]
                public ExclusiveTableChanges* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleSharedTableChangesScan_Tag
    {
        OkHandleSharedTableChangesScan,
        ErrHandleSharedTableChangesScan,
    }

    internal unsafe partial struct ExternResultHandleSharedTableChangesScan
    {
        [NativeTypeName("ffi::ExternResultHandleSharedTableChangesScan_Tag")]
        public ExternResultHandleSharedTableChangesScan_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1233_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1234_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1237_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedTableChangesScan")]
                public SharedTableChangesScan* ok;
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

    internal enum ExternResultHandleSharedScanTableChangesIterator_Tag
    {
        OkHandleSharedScanTableChangesIterator,
        ErrHandleSharedScanTableChangesIterator,
    }

    internal unsafe partial struct ExternResultHandleSharedScanTableChangesIterator
    {
        [NativeTypeName("ffi::ExternResultHandleSharedScanTableChangesIterator_Tag")]
        public ExternResultHandleSharedScanTableChangesIterator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1305_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1306_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1309_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedScanTableChangesIterator")]
                public SharedScanTableChangesIterator* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum OptionalValueHandleExclusiveRustString_Tag
    {
        SomeHandleExclusiveRustString,
        NoneHandleExclusiveRustString,
    }

    internal unsafe partial struct OptionalValueHandleExclusiveRustString
    {
        [NativeTypeName("ffi::OptionalValueHandleExclusiveRustString_Tag")]
        public OptionalValueHandleExclusiveRustString_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1361_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1362_C5")]
            public _Anonymous_e__Struct Anonymous;

            internal unsafe partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveRustString")]
                public ExclusiveRustString* some;
            }
        }
    }

    internal partial struct Commit
    {
        [NativeTypeName("int64_t")]
        public long version;

        [NativeTypeName("int64_t")]
        public long timestamp;

        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice file_name;

        [NativeTypeName("int64_t")]
        public long file_size;

        [NativeTypeName("int64_t")]
        public long file_modification_timestamp;
    }

    internal enum OptionalValueCommit_Tag
    {
        SomeCommit,
        NoneCommit,
    }

    internal unsafe partial struct OptionalValueCommit
    {
        [NativeTypeName("ffi::OptionalValueCommit_Tag")]
        public OptionalValueCommit_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1389_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1390_C5")]
            public _Anonymous_e__Struct Anonymous;

            internal partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("struct Commit")]
                public Commit some;
            }
        }
    }

    internal enum OptionalValuei64_Tag
    {
        Somei64,
        Nonei64,
    }

    internal unsafe partial struct OptionalValuei64
    {
        [NativeTypeName("ffi::OptionalValuei64_Tag")]
        public OptionalValuei64_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1406_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1407_C5")]
            public _Anonymous_e__Struct Anonymous;

            internal partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("int64_t")]
                public long some;
            }
        }
    }

    internal partial struct CommitRequest
    {
        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice table_id;

        [NativeTypeName("struct KernelStringSlice")]
        public KernelStringSlice table_uri;

        [NativeTypeName("struct OptionalValueCommit")]
        public OptionalValueCommit commit_info;

        [NativeTypeName("struct OptionalValuei64")]
        public OptionalValuei64 latest_backfilled_version;

        [NativeTypeName("struct OptionalValueKernelStringSlice")]
        public OptionalValueKernelStringSlice metadata;

        [NativeTypeName("struct OptionalValueKernelStringSlice")]
        public OptionalValueKernelStringSlice protocol;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: NativeTypeName("struct OptionalValueHandleExclusiveRustString")]
    internal unsafe delegate OptionalValueHandleExclusiveRustString CCommit([NativeTypeName("ffi::NullableCvoid")] void* context, [NativeTypeName("struct CommitRequest")] CommitRequest request);

    internal enum ExternResultHandleMutableCommitter_Tag
    {
        OkHandleMutableCommitter,
        ErrHandleMutableCommitter,
    }

    internal unsafe partial struct ExternResultHandleMutableCommitter
    {
        [NativeTypeName("ffi::ExternResultHandleMutableCommitter_Tag")]
        public ExternResultHandleMutableCommitter_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1486_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1487_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1490_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleMutableCommitter")]
                public MutableCommitter* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni32(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("int32_t")] int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni64(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("int64_t")] long value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni16(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("int16_t")] short value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFni8(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("int8_t")] sbyte value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnf32(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnf64(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, double value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnKernelStringSlice(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitLiteralFnbool(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, bool value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitJunctionFn(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("uintptr_t")] ulong child_list_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitUnaryFn(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("uintptr_t")] ulong child_list_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitParseJsonFn(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("uintptr_t")] ulong child_list_id, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* output_schema);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitBinaryFn(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("uintptr_t")] ulong child_list_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void VisitVariadicFn(void* data, [NativeTypeName("uintptr_t")] ulong sibling_list_id, [NativeTypeName("uintptr_t")] ulong child_list_id);

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

        [NativeTypeName("ffi::VisitUnaryFn")]
        public IntPtr visit_to_json;

        [NativeTypeName("ffi::VisitParseJsonFn")]
        public IntPtr visit_parse_json;

        [NativeTypeName("ffi::VisitUnaryFn")]
        public IntPtr visit_map_to_struct;

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

        [NativeTypeName("ffi::VisitVariadicFn")]
        public IntPtr visit_coalesce;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice)")]
        public IntPtr visit_column;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t)")]
        public IntPtr visit_struct_expr;

        [NativeTypeName("void (*)(void *, uintptr_t, uintptr_t, uintptr_t)")]
        public IntPtr visit_transform_expr;

        [NativeTypeName("void (*)(void *, uintptr_t, const struct KernelStringSlice *, uintptr_t, bool)")]
        public IntPtr visit_field_transform;

        [NativeTypeName("void (*)(void *, uintptr_t, HandleSharedOpaqueExpressionOp, uintptr_t)")]
        public IntPtr visit_opaque_expr;

        [NativeTypeName("void (*)(void *, uintptr_t, HandleSharedOpaquePredicateOp, uintptr_t)")]
        public IntPtr visit_opaque_pred;

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice)")]
        public IntPtr visit_unknown;
    }

    internal unsafe partial struct EngineIterator
    {
        public void* data;

        [NativeTypeName("const void *(*)(void *)")]
        public IntPtr get_next;
    }

    internal enum ExternResultusize_Tag
    {
        Okusize,
        Errusize,
    }

    internal unsafe partial struct ExternResultusize
    {
        [NativeTypeName("ffi::ExternResultusize_Tag")]
        public ExternResultusize_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1996_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L1997_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2000_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("uintptr_t")]
                public ulong ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleSharedExpression_Tag
    {
        OkHandleSharedExpression,
        ErrHandleSharedExpression,
    }

    internal unsafe partial struct ExternResultHandleSharedExpression
    {
        [NativeTypeName("ffi::ExternResultHandleSharedExpression_Tag")]
        public ExternResultHandleSharedExpression_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2017_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2018_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2021_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedExpression")]
                public SharedExpression* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal unsafe partial struct EngineExpression
    {
        public void* expression;

        [NativeTypeName("uintptr_t (*)(void *, struct KernelExpressionVisitorState *)")]
        public IntPtr visitor;
    }

    internal enum ExternResultHandleSharedPredicate_Tag
    {
        OkHandleSharedPredicate,
        ErrHandleSharedPredicate,
    }

    internal unsafe partial struct ExternResultHandleSharedPredicate
    {
        [NativeTypeName("ffi::ExternResultHandleSharedPredicate_Tag")]
        public ExternResultHandleSharedPredicate_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2053_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2054_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2057_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedPredicate")]
                public SharedPredicate* ok;
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

    internal enum ExternResultKernelBoolSlice_Tag
    {
        OkKernelBoolSlice,
        ErrKernelBoolSlice,
    }

    internal unsafe partial struct ExternResultKernelBoolSlice
    {
        [NativeTypeName("ffi::ExternResultKernelBoolSlice_Tag")]
        public ExternResultKernelBoolSlice_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2141_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2142_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2145_C5")]
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

    internal enum ExternResultHandleSharedScan_Tag
    {
        OkHandleSharedScan,
        ErrHandleSharedScan,
    }

    internal unsafe partial struct ExternResultHandleSharedScan
    {
        [NativeTypeName("ffi::ExternResultHandleSharedScan_Tag")]
        public ExternResultHandleSharedScan_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2198_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2199_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2202_C5")]
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

    internal unsafe partial struct EngineSchema
    {
        public void* schema;

        [NativeTypeName("uintptr_t (*)(void *, struct KernelSchemaVisitorState *)")]
        public IntPtr visitor;
    }

    internal enum ExternResultHandleExclusiveScanBuilder_Tag
    {
        OkHandleExclusiveScanBuilder,
        ErrHandleExclusiveScanBuilder,
    }

    internal unsafe partial struct ExternResultHandleExclusiveScanBuilder
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveScanBuilder_Tag")]
        public ExternResultHandleExclusiveScanBuilder_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2269_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2270_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2273_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveScanBuilder")]
                public ExclusiveScanBuilder* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleSharedScanMetadataIterator_Tag
    {
        OkHandleSharedScanMetadataIterator,
        ErrHandleSharedScanMetadataIterator,
    }

    internal unsafe partial struct ExternResultHandleSharedScanMetadataIterator
    {
        [NativeTypeName("ffi::ExternResultHandleSharedScanMetadataIterator_Tag")]
        public ExternResultHandleSharedScanMetadataIterator_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2326_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2327_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2330_C5")]
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

    internal enum OptionalValueHandleSharedExpression_Tag
    {
        SomeHandleSharedExpression,
        NoneHandleSharedExpression,
    }

    internal unsafe partial struct OptionalValueHandleSharedExpression
    {
        [NativeTypeName("ffi::OptionalValueHandleSharedExpression_Tag")]
        public OptionalValueHandleSharedExpression_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2346_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2347_C5")]
            public _Anonymous_e__Struct Anonymous;

            internal unsafe partial struct _Anonymous_e__Struct
            {
                [NativeTypeName("ffi::HandleSharedExpression")]
                public SharedExpression* some;
            }
        }
    }

    internal enum ExternResultKernelRowIndexArray_Tag
    {
        OkKernelRowIndexArray,
        ErrKernelRowIndexArray,
    }

    internal unsafe partial struct ExternResultKernelRowIndexArray
    {
        [NativeTypeName("ffi::ExternResultKernelRowIndexArray_Tag")]
        public ExternResultKernelRowIndexArray_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2364_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2365_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2368_C5")]
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
        public ulong num_records;
    }

    internal unsafe partial struct CDvInfo
    {
        [NativeTypeName("const struct DvInfo *")]
        public DvInfo* info;

        public bool has_vector;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate void CScanCallback([NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("int64_t")] long size, [NativeTypeName("int64_t")] long mod_time, [NativeTypeName("const struct Stats *")] Stats* stats, [NativeTypeName("const struct CDvInfo *")] CDvInfo* dv_info, [NativeTypeName("const struct Expression *")] Expression* transform, [NativeTypeName("const struct CStringMap *")] CStringMap* partition_map);

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

        [NativeTypeName("void (*)(void *, uintptr_t, struct KernelStringSlice, bool, const struct CStringMap *)")]
        public IntPtr visit_variant;
    }

    internal enum ExternResultHandleExclusiveTransaction_Tag
    {
        OkHandleExclusiveTransaction,
        ErrHandleExclusiveTransaction,
    }

    internal unsafe partial struct ExternResultHandleExclusiveTransaction
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveTransaction_Tag")]
        public ExternResultHandleExclusiveTransaction_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2654_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2655_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2658_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveTransaction")]
                public ExclusiveTransaction* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultu64_Tag
    {
        Oku64,
        Erru64,
    }

    internal unsafe partial struct ExternResultu64
    {
        [NativeTypeName("ffi::ExternResultu64_Tag")]
        public ExternResultu64_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2675_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2676_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2679_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("uint64_t")]
                public ulong ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleExclusiveCreateTransaction_Tag
    {
        OkHandleExclusiveCreateTransaction,
        ErrHandleExclusiveCreateTransaction,
    }

    internal unsafe partial struct ExternResultHandleExclusiveCreateTransaction
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveCreateTransaction_Tag")]
        public ExternResultHandleExclusiveCreateTransaction_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2732_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2733_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2736_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveCreateTransaction")]
                public ExclusiveCreateTransaction* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultHandleExclusiveCreateTableBuilder_Tag
    {
        OkHandleExclusiveCreateTableBuilder,
        ErrHandleExclusiveCreateTableBuilder,
    }

    internal unsafe partial struct ExternResultHandleExclusiveCreateTableBuilder
    {
        [NativeTypeName("ffi::ExternResultHandleExclusiveCreateTableBuilder_Tag")]
        public ExternResultHandleExclusiveCreateTableBuilder_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2789_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2790_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2793_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal unsafe partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("ffi::HandleExclusiveCreateTableBuilder")]
                public ExclusiveCreateTableBuilder* ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal enum ExternResultOptionalValuei64_Tag
    {
        OkOptionalValuei64,
        ErrOptionalValuei64,
    }

    internal unsafe partial struct ExternResultOptionalValuei64
    {
        [NativeTypeName("ffi::ExternResultOptionalValuei64_Tag")]
        public ExternResultOptionalValuei64_Tag tag;

        [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2810_C3")]
        public _Anonymous_e__Union Anonymous;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe partial struct _Anonymous_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2811_C5")]
            public _Anonymous1_e__Struct Anonymous1;

            [FieldOffset(0)]
            [NativeTypeName("__AnonymousRecord_delta_kernel_ffi_L2814_C5")]
            public _Anonymous2_e__Struct Anonymous2;

            internal partial struct _Anonymous1_e__Struct
            {
                [NativeTypeName("struct OptionalValuei64")]
                public OptionalValuei64 ok;
            }

            internal unsafe partial struct _Anonymous2_e__Struct
            {
                [NativeTypeName("struct EngineError *")]
                public EngineError* err;
            }
        }
    }

    internal static unsafe partial class Methods
    {
        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveRustString")]
        public static extern ExternResultHandleExclusiveRustString allocate_kernel_string([NativeTypeName("struct KernelStringSlice")] KernelStringSlice kernel_str, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr error_fn);

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
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool set_builder_option([NativeTypeName("struct EngineBuilder *")] EngineBuilder* builder, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice key, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void set_builder_with_multithreaded_executor([NativeTypeName("struct EngineBuilder *")] EngineBuilder* builder, [NativeTypeName("uintptr_t")] ulong worker_threads, [NativeTypeName("uintptr_t")] ulong max_blocking_threads);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExternEngine")]
        public static extern ExternResultHandleSharedExternEngine builder_build([NativeTypeName("struct EngineBuilder *")] EngineBuilder* builder);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExternEngine")]
        public static extern ExternResultHandleSharedExternEngine get_default_engine([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_engine([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleMutableFfiSnapshotBuilder")]
        public static extern ExternResultHandleMutableFfiSnapshotBuilder get_snapshot_builder([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleMutableFfiSnapshotBuilder")]
        public static extern ExternResultHandleMutableFfiSnapshotBuilder get_snapshot_builder_from([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* prev_snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void snapshot_builder_set_version([NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder *")] MutableFfiSnapshotBuilder** builder, [NativeTypeName("ffi::Version")] ulong version);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool snapshot_builder_set_log_tail([NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder *")] MutableFfiSnapshotBuilder** builder, [NativeTypeName("struct LogPathArray")] LogPathArray log_tail);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void snapshot_builder_set_max_catalog_version([NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder *")] MutableFfiSnapshotBuilder** builder, [NativeTypeName("ffi::Version")] ulong max_catalog_version);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedSnapshot")]
        public static extern ExternResultHandleSharedSnapshot snapshot_builder_build([NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder")] MutableFfiSnapshotBuilder* builder);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_snapshot_builder([NativeTypeName("ffi::HandleMutableFfiSnapshotBuilder")] MutableFfiSnapshotBuilder* builder);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_snapshot([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool checkpoint_snapshot([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong version([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResulti64")]
        public static extern ExternResulti64 snapshot_timestamp([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

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
        public static extern ulong get_partition_column_count([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleStringSliceIterator")]
        public static extern StringSliceIterator* get_partition_columns([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_metadata_configuration([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice, struct KernelStringSlice)")] IntPtr visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedProtocol")]
        public static extern SharedProtocol* snapshot_get_protocol([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_protocol([NativeTypeName("ffi::HandleSharedProtocol")] SharedProtocol* protocol);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_protocol([NativeTypeName("ffi::HandleSharedProtocol")] SharedProtocol* protocol, [NativeTypeName("ffi::NullableCvoid")] void* context, [NativeTypeName("void (*)(NullableCvoid, int32_t, int32_t)")] IntPtr visit_versions, [NativeTypeName("void (*)(NullableCvoid, bool, struct KernelStringSlice)")] IntPtr visit_feature);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedMetadata")]
        public static extern SharedMetadata* snapshot_get_metadata([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_metadata([NativeTypeName("ffi::HandleSharedMetadata")] SharedMetadata* metadata);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_metadata([NativeTypeName("ffi::HandleSharedMetadata")] SharedMetadata* metadata, [NativeTypeName("ffi::NullableCvoid")] void* context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice, struct OptionalValueKernelStringSlice, struct OptionalValueKernelStringSlice, struct KernelStringSlice, bool, int64_t)")] IntPtr visit_metadata_fields);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool string_slice_next([NativeTypeName("ffi::HandleStringSliceIterator")] StringSliceIterator* data, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice)")] IntPtr engine_visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_string_slice_data([NativeTypeName("ffi::HandleStringSliceIterator")] StringSliceIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultNullableCvoid")]
        public static extern ExternResultNullableCvoid get_domain_metadata([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice domain, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool visit_domain_metadata([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice, struct KernelStringSlice)")] IntPtr visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong engine_data_length([NativeTypeName("ffi::HandleExclusiveEngineData *")] ExclusiveEngineData** data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void* get_raw_engine_data([NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultArrowFFIData")]
        public static extern ExternResultArrowFFIData get_raw_arrow_data([NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* data, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveEngineData")]
        public static extern ExternResultHandleExclusiveEngineData get_engine_data([NativeTypeName("struct FFI_ArrowArray")] FFI_ArrowArray array, [NativeTypeName("const struct FFI_ArrowSchema *")] FFI_ArrowSchema* schema, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool read_result_next([NativeTypeName("ffi::HandleExclusiveFileReadResultIterator")] ExclusiveFileReadResultIterator* data, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, HandleExclusiveEngineData)")] IntPtr engine_visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_read_result_iter([NativeTypeName("ffi::HandleExclusiveFileReadResultIterator")] ExclusiveFileReadResultIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveFileReadResultIterator")]
        public static extern ExternResultHandleExclusiveFileReadResultIterator read_parquet_file([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("const struct FileMeta *")] FileMeta* file, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* physical_schema);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExpressionEvaluator")]
        public static extern ExternResultHandleSharedExpressionEvaluator new_expression_evaluator([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* input_schema, [NativeTypeName("const struct Expression *")] Expression* expression, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* output_type);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_expression_evaluator([NativeTypeName("ffi::HandleSharedExpressionEvaluator")] SharedExpressionEvaluator* evaluator);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveEngineData")]
        public static extern ExternResultHandleExclusiveEngineData evaluate_expression([NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleExclusiveEngineData *")] ExclusiveEngineData** batch, [NativeTypeName("ffi::HandleSharedExpressionEvaluator")] SharedExpressionEvaluator* evaluator);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTableChanges")]
        public static extern ExternResultHandleExclusiveTableChanges table_changes_from_version([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::Version")] ulong start_version);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTableChanges")]
        public static extern ExternResultHandleExclusiveTableChanges table_changes_between_versions([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::Version")] ulong start_version, [NativeTypeName("ffi::Version")] ulong end_version);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_table_changes([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* table_changes_schema([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* table_changes_table_root([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong table_changes_start_version([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong table_changes_end_version([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedTableChangesScan")]
        public static extern ExternResultHandleSharedTableChangesScan table_changes_scan([NativeTypeName("ffi::HandleExclusiveTableChanges")] ExclusiveTableChanges* table_changes, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct EnginePredicate *")] EnginePredicate* predicate);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_table_changes_scan([NativeTypeName("ffi::HandleSharedTableChangesScan")] SharedTableChangesScan* table_changes_scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* table_changes_scan_table_root([NativeTypeName("ffi::HandleSharedTableChangesScan")] SharedTableChangesScan* table_changes_scan, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* table_changes_scan_logical_schema([NativeTypeName("ffi::HandleSharedTableChangesScan")] SharedTableChangesScan* table_changes_scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* table_changes_scan_physical_schema([NativeTypeName("ffi::HandleSharedTableChangesScan")] SharedTableChangesScan* table_changes_scan);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedScanTableChangesIterator")]
        public static extern ExternResultHandleSharedScanTableChangesIterator table_changes_scan_execute([NativeTypeName("ffi::HandleSharedTableChangesScan")] SharedTableChangesScan* table_changes_scan, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_scan_table_changes_iter([NativeTypeName("ffi::HandleSharedScanTableChangesIterator")] SharedScanTableChangesIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultArrowFFIData")]
        public static extern ExternResultArrowFFIData scan_table_changes_next([NativeTypeName("ffi::HandleSharedScanTableChangesIterator")] SharedScanTableChangesIterator* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedFfiUCCommitClient")]
        public static extern SharedFfiUCCommitClient* get_uc_commit_client([NativeTypeName("ffi::NullableCvoid")] void* context, [NativeTypeName("ffi::CCommit")] IntPtr commit_callback);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_uc_commit_client([NativeTypeName("ffi::HandleSharedFfiUCCommitClient")] SharedFfiUCCommitClient* commit_client);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleMutableCommitter")]
        public static extern ExternResultHandleMutableCommitter get_uc_committer([NativeTypeName("ffi::HandleSharedFfiUCCommitClient")] SharedFfiUCCommitClient* commit_client, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice table_id, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr error_fn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_uc_committer([NativeTypeName("ffi::HandleMutableCommitter")] MutableCommitter* commit_client);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_expression([NativeTypeName("ffi::HandleSharedExpression")] SharedExpression* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_predicate([NativeTypeName("ffi::HandleSharedPredicate")] SharedPredicate* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_opaque_expression_op([NativeTypeName("ffi::HandleSharedOpaqueExpressionOp")] SharedOpaqueExpressionOp* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_kernel_opaque_predicate_op([NativeTypeName("ffi::HandleSharedOpaquePredicateOp")] SharedOpaquePredicateOp* data);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_kernel_opaque_expression_op_name([NativeTypeName("ffi::HandleSharedOpaqueExpressionOp")] SharedOpaqueExpressionOp* op, void* data, [NativeTypeName("void (*)(void *, struct KernelStringSlice)")] IntPtr visit);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_kernel_opaque_predicate_op_name([NativeTypeName("ffi::HandleSharedOpaquePredicateOp")] SharedOpaquePredicateOp* op, void* data, [NativeTypeName("void (*)(void *, struct KernelStringSlice)")] IntPtr visit);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression([NativeTypeName("const HandleSharedExpression *")] SharedExpression** expression, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_ref([NativeTypeName("const struct Expression *")] Expression* expression, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate([NativeTypeName("const HandleSharedPredicate *")] SharedPredicate** predicate, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_ref([NativeTypeName("const struct Predicate *")] Predicate* predicate, [NativeTypeName("struct EngineExpressionVisitor *")] EngineExpressionVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_and([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct EngineIterator *")] EngineIterator* children);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_plus([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_minus([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_multiply([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_divide([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_lt([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_le([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_gt([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_ge([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_eq([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_ne([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_unknown([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_unknown([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_column([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_not([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong inner_pred);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_is_null([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong inner_expr);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_literal_string([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_int([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int32_t")] int value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_long([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int64_t")] long value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_short([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int16_t")] short value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_byte([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int8_t")] sbyte value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_float([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, float value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_double([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, double value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_bool([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, bool value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_date([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int32_t")] int value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_timestamp([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int64_t")] long value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_timestamp_ntz([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("int64_t")] long value);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_literal_binary([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("const uint8_t *")] byte* value, [NativeTypeName("uintptr_t")] ulong len);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_literal_decimal([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uint64_t")] ulong value_hi, [NativeTypeName("uint64_t")] ulong value_lo, [NativeTypeName("uint8_t")] byte precision, [NativeTypeName("uint8_t")] byte scale, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_expression_literal_null([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* _state, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_distinct([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_in([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong a, [NativeTypeName("uintptr_t")] ulong b);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_predicate_or([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct EngineIterator *")] EngineIterator* children);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_struct([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("struct EngineIterator *")] EngineIterator* children);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_expression_map_to_struct([NativeTypeName("struct KernelExpressionVisitorState *")] KernelExpressionVisitorState* state, [NativeTypeName("uintptr_t")] ulong child_expr);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedExpression")]
        public static extern ExternResultHandleSharedExpression visit_engine_expression([NativeTypeName("struct EngineExpression *")] EngineExpression* engine_expression, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedPredicate")]
        public static extern ExternResultHandleSharedPredicate visit_engine_predicate([NativeTypeName("struct EnginePredicate *")] EnginePredicate* engine_predicate, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

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
        public static extern ExternResultHandleSharedScan scan([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct EnginePredicate *")] EnginePredicate* predicate, [NativeTypeName("struct EngineSchema *")] EngineSchema* schema);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleExclusiveScanBuilder")]
        public static extern ExclusiveScanBuilder* scan_builder([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveScanBuilder")]
        public static extern ExternResultHandleExclusiveScanBuilder scan_builder_with_predicate([NativeTypeName("ffi::HandleExclusiveScanBuilder")] ExclusiveScanBuilder* builder, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct EnginePredicate *")] EnginePredicate* predicate);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveScanBuilder")]
        public static extern ExternResultHandleExclusiveScanBuilder scan_builder_with_schema([NativeTypeName("ffi::HandleExclusiveScanBuilder")] ExclusiveScanBuilder* builder, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct EngineSchema *")] EngineSchema* schema);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleSharedScan")]
        public static extern ExternResultHandleSharedScan scan_builder_build([NativeTypeName("ffi::HandleExclusiveScanBuilder")] ExclusiveScanBuilder* builder, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_scan_builder([NativeTypeName("ffi::HandleExclusiveScanBuilder")] ExclusiveScanBuilder* builder);

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
        [return: NativeTypeName("struct ExternResultNullableCvoid")]
        public static extern ExternResultNullableCvoid get_from_string_map([NativeTypeName("const struct CStringMap *")] CStringMap* map, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice key, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void visit_string_map([NativeTypeName("const struct CStringMap *")] CStringMap* map, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("void (*)(NullableCvoid, struct KernelStringSlice, struct KernelStringSlice)")] IntPtr visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct OptionalValueHandleSharedExpression")]
        public static extern OptionalValueHandleSharedExpression get_transform_for_row([NativeTypeName("uintptr_t")] ulong row, [NativeTypeName("const struct CTransforms *")] CTransforms* transforms);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultKernelBoolSlice")]
        public static extern ExternResultKernelBoolSlice selection_vector_from_dv([NativeTypeName("const struct DvInfo *")] DvInfo* dv_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice root_url);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultKernelRowIndexArray")]
        public static extern ExternResultKernelRowIndexArray row_indexes_from_dv([NativeTypeName("const struct DvInfo *")] DvInfo* dv_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice root_url);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool visit_scan_metadata([NativeTypeName("ffi::HandleSharedScanMetadata")] SharedScanMetadata* scan_metadata, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::NullableCvoid")] void* engine_context, [NativeTypeName("ffi::CScanCallback")] IntPtr callback);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uintptr_t")]
        public static extern ulong visit_schema([NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* schema, [NativeTypeName("struct EngineSchemaVisitor *")] EngineSchemaVisitor* visitor);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_string([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_long([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_integer([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_short([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_byte([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_float([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_double([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_boolean([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_binary([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_date([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_timestamp([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_timestamp_ntz([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_decimal([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("uint8_t")] byte precision, [NativeTypeName("uint8_t")] byte scale, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_struct([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("const uintptr_t *")] ulong* field_ids, [NativeTypeName("uintptr_t")] ulong field_count, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_array([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("uintptr_t")] ulong element_type_id, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_map([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("uintptr_t")] ulong key_type_id, [NativeTypeName("uintptr_t")] ulong value_type_id, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultusize")]
        public static extern ExternResultusize visit_field_variant([NativeTypeName("struct KernelSchemaVisitorState *")] KernelSchemaVisitorState* state, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice name, [NativeTypeName("uintptr_t")] ulong variant_struct_id, bool nullable, [NativeTypeName("ffi::AllocateErrorFn")] IntPtr allocate_error);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedExpression")]
        public static extern SharedExpression* get_testing_kernel_expression();

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedPredicate")]
        public static extern SharedPredicate* get_testing_kernel_predicate();

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedExpression")]
        public static extern SharedExpression* get_simple_testing_kernel_expression();

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedPredicate")]
        public static extern SharedPredicate* get_simple_testing_kernel_predicate();

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool expressions_are_equal([NativeTypeName("const HandleSharedExpression *")] SharedExpression** expr1, [NativeTypeName("const HandleSharedExpression *")] SharedExpression** expr2);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool predicates_are_equal([NativeTypeName("const HandleSharedPredicate *")] SharedPredicate** pred1, [NativeTypeName("const HandleSharedPredicate *")] SharedPredicate** pred2);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction transaction([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction transaction_with_committer([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine, [NativeTypeName("ffi::HandleMutableCommitter")] MutableCommitter* committer);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_transaction([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction with_engine_info([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice engine_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction with_domain_metadata([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice domain, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice configuration, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction with_domain_metadata_removed([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice domain, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void add_files([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* write_metadata);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void set_data_change([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, bool data_change);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultu64")]
        public static extern ExternResultu64 commit([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void create_table_free_transaction([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveCreateTransaction")]
        public static extern ExternResultHandleExclusiveCreateTransaction create_table_with_engine_info([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice engine_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void create_table_add_files([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn, [NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* write_metadata);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void create_table_set_data_change([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn, bool data_change);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultu64")]
        public static extern ExternResultu64 create_table_commit([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveCreateTableBuilder")]
        public static extern ExternResultHandleExclusiveCreateTableBuilder get_create_table_builder([NativeTypeName("struct KernelStringSlice")] KernelStringSlice path, [NativeTypeName("ffi::HandleSharedSchema")] SharedSchema* schema, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice engine_info, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveCreateTableBuilder")]
        public static extern ExternResultHandleExclusiveCreateTableBuilder create_table_builder_with_table_property([NativeTypeName("ffi::HandleExclusiveCreateTableBuilder")] ExclusiveCreateTableBuilder* builder, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice key, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice value, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveCreateTransaction")]
        public static extern ExternResultHandleExclusiveCreateTransaction create_table_builder_build([NativeTypeName("ffi::HandleExclusiveCreateTableBuilder")] ExclusiveCreateTableBuilder* builder, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveCreateTransaction")]
        public static extern ExternResultHandleExclusiveCreateTransaction create_table_builder_build_with_committer([NativeTypeName("ffi::HandleExclusiveCreateTableBuilder")] ExclusiveCreateTableBuilder* builder, [NativeTypeName("ffi::HandleMutableCommitter")] MutableCommitter* committer, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_create_table_builder([NativeTypeName("ffi::HandleExclusiveCreateTableBuilder")] ExclusiveCreateTableBuilder* builder);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultbool")]
        public static extern ExternResultbool remove_files([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("ffi::HandleExclusiveEngineData")] ExclusiveEngineData* data, [NativeTypeName("const uint8_t *")] byte* selection_vector, [NativeTypeName("uintptr_t")] ulong selection_vector_len, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultHandleExclusiveTransaction")]
        public static extern ExternResultHandleExclusiveTransaction with_transaction_id([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice app_id, [NativeTypeName("int64_t")] long version, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct ExternResultOptionalValuei64")]
        public static extern ExternResultOptionalValuei64 get_app_id_version([NativeTypeName("ffi::HandleSharedSnapshot")] SharedSnapshot* snapshot, [NativeTypeName("struct KernelStringSlice")] KernelStringSlice app_id, [NativeTypeName("ffi::HandleSharedExternEngine")] SharedExternEngine* engine);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedWriteContext")]
        public static extern SharedWriteContext* get_write_context([NativeTypeName("ffi::HandleExclusiveTransaction")] ExclusiveTransaction* txn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedWriteContext")]
        public static extern SharedWriteContext* create_table_get_write_context([NativeTypeName("ffi::HandleExclusiveCreateTransaction")] ExclusiveCreateTransaction* txn);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void free_write_context([NativeTypeName("ffi::HandleSharedWriteContext")] SharedWriteContext* write_context);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::HandleSharedSchema")]
        public static extern SharedSchema* get_write_schema([NativeTypeName("ffi::HandleSharedWriteContext")] SharedWriteContext* write_context);

        [DllImport("delta_kernel_ffi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("ffi::NullableCvoid")]
        public static extern void* get_write_path([NativeTypeName("ffi::HandleSharedWriteContext")] SharedWriteContext* write_context, [NativeTypeName("ffi::AllocateStringFn")] IntPtr allocate_fn);
    }
}
