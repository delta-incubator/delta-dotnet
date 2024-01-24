using System;
using System.Threading.Tasks;
using DeltaLake.Runtime;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class DeltaTableTests
{
    [Fact]
    public async Task Create_InMemory_Test()
    {
        var uri = $"memory://{Guid.NewGuid().ToString("N")}";
        var runtime = new DeltaRuntime(RuntimeOptions.Default);
        var table = await DeltaTable.CreateAsync(runtime, uri, new TableOptions());
        Assert.NotNull(table);
    }
}