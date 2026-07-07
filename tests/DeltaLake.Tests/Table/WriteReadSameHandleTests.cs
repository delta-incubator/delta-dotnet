using DeltaLake.Kernel.Core;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

/// <summary>
/// Regression tests for reading DATA after a write on the SAME table handle.
/// These guard the incremental-snapshot generalization: kernel reads
/// (ReadAsArrowTableAsync/ReadAsDataFrameAsync) go through the now-incremental
/// RefreshSnapshot, which must still observe commits made on the same handle by
/// both the kernel commit path (CreateWriteTransactionAsync) and the delta-rs
/// bridge write path (InsertAsync).
/// </summary>
public class WriteReadSameHandleTests
{
    [Fact]
    public async Task Kernel_Commit_Then_Read_Reflects_Write_On_Same_Handle()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var path = DirectoryHelpers.ToFileUri(info.FullName);
            // SetupTable creates v0 and writes an initial commit at v1 (2 rows) into a real
            // parquet file at the table root.
            var data = await TableHelpers.SetupTable(path, 2);
            using var table = data.table;

            // Read once to materialize + cache a kernel snapshot at v1.
            using (OwnedArrowTable initial = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(2, initial.Table.RowCount);
            }

            // Kernel commit (add-file action) on the SAME handle -> v2. Reference the REAL
            // parquet the initial insert wrote so the post-commit kernel scan can physically
            // read it. Delta log replay dedupes active files by path, so the logical row set
            // is unchanged; the point is to drive the full Arrow read pipeline through the
            // post-commit incremental snapshot, not to change the data.
            var realFile = Directory
                .GetFiles(info.FullName, "*.parquet", SearchOption.AllDirectories)
                .First();
            var relativePath = Path.GetRelativePath(info.FullName, realFile).Replace('\\', '/');
            var actions = new List<AddAction>
            {
                new AddAction
                {
                    Path = relativePath,
                    Size = new FileInfo(realFile).Length,
                    ModificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    DataChange = true,
                },
            };
            var newVersion = await table.CreateWriteTransactionAsync(actions, CancellationToken.None);
            Assert.Equal(2UL, (ulong)newVersion);

            // A kernel DATA read on the SAME handle must advance the cached snapshot to v2
            // (incremental RefreshSnapshot) and drive the full Arrow read pipeline
            // (scan/schema/partition/physical-schema) end-to-end without error.
            using (OwnedArrowTable afterCommit = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(2, afterCommit.Table.RowCount);
            }

            // The kernel Version() (now incremental) must reflect the commit.
            Assert.Equal(2UL, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task Bridge_Insert_Then_Kernel_Read_Reflects_Write_On_Same_Handle()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var path = DirectoryHelpers.ToFileUri(info.FullName);
            // SetupTable creates v0 and writes an initial commit at v1 (1 row).
            var data = await TableHelpers.SetupTable(path, 1);
            using var table = data.table;
            var schema = table.Schema();
            var options = new InsertOptions { SaveMode = SaveMode.Append };

            // Read once to materialize + cache a kernel snapshot at v1 (1 row).
            using (OwnedArrowTable initial = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(1, initial.Table.RowCount);
            }

            // delta-rs BRIDGE write (Insert) on the SAME handle: adds 3 rows -> v2.
            // The bridge does not touch the kernel snapshot, so the cache is stale;
            // the next kernel read must incrementally re-list and observe the insert.
            await table.InsertAsync(
                [TableHelpers.BuildBasicRecordBatch(3)],
                schema,
                options,
                CancellationToken.None);

            using (OwnedArrowTable afterInsert = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(1 + 3, afterInsert.Table.RowCount);
            }
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task Repeated_Read_No_New_Commits_Returns_Consistent_Data()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var path = DirectoryHelpers.ToFileUri(info.FullName);
            var data = await TableHelpers.SetupTable(path, 5);
            using var table = data.table;

            // Two consecutive reads with no new commits: exercises the equal-version
            // short-circuit (derived caches reused) and must return identical data.
            using (OwnedArrowTable first = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(5, first.Table.RowCount);
            }

            using (OwnedArrowTable second = await table.ReadAsArrowTableAsync(CancellationToken.None))
            {
                Assert.Equal(5, second.Table.RowCount);
            }

            Assert.Equal(1UL, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }
}
