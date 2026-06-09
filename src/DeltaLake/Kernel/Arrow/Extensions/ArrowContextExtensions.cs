// -----------------------------------------------------------------------------
// <summary>
// Extension methods for transforming Arrow Context.
// </summary>
//
// <copyright company="The Delta Lake Project Authors">
// Copyright (2024) The Delta Lake Project Authors.  All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Apache.Arrow;
using Apache.Arrow.C;
using Apache.Arrow.Types;
using DeltaLake.Extensions;
using DeltaLake.Kernel.Interop;
using DeltaLake.Kernel.State;
using DeltaArrowContext = DeltaLake.Kernel.State.ArrowContext;

namespace DeltaLake.Kernel.Arrow.Extensions
{
    /// <summary>
    /// Static class containing extension methods for Arrow Context.
    /// </summary>
    internal static class ArrowContextExtensions
    {

        #region Extension Methods

        /// <summary>
        /// Builds an <see cref="Apache.Arrow.Table"/> from an already-imported set of
        /// record batches. Accepts caller-owned batches so the caller is free to manage
        /// <see cref="ArrowContextHandle"/> lifetime explicitly.
        /// </summary>
        /// <param name="schema">The Arrow schema describing every batch.</param>
        /// <param name="batches">The imported <see cref="RecordBatch"/> instances.</param>
        /// <returns>An <see cref="Apache.Arrow.Table"/> sharing the underlying buffers.</returns>
        internal static Apache.Arrow.Table BuildSanitizedTable(Schema schema, IReadOnlyList<RecordBatch> batches)
        {
            List<RecordBatch> batchList = batches as List<RecordBatch> ?? new List<RecordBatch>(batches);
            return Apache.Arrow.Table.TableFromRecordBatches(schema, batchList);
        }

        /// <summary>
        /// Concatenates the imported record batches into a single <see cref="RecordBatch"/>,
        /// applying the StringArray / PrimitiveArray null-value-buffer sanitization required
        /// by <see cref="Microsoft.Data.Analysis.DataFrame.FromArrowRecordBatch"/> on macOS
        /// ARM64. Accepts caller-owned batches so the caller manages
        /// <see cref="ArrowContextHandle"/> lifetime explicitly.
        /// </summary>
        /// <param name="schema">The Arrow schema describing every batch.</param>
        /// <param name="batches">The imported <see cref="RecordBatch"/> instances.</param>
        /// <returns>A single concatenated <see cref="RecordBatch"/> safe for DataFrame import.</returns>
        internal static RecordBatch ConcatenateAndSanitize(Schema schema, IReadOnlyList<RecordBatch> batches)
        {
            List<IArrowArray> concatenatedColumns = new();

            foreach (Field field in schema.FieldsList)
            {
                List<IArrowArray> columnArrays = new();
                foreach (RecordBatch recordBatch in batches)
                {
                    IArrowArray column = recordBatch.Column(field.Name);
                    columnArrays.Add(column);
                }
                IArrowArray concatenatedColumn = ArrowArrayConcatenator.Concatenate(columnArrays);
                concatenatedColumn = SanitizeColumnIfNeeded(concatenatedColumn);
                concatenatedColumns.Add(concatenatedColumn);
            }
            return new RecordBatch(schema, concatenatedColumns, concatenatedColumns[0].Length);
        }

        /// <summary>
        /// Applies the null-value-buffer sentinel substitution to a concatenated column when
        /// its type is one of the known affected variants (<see cref="StringArray"/>,
        /// <see cref="PrimitiveArray{T}"/> of <see cref="int"/> or <see cref="long"/>).
        /// Other column types are returned unchanged.
        /// </summary>
        private static IArrowArray SanitizeColumnIfNeeded(IArrowArray concatenatedColumn)
        {
            if (concatenatedColumn is StringArray stringArray)
            {
                return EnsureNonNullValueBuffer(stringArray);
            }
            if (concatenatedColumn is PrimitiveArray<int> int32Array)
            {
                return EnsureNonNullValueBuffer(int32Array, sizeof(int));
            }
            if (concatenatedColumn is PrimitiveArray<long> int64Array)
            {
                return EnsureNonNullValueBuffer(int64Array, sizeof(long));
            }
            return concatenatedColumn;
        }

