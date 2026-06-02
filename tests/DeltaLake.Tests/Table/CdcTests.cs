using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Interfaces;
using DeltaLake.Kernel.Callbacks.Errors;
using DeltaLake.Kernel.Interop;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class CdcTests
{
    private const string ChangeTypeColumn = "_change_type";
    private const string CommitVersionColumn = "_commit_version";
    private const string CommitTimestampColumn = "_commit_timestamp";

    // -----------------------------------------------------------------------
    // Happy-path: fixture table already has CDF enabled across 3 versions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_ExistingFixture_ReturnsRecordBatches()
    {
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        using ITable table = await engine.LoadTableAsync(
            new TableOptions { TableLocation = TableIdentifier.SimpleTableWithCdc.TablePath() },
            CancellationToken.None);

        var batches = new List<RecordBatch>();
        await foreach (RecordBatch batch in table.QueryTableChangesAsync(
            new TableChangesOptions { StartVersion = 0 }, CancellationToken.None))
        {
            batches.Add(batch);
        }

        Assert.NotEmpty(batches);
    }

    [Fact]
    public async Task ReadTableChanges_ExistingFixture_SchemaContainsSystemColumns()
    {
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        using ITable table = await engine.LoadTableAsync(
            new TableOptions { TableLocation = TableIdentifier.SimpleTableWithCdc.TablePath() },
            CancellationToken.None);

        var batches = new List<RecordBatch>();
        await foreach (RecordBatch batch in table.QueryTableChangesAsync(
            new TableChangesOptions { StartVersion = 0 }, CancellationToken.None))
        {
            batches.Add(batch);
        }

        Assert.NotEmpty(batches);
        Schema schema = batches[0].Schema;
        Assert.NotNull(schema.GetFieldByName(ChangeTypeColumn));
        Assert.NotNull(schema.GetFieldByName(CommitVersionColumn));
        Assert.NotNull(schema.GetFieldByName(CommitTimestampColumn));
    }

    // -----------------------------------------------------------------------
    // Version range: EndVersion limits which commits are returned
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_WithEndVersion_RespectsVersionBound()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            using IEngine engine = new DeltaEngine(EngineOptions.Default);
            using ITable table = await CreateCdfTableAsync(engine, tempDir.FullName);

            // v1: insert 3 rows
            await table.InsertAsync(
                [BuildIdNameBatch(startId: 1, count: 3)], table.Schema(),
                new InsertOptions { SaveMode = SaveMode.Append },
                CancellationToken.None);

            // v2: insert 3 more rows
            await table.InsertAsync(
                [BuildIdNameBatch(startId: 4, count: 3)], table.Schema(),
                new InsertOptions { SaveMode = SaveMode.Append },
                CancellationToken.None);

            // Read only v1 changes
            var batches = new List<RecordBatch>();
            await foreach (RecordBatch batch in table.QueryTableChangesAsync(
                new TableChangesOptions { StartVersion = 1, EndVersion = 1 }, CancellationToken.None))
            {
                batches.Add(batch);
            }

            Assert.NotEmpty(batches);

            // Every row in every batch must come from commit version 1
            foreach (RecordBatch batch in batches)
            {
                Int64Array versionCol = (Int64Array)batch.Column(CommitVersionColumn);
                for (int i = 0; i < versionCol.Length; i++)
                {
                    Assert.Equal(1L, versionCol.GetValue(i));
                }
            }
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Data integrity: inserted rows appear in CDC output
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_RoundTrip_InsertedRowsAppearInCdc()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            using IEngine engine = new DeltaEngine(EngineOptions.Default);
            using ITable table = await CreateCdfTableAsync(engine, tempDir.FullName);

            const int rowCount = 5;
            await table.InsertAsync(
                [BuildIdNameBatch(startId: 1, count: rowCount)], table.Schema(),
                new InsertOptions { SaveMode = SaveMode.Append },
                CancellationToken.None);

            int cdcRowCount = 0;
            await foreach (RecordBatch batch in table.QueryTableChangesAsync(
                new TableChangesOptions { StartVersion = 1 }, CancellationToken.None))
            {
                cdcRowCount += batch.Length;
            }

            Assert.Equal(rowCount, cdcRowCount);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // CDF disabled: kernel error code 37
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_NoCdfEnabled_ThrowsKernelException()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var (engine, table) = await TableHelpers.SetupTable(tempDir.FullName, length: 3);
            using var _ = engine;
            using var __ = table;

            var ex = await Assert.ThrowsAsync<KernelException>(async () =>
            {
                await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                    new TableChangesOptions { StartVersion = 0 }, CancellationToken.None))
                {
                    // should not reach here
                }
            });

            Assert.Equal(KernelError.ChangeDataFeedUnsupported, ex.ErrorCode);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Null options guard
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_NullOptions_ThrowsArgumentNullException()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var (engine, table) = await TableHelpers.SetupTable(tempDir.FullName, length: 1);
            using var _ = engine;
            using var __ = table;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                    null!, CancellationToken.None))
                { }
            });
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static async Task<ITable> CreateCdfTableAsync(IEngine engine, string location)
    {
        var schema = new Schema.Builder()
            .Field(fb => { fb.Name("id"); fb.DataType(Int32Type.Default); fb.Nullable(true); })
            .Field(fb => { fb.Name("name"); fb.DataType(StringType.Default); fb.Nullable(true); })
            .Build();

        return await engine.CreateTableAsync(
            new TableCreateOptions(location, schema)
            {
                Configuration = new Dictionary<string, string>
                {
                    ["delta.enableChangeDataFeed"] = "true",
                },
            },
            CancellationToken.None);
    }

    private static RecordBatch BuildIdNameBatch(int startId, int count)
    {
        var allocator = new NativeMemoryAllocator();
        return new RecordBatch.Builder(allocator)
            .Append("id", true, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(startId, count))))
            .Append("name", true, col => col.String(arr => arr.AppendRange(
                Enumerable.Range(startId, count).Select(i => $"name_{i}"))))
            .Build();
    }
}
