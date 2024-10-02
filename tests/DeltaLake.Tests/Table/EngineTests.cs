using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class EngineTests
{
    [Fact]
    public void Create_Engine_Test()
    {
        using var engine = new DeltaEngine(new EngineOptions());
        Assert.NotNull(engine);
    }
}