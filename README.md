<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**  *generated with [DocToc](https://github.com/thlorenz/doctoc)*

- [Quick Start](#quick-start)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

This package is a C# wrapper around [delta-rs](https://github.com/delta-io/delta-rs/tree/rust-v0.17.0).

It uses the [tokio-rs](https://tokio.rs/) runtime to provide asynchronous behavior. This allows the usage of .NET Tasks and async/await to take advantage of the same behavior provided by the underlying rust library.
This library also takes advantage of the [Apache Arrow](https://github.com/apache/arrow/blob/main/csharp/README.md) [C Data Interface](https://arrow.apache.org/docs/format/CDataInterface.html) to minimize the amount of copying required to move data between runtimes.

![alt text](/media/images/delta-dot-net-pkg.png "Using a Rust bridge library with .NET p/invoke")

The bridge library incorporates delta-rs and [tokio-rs](https://tokio.rs/) as shown in the image below.
![alt text](/media/images/bridge-library.png "Rust bridge library with tokio")

## Quick Start

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Runtime;
using DeltaLake.Table;


public static Runtime CreateRuntime()
{
    return new DeltaRuntime(RuntimeOptions.Default);
}

public static Task<DeltaTable> CreateDeltaTable(
    Runtime runtime,
    string path,
    CancellationToken cancellationToken
)
{
    var builder = new Apache.Arrow.Schema.Builder();
    builder.Field(fb =>
    {
        fb.Name("test");
        fb.DataType(Int32Type.Default);
        fb.Nullable(false);
    });
    var schema = builder.Build();
    return DeltaTable.CreateAsync(
        runtime,
        new TableCreateOptions(uri, schema)
        {
            Configuration = new Dictionary<string, string>(),
        },
        cancellationToken);
}

public static Task<DeltaTable, Runtime> InsertIntoTable(
    DeltaTable table,
    CancellationToken cancellationToken)
{
    var allocator = new NativeMemoryAllocator();
    var recordBatchBuilder = new RecordBatch.Builder(allocator)
        .Append(
            "test",
            false,
            col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, length))));
    var options = new InsertOptions
    {
        SaveMode = SaveMode.Append,
    };
    await table.InsertAsync(
        [recordBatchBuilder.Build()],
        schema,
        options,
        cancellationToken);
}
```
