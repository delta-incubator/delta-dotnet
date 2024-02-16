using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Runtime;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table
{
    public class ConstraintTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task Contraint_Greater_Memory_Test(
            int first,
            string second,
            long third,
            Dictionary<string, string> constraints,
            bool throws
        )
        {
            Func<Task> task = () => BaseConstraintTest(
                $"memory://{Guid.NewGuid():N}",
                async table =>
                {
                    await table.AddConstraintsAsync(
                                    constraints,
                                    CancellationToken.None);
                },
                first,
                second,
                third
            );
            if (throws)
            {
                await Assert.ThrowsAsync<DeltaLakeException>(task);
            }
            else
            {
                await task();
            }
        }

        [Fact]
        public async Task Invalid_Constraint_Test()
        {
            var tableParts = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 0);
            using var runtime = tableParts.runtime;
            using var table = tableParts.table;
            await Assert.ThrowsAsync<DeltaLakeException>(() => table.AddConstraintsAsync(
                new Dictionary<string, string>
                {
                    ["something isn't right"] = "invalid constraint",
                },
                new Dictionary<string, string>(),
                CancellationToken.None));
        }

        [Fact]
        public async Task Empty_Constraint_Test()
        {
            var tableParts = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 0);
            using var runtime = tableParts.runtime;
            using var table = tableParts.table;
            var version = table.Version();
            await table.AddConstraintsAsync(
                new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            CancellationToken.None);
            Assert.Equal(version, table.Version());
        }

        public static IEnumerable<object[]> TestCases()
        {
            yield return [1, "hello", 0, new Dictionary<string, string> { ["first"] = "first > 0" }, false];
            yield return [0,
                "hello",
                0,
                new Dictionary<string, string>
                {
                    ["first"] = "first > 0",
                },
                true];
            yield return [0,
                "hello",
                1,
                new Dictionary<string, string>
                {
                    ["third"] = "third > 0",
                },
                false];
            yield return [0,
                "hello",
                0,
                new Dictionary<string, string>
                {
                    ["third"] = "third > 0",
                },
                true];
        }

        private async Task BaseConstraintTest(
            string path,
            Func<DeltaTable, Task> addConstraints,
            int first,
            string second,
            long third)
        {
            using var runtime = new DeltaRuntime(RuntimeOptions.Default);
            var builder = new Schema.Builder();
            builder.Field(fb =>
            {
                fb.Name("first");
                fb.DataType(Int32Type.Default);
                fb.Nullable(false);
            })
            .Field(fb =>
            {
                fb.Name("second");
                fb.DataType(StringType.Default);
                fb.Nullable(false);
            })
            .Field(fb =>
            {
                fb.Name("third");
                fb.DataType(Int64Type.Default);
                fb.Nullable(false);
            });
            var schema = builder.Build();
            using var table = await DeltaTable.CreateAsync(
                runtime,
                new TableCreateOptions(path, schema),
                CancellationToken.None);
            await addConstraints(table);
            var allocator = new NativeMemoryAllocator();
            var recordBatchBuilder = new RecordBatch.Builder(allocator)
                .Append("test", false, col => col.Int32(arr => arr.Append(first)))
                .Append("second", false, col => col.String(arr => arr.Append(second)))
                .Append("third", false, col => col.Int64(arr => arr.Append(third)));
            var options = new InsertOptions
            {
                SaveMode = SaveMode.Append,
            };
            await table.InsertAsync([recordBatchBuilder.Build()], schema, options, CancellationToken.None);

        }
    }
}