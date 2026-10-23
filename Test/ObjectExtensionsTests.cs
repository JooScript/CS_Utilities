using Utils.Extensions;

namespace Utils.Test;

public class ObjectExtensionsTests
{
    [Fact]
    public void TryParseString_WhenValuePresent_ReturnsString()
    {
        var result = 123.TryParseString(out var text);

        Assert.True(result);
        Assert.Equal("123", text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseString_WhenMissing_ReturnsFalse(string? value)
    {
        var result = value.TryParseString(out var text);

        Assert.False(result);
        Assert.Equal(string.Empty, text);
    }
}
