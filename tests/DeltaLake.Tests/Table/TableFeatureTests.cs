using System.Text.Json;
using DeltaLake.Errors;
using DeltaLake.Table;

namespace DeltaLake.Tests.Table;

public class TableFeatureTests
{
    public static IEnumerable<object[]> LegacyWriterFeatures()
    {
        yield return [2, new[] { "appendOnly", "invariants", "v2Checkpoint" }];
        yield return [3, new[] { "appendOnly", "checkConstraints", "invariants", "v2Checkpoint" }];
        yield return [4, new[] { "appendOnly", "changeDataFeed", "checkConstraints", "generatedColumns", "invariants", "v2Checkpoint" }];
    }

    public static IEnumerable<object[]> TableFeatureMappings()
    {
        yield return [TableFeature.ColumnMapping, "columnMapping"];
        yield return [TableFeature.DeletionVectors, "deletionVectors"];
        yield return [TableFeature.TimestampWithoutTimezone, "timestampNtz"];
        yield return [TableFeature.V2Checkpoint, "v2Checkpoint"];
        yield return [TableFeature.AppendOnly, "appendOnly"];
        yield return [TableFeature.Invariants, "invariants"];
        yield return [TableFeature.CheckConstraints, "checkConstraints"];
        yield return [TableFeature.ChangeDataFeed, "changeDataFeed"];
        yield return [TableFeature.GeneratedColumns, "generatedColumns"];
        yield return [TableFeature.IdentityColumns, "identityColumns"];
        yield return [TableFeature.RowTracking, "rowTracking"];
        yield return [TableFeature.DomainMetadata, "domainMetadata"];
        yield return [TableFeature.IcebergCompatV1, "icebergCompatV1"];
        yield return [TableFeature.MaterializePartitionColumns, "materializePartitionColumns"];
    }

    [Fact]
    public async Task AddTableFeaturesAsync_DefaultProtocolIncrease_FailsWithoutCommit()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var data = await TableHelpers.SetupTable(DirectoryHelpers.ToFileUri(info.FullName), 1);
            using var engine = data.engine;
            using var table = data.table;
            var version = table.Version();

            var exception = await Assert.ThrowsAsync<DeltaRuntimeException>(
                () => table.AddTableFeaturesAsync(
                    [TableFeature.V2Checkpoint],
                    CancellationToken.None));

            Assert.Contains("allow_protocol_versions_increase", exception.Message);
            Assert.Equal(version, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task AddTableFeaturesAsync_ProtocolIncreaseAndMetadata_CommitsV2CheckpointFeature()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var data = await TableHelpers.SetupTable(DirectoryHelpers.ToFileUri(info.FullName), 1);
            using var engine = data.engine;
            using var table = data.table;

            await table.AddTableFeaturesAsync(
                [TableFeature.V2Checkpoint],
                new AddTableFeatureOptions
                {
                    AllowProtocolVersionsIncrease = true,
                    CustomMetadata = new Dictionary<string, string>
                    {
                        ["workItem"] = "add-table-features",
                    },
                },
                CancellationToken.None);

            Assert.Equal(2UL, table.Version());
            var protocol = table.ProtocolVersions();
            Assert.Equal(3, protocol.MinimumReaderVersion);
            Assert.Equal(7, protocol.MinimumWriterVersion);

            var commitPath = Path.Join(info.FullName, "_delta_log", "00000000000000000002.json");
            var protocolAction = ReadAction(commitPath, "protocol");
            Assert.Equal(
                ["v2Checkpoint"],
                ReadFeatureSet(protocolAction, "readerFeatures"));
            Assert.Equal(
                ["appendOnly", "invariants", "v2Checkpoint"],
                ReadFeatureSet(protocolAction, "writerFeatures"));

            var commitInfo = ReadAction(commitPath, "commitInfo");
            Assert.Equal("add-table-features", commitInfo.GetProperty("workItem").GetString());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Theory]
    [MemberData(nameof(LegacyWriterFeatures))]
    public async Task AddTableFeaturesAsync_LegacyProtocol_PreservesImpliedWriterFeatures(
        int minWriterVersion,
        string[] expectedWriterFeatures)
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            using var schemaBatch = TableHelpers.BuildBasicRecordBatch(0);
            using var engine = new DeltaEngine(EngineOptions.Default);
            using var table = await engine.CreateTableAsync(
                new TableCreateOptions(DirectoryHelpers.ToFileUri(info.FullName), schemaBatch.Schema)
                {
                    Configuration = new Dictionary<string, string>
                    {
                        ["delta.minReaderVersion"] = "1",
                        ["delta.minWriterVersion"] = minWriterVersion.ToString(),
                    },
                },
                CancellationToken.None);

