namespace DeltaLake.Tests.Table;

public enum TableIdentifier
{
    CheckpointWithPartitions,
    Checkpoints,
    CheckpointsTombstones,
    CheckpointsVacuumed,
    ConcurrentWorkers,
    Covid19NYT,
    Delta020,
    Delta08Empty,
    Delta08,
    Delta08Date,
    Delta08NullPartition,
    Delta08NumericPartition,
    Delta08Partition,
    Delta08SpecialPartition,
    Delta121OnlyStructStats,
    Delta220PartitionedTypes,
    DeltaLiveTable,
    Golden,
    HttpRequests,
    Issue1374,
    SimpleCommit,
    SimpleTable,
    SimpleTableFeatures,
    SimpleTableWithCdc,
    SimpleTableWithCheckPoint,
    TableWithColumnMapping,
    TableWithDeletionLogs,
    TableWithEdgeTimestamps,
    TableWithLiquidClustering,
    TableWithDvSmall,
    TableWithoutDvSmall,
    WithCheckpointNoLastcheckpoint,
}
public static class TableHelpers
{
    public static readonly IReadOnlyDictionary<TableIdentifier, string> Tables = new Dictionary<TableIdentifier, string>
    {
        [TableIdentifier.CheckpointWithPartitions] = "checkpoint_with_partitions",
        [TableIdentifier.Checkpoints] = "checkpoints",
        [TableIdentifier.CheckpointsTombstones] = "checkpoints_tombstones",
        [TableIdentifier.CheckpointsVacuumed] = "checkpoints_vacuumed",
        [TableIdentifier.ConcurrentWorkers] = "concurrent_workers",
        [TableIdentifier.Covid19NYT] = "COVID-19_NYT",
        [TableIdentifier.Delta020] = "delta-0.2.0",
        [TableIdentifier.Delta08Empty] = "delta-0.8-empty",
        [TableIdentifier.Delta08] = "delta-0.8.0",
        [TableIdentifier.Delta08Date] = "delta-0.8.0-date",
        [TableIdentifier.Delta08NullPartition] = "delta-0.8.0-null-partition",
        [TableIdentifier.Delta08NumericPartition] = "delta-0.8.0-numeric-partition",
        [TableIdentifier.Delta08Partition] = "delta-0.8.0-partitioned",
        [TableIdentifier.Delta08SpecialPartition] = "delta-0.8.0-special-partition",
        [TableIdentifier.Delta121OnlyStructStats] = "delta-1.2.1-only-struct-stats",
        [TableIdentifier.Delta220PartitionedTypes] = "delta-2.2.0-partitioned-types",
        [TableIdentifier.DeltaLiveTable] = "delta-live-table",
        [TableIdentifier.Golden] = Path.Join("golden", "data-reader-array-primitives"),
        [TableIdentifier.HttpRequests] = "http_requests",
        [TableIdentifier.Issue1374] = "issue_1374",
        [TableIdentifier.SimpleCommit] = "simple_commit",
        [TableIdentifier.SimpleTable] = "simple_table",
        [TableIdentifier.SimpleTableFeatures] = "simple_table_features",
        [TableIdentifier.SimpleTableWithCdc] = "simple_table_with_cdc",
        [TableIdentifier.SimpleTableWithCheckPoint] = "simple_table_with_checkpoint",
        [TableIdentifier.TableWithColumnMapping] = "table_with_column_mapping",
        [TableIdentifier.TableWithDeletionLogs] = "table_with_deletion_logs",
        [TableIdentifier.TableWithEdgeTimestamps] = "table_with_edge_timestamps",
        [TableIdentifier.TableWithLiquidClustering] = "table_with_liquid_clustering",
        [TableIdentifier.TableWithDvSmall] = "table-with-dv-small",
        [TableIdentifier.TableWithoutDvSmall] = "table-without-dv-small",
        [TableIdentifier.WithCheckpointNoLastcheckpoint] = "with_checkpoint_no_last_checkpoint",
    };

    public static IEnumerable<TableIdentifier> ValidTables => Tables.Keys.Where(t => t switch
    {
        TableIdentifier.CheckpointsTombstones => false,
        _ => true,
    });

    public static string LogPath(this TableIdentifier tid, string? pathRoot = null)
    {
        return Path.Join(pathRoot ?? Settings.TestRoot, Tables[tid], "_delta_log");
    }

    public static string TablePath(this TableIdentifier tid, string? pathRoot = null)
    {
        return Path.Join(pathRoot ?? Settings.TestRoot, Tables[tid]);
    }
}
