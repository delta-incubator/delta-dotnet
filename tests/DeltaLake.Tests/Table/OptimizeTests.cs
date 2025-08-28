using Apache.Arrow;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public sealed class OptimizeTests
{
    [Fact]
    public async Task OptimizeTestWithCustomOptions()
    {
        await BaseOptimizeTest(new OptimizeOptions
        {
            MaxConcurrentTasks = 2,
            MaxSpillSize = 100_000,
            MinCommitInterval = TimeSpan.FromSeconds(1),
            PreserveInsertionOrder = true,
            TargetSize = 1_000_000,
            OptimizeType = OptimizeType.BinPack
        });
    }

    [Fact]
    public async Task OptimizeTestWithDefaultOptions()
    {
        await BaseOptimizeTest(new OptimizeOptions());
    }

    [Fact]
    public async Task OptimizeTestZOrder()
    {
        await BaseOptimizeTest(new OptimizeOptions
        {
            ZOrderColumns = ["test", "second"],
            OptimizeType = OptimizeType.ZOrder,
            MaxConcurrentTasks = 2,
        });
    }

    private async Task BaseOptimizeTest(OptimizeOptions options)
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 10_000);
        using var table = data.table;

        await table.OptimizeAsync(options, CancellationToken.None);

        long count = 0;
        await foreach (var recordBatch in table.QueryAsync(new("SELECT COUNT(*) FROM deltatable"), CancellationToken.None))
        {
            switch (recordBatch.Column(0))
            {
                case Int32Array integers:
                    count = integers.GetValue(0)!.Value;
                    break;
                case Int64Array longs:
                    count = longs.GetValue(0)!.Value;
                    break;
            }
        }

        Assert.Equal(10_000, count);
    }
}
