using Apache.Arrow;
using DeltaLake.Errors;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class UpdateTests
{
    private const string UpdateGreaterThanOne = "UPDATE test SET test = test + CAST(1 AS INT) WHERE test > CAST(1 AS INT)";
    private const string UpdateNoPredicate = "UPDATE test SET test = test + CAST(1 AS INT)";

    private const string UpdatePredicateMiss = "UPDATE test SET test = test + CAST(1 AS INT) WHERE test < CAST(0 as INT)";

    [Theory]
    [InlineData(1, UpdateGreaterThanOne)]
    [InlineData(2, UpdateGreaterThanOne)]
    [InlineData(10, UpdateGreaterThanOne)]
    [InlineData(100, UpdateGreaterThanOne)]
    public async Task Memory_Update_Variable_Record_Test(
        int length,
        string predicate)
    {
        var totalValue = length < 2 ? length - 1 : (length - 2) / 2 * (length + 3) + 1;
        await BaseUpdateTest($"memory:///{Guid.NewGuid():N}", length, predicate, totalValue);
    }

    [Theory]
    [InlineData(1, UpdateNoPredicate)]
    [InlineData(2, UpdateNoPredicate)]
    [InlineData(10, UpdateNoPredicate)]
    [InlineData(100, UpdateNoPredicate)]
    public async Task Memory_Update_No_Predicate_Variable_Record_Test(
        int length,
        string predicate)
    {
        var totalValue = length == 1 ? 1 : length / 2 * (length + 1);
        await BaseUpdateTest($"memory:///{Guid.NewGuid():N}", length, predicate, totalValue);
    }

    [Theory]
    [InlineData(1, UpdatePredicateMiss)]
    [InlineData(2, UpdatePredicateMiss)]
    [InlineData(10, UpdatePredicateMiss)]
    [InlineData(100, UpdatePredicateMiss)]
    public async Task Memory_Update_Predicate_Miss_Variable_Record_Test(
        int length,
        string predicate)
    {
        var totalValue = length / 2 * (length - 1);
        await BaseUpdateTest($"memory:///{Guid.NewGuid():N}", length, predicate, totalValue);
    }

    [Fact]
    public async Task Memory_Update_Invalid_Predicate__Test()
    {
        await Assert.ThrowsAsync<DeltaRuntimeException>(() =>
        BaseUpdateTest($"memory:///{Guid.NewGuid():N}", 10, "predicate", 0));
    }

    [Fact]
    public async Task Update_Cancellation_Test()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 10);
        using var table = data.table;
        var version = table.Version();
        try
        {
            await table.UpdateAsync("this will throw", new CancellationToken(true));
            throw new InvalidOperationException();
        }
        catch (OperationCanceledException)
        {
            Assert.Equal(version, table.Version());
        }
    }

    private async static Task BaseUpdateTest(
        string path,
        int length,
        string query,
        long expectedTotal)
    {
        var data = await TableHelpers.SetupTable(path, length);
        using var table = data.table;
        await table.UpdateAsync(query, CancellationToken.None);
        var queryResult = table.QueryAsync(new SelectQuery("select SUM(test) from test")
        {
            TableAlias = "test",
        },
        CancellationToken.None).ToBlockingEnumerable().ToList();
        var totalValue = queryResult.Select(rb => ((Int64Array)rb.Column(0)).Sum(i => i!.Value)).Sum();
        var totalRecords = queryResult.Select(s => s.Length).Sum();
        Assert.Equal(1, totalRecords);
        Assert.Equal(expectedTotal, totalValue);
    }
}