        /// <summary>
        /// Returns a <see cref="StringArray"/> whose values buffer is guaranteed to have a
        /// non-null backing reference, preserving zero-copy semantics for the validity and
        /// offsets buffers.
        /// </summary>
        /// <remarks>
        /// Apache.Arrow 23's <c>CArrowArrayImporter.ImportCArrayBuffer</c> returns
        /// <see cref="ArrowBuffer.Empty"/> (backed by <see cref="Memory{T}.Empty"/>, whose
        /// internal reference is <c>null</c>) for a StringArray's values buffer when the
        /// computed values length is 0 — i.e. when the column contains only nulls or only
        /// empty strings.
        ///
        /// Microsoft.Data.Analysis 0.21.1's <c>ArrowStringDataFrameColumn.GetValueImplementation</c>
        /// then executes:
        /// <code>
        ///     fixed (byte* data = &amp;MemoryMarshal.GetReference(bytes))
        ///         return Encoding.UTF8.GetString(data, bytes.Length);
        /// </code>
        /// On a span over <c>Memory&lt;byte&gt;.Empty</c>, <c>GetReference</c> yields a null
        /// managed reference, <c>fixed</c> pins it to <c>data == null</c>, and
        /// <c>Encoding.UTF8.GetString((byte*)null, 0)</c> throws <see cref="ArgumentNullException"/>.
        /// This surfaces on macOS ARM64 where the JIT does not elide the null reference into
        /// a non-null sentinel as it does on x64.
        ///
        /// The fix substitutes a single-byte managed values buffer only when the original is
        /// empty, providing a non-null backing pointer for MDA's <c>fixed</c>/<c>GetString</c>
        /// pattern. The offsets and validity buffers — which carry the actual data — remain
        /// the original zero-copy Arrow-imported buffers.
        /// </remarks>
        private static StringArray EnsureNonNullValueBuffer(StringArray source)
        {
            if (source.ValueBuffer.Length != 0)
            {
                return source;
            }

            ArrayData original = source.Data;
            ArrowBuffer[] buffers = new ArrowBuffer[original.Buffers.Length];
            buffers[0] = original.Buffers[0]; // validity (zero-copy)
            buffers[1] = original.Buffers[1]; // offsets  (zero-copy)
            buffers[2] = new ArrowBuffer(new byte[1]); // sentinel: non-null managed backing
            return new StringArray(new ArrayData(
                original.DataType,
                original.Length,
                original.NullCount,
                original.Offset,
                buffers));
        }

        /// <summary>
        /// Returns a primitive <see cref="PrimitiveArray{T}"/> whose values buffer is
        /// guaranteed to have a non-null backing reference. Same root cause and remedy as
        /// <see cref="EnsureNonNullValueBuffer(StringArray)"/>; a primitive array has only
        /// validity + values buffers (no offsets). Sentinel size equals one element width
        /// so the buffer remains correctly aligned.
        /// </summary>
        private static PrimitiveArray<T> EnsureNonNullValueBuffer<T>(PrimitiveArray<T> source, int elementByteSize)
            where T : struct, IEquatable<T>
        {
            if (source.ValueBuffer.Length != 0)
            {
                return source;
            }

            ArrayData original = source.Data;
            ArrowBuffer[] buffers = new ArrowBuffer[original.Buffers.Length];
            buffers[0] = original.Buffers[0]; // validity (zero-copy)
            buffers[1] = new ArrowBuffer(new byte[elementByteSize]); // sentinel: non-null managed backing
            return (PrimitiveArray<T>)ArrowArrayFactory.BuildArray(new ArrayData(
                original.DataType,
                original.Length,
                original.NullCount,
                original.Offset,
                buffers));
        }

        /// <summary>
        /// Converts an Arrow Context to a list of <see cref="Apache.Arrow.RecordBatch"/>"/>
        /// and <see cref="Apache.Arrow.Schema"/>.
        /// </summary>
        /// <param name="context">The Arrow Context to convert.</param>
        /// <returns>The converted tuple of Arrow Schema and Record Batches.</returns>
        internal static (Schema, List<RecordBatch>) ToSchematizedBatches(this DeltaArrowContext context)
        {
            context.ValidateContext();

            List<RecordBatch> recordBatches = new(context.NumBatches);
            List<Schema> schemas = new(context.NumBatches);

            for (int i = 0; i < context.NumBatches; i++)
            {
                unsafe
                {
                    ArrowFFIData* arrowStructPtr = context.ArrowStructs[i];
                    ParquetStringPartitions* partitionsPtr = context.Partitions[i];

                    Schema schemaWithoutPartition = CArrowSchemaImporter.ImportSchema((CArrowSchema*)&arrowStructPtr->schema);
                    RecordBatch batchWithoutPartition = CArrowArrayImporter.ImportRecordBatch((CArrowArray*)&arrowStructPtr->array, schemaWithoutPartition);

                    Schema schemaWithPartition = AddPartitionColumnsToSchema(schemaWithoutPartition, partitionsPtr);
                    RecordBatch batchWithPartition = AddPartitionColumnsToRecordBatch(batchWithoutPartition, partitionsPtr);

                    recordBatches.Add(batchWithPartition);
                    schemas.Add(schemaWithPartition);
                }
            }

            for (int i = 1; i < schemas.Count; i++)
            {
                if (schemas[i] == schemas[0]) throw new InvalidOperationException($"All schemas must be the same in - got {i}th {schemas[i]} != 0th {schemas[0]}");
            }

            return (schemas[0], recordBatches);
        }

