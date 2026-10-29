using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class StorageOptionsTests
{
    // Storage options are passed to the kernel engine builder through set_builder_option,
    // which returns ExternResultbool. memory:// tables never build a kernel engine, so this
    // needs a file system table to exercise the call.
    [Fact]
    public async Task Load_FileSystem_Table_With_StorageOptions()
    {
        var directory = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var uri = DirectoryHelpers.ToFileUri(directory.FullName);
            var data = await TableHelpers.SetupTable(uri, 10);
            using var engine = data.engine;
            var expectedVersion = data.table.Version();
            data.table.Dispose();

            var options = new TableOptions
            {
                TableLocation = uri,
                StorageOptions = new Dictionary<string, string> { ["allow_http"] = "true" },
            };
            using var table = await engine.LoadTableAsync(options, CancellationToken.None);
            Assert.Equal(expectedVersion, table.Version());
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
