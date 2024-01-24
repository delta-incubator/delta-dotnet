using DeltaLake.Runtime;

namespace DeltaLake.Tests.Runtime;

public class RuntimeTests
{
    [Fact]
    public void Create_Runtime_Test()
    {
        using var rt = new DeltaRuntime(new RuntimeOptions());
        Assert.NotNull(rt);
    }
}