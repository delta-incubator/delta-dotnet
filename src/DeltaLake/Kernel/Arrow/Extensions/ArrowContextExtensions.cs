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
using DeltaLake.Kernel.State;

namespace DeltaLake.Kernel.Arrow.Extensions
{
    /// <summary>
    /// Static class containing extension methods for Arrow Context.
    /// </summary>
    internal static class ArrowContextExtensions
    {
        /// <summary>
        /// Converts an Arrow Context to an Arrow Table.
        /// </summary>
        /// <param name="context">The Arrow Context to convert.</param>
        /// <returns>The converted Arrow Table.</returns>
        internal static unsafe Apache.Arrow.Table ToTable(this ArrowContext context)
        {
            if (context == null || context.Schema == null)
            {
                throw new ArgumentException($"Invalid ArrowContext provided for Table conversion: cannot convert to Table without schema.");
            }
            return Apache.Arrow.Table.TableFromRecordBatches(context.Schema, context.ToRecordBatches());
        }

        internal static unsafe List<RecordBatch> ToRecordBatches(this ArrowContext context)
        {
            if (context == null || context.NumBatches == 0 || context.Batches == null)
            {
                throw new ArgumentException($"Invalid ArrowContext provided for RecordBatch conversion: contains one or more null pointers with {context.NumBatches} Record Batches.");
            }
            List<RecordBatch> recordBatches = new(context.NumBatches);
            for (int i = 0; i < context.NumBatches; i++) recordBatches.Add(*context.Batches[i]);
            return recordBatches;
        }
    }
}
