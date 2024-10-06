// -----------------------------------------------------------------------------
// <summary>
// Arrow InterOp handler for Delta Kernel FFI that converts Parquet files to
// Arrow in-memory with zero-copy.
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
using Apache.Arrow;
using Apache.Arrow.C;
using Apache.Arrow.Types;
using DeltaLake.Extensions;
using DeltaLake.Kernel.Callbacks.Allocators;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Callbacks.Visit;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.State;

namespace DeltaLake.Kernel.Arrow.Handlers
{
    /// <summary>
    /// Arrow InterOp handler for Delta Kernel FFI, performs zero-copy
    /// operations.
    /// </summary>
    internal class ArrowFFIInterOpHandler : IArrowInteropHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrowFFIInterOpHandler"/> class.
        /// </summary>
        public ArrowFFIInterOpHandler() { }

        #region IArrowInteropHandler implementation

        /// <inheritdoc/>
        /// <remarks>
        /// Caller is responsible for freeing the allocated <see
        /// cref="RecordBatch"/>* AND <see cref="RecordBatch"/>** when disposing
        /// <see cref="ArrowContext"/>.
        /// </remarks>
        public unsafe void ZeroCopyRecordBatchToArrowContext(
            ArrowContext* context,
            ArrowFFIData* arrowData,
            PartitionList* partitionCols,
            CStringMap* partitionValues
        )
        {
            // Allocate memory for the new RecordBatch and a newly expanded (old
            // size + 1) RecordBatch pointers array that will replace the old
            // pointers array in ArrowContext.Batches.
            //
            RecordBatch* recordBatchPtr = (RecordBatch*)Marshal.AllocHGlobal(sizeof(RecordBatch));
            RecordBatch** newBatchPointersArrayPtr = (RecordBatch**)Marshal.AllocHGlobal(sizeof(RecordBatch*) * (context->NumBatches + 1));

            Apache.Arrow.Schema arrowSchema = ConvertFFISchemaToArrowSchema(&arrowData->schema);
            context->Schema = AddPartitionColumnsToSchema(arrowSchema, partitionCols, partitionValues);

            *recordBatchPtr = ConvertFFIArrayToArrowRecordBatch(&arrowData->array, arrowSchema);
#pragma warning disable CA2000
            *recordBatchPtr = AddPartitionColumnsToRecordBatch(*recordBatchPtr, partitionCols, partitionValues);
#pragma warning restore CA2000

            if (recordBatchPtr == null)
            {
                throw new InvalidOperationException("Failed to add partition columns, not adding Record Batch");
            }

            // Copy old pre-allocated RecordBatch pointers from the
            // ArrowContext, if exists. This is a zero-copy operation,
            // since we're just copying the pointers around rapidly.
            //
            if (context->NumBatches > 0)
            {
                for (int i = 0; i < context->NumBatches; i++)
                {
                    newBatchPointersArrayPtr[i] = context->Batches[i];
                }
            }

            // Append the new RecordBatch that just got read from the Kernel to
            // the end, and set the final size from this operation.
            //
            newBatchPointersArrayPtr[context->NumBatches] = recordBatchPtr;
            context->Batches = newBatchPointersArrayPtr;
            context->NumBatches++;
        }

        /// <inheritdoc/>
        public unsafe void ReadParquetFileAsArrow(
            EngineContext* context,
            KernelStringSlice path,
            KernelBoolSlice selectionVector
        )
        {
#pragma warning disable CS8600
            string tableRoot = Marshal.PtrToStringAnsi((IntPtr)context->TableRoot);
#pragma warning restore CS8600
            string parquetAbsolutePath = $"{tableRoot}{Marshal.PtrToStringAnsi((IntPtr)path.ptr, (int)path.len)}";

            (GCHandle parquetAbsolutePathHandle, IntPtr gcPinnedParquetAbsolutePathPtr) = parquetAbsolutePath.ToPinnedSBytePointer();
            KernelStringSlice parquetAbsolutePathSlice = new() { ptr = (sbyte*)gcPinnedParquetAbsolutePathPtr, len = (nuint)parquetAbsolutePath.Length };
            FileMeta parquetMeta = new() { path = parquetAbsolutePathSlice };

            try
            {
                ExternResultHandleExclusiveFileReadResultIterator isParquetFileReadOk = Methods.read_parquet_file(context->Engine, &parquetMeta, context->Schema);
                if (isParquetFileReadOk.tag != ExternResultHandleExclusiveFileReadResultIterator_Tag.OkHandleExclusiveFileReadResultIterator)
                {
                    throw new InvalidOperationException($"Kernel failed to read parquet file at: {parquetAbsolutePath}");
                }

                ExclusiveFileReadResultIterator* arrowReadIterator = isParquetFileReadOk.Anonymous.Anonymous1.ok;
                for (; ; )
                {
                    ExternResultbool isArrowResultReadOk = Methods.read_result_next(arrowReadIterator, context, Marshal.GetFunctionPointerForDelegate(VisitCallbacks.IngestArrowData));
                    if (isArrowResultReadOk.tag != ExternResultbool_Tag.Okbool)
                    {
                        KernelReadError* arrowReadError = (KernelReadError*)isArrowResultReadOk.Anonymous.Anonymous2.err;
                        throw new InvalidOperationException($"Failed to iterate on reading arrow data from parquet: {arrowReadError->Message}");
                    }
                    else if (!isArrowResultReadOk.Anonymous.Anonymous1.ok) break;
                }
                Methods.free_read_result_iter(arrowReadIterator);
            }
            finally
            {
                if (parquetAbsolutePathHandle.IsAllocated) parquetAbsolutePathHandle.Free();
            }
        }

