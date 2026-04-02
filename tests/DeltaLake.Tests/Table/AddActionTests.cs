using System.Collections.Generic;
using System.Text.Json;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class AddActionTests
{
    [Fact]
    public void ToJson_BasicAction_Returns_Valid_Json()
    {
        var action = new AddAction
        {
            Path = "part-00000.parquet",
            Size = 1024,
            ModificationTime = 1711929600000,
            DataChange = true,
        };

        var json = action.ToJson();

        Assert.Contains("\"path\":\"part-00000.parquet\"", json);
        Assert.Contains("\"size\":1024", json);
        Assert.Contains("\"modificationTime\":1711929600000", json);
        Assert.Contains("\"dataChange\":true", json);
    }

    [Fact]
    public void ToJson_WithPartitionValues_Includes_Partitions()
    {
        var action = new AddAction
        {
            Path = "year=2024/part-00000.parquet",
            Size = 2048,
            ModificationTime = 1711929600000,
            PartitionValues = new Dictionary<string, string?>
            {
                ["year"] = "2024",
                ["month"] = "03",
            },
        };

        var json = action.ToJson();

        Assert.Contains("\"partitionValues\":{\"year\":\"2024\",\"month\":\"03\"}", json);
    }

    [Fact]
    public void ToJson_NullPartitionValues_Omits_Field()
    {
        var action = new AddAction
        {
            Path = "part-00000.parquet",
            Size = 512,
            ModificationTime = 1711929600000,
            PartitionValues = null,
        };

        var json = action.ToJson();

        Assert.DoesNotContain("partitionValues", json);
    }

    [Fact]
    public void FromJson_ValidJson_Returns_Action()
    {
        var json = """{"path":"part-00000.parquet","size":1024,"modificationTime":1711929600000,"dataChange":true}""";

        var action = AddAction.FromJson(json);

        Assert.Equal("part-00000.parquet", action.Path);
        Assert.Equal(1024, action.Size);
        Assert.Equal(1711929600000, action.ModificationTime);
        Assert.True(action.DataChange);
    }

    [Fact]
    public void FromJson_WithPartitionValues_Deserializes_Correctly()
    {
        var json = """{"path":"year=2024/part-00000.parquet","size":2048,"partitionValues":{"year":"2024"},"modificationTime":1711929600000,"dataChange":true}""";

        var action = AddAction.FromJson(json);

        Assert.Equal("year=2024/part-00000.parquet", action.Path);
        Assert.NotNull(action.PartitionValues);
        Assert.Equal("2024", action.PartitionValues!["year"]);
    }

    [Fact]
    public void FromJson_InvalidJson_Throws()
    {
        Assert.Throws<JsonException>(() => AddAction.FromJson("not valid json"));
    }

    [Fact]
    public void ToJson_FromJson_Roundtrip()
    {
        var original = new AddAction
        {
            Path = "year=2024/month=03/part-00000.parquet",
            Size = 4096,
            ModificationTime = 1711929600000,
            DataChange = false,
            PartitionValues = new Dictionary<string, string?>
            {
                ["year"] = "2024",
                ["month"] = "03",
            },
        };

        var json = original.ToJson();
        var deserialized = AddAction.FromJson(json);

        Assert.Equal(original.Path, deserialized.Path);
        Assert.Equal(original.Size, deserialized.Size);
        Assert.Equal(original.ModificationTime, deserialized.ModificationTime);
        Assert.Equal(original.DataChange, deserialized.DataChange);
        Assert.Equal(original.PartitionValues!["year"], deserialized.PartitionValues!["year"]);
        Assert.Equal(original.PartitionValues!["month"], deserialized.PartitionValues!["month"]);
    }

    [Fact]
    public void FromJson_NullLiteral_Throws()
    {
        Assert.Throws<JsonException>(() => AddAction.FromJson("null"));
    }
}
