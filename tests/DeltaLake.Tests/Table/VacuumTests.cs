using Apache.Arrow;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public sealed class VacuumTests
{
    [Fact]
    public async Task VacuumTestWithCustomOptions()
    {
        await BaseVacuumTest(new VacuumOptions
        {
            VacuumMode = VacuumMode.Full
        });
    }

    [Fact]
    public async Task VacuumTestWithDefaultOptions()
    {
        await BaseVacuumTest(new VacuumOptions());
    }

    private async Task BaseVacuumTest(VacuumOptions options)
    {
        using var source = new CancellationTokenSource(30_000);
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 10_000);
        using var table = data.table;

        await table.VacuumAsync(options, source.Token);

        long count = 0;
        await foreach (var recordBatch in table.QueryAsync(new("SELECT COUNT(*) FROM deltatable"), source.Token))
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
