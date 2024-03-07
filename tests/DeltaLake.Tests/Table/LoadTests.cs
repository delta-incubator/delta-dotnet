using System.Runtime.InteropServices;
using DeltaLake.Errors;
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
    public async Task Load_Table_At_Version_Test(ulong version)
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
    public async Task Table_Load_Version_Test(ulong version)
    {
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4UL, table.Version());
        await table.LoadVersionAsync(version, CancellationToken.None);
        Assert.Equal(version, table.Version());
    }

    [Fact]
    public async Task Table_Load_Invalid_Version_Test()
    {
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4UL, table.Version());
        await Assert.ThrowsAsync<DeltaRuntimeException>(async () =>
        await table.LoadVersionAsync(ulong.MaxValue, CancellationToken.None));
        // table is in an invalid state
        Assert.NotEqual(4UL, table.Version());
    }

    [Fact]
    public async Task Table_Load_Version_Cancellation_Test()
    {
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4UL, table.Version());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        await table.LoadVersionAsync(4, new CancellationToken(true)));
        Assert.Equal(4UL, table.Version());
    }


    [Fact]
    public async Task Table_Load_At_Now()
    {
        var dateTimeOffset = DateTimeOffset.UtcNow;
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = 1UL,
        },
        CancellationToken.None);
        Assert.Equal(1UL, table.Version());
        await table.LoadDateTimeAsync(dateTimeOffset, CancellationToken.None);
        Assert.Equal(4UL, table.Version());
    }

    [Fact]
    public async Task Table_Load_DateTime_Max_Value_Timestamp()
    {
        var dateTimeOffset = DateTimeOffset.MaxValue;
        var location = TableIdentifier.SimpleTable.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = 1,
        },
        CancellationToken.None);
        Assert.Equal(1UL, table.Version());
        await table.LoadDateTimeAsync(dateTimeOffset, CancellationToken.None);
        Assert.Equal(4UL, table.Version());
    }

    [Theory]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public async Task Table_Load_Invalid_Timestamp(long value)
    {
        await Assert.ThrowsAsync<DeltaRuntimeException>(async () =>
        {
            var location = TableIdentifier.SimpleTable.TablePath();
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
            {
                Version = 1,
            },
            CancellationToken.None);
            Assert.Equal(1UL, table.Version());
            await table.LoadDateTimeAsync(value, CancellationToken.None);
            Assert.Equal(4UL, table.Version());
        });
    }

    [Fact]
    public async Task Table_Cancellation_Timestamp_Test()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var location = TableIdentifier.SimpleTable.TablePath();
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
            {
                Version = 1,
            },
            CancellationToken.None);
            Assert.Equal(1UL, table.Version());
            await table.LoadDateTimeAsync(long.MaxValue, new CancellationToken(true));
            Assert.Equal(1UL, table.Version());
        });
    }

    [Fact]
    public async Task Table_Cancellation_DateTimeOffset_Test()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var location = TableIdentifier.SimpleTable.TablePath();
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
            {
                Version = 1,
            },
            CancellationToken.None);
            Assert.Equal(1UL, table.Version());
            await table.LoadDateTimeAsync(DateTimeOffset.UtcNow, new CancellationToken(true));
            Assert.Equal(1UL, table.Version());
        });
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
            Version = (ulong)minVersion,
        },
        CancellationToken.None);
        Assert.Equal((ulong)minVersion, table.Version());
        await table.LoadDateTimeAsync(DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.Equal((ulong)expectedVersion, table.Version());
    }

    [Fact]
    public async Task Table_Load_Latest_Cancel()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var location = TableIdentifier.Checkpoints.TablePath();
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(
                runtime,
                 location,
                  new TableOptions(),
            new CancellationToken(true));
        });
    }

    [Theory]
    [InlineData(TableIdentifier.Checkpoints, 1, 12)]
    [InlineData(TableIdentifier.CheckpointsVacuumed, 5, 12)]
    [InlineData(TableIdentifier.Delta020, 1, 3)]
    public async Task Table_Load_Update_Incremental(TableIdentifier identifier, int minVersion, int expectedVersion)
    {
        var location = identifier.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = (ulong)minVersion,
        },
        CancellationToken.None);
        Assert.Equal((ulong)minVersion, table.Version());
        for (var version = minVersion; version <= expectedVersion; version++)
        {
            await table.UpdateIncrementalAsync(version, CancellationToken.None);
            Assert.Equal((ulong)version, table.Version());
        }
    }

    [Theory]
    [InlineData(TableIdentifier.Checkpoints, 1, 12)]
    [InlineData(TableIdentifier.CheckpointsVacuumed, 5, 12)]
    [InlineData(TableIdentifier.Delta020, 1, 3)]
    public async Task Table_Load_Update_Incremental_Cancellation(TableIdentifier identifier, int minVersion, int expectedVersion)
    {
        var location = identifier.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = (ulong)minVersion,
        },
        CancellationToken.None);
        Assert.Equal((ulong)minVersion, table.Version());
        for (var version = minVersion; version <= expectedVersion; version++)
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            table.UpdateIncrementalAsync(version, new CancellationToken(true)));
            Assert.Equal((ulong)minVersion, table.Version());
        }
    }

    [Theory]
    [InlineData(TableIdentifier.Checkpoints, 1, 12)]
    [InlineData(TableIdentifier.CheckpointsVacuumed, 5, 12)]
    [InlineData(TableIdentifier.Delta020, 1, 3)]
    public async Task Table_Load_Update_Incremental_Latest(TableIdentifier identifier, int minVersion, int expectedVersion)
    {
        var location = identifier.TablePath();
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = (ulong)minVersion,
        },
        CancellationToken.None);
        Assert.Equal((ulong)minVersion, table.Version());
        await table.UpdateIncrementalAsync(100, CancellationToken.None);
        Assert.Equal((ulong)expectedVersion, table.Version());
    }

    [Fact]
    public async Task Table_Load_Invalid_Uri_Type_Test()
    {
        await Assert.ThrowsAsync<DeltaRuntimeException>(async () =>
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await Assert.ThrowsAsync<SEHException>(TestBodyAsync);
        }
        else
        {

            await Assert.ThrowsAsync<DeltaRuntimeException>(TestBodyAsync);
        }

        static async Task TestBodyAsync()
        {
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            using var table = await DeltaTable.LoadAsync(runtime, "file://invalid.uri", new TableOptions
            {
                Version = 50,
            },
        CancellationToken.None);
        }
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
                Assert.Equal(0UL, table.Version());
                break;
            default:
                Assert.NotEqual(0UL, table.Version());
                break;
        }
    }
}