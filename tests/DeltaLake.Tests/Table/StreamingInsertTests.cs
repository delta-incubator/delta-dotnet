using Apache.Arrow;
using Apache.Arrow.Ipc;
using DeltaLake.Errors;
using DeltaLake.Interfaces;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

/// <summary>
/// Tests for inserting from an <see cref="IArrowArrayStream"/>, where the record batches are
/// produced lazily as the writer consumes them rather than being materialized up front.
/// </summary>
public class StreamingInsertTests
{
    // A stream that produces no batches still commits a new version
    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 10)]
    [InlineData(20, 13)]
    public async Task Memory_Insert_Stream_Of_Batches_Test(int batchCount, int rowsPerBatch)
    {
        var tableParts = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 0);
        using var engine = tableParts.engine;
        using var table = tableParts.table;
        var version = table.Version();

        using var producer = TestRecordBatchProducer.Basic(table.Schema(), batchCount, rowsPerBatch);
        await table.InsertAsync(producer, new InsertOptions(), CancellationToken.None);

        Assert.Equal(version + 1, table.Version());
        Assert.Equal(batchCount, producer.BatchesProduced);
        Assert.Equal(Enumerable.Range(0, batchCount * rowsPerBatch), ReadTestColumn(table));
    }

    [Fact]
    public async Task File_System_Insert_Stream_Of_Batches_Test()
    {
        var tempDir = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var tableParts = await TableHelpers.SetupTable($"file://{tempDir.FullName}", 0);
            using var engine = tableParts.engine;
            using var table = tableParts.table;
            var version = table.Version();

            using var producer = TestRecordBatchProducer.Basic(table.Schema(), totalBatches: 50, rowsPerBatch: 11);
            // A small row group size forces multiple row groups per file, so the write
            // has to flush while the producer is still generating batches.
            await table.InsertAsync(producer, new InsertOptions { MaxRowsPerGroup = 10 }, CancellationToken.None);

            Assert.Equal(version + 1, table.Version());
            Assert.Equal(50, producer.BatchesProduced);
            Assert.Equal(Enumerable.Range(0, 50 * 11), ReadTestColumn(table));
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    [Theory]
    [InlineData(0, SaveMode.Append)]
    [InlineData(3, SaveMode.Append)]
    [InlineData(3, SaveMode.Overwrite)]
    public async Task Insert_Stream_Producer_Error_Test(int failAfterBatches, SaveMode saveMode)
    {
        var tableParts = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 20);
        using var engine = tableParts.engine;
        using var table = tableParts.table;
        var version = table.Version();

        using var producer = TestRecordBatchProducer.Basic(
            table.Schema(),
            totalBatches: 100,
            rowsPerBatch: 10,
            failAfterBatches: failAfterBatches);

        await Assert.ThrowsAsync<DeltaRuntimeException>(
            () => table.InsertAsync(
                producer,
                new InsertOptions { SaveMode = saveMode },
                CancellationToken.None));

        // The stream is consumed lazily, so the producer is abandoned at the point it failed,
        // nothing is committed, and the existing data is left intact even when overwriting.
        Assert.Equal(failAfterBatches, producer.BatchesProduced);
        Assert.Equal(version, table.Version());
        Assert.Equal(Enumerable.Range(0, 20), ReadTestColumn(table));
    }

    [Fact]
    public async Task Insert_Stream_Will_Cancel_Midway_Test()
    {
        var tableParts = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 0);
        using var engine = tableParts.engine;
        using var table = tableParts.table;
        var version = table.Version();

        using var cancellationTokenSource = new CancellationTokenSource();
        const int totalBatches = 20000;
        using var producer = TestRecordBatchProducer.Basic(
            table.Schema(),
            totalBatches,
            rowsPerBatch: 1,
            onBatchProduced: produced =>
            {
                if (produced == 10)
                {
                    cancellationTokenSource.Cancel();
                }
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => table.InsertAsync(producer, new InsertOptions(), cancellationTokenSource.Token));

        // Cancellation propagates back to the producer instead of it being drained.
        Assert.InRange(producer.BatchesProduced, 10, totalBatches - 1);
        Assert.Equal(version, table.Version());
        Assert.Empty(ReadTestColumn(table));
    }

    private static int[] ReadTestColumn(ITable table)
    {
        return table.QueryAsync(
                new SelectQuery("SELECT test FROM test") { TableAlias = "test" },
                CancellationToken.None)
            .ToBlockingEnumerable()
            .SelectMany(batch => ((Int32Array)batch.Column(0)).Values.ToArray())
            .OrderBy(value => value)
            .ToArray();
    }

    /// <summary>
    /// An <see cref="IArrowArrayStream"/> that generates record batches on demand, and can be
    /// configured to fail part way through the stream.
    /// </summary>
    private sealed class TestRecordBatchProducer : IArrowArrayStream
    {
        private const string FailureMessage = "Simulated failure while producing record batches";

        private readonly int _totalBatches;
        private readonly Func<int, RecordBatch> _batchFactory;
        private readonly int _failAfterBatches;
        private readonly Action<int>? _onBatchProduced;

        private TestRecordBatchProducer(
            Schema schema,
            int totalBatches,
            Func<int, RecordBatch> batchFactory,
            int failAfterBatches = -1,
            Action<int>? onBatchProduced = null)
        {
            Schema = schema;
            _totalBatches = totalBatches;
            _batchFactory = batchFactory;
            _failAfterBatches = failAfterBatches;
            _onBatchProduced = onBatchProduced;
        }

        public Schema Schema { get; }

        /// <summary>
        /// Number of batches that have been handed to the consumer so far.
        /// </summary>
        public int BatchesProduced { get; private set; }

        /// <summary>
        /// Creates a producer of batches matching the schema built by
        /// <see cref="TableHelpers.SetupTable(string, int, InsertOptions)"/>, where the values in
        /// each batch continue on from the previous one.
        /// </summary>
        public static TestRecordBatchProducer Basic(
            Schema schema,
            int totalBatches,
            int rowsPerBatch,
            int failAfterBatches = -1,
            Action<int>? onBatchProduced = null)
        {
            return new TestRecordBatchProducer(
                schema,
                totalBatches,
                index => TableHelpers.BuildBasicRecordBatch(index * rowsPerBatch, rowsPerBatch),
                failAfterBatches,
                onBatchProduced);
        }

        public ValueTask<RecordBatch> ReadNextRecordBatchAsync(CancellationToken cancellationToken = default)
        {
            if (BatchesProduced == _failAfterBatches)
            {
                throw new InvalidOperationException(FailureMessage);
            }

            if (BatchesProduced >= _totalBatches)
            {
                // A null batch signals the end of the stream
                return new ValueTask<RecordBatch>((RecordBatch)null!);
            }

            var batch = _batchFactory(BatchesProduced);
            BatchesProduced++;
            _onBatchProduced?.Invoke(BatchesProduced);
            return new ValueTask<RecordBatch>(batch);
        }

        public void Dispose()
        {
        }
    }
}
