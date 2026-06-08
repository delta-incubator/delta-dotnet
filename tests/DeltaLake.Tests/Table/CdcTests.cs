using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        IReadOnlyList<RecordBatch> batches = await LoadFixtureBatchesAsync();
        Assert.NotEmpty(batches);
    }

    [Fact]
    public async Task ReadTableChanges_ExistingFixture_SchemaContainsSystemColumns()
    {
        IReadOnlyList<RecordBatch> batches = await LoadFixtureBatchesAsync();
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
                new TableChangesOptions(startVersion: 1) { EndVersion = 1 }, CancellationToken.None))
            {
                batches.Add(batch);
            }

            Assert.NotEmpty(batches);

            // Every row in every batch must come from commit version 1
            foreach (RecordBatch batch in batches)
            {
                IArrowArray versionCol = batch.Column(CommitVersionColumn);
                for (int i = 0; i < versionCol.Length; i++)
                {
                    Assert.Equal(1L, GetInt64Value(versionCol, i));
                }
            }
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Data integrity: inserted rows appear in CDC output with correct type
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
                new TableChangesOptions(startVersion: 1), CancellationToken.None))
            {
                // All rows from an INSERT commit must have _change_type == "insert"
                var changeTypeCol = (StringArray)batch.Column(ChangeTypeColumn);
                for (int i = 0; i < changeTypeCol.Length; i++)
                {
                    Assert.Equal("insert", changeTypeCol.GetString(i));
                }

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
    // Empty range: CREATE TABLE commit (v0) produces no row-level CDC records
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_EmptyVersionRange_ReturnsNoBatches()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            using IEngine engine = new DeltaEngine(EngineOptions.Default);
            using ITable table = await CreateCdfTableAsync(engine, tempDir.FullName);

            // v0 is the CREATE TABLE commit — no row data, so CDC should yield nothing.
            int batchCount = 0;
            await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                new TableChangesOptions(startVersion: 0) { EndVersion = 0 }, CancellationToken.None))
            {
                batchCount++;
            }

            Assert.Equal(0, batchCount);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Out-of-range start version: must surface an error, not crash
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_StartVersionBeyondTable_ThrowsOrReturnsEmpty()
    {
        // The kernel's behavior for a start version beyond the table's current
        // version is implementation-defined, but it must not crash the process.
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        using ITable table = await engine.LoadTableAsync(
            new TableOptions { TableLocation = TableIdentifier.SimpleTableWithCdc.TablePath() },
            CancellationToken.None);

        Exception? ex = await Record.ExceptionAsync(async () =>
        {
            await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                new TableChangesOptions(startVersion: 9999), CancellationToken.None))
            { }
        });

        Assert.True(
            ex is null || ex is KernelException,
            $"Expected null or KernelException but got {ex?.GetType().Name}: {ex?.Message}");
    }

    // -----------------------------------------------------------------------
    // Cancellation: token cancels enumeration
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReadTableChanges_Cancelled_ThrowsOperationCanceledException()
    {
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        using ITable table = await engine.LoadTableAsync(
            new TableOptions { TableLocation = TableIdentifier.SimpleTableWithCdc.TablePath() },
            CancellationToken.None);

        using var cts = new CancellationTokenSource();
#pragma warning disable VSTHRD103 // CancelAsync not available on net472
        cts.Cancel();
#pragma warning restore VSTHRD103

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                new TableChangesOptions(startVersion: 0), cts.Token))
            { }
        });
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
            // Table created without delta.enableChangeDataFeed — kernel returns
            // KernelError.ChangeDataFeedUnsupported (code 37).
            var (engine, table) = await TableHelpers.SetupTable(tempDir.FullName, length: 3);
            using var _ = engine;
            using var __ = table;

            var ex = await Assert.ThrowsAsync<KernelException>(async () =>
            {
                await foreach (RecordBatch _ in table.QueryTableChangesAsync(
                    new TableChangesOptions(startVersion: 0), CancellationToken.None))
                { }
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

            // The null check fires at the call site (not deferred to first MoveNextAsync).
            Assert.Throws<ArgumentNullException>(() =>
                table.QueryTableChangesAsync(null!, CancellationToken.None));
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static async Task<IReadOnlyList<RecordBatch>> LoadFixtureBatchesAsync()
    {
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        using ITable table = await engine.LoadTableAsync(
            new TableOptions { TableLocation = TableIdentifier.SimpleTableWithCdc.TablePath() },
            CancellationToken.None);

        var batches = new List<RecordBatch>();
        await foreach (RecordBatch batch in table.QueryTableChangesAsync(
            new TableChangesOptions(startVersion: 0), CancellationToken.None))
        {
            batches.Add(batch);
        }

        return batches;
    }

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

    /// <summary>
    /// Reads a long value from a column that may be physically Int32 or Int64,
    /// guarding against Arrow physical-type assumptions.
    /// </summary>
    private static long GetInt64Value(IArrowArray array, int index) => array switch
    {
        Int64Array a => a.GetValue(index) ?? throw new System.InvalidOperationException($"Null at index {index}"),
        Int32Array a => a.GetValue(index) ?? throw new System.InvalidOperationException($"Null at index {index}"),
        _ => throw new System.InvalidCastException($"Unexpected column type for {CommitVersionColumn}: {array.GetType().Name}"),
    };
}
