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

using System.Collections.Generic;
using Apache.Arrow;
using Apache.Arrow.Types;
using DeltaLake.Table;

namespace DeltaLake.Kernel.Builders
{
    /// <summary>
    /// Builds an Arrow RecordBatch conforming to the delta-kernel add-files schema.
    /// Schema: path (Utf8), partitionValues (Map&lt;Utf8,Utf8&gt;), size (Int64),
    ///         modificationTime (Int64), dataChange (Boolean)
    /// </summary>
    internal static class AddActionRecordBatchBuilder
    {
        private static readonly MapType PartitionMapType = new MapType(
            new Field("key", StringType.Default, nullable: false),
            new Field("value", StringType.Default, nullable: true));

        private static readonly Schema AddFilesSchema = new Schema.Builder()
            .Field(new Field("path", StringType.Default, nullable: false))
            .Field(new Field("partitionValues", PartitionMapType, nullable: false))
            .Field(new Field("size", Int64Type.Default, nullable: false))
            .Field(new Field("modificationTime", Int64Type.Default, nullable: false))
            .Field(new Field("dataChange", BooleanType.Default, nullable: false))
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
            var keyBuilder = mapBuilder.KeyBuilder as StringArray.Builder;
            var valueBuilder = mapBuilder.ValueBuilder as StringArray.Builder;
            var sizeBuilder = new Int64Array.Builder();
            var modTimeBuilder = new Int64Array.Builder();
            var dataChangeBuilder = new BooleanArray.Builder();

            foreach (var action in actions)
            {
                pathBuilder.Append(action.Path);
                sizeBuilder.Append(action.Size);
                modTimeBuilder.Append(action.ModificationTime);
                dataChangeBuilder.Append(action.DataChange);

                mapBuilder.Append();
                foreach (KeyValuePair<string, string?> kvp in action.PartitionValues)
                {
                    keyBuilder!.Append(kvp.Key);
                    if (kvp.Value != null)
                    {
                        valueBuilder!.Append(kvp.Value);
                    }
                    else
                    {
                        valueBuilder!.AppendNull();
                    }
                }
            }

            return new RecordBatch(AddFilesSchema, new IArrowArray[]
            {
                pathBuilder.Build(),
                mapBuilder.Build(),
                sizeBuilder.Build(),
                modTimeBuilder.Build(),
                dataChangeBuilder.Build(),
            }, numRows);
        }
    }
}
