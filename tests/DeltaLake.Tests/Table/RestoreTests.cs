using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class RestoreTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Restore_Cancellation_Test(bool useVersion)
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var table = data.table;
        var options = new RestoreOptions
        {
            IgnoreMissingFiles = true,
            ProtocolDowngradeAllowed = true,
            Version = useVersion ? 1 : null,
            Timestamp = useVersion ? null : DateTimeOffset.UtcNow,
        };
        var version = table.Version();
        try
        {
            await table.RestoreAsync(options, new CancellationToken(true));
            throw new InvalidOperationException();
        }
        catch (OperationCanceledException)
        {
            Assert.Equal(version, table.Version());
        }
    }
}