        #endregion Extension Methods

        #region Private Methods

        private static void ValidateContext(this DeltaArrowContext context)
        {
            if (context.NumBatches == 0)
            {
                throw new InvalidOperationException("Arrow Context must contain at least one RecordBatch");
            }
        }

#pragma warning disable CA1859, IDE0060 // Although we're not using partitionValue right now, it will be used when Kernel supports reporting Arrow Schema
        private static unsafe IArrowType DeterminePartitionColumnType(string colName, string colValue)
        {
            // Currently, there's no way to determine the type of the partition,
            // because the Kernel always represents partition values as strings in CStringMap.
            //
            // We have a request with Kernel team here to get back the Arrow Schema from
            // the Delta Transaction Log:
            //
            // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1728178727958499
            // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1728001059452499?thread_ts=1727999835.930339&cid=C04TRPG3LHZ
            //
            return StringType.Default;
        }
#pragma warning restore CA1859, IDE0060

        private static unsafe Schema AddPartitionColumnsToSchema(Apache.Arrow.Schema originalSchema, ParquetStringPartitions* partitionsPtr)
        {
            Apache.Arrow.Schema.Builder schemaBuilder = new();
            foreach (Field field in originalSchema.FieldsList)
            {
                schemaBuilder = schemaBuilder.Field(field);
            }

            for (int i = 0; i < partitionsPtr->Len; i++)
            {
#pragma warning disable CS8600, CS8604 // If Kernel sends us back null pointers, we are in trouble anyway
                string colName = MarshalExtensions.PtrToStringUTF8((IntPtr)partitionsPtr->ColNames[i]);
                string colValue = MarshalExtensions.PtrToStringUTF8((IntPtr)partitionsPtr->ColValues[i]);
                IArrowType dataType = DeterminePartitionColumnType(colName, colValue);
#pragma warning restore CS8600, CS8604

                Field field = new(colName, dataType, nullable: true);
                schemaBuilder = schemaBuilder.Field(field);
            }

            return schemaBuilder.Build();
        }

        private static unsafe RecordBatch AddPartitionColumnsToRecordBatch(RecordBatch recordBatch, ParquetStringPartitions* partitionsPtr)
        {
            Apache.Arrow.Schema.Builder schemaBuilder = new();
            var columnList = new List<IArrowArray>();

            // Append the original fieldList to the schema builder.
            //
            foreach (Field field in recordBatch.Schema.FieldsList) schemaBuilder = schemaBuilder.Field(field);

            // Append the original columns to the column list.
            //
            for (int i = 0; i < recordBatch.Schema.FieldsList.Count; i++) columnList.Add(recordBatch.Column(i));

            // Convert each of the partition column metadata structs into actual
            // columns, then add it to the schema builder and column list.
            //
            for (int i = 0; i < partitionsPtr->Len; i++)
            {
                StringArray.Builder columnBuilder = new();

#pragma warning disable CS8600
                string colName = MarshalExtensions.PtrToStringUTF8((IntPtr)partitionsPtr->ColNames[i]);
                string colValue = MarshalExtensions.PtrToStringUTF8((IntPtr)partitionsPtr->ColValues[i]);
#pragma warning restore CS8600

                Field field = new(colName, StringType.Default, nullable: true);
                schemaBuilder = schemaBuilder.Field(field);

                for (int j = 0; j < recordBatch.Length; j++)
                {
                    columnBuilder = columnBuilder.Append(colValue ?? "");
                }
                columnList.Add(columnBuilder.Build());
            }
            return new RecordBatch(schemaBuilder.Build(), columnList, recordBatch.Length);
        }

        #endregion Private Methods
    }
}
