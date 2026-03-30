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
    public void Build_EmptyPartitions_Returns_Valid_Batch()
    {
        var actions = new List<AddAction>
        {
            new AddAction
            {
                Path = "data/part-00000.parquet",
                Size = 512,
                ModificationTime = 1711929600000,
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
}
