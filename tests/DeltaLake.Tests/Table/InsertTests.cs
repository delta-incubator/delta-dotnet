using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class InsertTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Memory_Insert_Variable_Record_Count_Test(int length)
    {
        await BaseInsertTest($"memory://{Guid.NewGuid():N}", length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task File_System_Insert_Variable_Record_Count_Test(int length)
    {
        var info = Directory.CreateTempSubdirectory();
        try
        {
            await BaseInsertTest($"file://{info.FullName}", length);
        }
        finally
        {
            info.Delete(true);
        }
    }

    private async Task BaseInsertTest(string path, int length)
    {
        var data = await TableHelpers.SetupTable(path, length);
        using var runtime = data.runtime;
        using var table = data.table;
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