        #endregion IArrowInteropHandler implementation

        #region Private methods

        private static unsafe Apache.Arrow.Schema ConvertFFISchemaToArrowSchema(FFI_ArrowSchema* ffiSchema)
        {
            CArrowSchema* clonedSchema = CArrowSchema.Create();
            Apache.Arrow.Schema convertedSchema = CArrowSchemaImporter.ImportSchema((CArrowSchema*)ffiSchema);
            CArrowSchema.Free(clonedSchema);
            return convertedSchema;
        }

        private static unsafe RecordBatch ConvertFFIArrayToArrowRecordBatch(FFI_ArrowArray* ffiArray, Apache.Arrow.Schema schema)
        {
            CArrowArray* clonedArray = CArrowArray.Create();
            RecordBatch generatedBatch = CArrowArrayImporter.ImportRecordBatch((CArrowArray*)ffiArray, schema);
            CArrowArray.Free(clonedArray);
            return generatedBatch;
        }

        private static unsafe RecordBatch AddPartitionColumnsToRecordBatch(RecordBatch recordBatch, PartitionList* partitionCols, CStringMap* partitionValues)
        {
            Apache.Arrow.Schema.Builder schemaBuilder = new();
            foreach (Field field in recordBatch.Schema.FieldsList)
            {
                schemaBuilder = schemaBuilder.Field(field);
            }

            var fields = new List<Field>(recordBatch.Schema.FieldsList);
            var columns = new List<IArrowArray>();
            for (int i = 0; i < recordBatch.Schema.FieldsList.Count; i++)
            {
                columns.Add(recordBatch.Column(i));
            }

            for (int i = 0; i < partitionCols->Len; i++)
            {
#pragma warning disable CS8600
                string colName = Marshal.PtrToStringAnsi((IntPtr)partitionCols->Cols[i]);
#pragma warning restore CS8600
                Field field = new(colName, StringType.Default, nullable: true);
                schemaBuilder = schemaBuilder.Field(field);
                fields.Add(field);

                StringArray.Builder columnBuilder = new();

                // The Kernel can currently only report String values back as
                // partition values, even if it's a different type (like
                // Integer, DateTime etc). This is a limitation of the Kernel
                // today, more information here:
                //
                // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1728178727958499
                //
#pragma warning disable CS1024, CS8629, CS8600 // If Kernel sends us back null pointers, we are in trouble anyway
                void* partitionValPtr = Methods.get_from_map(
                    partitionValues,
                    new KernelStringSlice
                    {
                        ptr = (sbyte*)partitionCols->Cols[i],
                        len = (ulong)colName?.Length
                    },
                    Marshal.GetFunctionPointerForDelegate<AllocateStringFn>(StringAllocatorCallbacks.AllocateString)
                );
                string partitionVal = partitionValPtr != null ? Marshal.PtrToStringAnsi((IntPtr)partitionValPtr) : String.Empty;
#pragma warning restore CS1024, CS8629, CS8600

                for (int j = 0; j < recordBatch.Length; j++)
                {
                    columnBuilder = columnBuilder.Append(partitionVal ?? "");
                }
                columns.Add(columnBuilder.Build());
            }
            return new RecordBatch(schemaBuilder.Build(), columns, recordBatch.Length);
        }

#pragma warning disable CA1859, IDE0060 // Although we're not using partitionValues right now, it will be used when Kernel supports reporting Arrow Schema
        private static unsafe IArrowType DeterminePartitionColumnType(string colName, CStringMap* partitionValues)
        {
            // Currently, there's no way to determine the type of the partition,
            // because the Kernel always represents partition values as strings in CStringMap.
            //
            // We have a request with Kernel team here to get back the Arrow Schema from
            // the Delta Transaction Log:
            //
            // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1728001059452499?thread_ts=1727999835.930339&cid=C04TRPG3LHZ
            //
            return StringType.Default;
        }
#pragma warning restore CA1859, IDE0060

        private static unsafe Apache.Arrow.Schema AddPartitionColumnsToSchema(Apache.Arrow.Schema originalSchema, PartitionList* partitionCols, CStringMap* partitionValues)
        {
            Apache.Arrow.Schema.Builder schemaBuilder = new();
            foreach (Field field in originalSchema.FieldsList)
            {
                schemaBuilder = schemaBuilder.Field(field);
            }

            for (int i = 0; i < partitionCols->Len; i++)
            {
#pragma warning disable CS8600, CS8604 // If Kernel sends us back null pointers, we are in trouble anyway
                string colName = Marshal.PtrToStringAnsi((IntPtr)partitionCols->Cols[i]);
                IArrowType dataType = DeterminePartitionColumnType(colName, partitionValues);
#pragma warning restore CS8600, CS8604

                Field field = new(colName, dataType, nullable: true);
                schemaBuilder = schemaBuilder.Field(field);
            }

            return schemaBuilder.Build();
        }

        #endregion Private methods
    }
}
