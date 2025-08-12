using System;
using System.Collections.Generic;

namespace DeltaLake.Table
{
    /// <summary>
    /// Options for controlling the Delta table Optimize operation.
    /// </summary>
    public sealed class OptimizeOptions
    {
        /// <summary>
        /// The maximum number of concurrent tasks to use during optimization.
        /// </summary>
        public uint? MaxConcurrentTasks { get; set; }

        /// <summary>
        /// The maximum spill size (in bytes) allowed during optimization.
        /// </summary>
        public ulong? MaxSpillSize { get; set; }

        /// <summary>
        /// The minimum interval between commits during optimization.
        /// </summary>
        public TimeSpan? MinCommitInterval { get; set; }

        /// <summary>
        /// If true, preserves the original insertion order of data.
        /// </summary>
        public bool? PreserveInsertionOrder { get; set; }

        /// <summary>
        /// The target file size (in bytes) for optimized files.
        /// </summary>
        public ulong? TargetSize { get; set; }

        /// <summary>
        /// List of column names to use for Z-Ordering during optimization. If <see cref="OptimizeType"/> is ZOrder,
        /// this must be set and non-empty, and these columns will be used to perform Z-Order clustering, improving
        /// query performance for those columns. Ignored if <see cref="OptimizeType"/> is BinPack.
        /// </summary>
        public IReadOnlyList<string>? ZOrderColumns { get; set; }

        /// <summary>
        /// The optimization strategy to use (e.g. BinPack or ZOrder).
        /// </summary>
        public OptimizeType OptimizeType { get; set; } = OptimizeType.BinPack;
    }

    /// <summary>
    /// Specifies the type of optimization to perform on the Delta table.
    /// </summary>
#pragma warning disable CA1008
#pragma warning disable CA1028
    public enum OptimizeType : uint
#pragma warning restore CA1008
#pragma warning restore CA1028
    {
        /// <summary>
        /// BinPack optimization: packs small files into larger ones for efficiency.
        /// </summary>
        BinPack = 1,

        /// <summary>
        /// Z-Order optimization: reorders data to improve query performance for specific columns.
        /// </summary>
        ZOrder = 2,
    }
}