using System.Text.Json;
using Utils.ValueCoercion;

namespace Utils.Test;

public class ValueCoercionHelperTests
{
    [Fact]
    public void TryParse_ValidValue_ReturnsConvertedValue()
    {
        var success = ValueCoercionHelper.TryParse<int>("42", out var result);

        Assert.True(success);
        Assert.Equal(42, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-number")]
    public void TryParse_InvalidValue_ReturnsFalse(string? value)
    {
        var success = ValueCoercionHelper.TryParse<int>(value, out var result);

        Assert.False(success);
        Assert.Equal(0, result);
    }

    [Fact]
    public void IsEmpty_ReturnsExpected()
    {
        using var nullDoc = JsonDocument.Parse("null");
        using var arrayDoc = JsonDocument.Parse("[]");
        using var stringDoc = JsonDocument.Parse("\"  \"");

        Assert.True(ValueCoercionHelper.IsEmpty(null));
        Assert.True(ValueCoercionHelper.IsEmpty(""));
        Assert.True(ValueCoercionHelper.IsEmpty("  "));
        Assert.True(ValueCoercionHelper.IsEmpty(Array.Empty<int>()));
        Assert.True(ValueCoercionHelper.IsEmpty(nullDoc.RootElement));
        Assert.True(ValueCoercionHelper.IsEmpty(arrayDoc.RootElement));
        Assert.True(ValueCoercionHelper.IsEmpty(stringDoc.RootElement));
        Assert.False(ValueCoercionHelper.IsEmpty("value"));
        Assert.False(ValueCoercionHelper.IsEmpty(new[] { 1 }));
        Assert.False(ValueCoercionHelper.IsEmpty(0));
    }

    [Fact]
    public void AsString_ReturnsExpected()
    {
        using var doc = JsonDocument.Parse("""{"text":"abc","number":12,"nothing":null}""");

        Assert.Null(ValueCoercionHelper.AsString(null));
        Assert.Equal("abc", ValueCoercionHelper.AsString("abc"));
        Assert.Equal("12", ValueCoercionHelper.AsString(12));
        Assert.Equal("abc", ValueCoercionHelper.AsString(doc.RootElement.GetProperty("text")));
        Assert.Equal("12", ValueCoercionHelper.AsString(doc.RootElement.GetProperty("number")));
        Assert.Null(ValueCoercionHelper.AsString(doc.RootElement.GetProperty("nothing")));
    }

    [Fact]
    public void AsBoolean_ReturnsExpected()
    {
        Assert.True(ValueCoercionHelper.AsBoolean(true, out var boolResult));
        Assert.True(boolResult);
        Assert.True(ValueCoercionHelper.AsBoolean("true", out boolResult));
        Assert.True(boolResult);
        Assert.True(ValueCoercionHelper.AsBoolean("1", out boolResult));
        Assert.True(boolResult);
        Assert.True(ValueCoercionHelper.AsBoolean("0", out boolResult));
        Assert.False(boolResult);
        Assert.False(ValueCoercionHelper.AsBoolean("invalid", out _));
        Assert.False(ValueCoercionHelper.AsBoolean(null, out _));
    }

    [Fact]
    public void AsInteger_ReturnsExpected()
    {
        using var doc = JsonDocument.Parse("42");

        Assert.True(ValueCoercionHelper.AsInteger(42, out var result));
        Assert.Equal(42, result);
        Assert.True(ValueCoercionHelper.AsInteger(42L, out result));
        Assert.Equal(42, result);
        Assert.True(ValueCoercionHelper.AsInteger("42", out result));
        Assert.Equal(42, result);
        Assert.True(ValueCoercionHelper.AsInteger(doc.RootElement, out result));
        Assert.Equal(42, result);
        Assert.False(ValueCoercionHelper.AsInteger(long.MaxValue, out _));
        Assert.False(ValueCoercionHelper.AsInteger(null, out _));
        Assert.False(ValueCoercionHelper.AsInteger("invalid", out _));
    }

    [Fact]
    public void AsDecimal_ReturnsExpected()
    {
        using var doc = JsonDocument.Parse("12.5");

        Assert.True(ValueCoercionHelper.AsDecimal(12.5m, out var result));
        Assert.Equal(12.5m, result);
        Assert.True(ValueCoercionHelper.AsDecimal(12.5d, out result));
        Assert.Equal(12.5m, result);
        Assert.True(ValueCoercionHelper.AsDecimal(12, out result));
        Assert.Equal(12m, result);
        Assert.True(ValueCoercionHelper.AsDecimal(doc.RootElement, out result));
        Assert.Equal(12.5m, result);
        Assert.True(ValueCoercionHelper.AsDecimal("12.5", out result));
        Assert.Equal(12.5m, result);
        Assert.False(ValueCoercionHelper.AsDecimal(null, out _));
    }

    [Fact]
    public void AsDateOnly_ReturnsExpected()
    {
        var date = new DateOnly(2024, 5, 10);

        Assert.True(ValueCoercionHelper.AsDateOnly(date, out var result));
        Assert.Equal(date, result);
        Assert.True(ValueCoercionHelper.AsDateOnly(new DateTime(2024, 5, 10), out result));
        Assert.Equal(date, result);
        Assert.True(ValueCoercionHelper.AsDateOnly("2024-05-10", out result));
        Assert.Equal(date, result);
        Assert.False(ValueCoercionHelper.AsDateOnly("invalid", out _));
        Assert.False(ValueCoercionHelper.AsDateOnly(null, out _));
    }

    [Fact]
    public void AsDateTime_ReturnsExpected()
    {
        var date = new DateTime(2024, 5, 10, 12, 30, 0, DateTimeKind.Utc);

        Assert.True(ValueCoercionHelper.AsDateTime(date, out var result));
        Assert.Equal(date, result);
        Assert.True(ValueCoercionHelper.AsDateTime("2024-05-10T12:30:00Z", out result));
        Assert.Equal(date, result.ToUniversalTime());
        Assert.False(ValueCoercionHelper.AsDateTime(null, out _));
    }

    [Fact]
    public void AsTime_ReturnsExpected()
    {
        var time = TimeSpan.FromMinutes(90);

        Assert.True(ValueCoercionHelper.AsTime(time, out var result));
        Assert.Equal(time, result);
        Assert.True(ValueCoercionHelper.AsTime("01:30:00", out result));
        Assert.Equal(time, result);
        Assert.False(ValueCoercionHelper.AsTime(null, out _));
    }

    [Fact]
    public void AsGuid_ReturnsExpected()
    {
        var guid = Guid.NewGuid();
        using var doc = JsonDocument.Parse($"\"{guid}\"");

        Assert.True(ValueCoercionHelper.AsGuid(guid, out var result));
        Assert.Equal(guid, result);
        Assert.True(ValueCoercionHelper.AsGuid(guid.ToString(), out result));
        Assert.Equal(guid, result);
        Assert.True(ValueCoercionHelper.AsGuid(doc.RootElement, out result));
        Assert.Equal(guid, result);
        Assert.False(ValueCoercionHelper.AsGuid("invalid", out _));
        Assert.False(ValueCoercionHelper.AsGuid(null, out _));
    }

    [Fact]
    public void AsStringList_FromArray_ReturnsAllItems()
    {
        using var doc = JsonDocument.Parse("""["a","b"]""");

        var result = ValueCoercionHelper.AsStringList(doc.RootElement, out var isList);

        Assert.True(isList);
        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public void AsStringList_FromJsonStringArray_ReturnsAllItems()
    {
        var result = ValueCoercionHelper.AsStringList("[\"a\",\"b\"]", out var isList);

        Assert.True(isList);
        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public void AsStringList_FromPlainString_ReturnsSingleItem()
    {
        var result = ValueCoercionHelper.AsStringList("abc", out var isList);

        Assert.False(isList);
        Assert.Equal(new[] { "abc" }, result);
    }

    [Fact]
    public void AsStringList_FromEnumerable_ReturnsAllItems()
    {
        var result = ValueCoercionHelper.AsStringList(new[] { 1, 2 }, out var isList);

        Assert.True(isList);
        Assert.Equal(new[] { "1", "2" }, result);
    }

    [Fact]
    public void AsStringList_FromNull_ReturnsEmpty()
    {
        var result = ValueCoercionHelper.AsStringList(null, out var isList);

        Assert.False(isList);
        Assert.Empty(result);
    }

    [Fact]
    public void AsDictionary_FromJsonObject_ReturnsValues()
    {
        using var doc = JsonDocument.Parse("""{"name":"abc","age":10}""");

        var success = ValueCoercionHelper.AsDictionary(doc.RootElement, out var result);

        Assert.True(success);
        Assert.Equal("abc", result["name"]!.ToString());
        Assert.Equal("10", result["age"]!.ToString());
    }

    [Fact]
    public void AsDictionary_FromDictionary_ReturnsValues()
    {
        var dictionary = new Dictionary<string, object?> { ["name"] = "abc" };

        var success = ValueCoercionHelper.AsDictionary(dictionary, out var result);

        Assert.True(success);
        Assert.Equal("abc", result["name"]);
    }

    [Fact]
    public void AsDictionary_WhenNotObject_ReturnsFalse()
    {
        Assert.False(ValueCoercionHelper.AsDictionary("abc", out _));
        Assert.False(ValueCoercionHelper.AsDictionary(null, out _));
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("[1,2]", true)]
    [InlineData("", false)]
    [InlineData("{", false)]
    public void IsValidJsonString_ReturnsExpected(string json, bool expected)
    {
        Assert.Equal(expected, ValueCoercionHelper.IsValidJsonString(json));
    }
}
