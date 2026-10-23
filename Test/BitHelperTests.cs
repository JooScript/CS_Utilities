using Utils.Bit;

namespace Utils.Test;

public class BitHelperTests
{
    [Fact]
    public void Combine_ReturnsBitwiseOrOfAllValues()
    {
        var result = BitHelper.Combine(1, 2, 4);

        Assert.Equal(7, result);
    }

    [Fact]
    public void Combine_WhenEmpty_ReturnsZero()
    {
        Assert.Equal(0, BitHelper.Combine());
    }

    [Theory]
    [InlineData(1, 5, true)]
    [InlineData(3, 7, true)]
    [InlineData(4, 3, false)]
    public void Has_ReturnsWhetherAllFlagsExist(int required, int value, bool expected)
    {
        Assert.Equal(expected, BitHelper.Has(required, value));
    }
}
