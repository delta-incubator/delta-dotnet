using Apache.Arrow;
using Apache.Arrow.Memory;
using DeltaLake.Errors;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;
public class MergeTests
{
    [Fact]
    public async Task Merge_Memory_Full_Test()
    {
        var query = @"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN MATCHED THEN
          UPDATE SET
            second = newdata.second,
            third = newdata.third
        WHEN NOT MATCHED BY SOURCE THEN DELETE
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            test,
            second,
            third
          )
          VALUES (
            newdata.test,
            'inserted data',
            99
          )";
        await BaseMergeTest(query, batches =>
        {
            var column1 = batches.SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column1.SequenceEqual([5, 6, 7, 8, 9, 10, 11, 12, 13, 14]));
            var column2 = batches.SelectMany(batch => ((IReadOnlyList<string>)(StringArray)batch.Column(1)).ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column2.SequenceEqual(["hello", "hello", "hello", "hello", "hello", "inserted data", "inserted data", "inserted data", "inserted data", "inserted data"]));
            var column3 = batches.SelectMany(batch => ((Int64Array)batch.Column(2)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column3.SequenceEqual([99L, 99L, 99L, 99L, 99L, 100L, 100L, 100L, 100L, 100L]));
        });
    }

    [Fact]
    public async Task Merge_Memory_No_Delete_Test()
    {
        var query = @"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN MATCHED THEN
          UPDATE SET
            second = newdata.second,
            third = newdata.third
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            test,
            second,
            third
          )
          VALUES (
            newdata.test,
            'inserted data',
            99
          )";
        await BaseMergeTest(query, batches =>
        {
            var column1 = batches.SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column1.SequenceEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]));
            var column2 = batches.SelectMany(batch => ((IReadOnlyList<string>)(StringArray)batch.Column(1)).ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column2.SequenceEqual(["0", "1", "2", "3", "4", "hello", "hello", "hello", "hello", "hello", "inserted data", "inserted data", "inserted data", "inserted data", "inserted data"]));
            var column3 = batches.SelectMany(batch => ((Int64Array)batch.Column(2)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column3.SequenceEqual([0L, 1L, 2L, 3L, 4L, 99L, 99L, 99L, 99L, 99L, 100L, 100L, 100L, 100L, 100L]));
        });
    }

    [Fact]
    public async Task Merge_Memory_No_Insert_Test()
    {
        var query = @"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN MATCHED THEN
          UPDATE SET
            second = newdata.second,
            third = newdata.third";
        await BaseMergeTest(query, batches =>
        {
            var column1 = batches.SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column1.SequenceEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]));
            var column2 = batches.SelectMany(batch => ((IReadOnlyList<string>)(StringArray)batch.Column(1)).ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column2.SequenceEqual(["0", "1", "2", "3", "4", "hello", "hello", "hello", "hello", "hello"]));
            var column3 = batches.SelectMany(batch => ((Int64Array)batch.Column(2)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column3.SequenceEqual([0L, 1L, 2L, 3L, 4L, 100L, 100L, 100L, 100L, 100L]));
        });
    }

    [Fact]
    public async Task Merge_Memory_No_Update_Test()
    {
        var query = @"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            test,
            second,
            third
          )
          VALUES (
            newdata.test,
            'inserted data',
            99
          )";
        await BaseMergeTest(query, batches =>
        {
            var column1 = batches.SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column1.SequenceEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]));
            var column2 = batches.SelectMany(batch => ((IReadOnlyList<string>)(StringArray)batch.Column(1)).ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column2.SequenceEqual(["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "inserted data", "inserted data", "inserted data", "inserted data", "inserted data"]));
            var column3 = batches.SelectMany(batch => ((Int64Array)batch.Column(2)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column3.SequenceEqual([0L, 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 99L, 99L, 99L, 99L, 99L]));
        });
    }

    [Fact]
    public async Task Merge_Memory_Invalid_Query_Test()
    {
        await Assert.ThrowsAsync<DeltaRuntimeException>(() =>
        {
            var query = @"MERGE mytable USING newdata
        ON newdata.test = mytable.test
        WHEN MATCHED THEN
          UPDATE SET
            second = newdata.second,
            third = newdata.third";
            return BaseMergeTest(query, batches =>
        {
            var column1 = batches.SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column1.SequenceEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]));
            var column2 = batches.SelectMany(batch => ((IReadOnlyList<string>)(StringArray)batch.Column(1)).ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column2.SequenceEqual(["0", "1", "2", "3", "4", "hello", "hello", "hello", "hello", "hello"]));
            var column3 = batches.SelectMany(batch => ((Int64Array)batch.Column(2)).Values.ToArray()).OrderBy(i => i).ToArray();
            Assert.True(column3.SequenceEqual([0L, 1L, 2L, 3L, 4L, 100L, 100L, 100L, 100L, 100L]));
        });
        });
    }

    [Fact]
    public async Task Merge_Memory_Cancel_Test()
    {
        var query = @"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            test,
            second,
            third
          )
          VALUES (
            newdata.test,
            'inserted data',
            99
          )";
        var pair = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 10);
        using var runtime = pair.runtime;
        using var table = pair.table;
        var allocator = new NativeMemoryAllocator();
        var enumerable = Enumerable.Range(5, 10);
        var recordBatchBuilder = new RecordBatch.Builder(allocator)
            .Append("test", false, col => col.Int32(arr => arr.AppendRange(enumerable)))
            .Append("second", false, col => col.String(arr => arr.AppendRange(enumerable.Select(_ => "hello"))))
            .Append("third", false, col => col.Int64(arr => arr.AppendRange(enumerable.Select(_ => 100L))));
        using var rb = recordBatchBuilder.Build();
        var version = table.Version();
        try
        {
            await table.MergeAsync(query, [rb], rb.Schema, new CancellationToken(true));
            throw new InvalidOperationException();
        }
        catch (OperationCanceledException)
        {
            Assert.Equal(version, table.Version());
        }
    }

    [Fact]
    public async Task Merge_Zero_Record_Count_Test()
    {
        var tableParts = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 0);
        using var runtime = tableParts.runtime;
        using var table = tableParts.table;
        var version = table.Version();
        await table.MergeAsync(@"MERGE INTO mytable USING newdata
        ON newdata.test = mytable.test
        WHEN NOT MATCHED BY TARGET
          THEN INSERT (
            test,
            second,
            third
          )
          VALUES (
            newdata.test,
            'inserted data',
            99
          )", [], table.Schema(), CancellationToken.None);
        Assert.Equal(version, table.Version());
    }

    private async Task BaseMergeTest(string query, Action<IReadOnlyList<RecordBatch>> assertions)
    {
        var pair = await TableHelpers.SetupTable($"memory://{Guid.NewGuid():N}", 10);
        using var runtime = pair.runtime;
        using var table = pair.table;
        var allocator = new NativeMemoryAllocator();
        var enumerable = Enumerable.Range(5, 10);
        var recordBatchBuilder = new RecordBatch.Builder(allocator)
            .Append("test", false, col => col.Int32(arr => arr.AppendRange(enumerable)))
            .Append("second", false, col => col.String(arr => arr.AppendRange(enumerable.Select(_ => "hello"))))
            .Append("third", false, col => col.Int64(arr => arr.AppendRange(enumerable.Select(_ => 100L))));
        using var rb = recordBatchBuilder.Build();
        await table.MergeAsync(query, [rb], rb.Schema, CancellationToken.None);
        var batches = table.QueryAsync(new SelectQuery("select * from deltatable"), CancellationToken.None).ToBlockingEnumerable().ToList();
        assertions(batches);
    }
}