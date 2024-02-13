using DeltaLake.Runtime;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public partial class LoadTests
{
    public static TableIdentifier[] AllTables = TableHelpers.Tables.Keys.ToArray();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Load_Table_At_Version_Test(long version)
    {
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = version,
        },
        CancellationToken.None);
        Assert.Equal(version, table.Version());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Table_Load_Version_Test(long version)
    {
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4, table.Version());
        await table.LoadVersionAsync(version, CancellationToken.None);
        Assert.Equal(version, table.Version());
    }


    [Fact]
    public async Task Table_Load_At_Now()
    {
        var dateTimeOffset = DateTimeOffset.UtcNow;
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = 1,
        },
        CancellationToken.None);
        Assert.Equal(1, table.Version());
        await table.LoadDateTimeAsync(dateTimeOffset, CancellationToken.None);
        Assert.Equal(4, table.Version());
    }

    [Theory]
    [InlineData(TableIdentifier.Checkpoints, 1, 12)]
    [InlineData(TableIdentifier.CheckpointsVacuumed, 5, 12)]
    [InlineData(TableIdentifier.Delta020, 1, 3)]
    public async Task Table_Load_Latest(TableIdentifier identifier, int minVersion, int expectedVersion)
    {
        var location = identifier.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = minVersion,
        },
        CancellationToken.None);
        Assert.Equal(minVersion, table.Version());
        await table.LoadDateTimeAsync(DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.Equal(expectedVersion, table.Version());
    }


    [Fact]
    public async Task Table_Load_Invalid_Uri_Type_Test()
    {
        await Assert.ThrowsAsync<DeltaLakeException>(async () =>
        {
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, "invalid://invalid.uri", new TableOptions
            {
                Version = 50,
            },
        CancellationToken.None);
        });
    }

    [Fact]
    public async Task Table_Load_Invalid_Path_Test()
    {
        await Assert.ThrowsAsync<DeltaLakeException>(async () =>
        {
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, "file://invalid.uri", new TableOptions
            {
                Version = 50,
            },
        CancellationToken.None);
        });
    }

    [Theory]
    [ClassData(typeof(AllTablesEnumerable))]
    public async Task Table_Will_Load_Test(TableIdentifier identifier)
    {
        var location = identifier.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        switch (identifier)
        {
            case TableIdentifier.Covid19NYT:
            case TableIdentifier.Delta220PartitionedTypes:
            case TableIdentifier.SimpleCommit:
            case TableIdentifier.Delta08NumericPartition:
            case TableIdentifier.Delta08Date:
            case TableIdentifier.Golden:
            case TableIdentifier.ConcurrentWorkers:
            case TableIdentifier.Delta08NullPartition:
            case TableIdentifier.Delta08Partition:
            case TableIdentifier.Delta08SpecialPartition:
            case TableIdentifier.TableWithColumnMapping:
            case TableIdentifier.TableWithEdgeTimestamps:
            case TableIdentifier.TableWithLiquidClustering:
            case TableIdentifier.TableWithoutDvSmall:
                Assert.Equal(0, table.Version());
                break;
            default:
                Assert.NotEqual(0, table.Version());
                break;
        }
    }
}