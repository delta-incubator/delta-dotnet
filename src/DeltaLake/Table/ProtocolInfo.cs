namespace DeltaLake.Table
{
    public readonly struct ProtocolInfo
    {
        public int MinimumReaderVersion { get; init; }

        public int MinimumWriterVersion { get; init; }
    }
}