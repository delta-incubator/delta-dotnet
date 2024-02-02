using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Runtime;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class DeltaTableTests
{
    [Fact]
    public async Task Create_InMemory_Test()
    {
        var uri = $"memory://{Guid.NewGuid():N}";
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        var builder = new Apache.Arrow.Schema.Builder();
        builder.Field(fb =>
        {
            fb.Name("test");
            fb.DataType(Int32Type.Default);
            fb.Nullable(false);
        });
        var schema = builder.Build();
        using var table = await DeltaTable.CreateAsync(
            runtime,
            new TableCreateOptions(uri, schema),
            CancellationToken.None);
        Assert.NotNull(table);
        var version = table.Version();
        Assert.Equal(0, version);
        var location = table.Location();
        Assert.Equal(uri, location);
        var files = table.Files();
        Assert.Empty(files);
        var fileUris = table.FileUris();
        Assert.Empty(fileUris);
        var returnedSchema = table.Schema();
        Assert.NotNull(returnedSchema);
        Assert.Equal(schema.FieldsList.Count, returnedSchema.FieldsList.Count);
    }

    [Fact]
    public async Task Load_Table_Test()
    {
        var location = Path.Join(Settings.TestRoot, "simple_table");
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4, table.Version());
    }

    [Fact]
    public async Task Load_Table_Memory_Test()
    {
        var location = Path.Join(Settings.TestRoot, "simple_table");
        var memory = System.Text.Encoding.UTF8.GetBytes(location);
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, memory.AsMemory(), new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4, table.Version());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Load_Table_At_Version_Test(long version)
    {
        var location = Path.Join(Settings.TestRoot, "simple_table");
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions
        {
            Version = version,
        },
        CancellationToken.None);
        Assert.Equal(version, table.Version());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Table_Load_Version_Test(long version)
    {
        var location = Path.Join(Settings.TestRoot, "simple_table");
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        using var table = await DeltaTable.LoadAsync(runtime, location, new TableOptions(),
        CancellationToken.None);
        Assert.Equal(4, table.Version());
        await table.LoadVersionAsync(version, CancellationToken.None);
        Assert.Equal(version, table.Version());
    }

    [Fact]
    public async Task Table_Insert_Test()
    {
        var uri = $"memory://{Guid.NewGuid():N}";
        using var runtime = new DeltaRuntime(RuntimeOptions.Default);
        var builder = new Apache.Arrow.Schema.Builder();
        builder.Field(fb =>
        {
            fb.Name("test");
            fb.DataType(Int32Type.Default);
            fb.Nullable(false);
        });
        var schema = builder.Build();
        using var table = await DeltaTable.CreateAsync(
            runtime,
            new TableCreateOptions(uri, schema),
            CancellationToken.None);
        Assert.NotNull(table);
        int length = 10;
        var allocator = new NativeMemoryAllocator();
        var recordBatchBuilder = new RecordBatch.Builder(allocator)
            .Append("test", false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, length))));


        var options = new InsertOptions
        {
            SaveMode = SaveMode.Append,
        };
        await table.InsertAsync([recordBatchBuilder.Build()], schema, options, CancellationToken.None);
        var version = table.Version();
        Assert.Equal(1, version);
        await foreach (var result in table.QueryAsync(new SelectQuery("SELECT test FROM test WHERE test = 1")
        {
            TableAlias = "test",
        },
        CancellationToken.None))
        {
            Assert.NotNull(result);
        }
    }
}