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
        public async Task Memory_Insert_Variable_Record_Count_Test(int length)
        {
            await BaseCheckpointTest($"memory://{Guid.NewGuid():N}", length);
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
                await BaseCheckpointTest($"file://{info.FullName}", length);
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
                    $"file://{info.FullName}",
                    length,
                    async table =>
                    {
                        await table.VacuumAsync(new VacuumOptions
                        {
                            DryRun = true,
                        },
                        CancellationToken.None);
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