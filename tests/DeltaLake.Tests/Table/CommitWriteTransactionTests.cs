using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class CommitWriteTransactionTests
{
    [Fact]
    public async Task CommitWriteTransaction_Append_Creates_Delta_Log_Entry()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var initialVersion = table.Version();

            var actions = new List<AddAction>
            {
                new AddAction
                {
                    Path = "part-00000.parquet",
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };

            var newVersion = await table.CommitWriteTransactionAsync(
                actions,
                new CommitOptions(),
                CancellationToken.None);

            Assert.Equal(initialVersion + 1, (ulong)newVersion);
            Assert.Equal((ulong)newVersion, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CommitWriteTransaction_With_PartitionValues()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var actions = new List<AddAction>
            {
                new AddAction
                {
                    Path = "year=2024/part-00000.parquet",
                    Size = 2048,
                    PartitionValues = new Dictionary<string, string?>
                    {
                        ["year"] = "2024",
                    },
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CommitWriteTransactionAsync(
                actions,
                new CommitOptions(),
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CommitWriteTransaction_Multiple_Commits_Increment_Version()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var baseVersion = (long)table.Version()!;

            for (int i = 1; i <= 5; i++)
            {
                var actions = new List<AddAction>
                {
                    new AddAction
                    {
                        Path = $"part-{i:D5}.parquet",
                        Size = 1024 * i,
                        ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    },
                };

                var newVersion = await table.CommitWriteTransactionAsync(
                    actions,
                    new CommitOptions(),
                    CancellationToken.None);

                Assert.Equal(baseVersion + i, newVersion);
            }

            Assert.Equal((ulong)(baseVersion + 5), table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CommitWriteTransaction_With_EngineInfo()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var actions = new List<AddAction>
            {
                new AddAction
                {
                    Path = "part-00000.parquet",
                    Size = 512,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var options = new CommitOptions
            {
                EngineInfo = "delta-dotnet-test/1.0",
            };

            var newVersion = await table.CommitWriteTransactionAsync(
                actions,
                options,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CommitWriteTransaction_Empty_Actions_Throws()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            await Assert.ThrowsAsync<DeltaLake.Errors.DeltaConfigurationException>(
                () => table.CommitWriteTransactionAsync(
                    new List<AddAction>(),
                    new CommitOptions(),
                    CancellationToken.None));
        }
        finally
        {
            info.Delete(true);
        }
    }
}
