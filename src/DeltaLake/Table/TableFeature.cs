namespace DeltaLake.Table
{
    /// <summary>
    /// A table feature that can be added to a Delta table protocol.
    /// </summary>
    /// <remarks>
    /// Only <see cref="V2Checkpoint"/> is currently supported by
    /// <see cref="Interfaces.ITable.AddTableFeaturesAsync(System.Collections.Generic.IReadOnlyCollection{TableFeature}, System.Threading.CancellationToken)"/>.
    /// Other values are reserved for future delta-dotnet support and are rejected explicitly.
    /// </remarks>
    public enum TableFeature
    {
        /// <summary>Column mapping.</summary>
        ColumnMapping,

        /// <summary>Deletion vectors.</summary>
        DeletionVectors,

        /// <summary>Timestamps without a time zone.</summary>
        TimestampWithoutTimezone,

        /// <summary>V2 checkpoints, including checkpoint sidecars.</summary>
        V2Checkpoint,

        /// <summary>Append-only table enforcement.</summary>
        AppendOnly,

        /// <summary>Invariant enforcement.</summary>
        Invariants,

        /// <summary>Check constraints.</summary>
        CheckConstraints,

        /// <summary>Change data feed.</summary>
        ChangeDataFeed,

        /// <summary>Generated columns.</summary>
        GeneratedColumns,

        /// <summary>Identity columns.</summary>
        IdentityColumns,

        /// <summary>Row tracking.</summary>
        RowTracking,

        /// <summary>Domain metadata actions.</summary>
        DomainMetadata,

        /// <summary>Iceberg compatibility version 1.</summary>
        IcebergCompatV1,

        /// <summary>Materialized partition columns.</summary>
        MaterializePartitionColumns,
    }
}