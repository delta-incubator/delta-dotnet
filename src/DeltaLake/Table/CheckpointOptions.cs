namespace DeltaLake.Table
{
    /// <summary>
    /// Determines the format of the checkpoint written by a checkpoint operation.
    /// </summary>
    public enum CheckpointFormat
    {
        /// <summary>
        /// Let the kernel auto-pick a V1 or V2 checkpoint based on the table's protocol
        /// features.
        /// </summary>
        Auto,

        /// <summary>
        /// Force a classic V1 checkpoint (a single checkpoint parquet file, no sidecars).
        /// </summary>
        V1,

        /// <summary>
        /// Force a V2 checkpoint whose file actions are written inline in the checkpoint
        /// file (no sidecars). Requires a table that supports the <c>v2Checkpoint</c> feature.
        /// </summary>
        V2NoSidecar,

        /// <summary>
        /// Force a V2 checkpoint whose file actions are written into separate sidecar parquet
        /// files under <c>_delta_log/_sidecars/</c>. Requires a table that supports the
        /// <c>v2Checkpoint</c> feature.
        /// </summary>
        V2WithSidecar,
    }

    /// <summary>
    /// Options controlling how a checkpoint is written.
    /// </summary>
    public record CheckpointOptions
    {
        /// <summary>
        /// The checkpoint format to write. Defaults to <see cref="CheckpointFormat.Auto"/>.
        /// </summary>
        public CheckpointFormat Format { get; init; } = CheckpointFormat.Auto;

        /// <summary>
        /// Optional hint for the number of file actions to place in each sidecar file. Should be used only
        /// when <see cref="Format"/> is <see cref="CheckpointFormat.V2WithSidecar"/>; <see langword="null"/>
        /// lets the kernel choose a default.
        /// </summary>
        public ulong? FileActionsPerSidecarHint { get; init; }
    }
}
