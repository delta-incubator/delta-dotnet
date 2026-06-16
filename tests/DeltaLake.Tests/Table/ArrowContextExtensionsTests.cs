using Apache.Arrow;
using Apache.Arrow.Types;
using DeltaLake.Kernel.Arrow.Extensions;

namespace DeltaLake.Tests.Table;

public class ArrowContextExtensionsTests
{
    [Fact]
    public void SchemasMatch_SameReference_ReturnsTrue()
    {
        Schema schema = BuildSchema(("colA", StringType.Default, false));

        Assert.True(ArrowContextExtensions.SchemasMatch(schema, schema));
    }

    [Fact]
    public void SchemasMatch_BothNull_ReturnsTrue()
    {
        // ReferenceEquals(null, null) short-circuits to true — the helper treats
        // "both absent" as a degenerate match. Real call sites never pass null.
        Assert.True(ArrowContextExtensions.SchemasMatch(null!, null!));
    }

    [Fact]
    public void SchemasMatch_StructurallyIdenticalDistinctInstances_ReturnsTrue()
    {
        // The principal kernel use case: every imported batch produces a fresh Schema
        // instance via CArrowSchemaImporter, so reference equality always fails — the
        // structural per-field comparison is the only thing that can return true here.
        Schema first = BuildSchema(
            ("colString", StringType.Default, false),
            ("colInt32", Int32Type.Default, false),
            ("colInt64", Int64Type.Default, true));
        Schema second = BuildSchema(
            ("colString", StringType.Default, false),
            ("colInt32", Int32Type.Default, false),
            ("colInt64", Int64Type.Default, true));

        Assert.NotSame(first, second);
        Assert.True(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_FirstNull_ReturnsFalse()
    {
        Schema schema = BuildSchema(("colA", StringType.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(null!, schema));
    }

    [Fact]
    public void SchemasMatch_SecondNull_ReturnsFalse()
    {
        Schema schema = BuildSchema(("colA", StringType.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(schema, null!));
    }

    [Fact]
    public void SchemasMatch_DifferentFieldCount_ReturnsFalse()
    {
        Schema first = BuildSchema(("colA", StringType.Default, false));
        Schema second = BuildSchema(
            ("colA", StringType.Default, false),
            ("colB", Int32Type.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_DifferentFieldName_ReturnsFalse()
    {
        Schema first = BuildSchema(("colA", StringType.Default, false));
        Schema second = BuildSchema(("colB", StringType.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_DifferentDataType_ReturnsFalse()
    {
        Schema first = BuildSchema(("colA", StringType.Default, false));
        Schema second = BuildSchema(("colA", Int32Type.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_DifferentNullability_ReturnsFalse()
    {
        Schema first = BuildSchema(("colA", StringType.Default, false));
        Schema second = BuildSchema(("colA", StringType.Default, true));

        Assert.False(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_FieldOrderMatters_ReturnsFalse()
    {
        // Schemas are ordered tuples, not unordered sets — re-ordering fields
        // must produce a mismatch even when the field set is identical.
        Schema first = BuildSchema(
            ("colA", StringType.Default, false),
            ("colB", Int32Type.Default, false));
        Schema second = BuildSchema(
            ("colB", Int32Type.Default, false),
            ("colA", StringType.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_EmptySchemas_ReturnsTrue()
    {
        // Two schemas with zero fields are vacuously structurally identical.
        Schema first = new Schema.Builder().Build();
        Schema second = new Schema.Builder().Build();

        Assert.NotSame(first, second);
        Assert.True(ArrowContextExtensions.SchemasMatch(first, second));
    }

    [Fact]
    public void SchemasMatch_EmptyVsPopulated_ReturnsFalse()
    {
        Schema empty = new Schema.Builder().Build();
        Schema populated = BuildSchema(("colA", StringType.Default, false));

        Assert.False(ArrowContextExtensions.SchemasMatch(empty, populated));
    }

    private static Schema BuildSchema(params (string Name, IArrowType Type, bool Nullable)[] fields)
    {
        Schema.Builder builder = new();
        foreach ((string name, IArrowType type, bool nullable) in fields)
        {
            builder.Field(fb => fb.Name(name).DataType(type).Nullable(nullable));
        }
        return builder.Build();
    }
}
