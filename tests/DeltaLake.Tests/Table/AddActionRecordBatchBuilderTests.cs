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
        Assert.Equal("dataChange", batch.Schema.FieldsList[4].Name);
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
    public void Build_DataChange_False_Sets_Correctly()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "compacted.parquet",
                Size = 8192,
                ModificationTime = 1711929600000,
                DataChange = false,
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);
        var dataChangeColumn = (BooleanArray)batch.Column("dataChange");

        Assert.False(dataChangeColumn.GetValue(0));
    }

    [Fact]
    public void Build_DataChange_Default_Is_True()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "data.parquet",
                Size = 1024,
                ModificationTime = 1711929600000,
            },
        };

        var batch = AddActionRecordBatchBuilder.Build(actions);
        var dataChangeColumn = (BooleanArray)batch.Column("dataChange");

        Assert.True(dataChangeColumn.GetValue(0));
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
}
