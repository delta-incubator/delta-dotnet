using System;
using System.Threading.Tasks;
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
        var table = await DeltaTable.CreateAsync(runtime, new TableCreateOptions(uri, builder.Build()));
        Assert.NotNull(table);
        var version = table.Version();
        Assert.Equal(0, version);
        var location = table.Location();
        Assert.Equal(uri, location);
        var files = table.Files();
        Assert.Empty(files);
        var fileUris = table.FileUris();
        Assert.Empty(fileUris);
    }
}