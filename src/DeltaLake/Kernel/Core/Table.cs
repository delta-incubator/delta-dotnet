// -----------------------------------------------------------------------------
// <summary>
// The Kernel representation of a Delta Table, contains the memory management
// and lifecycle of the table on both sides (C# and Kernel).
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DeltaLake.Extensions;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Disposables;
using DeltaLake.Kernel.Interop;
using DeltaLake.Table;
using DeltaRustBridge = DeltaLake.Bridge;
using ICancellationToken = System.Threading.CancellationToken;

namespace DeltaLake.Kernel.Core
{
    /// <summary>
    /// Reference to unmanaged delta table.
    ///
    /// The idea is, we prioritize the Kernel FFI implementations, and
    /// operations not supported by the FFI yet falls back to Delta RS Runtime
    /// implementation.
    /// </summary>
    internal class Table : DeltaRustBridge.Table
    {
        /// <summary>
        /// Behavioral flags.
        /// </summary>
        private bool isKernelSupported;
        private bool isKernelAllocated;

        /// <summary>
        /// Regular managed objects.
        /// </summary>
        private readonly TableStorageOptions tableStorageOptions;

        /// <summary>
        /// Kernel InterOp objects.
        /// </summary>
        private readonly KernelStringSlice tableLocationSlice;
        private readonly KernelStringSlice[] storageOptionsKeySlices;
        private readonly KernelStringSlice[] storageOptionsValueSlices;
        private readonly ExternResultHandleSharedExternEngine sharedExternEngine;

        /// <summary>
        /// Disposable managed objects.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to dispose of these alongside this <see cref="Table"/> class.
        /// </remarks>
        private readonly IManagedTableSnapshot tableSnapshot;

        /// <summary>
        /// Pointers **WE** manage alongside this <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to release these pointers via the paired GC
        /// handles.
        /// </remarks>
        private readonly unsafe sbyte* tableLocationPtr;
        private readonly unsafe IntPtr allocateErrorCallbackPtr;
        private readonly unsafe IntPtr allocateStringCallbackPtr;
        private readonly unsafe sbyte** storageOptionsKeyPtrs;
        private readonly unsafe sbyte** storageOptionsValuePtrs;

        /// <summary>
        /// Pointers **KERNEL** manages related to this <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to ask Kernel to release these pointers
        /// when <see cref="Table"/> class is disposed.
        /// </remarks>
        private readonly unsafe EngineBuilder* engineBuilderPtr;
        private readonly unsafe SharedExternEngine* sharedExternEnginePtr;

