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

    [Fact]
    public async Task CreateWriteTransaction_Duplicate_AppId_Version_Should_Fail_Or_Succeed()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var options = new CommitOptions
            {
                AppId = "duplicate-test-app",
                TransactionVersion = 100,
            };

            // First commit with appId/version should succeed
            var actions1 = new List<AddAction>
            {
                new AddAction
                {
                    Path = "part-00000.parquet",
                    Size = 1024,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };

            var version1 = await table.CreateWriteTransactionAsync(
                actions1,
                options,
                CancellationToken.None);

            Assert.True(version1 > 0);

            // Second commit with the SAME appId and version
            var actions2 = new List<AddAction>
            {
                new AddAction
                {
                    Path = "part-00001.parquet",
                    Size = 2048,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };

            // The Delta Protocol does NOT prevent duplicate appId/version commits.
            // The kernel commits it successfully — the latest txn version for
            // a given appId simply overwrites the previous one during action
            // reconciliation. No error is thrown.
            var version2 = await table.CreateWriteTransactionAsync(
                actions2,
                options,
                CancellationToken.None);

            Assert.Equal(version1 + 1, version2);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task GetLatestTransactionVersion_Returns_Version_After_Commit()
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
                AppId = "version-query-app",
                TransactionVersion = 42,
            };

            await table.CreateWriteTransactionAsync(
                actions,
                options,
                CancellationToken.None);

            var txnVersion = await table.GetLatestTransactionVersionAsync(
                "version-query-app",
                CancellationToken.None);

            Assert.Equal(42, txnVersion);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task GetLatestTransactionVersion_Returns_Null_For_Unknown_AppId()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

            var txnVersion = await table.GetLatestTransactionVersionAsync(
                "nonexistent-app",
                CancellationToken.None);

            Assert.Null(txnVersion);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task GetLatestTransactionVersion_Returns_Latest_After_Multiple_Commits()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{info.FullName}", 0);
            using var table = tableParts.table;

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
                    AppId = "multi-version-app",
                    TransactionVersion = i * 100,
                };

                await table.CreateWriteTransactionAsync(
                    actions,
                    options,
                    CancellationToken.None);
            }

            var txnVersion = await table.GetLatestTransactionVersionAsync(
                "multi-version-app",
                CancellationToken.None);

            Assert.Equal(300, txnVersion);
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_With_NumRecords_Writes_Stats_To_Log()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var (engine, table) = await TableHelpers.SetupTable(info.FullName, 1);
            using (engine)
            using (table)
            {
                var actions = new List<AddAction>
                {
                    new AddAction
                    {
                        Path = "part-00000.parquet",
                        Size = 1234,
                        ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        DataChange = true,
                        NumRecords = 100,
                    },
                };

                long version = await table.CreateWriteTransactionAsync(
                    actions, CancellationToken.None);

                string logFile = Path.Combine(info.FullName, "_delta_log",
                    $"{version:D20}.json");
                string[] lines = await File.ReadAllLinesAsync(logFile);

                string? addLine = lines.FirstOrDefault(l => l.Contains("\"add\""));
                Assert.NotNull(addLine);
                // Kernel writes stats as JSON string: "stats":"{\"numRecords\":100}"
                // The inner JSON may also appear as "numRecords":100 without escaping
                Assert.True(
                    addLine.Contains("\"numRecords\":100") || addLine.Contains("numRecords\\\":100"),
                    $"Expected numRecords in log. Actual add line: {addLine}");
            }
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task CreateWriteTransaction_Mixed_NumRecords_Only_Populated_Files_Have_Stats()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var (engine, table) = await TableHelpers.SetupTable(info.FullName, 1);
            using (engine)
            using (table)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var actions = new List<AddAction>
                {
                    new AddAction
                    {
                        Path = "with-stats.parquet",
                        Size = 100,
                        ModificationTime = now,
                        NumRecords = 50,
                    },
                    new AddAction
                    {
                        Path = "no-stats.parquet",
                        Size = 200,
                        ModificationTime = now,
                    },
                };

                long version = await table.CreateWriteTransactionAsync(
                    actions, CancellationToken.None);

                string logFile = Path.Combine(info.FullName, "_delta_log",
                    $"{version:D20}.json");
                string content = await File.ReadAllTextAsync(logFile);

                Assert.True(
                    content.Contains("\"numRecords\":50") || content.Contains("numRecords\\\":50"),
                    $"Expected numRecords in log. Actual content: {content}");
                Assert.Contains("with-stats.parquet", content);
                Assert.Contains("no-stats.parquet", content);
            }
        }
        finally
        {
            info.Delete(true);
        }
    }
}
