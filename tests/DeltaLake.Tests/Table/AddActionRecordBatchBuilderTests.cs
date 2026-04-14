using Apache.Arrow;
using Apache.Arrow.Types;
using DeltaLake.Kernel.Arrow.Builders;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class AddActionRecordBatchBuilderTests
{
    [Fact]
    public void Build_SingleAction_Returns_Correct_Schema()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "file.parquet",
                Size = 100,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["year"] = "2024",
                },
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(5, batch.Schema.FieldsList.Count);
        Assert.Equal("path", batch.Schema.FieldsList[0].Name);
        Assert.Equal("partitionValues", batch.Schema.FieldsList[1].Name);
        Assert.Equal("size", batch.Schema.FieldsList[2].Name);
        Assert.Equal("modificationTime", batch.Schema.FieldsList[3].Name);
        Assert.Equal("stats", batch.Schema.FieldsList[4].Name);
        Assert.True(batch.Schema.FieldsList[4].IsNullable);
        var statsType = Assert.IsType<StructType>(batch.Schema.FieldsList[4].DataType);
        Assert.Single(statsType.Fields);
        Assert.Equal("numRecords", statsType.Fields[0].Name);
        Assert.IsType<Int64Type>(statsType.Fields[0].DataType);
        Assert.True(statsType.Fields[0].IsNullable);
        Assert.Equal(1, batch.Length);
    }

    [Fact]
    public void Build_NullPartitions_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "data/part-00000.parquet",
                Size = 512,
                ModificationTime = 1711929600000,
                PartitionValues = null,
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(1, batch.Length);
        Assert.IsType<MapArray>(batch.Column("partitionValues"));
    }

    [Fact]
    public void Build_EmptyPartitions_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "data/part-00000.parquet",
                Size = 512,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>(),
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(1, batch.Length);
        Assert.IsType<MapArray>(batch.Column("partitionValues"));
    }

    [Fact]
    public void Build_MultipleActions_Returns_Correct_RowCount()
    {
        var actions = new List<AddAction>();
        for (int i = 0; i < 10; i++)
        {
            actions.Add(new AddAction
            {
                Path = $"part-{i:D5}.parquet",
                Size = 1024 * (i + 1),
                ModificationTime = 1711929600000 + i,
            });
        }

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(10, batch.Length);
    }

    [Fact]
    public void Build_MultiplePartitionColumns_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "year=2024/month=03/day=15/part-00000.parquet",
                Size = 4096,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["year"] = "2024",
                    ["month"] = "03",
                    ["day"] = "15",
                },
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(1, batch.Length);
        var mapArray = Assert.IsType<MapArray>(batch.Column("partitionValues"));
        Assert.Equal(1, mapArray.Length);
    }

    [Fact]
    public void Build_NullPartitionValue_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "region=__HIVE_DEFAULT_PARTITION__/part-00000.parquet",
                Size = 2048,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["region"] = null,
                },
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(1, batch.Length);
    }

    [Fact]
    public void Build_MixedPartitionAndNoPartition_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "year=2024/part-00000.parquet",
                Size = 1024,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["year"] = "2024",
                },
            },
            new AddAction
            {
                Path = "part-00001.parquet",
                Size = 2048,
                ModificationTime = 1711929600000,
                PartitionValues = null,
            },
            new AddAction
            {
                Path = "year=2025/part-00002.parquet",
                Size = 3072,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["year"] = "2025",
                },
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(3, batch.Length);
    }

    [Fact]
    public void Build_DataChange_False_Is_Preserved_On_Model()
    {
        var action = new AddAction
        {
            Path = "compacted.parquet",
            Size = 8192,
            ModificationTime = 1711929600000,
            DataChange = false,
        };

        Assert.False(action.DataChange);
    }

    [Fact]
    public void Build_DataChange_Default_Is_True_On_Model()
    {
        var action = new AddAction
        {
            Path = "data.parquet",
            Size = 1024,
            ModificationTime = 1711929600000,
        };

        Assert.True(action.DataChange);
    }

    [Fact]
    public void Build_Verifies_Column_Values()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "test-file.parquet",
                Size = 42,
                ModificationTime = 1711929600000,
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        var pathColumn = (StringArray)batch.Column("path");
        var sizeColumn = (Int64Array)batch.Column("size");
        var modTimeColumn = (Int64Array)batch.Column("modificationTime");

        Assert.Equal("test-file.parquet", pathColumn.GetString(0));
        Assert.Equal(42L, sizeColumn.GetValue(0));
        Assert.Equal(1711929600000L, modTimeColumn.GetValue(0));
    }

    [Fact]
    public void Build_SpecialCharacters_In_PartitionValues()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "part-00000.parquet",
                Size = 1024,
                ModificationTime = 1711929600000,
                PartitionValues = new Dictionary<string, string?>
                {
                    ["country"] = "United States",
                    ["city"] = "São Paulo",
                    ["tag"] = "hello=world",
                },
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(1, batch.Length);
    }

    [Fact]
    public void Build_DuplicatePaths_Throws_ArgumentException()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "part-00000.parquet",
                Size = 1024,
                ModificationTime = 1711929600000,
            },
            new AddAction
            {
                Path = "part-00000.parquet",
                Size = 2048,
                ModificationTime = 1711929600001,
            },
        };

        var ex = Assert.Throws<ArgumentException>(() => AddActionRecordBatchBuilder.Build(actions));

        Assert.Contains("part-00000.parquet", ex.Message);
    }

    [Fact]
    public void Build_CaseSensitivePaths_Treats_As_Distinct()
    {
        // Delta protocol (Add File): "The path is a URI as specified by RFC 2396".
        // RFC 2396 §6.1 defines URI comparison as case-sensitive (octet-by-octet).
        // Action Reconciliation uses path as a primary key with no normalization.
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "Part-00000.parquet",
                Size = 1024,
                ModificationTime = 1711929600000,
            },
            new AddAction
            {
                Path = "part-00000.parquet",
                Size = 2048,
                ModificationTime = 1711929600001,
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);

        Assert.Equal(2, batch.Length);
    }

    [Fact]
    public void Build_WithNumRecords_PopulatesStatsColumn()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "file1.parquet",
                Size = 100,
                ModificationTime = 1000,
                NumRecords = 42,
            },
        };
        using var batch = AddActionRecordBatchBuilder.Build(actions);
        var statsArray = (StructArray)batch.Column("stats");
        Assert.False(statsArray.IsNull(0));
        var numRecords = (Int64Array)statsArray.Fields[0];
        Assert.False(numRecords.IsNull(0));
        Assert.Equal(42L, numRecords.GetValue(0));
    }

    [Fact]
    public void Build_WithoutNumRecords_StatsStructIsNonNull_InnerFieldIsNull()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "file1.parquet",
                Size = 100,
                ModificationTime = 1000,
            },
        };
        using var batch = AddActionRecordBatchBuilder.Build(actions);
        var statsArray = (StructArray)batch.Column("stats");
        Assert.False(statsArray.IsNull(0));
        var numRecords = (Int64Array)statsArray.Fields[0];
        Assert.True(numRecords.IsNull(0));
    }

    [Fact]
    public void Build_MixedNumRecords_CorrectValues()
    {
        var actions = new List<AddAction>
        {
            new AddAction { Path = "a.parquet", Size = 1, ModificationTime = 1, NumRecords = 10 },
            new AddAction { Path = "b.parquet", Size = 2, ModificationTime = 2 },
            new AddAction { Path = "c.parquet", Size = 3, ModificationTime = 3, NumRecords = 30 },
        };
        using var batch = AddActionRecordBatchBuilder.Build(actions);
        var statsArray = (StructArray)batch.Column("stats");

        var numRecords = (Int64Array)statsArray.Fields[0];
        Assert.Equal(10L, numRecords.GetValue(0));
        Assert.True(numRecords.IsNull(1));
        Assert.Equal(30L, numRecords.GetValue(2));
    }

    [Fact]
    public void Build_NumRecordsZero_IsValid()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "empty.parquet",
                Size = 50,
                ModificationTime = 1000,
                NumRecords = 0,
            },
        };
        using var batch = AddActionRecordBatchBuilder.Build(actions);
        var statsArray = (StructArray)batch.Column("stats");
        Assert.False(statsArray.IsNull(0));
        var numRecords = (Int64Array)statsArray.Fields[0];
        Assert.Equal(0L, numRecords.GetValue(0));
    }
}
