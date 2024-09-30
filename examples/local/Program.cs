using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Runtime;
using DeltaLake.Table;

namespace local;

// Foo
public class Program
{
    public static async Task Main(string[] args)
    {
        var uri = args[0];
        int length;
        if (args.Length < 2 || !int.TryParse(args[1], out length))
        {
            length = 10;
        }

        var runtime = new DeltaRuntime(RuntimeOptions.Default);
        {
            var builder = new Apache.Arrow.Schema.Builder();
            builder.Field(fb =>
            {
                fb.Name("test");
                fb.DataType(Int32Type.Default);
                fb.Nullable(false);
            });
            var schema = builder.Build();
            var allocator = new NativeMemoryAllocator();
            var recordBatchBuilder = new RecordBatch.Builder(allocator)
                .Append("test", false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, length))));
            using var table = await DeltaTable.CreateAsync(
                runtime,
                new TableCreateOptions(uri, schema)
                {
                    Configuration = new Dictionary<string, string>
                    {
                        ["delta.dataSkippingNumIndexedCols"] = "32",
                    }
                },
                CancellationToken.None);
            var options = new InsertOptions
            {
                SaveMode = SaveMode.Append,
            };
            await table.InsertAsync([recordBatchBuilder.Build()], schema, options, CancellationToken.None);
        }

        runtime.Dispose();
    }
}