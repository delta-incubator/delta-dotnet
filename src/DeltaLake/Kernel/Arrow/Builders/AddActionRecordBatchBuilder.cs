// -----------------------------------------------------------------------------
// <summary>
// Builds Arrow RecordBatch conforming to the delta-kernel add-files schema
// for registering pre-written Parquet files in the Delta transaction log.
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
using Apache.Arrow.Types;
using DeltaLake.Table;

namespace DeltaLake.Kernel.Arrow.Builders
{
    /// <summary>
    /// Builds an Arrow RecordBatch conforming to the delta-kernel add-files schema.
    /// Schema: path (Utf8), partitionValues (Map&lt;Utf8,Utf8&gt;), size (Int64),
    ///         modificationTime (Int64), stats (Struct{numRecords: Int64})
    /// </summary>
    internal static class AddActionRecordBatchBuilder
    {
        private static readonly MapType PartitionMapType = new MapType(
            new Field("key", StringType.Default, nullable: false),
            new Field("value", StringType.Default, nullable: true));

        internal static readonly StructType StatsStructType = new StructType(
            new List<Field>
            {
                new Field("numRecords", Int64Type.Default, nullable: true),
            });

        internal static readonly Schema AddFilesSchema = new Schema.Builder()
            .Field(new Field("path", StringType.Default, nullable: false))
            .Field(new Field("partitionValues", PartitionMapType, nullable: false))
            .Field(new Field("size", Int64Type.Default, nullable: false))
            .Field(new Field("modificationTime", Int64Type.Default, nullable: false))
            .Field(new Field("stats", StatsStructType, nullable: true))
            .Build();

        /// <summary>
        /// Build an Arrow RecordBatch from a list of file add actions.
        /// Each row represents one file to add to the Delta table.
        /// </summary>
        /// <param name="actions">The list of add actions describing pre-written files.</param>
        /// <returns>An Arrow RecordBatch with the add-files schema.</returns>
        public static RecordBatch Build(IReadOnlyList<AddAction> actions)
        {
            int numRows = actions.Count;

            var pathBuilder = new StringArray.Builder();
            var mapBuilder = new MapArray.Builder(PartitionMapType);
            var keyBuilder = (StringArray.Builder)mapBuilder.KeyBuilder;
            var valueBuilder = (StringArray.Builder)mapBuilder.ValueBuilder;
            var sizeBuilder = new Int64Array.Builder();
            var modTimeBuilder = new Int64Array.Builder();
            var numRecordsBuilder = new Int64Array.Builder();

            // Delta protocol defines path as a URI per RFC 2396 (§6.1: case-sensitive comparison).
            var seenPaths = new HashSet<string>(numRows, StringComparer.Ordinal);

            foreach (var action in actions)
            {
                if (!seenPaths.Add(action.Path))
                {
                    throw new ArgumentException(
                        $"Duplicate file path detected: '{action.Path}'",
                        nameof(actions));
                }

                pathBuilder.Append(action.Path);
                sizeBuilder.Append(action.Size);
                modTimeBuilder.Append(action.ModificationTime);
                numRecordsBuilder.AppendNull();

                mapBuilder.Append();
                if (action.PartitionValues != null)
                {
                    foreach (KeyValuePair<string, string?> kvp in action.PartitionValues)
                    {
                        keyBuilder.Append(kvp.Key);
                        if (kvp.Value != null)
                        {
                            valueBuilder.Append(kvp.Value);
                        }
                        else
                        {
                            valueBuilder.AppendNull();
                        }
                    }
                }
            }

            var numRecordsArray = numRecordsBuilder.Build();
            var statsArray = new StructArray(StatsStructType, numRows,
                new IArrowArray[] { numRecordsArray },
                ArrowBuffer.Empty);

            return new RecordBatch(AddFilesSchema, new IArrowArray[]
            {
                pathBuilder.Build(),
                mapBuilder.Build(),
                sizeBuilder.Build(),
                modTimeBuilder.Build(),
                statsArray,
            }, numRows);
        }
    }
}
