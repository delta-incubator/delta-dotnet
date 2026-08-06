using System.Text.Json;
using DeltaLake.Interfaces;
using DeltaLake.Kernel.Callbacks.Errors;
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

        [Fact]
        public async Task File_System_Checkpoint_After_Append_Advances_Incrementally()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);

                // The first checkpoint (inside BaseCheckpointTestWithMore) materializes and caches
                // the kernel snapshot at v3. The `more` callback then appends on the SAME table
                // handle and checkpoints again. Because the delta-rs bridge InsertAsync has no
                // kernel-snapshot invalidation, the cached snapshot is stale-but-non-null, so the
                // second CheckpointAsync -> Snapshot(refresh:true) -> RefreshSnapshot takes the
                // incremental get_snapshot_builder_from path (advances v3 -> v5 without re-reading
                // the full log). If InsertAsync ever begins invalidating the kernel snapshot,
                // RefreshSnapshot falls back to a full rebuild and the version assertion still holds.
                await BaseCheckpointTestWithMore(path, 2, async table =>
                {
                    var schema = table.Schema();
                    var options = new InsertOptions { SaveMode = SaveMode.Append };

                    await table.InsertAsync([TableHelpers.BuildBasicRecordBatch(1)], schema, options, CancellationToken.None);
                    await table.InsertAsync([TableHelpers.BuildBasicRecordBatch(1)], schema, options, CancellationToken.None);
                    Assert.Equal(5UL, table.Version());

                    await table.CheckpointAsync(CancellationToken.None);

                    var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                    Assert.True(File.Exists(lastCheckpoint));
                    Assert.Equal(5UL, ReadVersion(lastCheckpoint));
                });
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task File_System_Checkpoint_After_UpdateIncremental_Matches_Version()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);

                // Writer builds v0..v8.
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

                // Open a reader pinned at v1, incrementally advance it to v5, then checkpoint.
                // UpdateIncrementalAsync advances the bridge and mirrors the version via
                // PinSnapshotTo; the subsequent checkpoint must be produced at the advanced version.
                using IEngine engine = new DeltaEngine(EngineOptions.Default);
                using var reader = await engine.LoadTableAsync(
                    new TableOptions { TableLocation = path, Version = 1UL },
                    CancellationToken.None);
                Assert.Equal(1UL, reader.Version());

                await reader.UpdateIncrementalAsync(5, CancellationToken.None);
                Assert.Equal(5UL, reader.Version());

                await reader.CheckpointAsync(CancellationToken.None);

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
        public async Task File_System_Checkpoint_After_Backward_LoadVersion_Falls_Back()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);
                var data = await TableHelpers.SetupTable(path, 0);
                using var table = data.table;
                var schema = table.Schema();
                var options = new InsertOptions { SaveMode = SaveMode.Append };
                for (var i = 0; i < 7; i++)
                {
                    await table.InsertAsync([TableHelpers.BuildBasicRecordBatch(1)], schema, options, CancellationToken.None);
                }
                Assert.Equal(8UL, table.Version());

                // Materialize + cache a kernel snapshot at latest (v8) via a checkpoint.
                await table.CheckpointAsync(CancellationToken.None);
                var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.Equal(8UL, ReadVersion(lastCheckpoint));

                // Load an EARLIER version. LoadVersionAsync pins to v5 and invalidates the cached
                // snapshot, so RefreshSnapshot rebuilds from path at the pinned (earlier) version
                // rather than attempting an illegal backward incremental advance (get_snapshot_builder_from
                // only advances forward). The checkpoint must be produced at v5 without crashing.
                await table.LoadVersionAsync(5, CancellationToken.None);
                Assert.Equal(5UL, table.Version());

                await table.CheckpointAsync(CancellationToken.None);

                // The v5 checkpoint parquet is written (proving the backward fallback produced a
                // valid checkpoint at the pinned version). Note the kernel's _last_checkpoint hint
                // has a regression guard: it is NOT lowered from 8 to 5, so the hint legitimately
                // still reads 8 while both 8.checkpoint.parquet and 5.checkpoint.parquet exist.
                var checkpointV5 = Path.Join(info.FullName, "_delta_log", "00000000000000000005.checkpoint.parquet");
                Assert.True(File.Exists(checkpointV5), "expected 00000000000000000005.checkpoint.parquet to be written");
                Assert.Equal(8UL, ReadVersion(lastCheckpoint));
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

        [Fact]
        public async Task File_System_Checkpoint_With_Auto_Options_Writes_Checkpoint()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);
                var data = await TableHelpers.SetupTable(path, 1);
                using var table = data.table;

                // The parameterless default (CheckpointFormat.Auto) matches the pre-options behavior.
                await table.CheckpointAsync(new CheckpointOptions(), CancellationToken.None);

                var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(lastCheckpoint));
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task File_System_Checkpoint_With_V1_Spec_Writes_Checkpoint()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);
                var data = await TableHelpers.SetupTable(path, 1);
                using var table = data.table;

                await table.CheckpointAsync(
                    new CheckpointOptions { Format = CheckpointFormat.V1 },
                    CancellationToken.None);

                var lastCheckpoint = Path.Join(info.FullName, "_delta_log", "_last_checkpoint");
                Assert.True(File.Exists(lastCheckpoint));
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task File_System_Checkpoint_V2_Sidecar_Spec_Reaches_Kernel_And_Requires_Feature()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);
                var data = await TableHelpers.SetupTable(path, 1);
                using var table = data.table;

                // The table does not enable the v2Checkpoint feature, so forcing a V2 checkpoint
                // with sidecars surfaces the kernel's CheckpointWriteError. This confirms the sidecar
                // spec is marshalled and passed through to the kernel rather than silently ignored.
                var ex = await Assert.ThrowsAsync<KernelException>(
                    async () => await table.CheckpointAsync(
                        new CheckpointOptions
                        {
                            Format = CheckpointFormat.V2WithSidecar,
                            FileActionsPerSidecarHint = 1,
                        },
                        CancellationToken.None));
                Assert.Contains("v2Checkpoint", ex.Message);
            }
            finally
            {
                info.Delete(true);
            }
        }

        [Fact]
        public async Task File_System_Checkpoint_V2_Sidecar_With_Feature_WritesAndReopens()
        {
            var info = DirectoryHelpers.CreateTempSubdirectory();
            try
            {
                var path = DirectoryHelpers.ToFileUri(info.FullName);
                var data = await TableHelpers.SetupTable(path, 3);
                using var engine = data.engine;
                using var table = data.table;

                var schema = table.Schema();
                var insertOptions = new InsertOptions { SaveMode = SaveMode.Append };
                for (var i = 0; i < 3; i++)
                {
                    using var batch = TableHelpers.BuildBasicRecordBatch(1);
                    await table.InsertAsync(
                        [batch],
                        schema,
                        insertOptions,
                        CancellationToken.None);
                }

                // Materialize a pre-feature kernel snapshot. CheckpointAsync must incrementally
                // advance it after the bridge commits the feature on the same handle.
                Assert.Equal(4UL, table.Version());
                await table.AddTableFeaturesAsync(
                    [TableFeature.V2Checkpoint],
                    new AddTableFeatureOptions { AllowProtocolVersionsIncrease = true },
                    CancellationToken.None);

                var protocol = table.ProtocolVersions();
                Assert.Equal(3, protocol.MinimumReaderVersion);
                Assert.Equal(7, protocol.MinimumWriterVersion);

                var featureCommit = Path.Join(
                    info.FullName,
                    "_delta_log",
                    "00000000000000000005.json");
                var protocolAction = ReadAction(featureCommit, "protocol");
                Assert.Contains(
                    "v2Checkpoint",
                    protocolAction.GetProperty("readerFeatures").EnumerateArray().Select(value => value.GetString()));
                Assert.Contains(
                    "v2Checkpoint",
                    protocolAction.GetProperty("writerFeatures").EnumerateArray().Select(value => value.GetString()));

                await table.CheckpointAsync(
                    new CheckpointOptions
                    {
                        Format = CheckpointFormat.V2WithSidecar,
                        FileActionsPerSidecarHint = 1,
                    },
                    CancellationToken.None);

                var logPath = Path.Join(info.FullName, "_delta_log");
                var lastCheckpoint = Path.Join(logPath, "_last_checkpoint");
                Assert.True(File.Exists(lastCheckpoint));
                Assert.Equal(5UL, ReadVersion(lastCheckpoint));

                var manifest = Path.Join(logPath, "00000000000000000005.checkpoint.parquet");
                Assert.True(new FileInfo(manifest).Length > 0);

                var sidecarPath = Path.Join(logPath, "_sidecars");
                var sidecars = Directory.GetFiles(sidecarPath, "*.parquet");
                Assert.True(sidecars.Length > 1);
                Assert.All(sidecars, sidecar => Assert.True(new FileInfo(sidecar).Length > 0));

                table.Dispose();
                using var reopened = await engine.LoadTableAsync(
                    new TableOptions { TableLocation = path },
                    CancellationToken.None);
                using var rows = await reopened.ReadAsArrowTableAsync(CancellationToken.None);
                Assert.Equal(6, rows.Table.RowCount);
            }
            finally
            {
                info.Delete(true);
            }
        }

        private static JsonElement ReadAction(string commitPath, string actionName)
        {
            foreach (var line in File.ReadLines(commitPath))
            {
                using var document = JsonDocument.Parse(line);
                if (document.RootElement.TryGetProperty(actionName, out var action))
                {
                    return action.Clone();
                }
            }

            throw new InvalidOperationException($"Commit did not contain a {actionName} action.");
        }
    }
}