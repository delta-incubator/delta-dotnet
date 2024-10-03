using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using Azure.Core;
using Azure.Identity;
using DeltaLake.Interfaces;
using DeltaLake.Table;

namespace local;

public class Program
{
    private static readonly string stringColumnName = "colStringTest";
    private static readonly string intColumnName = "colIntegerTest";

    private static string envVarIfRunningInVisualStudio = Environment.GetEnvironmentVariable("VisualStudioVersion");

    private static readonly string azureStorageAuthScope = "https://storage.azure.com/.default";

    public static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException(
                $@"
                    Usage: <absolute-path> <num-rows>
                    Examples:
                        Local: 'C:\\folder\\demo-table' '20'
                        Azure: 'abfss://container@storage.dfs.core.windows.net/demo-table' '30'
                "
            );
        }

        var uri = args[0];
        int numRows = int.Parse(args[1]);

        var storageOptions = new Dictionary<string, string>();
        if (uri.StartsWith("abfss://"))
        {
            storageOptions.Add("bearer_token", GenerateAzureStorageOAuthToken());
        }

        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        {
            var builder = new Apache.Arrow.Schema.Builder();
            builder
                .Field(fb =>
                {
                    fb.Name(stringColumnName);
                    fb.DataType(StringType.Default);
                    fb.Nullable(false);
                })
                .Field(static fb =>
                {
                    fb.Name(intColumnName);
                    fb.DataType(Int32Type.Default);
                    fb.Nullable(false);
                });
            var schema = builder.Build();
            var allocator = new NativeMemoryAllocator();
            var randomValueGenerator = new Random();
            var recordBatchBuilder = new RecordBatch.Builder(allocator)
                .Append(stringColumnName, false, col => col.String(arr => arr.AppendRange(Enumerable.Range(0, numRows).Select(_ => GenerateRandomString(randomValueGenerator)))))
                .Append(intColumnName, false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, numRows).Select(_ => randomValueGenerator.Next()))));

            using var table = await engine.CreateTableAsync(
                new TableCreateOptions(uri, schema)
                {
                    Configuration = new Dictionary<string, string>
                    {
                        ["delta.dataSkippingNumIndexedCols"] = "32"
                    },
                    StorageOptions = storageOptions,
                },
                CancellationToken.None);

            Console.WriteLine($"Table root path: {table.Location()}");
            Console.WriteLine($"Table version before transaction: {table.Version()}");
            var options = new InsertOptions
            {
                SaveMode = SaveMode.Append,
            };
            await table.InsertAsync([recordBatchBuilder.Build()], schema, options, CancellationToken.None);
            Console.WriteLine($"Table version after transaction: {table.Version()}");
        }
    }

    private static string GenerateRandomString(Random random, int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GenerateAzureStorageOAuthToken()
    {
        bool isRunningInsideVisualStudio = !string.IsNullOrEmpty(envVarIfRunningInVisualStudio);

        // All supported Entra auth formats:
        //
        // >>> https://learn.microsoft.com/en-us/dotnet/api/azure.core.tokencredential?view=azure-dotnet
        //
        // We prioritize Visual Studio, because it's most guaranteed to be the
        // author's identity, fallback to DefaultAzureCredential which includes
        // a chain of auth sources to try.
        //
        if (isRunningInsideVisualStudio)
        {
            return new VisualStudioCredential().GetToken(new TokenRequestContext(new[] { azureStorageAuthScope }), default).Token;
        }
        return new DefaultAzureCredential().GetToken(new TokenRequestContext(new[] { azureStorageAuthScope }), default).Token;
    }
}