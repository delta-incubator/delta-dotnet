using DeltaLake.Errors;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class CreateWriteTransactionTests
{
    [Fact]
    public async Task CreateWriteTransaction_Append_Creates_Delta_Log_Entry()
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

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
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
    public async Task CreateWriteTransaction_Multiple_Commits_Increment_Version()
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

                var newVersion = await table.CreateWriteTransactionAsync(
                    actions,
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
    public async Task CreateWriteTransaction_Empty_Actions_Throws()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            await Assert.ThrowsAsync<DeltaConfigurationException>(
                () => table.CreateWriteTransactionAsync(
                    new List<AddAction>(),
                    CancellationToken.None));
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Null_Actions_Throws()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            await Assert.ThrowsAsync<DeltaConfigurationException>(
                () => table.CreateWriteTransactionAsync(
                    null!,
                    CancellationToken.None));
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Will_Cancel()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;
            var version = table.Version();

            var actions = new List<AddAction>
            {
                new AddAction
                {
                    Path = "part-00000.parquet",
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => table.CreateWriteTransactionAsync(
                    actions,
                    new CancellationToken(true)));

            Assert.Equal(version, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Null_PartitionValues()
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
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    PartitionValues = null,
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_Multiple_Partition_Columns()
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
                    Path = "year=2024/month=03/day=15/part-00000.parquet",
                    Size = 4096,
                    PartitionValues = new Dictionary<string, string?>
                    {
                        ["year"] = "2024",
                        ["month"] = "03",
                        ["day"] = "15",
                    },
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_Null_Partition_Value()
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
                    Path = "region=__HIVE_DEFAULT_PARTITION__/part-00000.parquet",
                    Size = 2048,
                    PartitionValues = new Dictionary<string, string?>
                    {
                        ["region"] = null,
                    },
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Multiple_Files_Single_Commit()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;
            var baseVersion = (long)table.Version()!;

            var actions = new List<AddAction>();
            for (int i = 0; i < 100; i++)
            {
                actions.Add(new AddAction
                {
                    Path = $"part-{i:D5}.parquet",
                    Size = 1024 * (i + 1),
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                });
            }

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.Equal(baseVersion + 1, newVersion);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_DataChange_False()
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
                    Path = "compacted-00000.parquet",
                    Size = 8192,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = false,
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Mixed_Partition_And_NonPartition_Files()
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
                    Size = 1024,
                    PartitionValues = new Dictionary<string, string?>
                    {
                        ["year"] = "2024",
                    },
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
                new AddAction
                {
                    Path = "part-00001.parquet",
                    Size = 2048,
                    PartitionValues = null,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Special_Characters_In_Path()
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
                    Path = "country=United%20States/city=New%20York/part-00000.parquet",
                    Size = 1024,
                    PartitionValues = new Dictionary<string, string?>
                    {
                        ["country"] = "United States",
                        ["city"] = "New York",
                    },
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Zero_Size_File()
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
                    Path = "empty-part-00000.parquet",
                    Size = 0,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                CancellationToken.None);

            Assert.True(newVersion > 0);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_TransactionId_Succeeds()
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
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };

            var options = new CommitOptions
            {
                AppId = "test-app-id-12345",
                TransactionVersion = 42,
            };

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                options,
                CancellationToken.None);

            Assert.True(newVersion > 0);

            // Verify the txn action is in the Delta log
            var logDir = Path.Combine(info.FullName, "_delta_log");
            var logFile = Path.Combine(logDir, $"{newVersion:D20}.json");
            var logContent = await File.ReadAllTextAsync(logFile);

            Assert.Contains("\"txn\"", logContent);
            Assert.Contains("test-app-id-12345", logContent);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_Options_No_TransactionId_Succeeds()
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
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };

            var options = new CommitOptions();

            var newVersion = await table.CreateWriteTransactionAsync(
                actions,
                options,
                CancellationToken.None);

            Assert.True(newVersion > 0);

            // Verify no txn action in the log
            var logDir = Path.Combine(info.FullName, "_delta_log");
            var logFile = Path.Combine(logDir, $"{newVersion:D20}.json");
            var logContent = await File.ReadAllTextAsync(logFile);

            Assert.DoesNotContain("\"txn\"", logContent);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_Options_Empty_Actions_Throws()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var options = new CommitOptions
            {
                AppId = "test-app",
                TransactionVersion = 1,
            };

            await Assert.ThrowsAsync<DeltaConfigurationException>(
                () => table.CreateWriteTransactionAsync(
                    new List<AddAction>(),
                    options,
                    CancellationToken.None));
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Multiple_Commits_With_TransactionId_Increment()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var baseVersion = (long)table.Version()!;

            for (int i = 1; i <= 3; i++)
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

                var options = new CommitOptions
                {
                    AppId = "streaming-app",
                    TransactionVersion = i,
                };

                var newVersion = await table.CreateWriteTransactionAsync(
                    actions,
                    options,
                    CancellationToken.None);

                Assert.Equal(baseVersion + i, newVersion);
            }
        }
        finally
        {
            info.Delete(true);
        }
    }
}