        /// <summary>
        /// GC handles to release the pointers we manage.
        /// </summary>
        /// <remarks>
        /// It is our responsibility to invoke <see cref="GCHandle.Free()"/> on
        /// each of these handles when <see cref="Table"/> class is disposed.
        /// </remarks>
        private readonly GCHandle tableLocationHandle;
        private readonly GCHandle allocateErrorCallbackHandle;
        private readonly GCHandle allocateStringCallbackHandle;
        private readonly GCHandle[] storageOptionsKeyHandles;
        private readonly GCHandle[] storageOptionsValueHandles;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <remarks>
        /// If the user passes in a version at load time, Kernel cannot be used.
        /// </remarks>
        /// <param name="table">The Delta Rust table.</param>
        /// <param name="options">The table options.</param>
        internal unsafe Table(DeltaRustBridge.Table table, TableOptions options)
            : this(table, options, options.IsKernelSupported()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="table">The Delta Rust table.</param>
        /// <param name="tableStorageOptions">The table storage options.</param>
        /// <param name="useKernel">Whether to use the Kernel or not.</param>
        internal unsafe Table(DeltaRustBridge.Table table, TableStorageOptions tableStorageOptions, Boolean useKernel = true)
            : base(table._runtime, table._ptr)
        {
            this.tableStorageOptions = tableStorageOptions;
            this.isKernelSupported = useKernel && tableStorageOptions.IsKernelSupported();

            if (this.isKernelSupported)
            {
                // Kernel String Slice is used to communicate the table location.
                //
                (GCHandle handle, IntPtr ptr) = tableStorageOptions.TableLocation.ToPinnedSBytePointer();
                this.tableLocationHandle = handle;
                this.tableLocationPtr = (sbyte*)ptr.ToPointer();
                this.tableLocationSlice = new KernelStringSlice { ptr = this.tableLocationPtr, len = (nuint)tableStorageOptions.TableLocation.Length };

                // Allocation callback is used to communicate memory allocation
                // requests to/from the Kernel via method hooks.
                //
                AllocateErrorFn allocationErrorDelegate = AllocateErrorCallbacks.ThrowAllocationError;
                this.allocateErrorCallbackHandle = GCHandle.Alloc(allocationErrorDelegate);
                this.allocateErrorCallbackPtr = Marshal.GetFunctionPointerForDelegate(allocationErrorDelegate);

                AllocateStringFn allocateStringDelegate = StringAllocatorCallbacks.AllocateString;
                this.allocateStringCallbackHandle = GCHandle.Alloc(allocateStringDelegate);
                this.allocateStringCallbackPtr = Marshal.GetFunctionPointerForDelegate(allocateStringDelegate);

                // Shared engine is the core runtime at the Kernel, tied to this table,
                // it is managed by the Kernel, but our responsibility to release it.
                //
                ExternResultEngineBuilder engineBuilder = Methods.get_engine_builder(this.tableLocationSlice, this.allocateErrorCallbackPtr);
                if (engineBuilder.tag != ExternResultEngineBuilder_Tag.OkEngineBuilder)
                {
                    throw new InvalidOperationException("Could not initiate engine builder from Delta Kernel");
                }
                this.engineBuilderPtr = engineBuilder.Anonymous.Anonymous1.ok;

                // The joys of unmanaged code, this is all to pass some Key:Value string pairs
                // to the Kernel's Engine Builder (e.g. Storage Account/S3 Keys etc.).
                //
                int index = 0;
                int count = tableStorageOptions.StorageOptions.Count;
                this.storageOptionsKeyHandles = new GCHandle[count];
                this.storageOptionsValueHandles = new GCHandle[count];
                this.storageOptionsKeyPtrs = (sbyte**)Marshal.AllocHGlobal(count * sizeof(sbyte*));
                this.storageOptionsValuePtrs = (sbyte**)Marshal.AllocHGlobal(count * sizeof(sbyte*));
                this.storageOptionsKeySlices = new KernelStringSlice[count];
                this.storageOptionsValueSlices = new KernelStringSlice[count];

                foreach (KeyValuePair<string, string> kvp in tableStorageOptions.StorageOptions)
                {
                    (GCHandle keyHandle, IntPtr keyPtr) = kvp.Key.ToPinnedSBytePointer();
                    (GCHandle valueHandle, IntPtr valuePtr) = kvp.Value.ToPinnedSBytePointer();

                    this.storageOptionsKeyHandles[index] = keyHandle;
                    this.storageOptionsValueHandles[index] = valueHandle;
                    this.storageOptionsKeyPtrs[index] = (sbyte*)keyPtr.ToPointer();
                    this.storageOptionsValuePtrs[index] = (sbyte*)valuePtr.ToPointer();
                    this.storageOptionsKeySlices[index] = new KernelStringSlice { ptr = this.storageOptionsKeyPtrs[index], len = (nuint)kvp.Key.Length };
                    this.storageOptionsValueSlices[index] = new KernelStringSlice { ptr = this.storageOptionsValuePtrs[index], len = (nuint)kvp.Value.Length };

                    Methods.set_builder_option(this.engineBuilderPtr, this.storageOptionsKeySlices[index], this.storageOptionsValueSlices[index]);

                    index++;
                }

                this.sharedExternEngine = Methods.builder_build(this.engineBuilderPtr);
                if (this.sharedExternEngine.tag != ExternResultHandleSharedExternEngine_Tag.OkHandleSharedExternEngine)
                {
                    throw new InvalidOperationException("Could not build engine from the engine builder sent to Delta Kernel.");
                }
                this.sharedExternEnginePtr = this.sharedExternEngine.Anonymous.Anonymous1.ok;
                this.tableSnapshot = new ManagedTableSnapshot(this.tableLocationSlice, this.sharedExternEnginePtr);
                this.isKernelAllocated = true;
            }
        }

        #region Delta Kernel table operations

        internal override long Version()
        {
            if (this.isKernelAllocated && this.isKernelSupported)
            {
                unsafe
                {
                    return unchecked((long)Methods.version(this.tableSnapshot.Snapshot));
                }
            }
            return base.Version();
        }

        internal override string Uri()
        {
            if (this.isKernelAllocated && this.isKernelSupported)
            {
                unsafe
                {
                    IntPtr tableRootPtr = IntPtr.Zero;
                    try
                    {
                        tableRootPtr = (IntPtr)Methods.snapshot_table_root(this.tableSnapshot.Snapshot, this.allocateStringCallbackPtr);

                        // Kernel returns an extra "/", delta-rs does not
                        //
                        return Marshal.PtrToStringAnsi(tableRootPtr)?.TrimEnd('/') ?? string.Empty;
                    }
                    finally
                    {
                        if (tableRootPtr != IntPtr.Zero) Marshal.FreeHGlobal(tableRootPtr);
                    }
                }
            }
            return base.Uri();
        }

        /// <remarks>
        /// Kernel does not support "loading". The moment the user invokes
        /// this, we run into a state inconsistency, because further calls to "Version()" via Kernel
        /// will return the latest version, as opposed to the version the user loaded.
        ///
        /// So - we un-support Kernel and invoke "delta-rs" going forward.
        /// </remarks>
        internal override async Task LoadVersionAsync(ulong version, ICancellationToken cancellationToken)
        {
            this.isKernelSupported = false;
            await base.LoadVersionAsync(version, cancellationToken).ConfigureAwait(false);
        }

        /// <remarks>
        /// <see cref="LoadVersionAsync"/> remarks.
        /// </remarks>
        internal override async Task LoadTimestampAsync(long timestampMilliseconds, ICancellationToken cancellationToken)
        {
            this.isKernelSupported = false;
            await base.LoadTimestampAsync(timestampMilliseconds, cancellationToken).ConfigureAwait(false);
        }

        #endregion Delta Kernel table operations

        #region SafeHandle implementation

        protected override unsafe bool ReleaseHandle()
        {
            if (this.isKernelAllocated)
            {
                this.tableSnapshot.Dispose();

                if (this.tableLocationHandle.IsAllocated) this.tableLocationHandle.Free();
                if (this.allocateErrorCallbackHandle.IsAllocated) this.allocateErrorCallbackHandle.Free();
                if (this.allocateStringCallbackHandle.IsAllocated) this.allocateStringCallbackHandle.Free();
                foreach (GCHandle handle in this.storageOptionsKeyHandles) if (handle.IsAllocated) handle.Free();
                foreach (GCHandle handle in this.storageOptionsValueHandles) if (handle.IsAllocated) handle.Free();

                Marshal.FreeHGlobal((IntPtr)this.storageOptionsKeyPtrs);
                Marshal.FreeHGlobal((IntPtr)this.storageOptionsValuePtrs);

                // EngineBuilder* does not need to be deallocated
                //
                // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1727978348653369
                //
                Methods.free_engine(sharedExternEnginePtr);
            }

            return base.ReleaseHandle();
        }

        #endregion SafeHandle implementation
    }
}
