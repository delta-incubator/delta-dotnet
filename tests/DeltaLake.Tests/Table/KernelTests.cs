using System.Text.RegularExpressions;
using Apache.Arrow;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;
using DeltaLake.Extensions;
using DeltaLake.Interfaces;
using DeltaLake.Table;
using Microsoft.Data.Analysis;

namespace DeltaLake.Tests.Table;

public class KernelTests
{
    private static readonly string stringColumnName = "colStringTest";
    private static readonly string intColumnName = "colIntegerTest";
    private static readonly string partitionStringColumnName = "colPartitionStringTest";
    private static readonly string partitionIntegerColumnName = "colPartitionIntegerTest";
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    [Fact]
    public async Task Multi_Partitioned_Table_Parallelized_Bridge_Write_Can_Be_Read_By_Kernel()
    {
        // Setup
        //
        int numRowsPerPartition = 10;
        int numPartitions = 5;
        int numTransactionPerStringPartition = 3;
        int numTransactionPerIntegerPartition = 5;
        int numRows = numRowsPerPartition * numPartitions * numTransactionPerStringPartition * numTransactionPerIntegerPartition;
        int numColumns = 4;

        var tempDir = Directory.CreateTempSubdirectory();
        using IEngine engine = new DeltaEngine(EngineOptions.Default);
        var builder = new Apache.Arrow.Schema.Builder()
                                .Field(fb => { fb.Name(stringColumnName); fb.DataType(StringType.Default); fb.Nullable(false); })
                                .Field(fb => { fb.Name(partitionStringColumnName); fb.DataType(StringType.Default); fb.Nullable(false); })
                                .Field(fb => { fb.Name(partitionIntegerColumnName); fb.DataType(Int32Type.Default); fb.Nullable(false); })
                                .Field(fb => { fb.Name(intColumnName); fb.DataType(Int32Type.Default); fb.Nullable(false); });
        var schema = builder.Build();
        var tableCreateOptions = new TableCreateOptions(tempDir.FullName, schema)
        {
            Configuration = new Dictionary<string, string> { ["delta.dataSkippingNumIndexedCols"] = "32" },
            PartitionBy = new[] { partitionStringColumnName, partitionIntegerColumnName },
        };
        var tableWriteOptions = new InsertOptions { SaveMode = SaveMode.Append };
        var allocator = new NativeMemoryAllocator();
        var randomValueGenerator = new Random();
        var hostNamePrefix = Environment.MachineName;

        try
        {
            // Exercise: Writes via Bridge
            //
            using ITable table = await engine.CreateTableAsync(tableCreateOptions, CancellationToken.None);
            for (int i = 0; i < numPartitions; i++)
            {
                for (int j = 0; j < numTransactionPerStringPartition; j++)
                {
                    for (int k = 0; k < numTransactionPerIntegerPartition; k++)
                    {
                        var partition = $"{hostNamePrefix}_{i}";
                        var recordBatchBuilder = new RecordBatch.Builder(allocator)
                                                            .Append(stringColumnName, false, col => col.String(arr => arr.AppendRange(Enumerable.Range(0, numRowsPerPartition).Select(_ => GenerateRandomString(randomValueGenerator)))))
                                                            .Append(partitionStringColumnName, false, col => col.String(arr => arr.AppendRange(Enumerable.Range(0, numRowsPerPartition).Select(_ => partition))))
                                                            .Append(partitionIntegerColumnName, false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, numRowsPerPartition).Select(_ => i * j * k))))
                                                            .Append(intColumnName, false, col => col.Int32(arr => arr.AppendRange(Enumerable.Range(0, numRowsPerPartition).Select(_ => randomValueGenerator.Next()))));
                        await table.InsertAsync(new[] { recordBatchBuilder.Build() }, schema, tableWriteOptions, CancellationToken.None);
                    }
                }
            }

            // Exercise: Reads via Kernel
            //
            Apache.Arrow.Table arrowTable = table.ReadAsArrowTable();
            DataFrame dataFrame = table.ReadAsDataFrame();
            string stringResult = dataFrame.ToMarkdown();

            // Validate: Data Integrity
            //
            Assert.Equal(numRows, arrowTable.RowCount);
            Assert.Equal(numRows, dataFrame.Rows.Count);
            Assert.Equal(numRows, Regex.Matches(stringResult, hostNamePrefix).Count);
            Assert.Equal(numColumns, arrowTable.ColumnCount);
            Assert.Equal(numColumns, dataFrame.Columns.Count);

            var writerSchemaFieldMap = schema.FieldsList.ToDictionary(field => field.Name);
            var kernelSchemaFieldMap = arrowTable.Schema.FieldsList.ToDictionary(field => field.Name);
            var bridgeSchemaFieldMap = table.Schema().FieldsList.ToDictionary(field => field.Name);

            // Validate: Schema Integrity
            //
            Assert.Equal(writerSchemaFieldMap.Count, kernelSchemaFieldMap.Count);
            Assert.Equal(writerSchemaFieldMap.Count, bridgeSchemaFieldMap.Count);
            Assert.Equal(writerSchemaFieldMap.Count, numColumns);

            foreach (var kvp in writerSchemaFieldMap)
            {
                Assert.True(bridgeSchemaFieldMap.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value.DataType, bridgeSchemaFieldMap[kvp.Key].DataType);
            }

            foreach (var kvp in writerSchemaFieldMap)
            {
                Assert.True(kernelSchemaFieldMap.ContainsKey(kvp.Key));
                if (kvp.Key == partitionIntegerColumnName)
                {
                    // Kernel has a limitation where it can only report back String as the Partition
                    // values:
                    //
                    // >>> https://delta-users.slack.com/archives/C04TRPG3LHZ/p1728178727958499
                    //
                    Assert.Equal(StringType.Default, kernelSchemaFieldMap[kvp.Key].DataType);
                    Assert.Equal(Int32Type.Default, writerSchemaFieldMap[kvp.Key].DataType);
                    continue;
                }
                else
                {
                    Assert.Equal(kvp.Value.DataType, kernelSchemaFieldMap[kvp.Key].DataType);
                }
            }
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    private static string GenerateRandomString(Random random, int length = 10) => new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
}
