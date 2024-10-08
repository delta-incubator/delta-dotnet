using DeltaLake.Errors;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class QueryTests
{
    [Fact]
    public async Task Query_Cancellation_Test()
    {
        var data = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 1);
        using var table = data.table;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            var query = new SelectQuery("select * from test")
            {
                TableAlias = "test",
            };
            table.QueryAsync(query, new CancellationToken(true))
            .ToBlockingEnumerable()
            .ToList();
        });
    }

    [Theory]
    [InlineData("SELECT * FROM TEST", "test")]
    [InlineData("SELECT * FROM test", "test")]
    [InlineData("SELECT * FROM mytable", "mytable")]
    public async Task Query_Valid_Query_Params_Test(string queryText, string tableAlias)
    {
        var data = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 1);
        using var table = data.table;
        var query = new SelectQuery(queryText)
        {
            TableAlias = tableAlias,
        };
        var results = table.QueryAsync(query, CancellationToken.None)
        .ToBlockingEnumerable()
        .ToList();
        Assert.Single(results);
        Assert.Equal(1, results[0].Length);
    }

    [Theory]
    [InlineData("SELECT * FROM test", "junk")]
    [InlineData("DELETE FROM TEST", "test")]
    [InlineData("CREATE TABLE foo (x INTEGER)", "foo")]
    [InlineData("UPDATE junk set first = 0", "junk")]
    public async Task Query_Invalid_Query_Params_Test(string queryText, string tableAlias)
    {
        var data = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 1);
        using var table = data.table;
        await Assert.ThrowsAsync<DeltaRuntimeException>(async () =>
        {
            var query = new SelectQuery(queryText)
            {
                TableAlias = tableAlias,
            };
            table.QueryAsync(query, CancellationToken.None)
            .ToBlockingEnumerable()
            .ToList();
        });
    }
}