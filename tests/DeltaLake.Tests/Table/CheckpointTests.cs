using DeltaLake.Interfaces;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table
{
    public class CheckpointTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Local_File_System_Insert_Variable_Record_Count_Test(int length)
        {
            // Converted from memory:// to file:// for the C-2b kernel-only checkpoint migration
            // (see .copilot-tracking/plans/2026-06-02/full-kernel-checkpoint-migration-plan.instructions.md).
            // CheckpointAsync now throws NotSupportedException on memory:// per DD-01.
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                await BaseCheckpointTest(DirectoryHelpers.ToFileUri(info.FullName), length);
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task File_System_Insert_Variable_Record_Count_Test(int length)
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                await BaseCheckpointTest(DirectoryHelpers.ToFileUri(info.FullName), length);
                var last_check_point = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(last_check_point));
                var version = ReadVersion(last_check_point);
                Assert.Equal((ulong)length + 1, version);
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task File_System_Insert_Variable_Record_Count_Vacuum_Test(int length)
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                await BaseCheckpointTestWithMore(
                    DirectoryHelpers.ToFileUri(info.FullName),
                    length,
                    async table =>
                    {
                        await table.VacuumAsync(new VacuumOptions(),
                        CancellationToken.None);
                    });
                var last_check_point = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(last_check_point));

            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task Memory_CheckpointAsync_Throws_NotSupported()
        {
            var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 0);
            using var table = data.table;
            var ex = await Assert.ThrowsAsync<NotSupportedException>(
                async () => await table.CheckpointAsync(CancellationToken.None));
            Assert.Contains("memory://", ex.Message);
        }

        [Fact]
        public async Task File_System_Checkpoint_After_LoadVersion_Pins_To_Loaded_Version()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var data = await TableHelpers.SetupTable(DirectoryHelpers.ToFileUri(info.FullName), 0);
                using var table = data.table;
                var schema = table.Schema();
                var options = new InsertOptions { SaveMode = SaveMode.Append };
                // SetupTable creates the table at v0 and writes an initial commit at v1.
                // The loop adds 7 more commits, producing v2..v8.
                for (var i = 0; i < 7; i++)
                {
                    await table.InsertAsync(
                        [TableHelpers.BuildBasicRecordBatch(1)],
                        schema,
                        options,
                        CancellationToken.None);
                }
                Assert.Equal(8UL, table.Version());

                await table.LoadVersionAsync(5, CancellationToken.None);
                Assert.Equal(5UL, table.Version());

                await table.CheckpointAsync(CancellationToken.None);

                var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(lastCheckpoint));
                Assert.Equal(5UL, ReadVersion(lastCheckpoint));
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task File_System_Open_With_Version_Pins_Kernel_To_Loaded_Version()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);

                // Build a table with v0..v8.
                {
                    var data = await TableHelpers.SetupTable(path, 0);
                    using var seed = data.table;
                    var schema = seed.Schema();
                    var insert = new InsertOptions { SaveMode = SaveMode.Append };
                    for (var i = 0; i < 7; i++)
                    {
                        await seed.InsertAsync(
                            [TableHelpers.BuildBasicRecordBatch(1)],
                            schema,
                            insert,
                            CancellationToken.None);
                    }
                    Assert.Equal(8UL, seed.Version());
                }

                // Re-open with TableOptions.Version = 5. With the kernel-only-checkpoint migration
                // the kernel engine is allocated for file:// even when Version is set, and
                // ManagedTableState.PinSnapshotTo wires the requested version into the first
                // snapshot materialization so CheckpointAsync produces a checkpoint at v5.
                using IEngine engine = new DeltaEngine(EngineOptions.Default);
                using var pinned = await engine.LoadTableAsync(
                    new TableOptions { TableLocation = path, Version = 5UL },
                    CancellationToken.None);
                Assert.Equal(5UL, pinned.Version());

                await pinned.CheckpointAsync(CancellationToken.None);

                var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(lastCheckpoint));
                Assert.Equal(5UL, ReadVersion(lastCheckpoint));
            }
            finally
            {
                info.Delete(true);
            }
        }

        private ulong? ReadVersion(string path)
        {
            var reader = new System.Text.Json.Utf8JsonReader(File.ReadAllBytes(path));
            while (reader.Read())
            {
                switch (reader.TokenType)
                {

                    case System.Text.Json.JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals("version"u8))
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            if (reader.TokenType != System.Text.Json.JsonTokenType.Number)
                            {
                                return null;
                            }

                            return reader.GetUInt64();
                        }
                        break;
                    default:
                        break;
                }
            }

            return null;
        }

        private async Task BaseCheckpointTest(string path, int length)
        {
            await BaseCheckpointTestWithMore(path, length, (_) => Task.CompletedTask);
        }

        private async Task BaseCheckpointTestWithMore(
            string path,
            int length,
            Func<ITable, Task> more)
        {
            var data = await TableHelpers.SetupTable(path, 0);
            using var table = data.table;
            var schema = table.Schema();
            var options = new InsertOptions
            {
                SaveMode = SaveMode.Append,
            };
            for (var i = 0; i < length; i++)
            {
                await table.InsertAsync([TableHelpers.BuildBasicRecordBatch(1)], schema, options, CancellationToken.None);
            }
            var history = await table.HistoryAsync((ulong)length + 3, CancellationToken.None);
            Assert.Equal(length + 2, history.Length);
            await table.CheckpointAsync(CancellationToken.None);
            await more(table);
        }
    }
}