            await table.AddTableFeaturesAsync(
                [TableFeature.V2Checkpoint],
                new AddTableFeatureOptions { AllowProtocolVersionsIncrease = true },
                CancellationToken.None);

            var protocolAction = ReadAction(
                Path.Join(info.FullName, "_delta_log", "00000000000000000001.json"),
                "protocol");
            Assert.Equal(["v2Checkpoint"], ReadFeatureSet(protocolAction, "readerFeatures"));
            Assert.Equal(expectedWriterFeatures, ReadFeatureSet(protocolAction, "writerFeatures"));
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task AddTableFeaturesAsync_PreCancelled_DoesNotCommit()
    {
        var info = DirectoryHelpers.CreateTempSubdirectory();
        try
        {
            var data = await TableHelpers.SetupTable(DirectoryHelpers.ToFileUri(info.FullName), 1);
            using var engine = data.engine;
            using var table = data.table;
            var version = table.Version();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => table.AddTableFeaturesAsync(
                    [TableFeature.V2Checkpoint],
                    new AddTableFeatureOptions { AllowProtocolVersionsIncrease = true },
                    new CancellationToken(true)));

            Assert.Equal(version, table.Version());
        }
        finally
        {
            info.Delete(true);
        }
    }

    [Fact]
    public async Task AddTableFeaturesAsync_NullFeatures_ThrowsArgumentNullException()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var engine = data.engine;
        using var table = data.table;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => table.AddTableFeaturesAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task AddTableFeaturesAsync_NullOptions_ThrowsArgumentNullException()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var engine = data.engine;
        using var table = data.table;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => table.AddTableFeaturesAsync(
                [TableFeature.V2Checkpoint],
                null!,
                CancellationToken.None));
    }

    [Fact]
    public async Task AddTableFeaturesAsync_EmptyFeatures_ThrowsConfigurationException()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var engine = data.engine;
        using var table = data.table;
        var version = table.Version();

        var exception = await Assert.ThrowsAsync<DeltaConfigurationException>(
            () => table.AddTableFeaturesAsync([], CancellationToken.None));

        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Equal(version, table.Version());
    }

    [Fact]
    public async Task AddTableFeaturesAsync_InvalidEnum_ThrowsArgumentOutOfRangeException()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var engine = data.engine;
        using var table = data.table;
        var version = table.Version();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => table.AddTableFeaturesAsync(
                [(TableFeature)int.MaxValue],
                new AddTableFeatureOptions { AllowProtocolVersionsIncrease = true },
                CancellationToken.None));

        Assert.Equal(version, table.Version());
    }

    [Fact]
    public async Task AddTableFeaturesAsync_UnsupportedFeatures_ThrowWithoutCommit()
    {
        var data = await TableHelpers.SetupTable($"memory:///{Guid.NewGuid():N}", 1);
        using var engine = data.engine;
        using var table = data.table;
        var version = table.Version();
        var unsupportedFeatures = Enum.GetValues<TableFeature>()
            .Where(feature => feature != TableFeature.V2Checkpoint);

        foreach (var feature in unsupportedFeatures)
        {
            var exception = await Assert.ThrowsAsync<NotSupportedException>(
                () => table.AddTableFeaturesAsync(
                    [feature],
                    new AddTableFeatureOptions { AllowProtocolVersionsIncrease = true },
                    CancellationToken.None));

            Assert.Contains(feature.ToString(), exception.Message);
            Assert.Equal(version, table.Version());
        }
    }

    [Theory]
    [MemberData(nameof(TableFeatureMappings))]
    public void ConvertTableFeature_AllPublicValues_ReturnCanonicalName(
        TableFeature feature,
        string expected)
    {
        Assert.Equal(expected, DeltaLake.Bridge.Table.ConvertTableFeature(feature));
    }

    private static JsonElement ReadAction(string commitPath, string actionName)
    {
        foreach (var line in File.ReadLines(commitPath))
        {
            using var document = JsonDocument.Parse(line);
            if (document.RootElement.TryGetProperty(actionName, out var action))
            {
                return action.Clone();
            }
        }

        throw new InvalidOperationException($"Commit did not contain a {actionName} action.");
    }

    private static string[] ReadFeatureSet(JsonElement protocolAction, string propertyName) =>
        protocolAction
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
}