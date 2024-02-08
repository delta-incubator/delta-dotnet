using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Runtime;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class InsertTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Insert_Variable_Record_Count_Test(int length)
    {
        var uri = $"memory://{Guid.NewGuid():N}";
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        var builder = new Apache.Arrow.Schema.Builder();
        builder.Field(fb =>
        {
            fb.Name("test");
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
            new TableCreateOptions(uri, schema),
            CancellationToken.None);
        Assert.NotNull(table);
        var allocator = new NativeMemoryAllocator();
        var recordBatchBuilder = new RecordBatch.Builder(allocator)
            .Append("test", false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, length))))
            .Append("second", false, col => col.String(arr => arr.AppendRange(Enumerable.Range(0, length).Select(x => x.ToString()))))
            .Append("third", false, col => col.Int64(arr => arr.AppendRange(Enumerable.Range(0, length).Select(x => (long)x))));
        var options = new InsertOptions
        {
            SaveMode = SaveMode.Append,
        };
        await table.InsertAsync([recordBatchBuilder.Build()], schema, options, CancellationToken.None);
        var queryResult = table.QueryAsync(new SelectQuery("SELECT test FROM test WHERE test > 1")
        {
            TableAlias = "test",
        },
        CancellationToken.None).ToBlockingEnumerable().ToList();

        if (length > 2)
        {
            var totalRecords = queryResult.Select(s => s.Length).Sum();
            Assert.Equal(length - 2, totalRecords);
        }
        else
        {
            Assert.Empty(queryResult);
        }
    }
}