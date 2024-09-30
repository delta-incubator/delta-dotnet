namespace DeltaLake.Table
{
    /// <summary>
    /// Protocol information for a delta table
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct ProtocolInfo
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        /// <summary>
        /// Table minimum reader version
        /// </summary>
        public int MinimumReaderVersion { get; init; }

        /// <summary>
        /// Table minimum writer version
        /// </summary>
        public int MinimumWriterVersion { get; init; }
    }
}