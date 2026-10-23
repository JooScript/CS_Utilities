using Utils.Format;

namespace Utils.Test;

public class NumberSystemConverterHelperTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1010", 10)]
    [InlineData("-101", -5)]
    public void BinaryToDecimal_ReturnsDecimal(string binary, int expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.BinaryToDecimal(binary));
    }

    [Theory]
    [InlineData("1010", "A")]
    [InlineData("11111111", "FF")]
    [InlineData("101", "5")]
    public void BinaryToHexadecimal_ReturnsHex(string binary, string expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.BinaryToHexadecimal(binary));
    }

    [Theory]
    [InlineData("111", "7")]
    [InlineData("1010", "12")]
    [InlineData("1000", "10")]
    public void BinaryToOctal_ReturnsOctal(string binary, string expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.BinaryToOctal(binary));
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(10, "1010")]
    [InlineData(-5, "-101")]
    public void DecimalToBinary_ReturnsBinary(int number, string expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.DecimalToBinary(number));
    }

    [Theory]
    [InlineData(10, "A")]
    [InlineData(255, "FF")]
    public void DecimalToHexadecimal_ReturnsHex(int number, string expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.DecimalToHexadecimal(number));
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(8, "10")]
    [InlineData(-8, "-10")]
    public void DecimalToOctal_ReturnsOctal(int number, string expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.DecimalToOctal(number));
    }

    [Theory]
    [InlineData("FF", 255)]
    [InlineData("ff", 255)]
    [InlineData("A", 10)]
    public void HexadecimalToDecimal_ReturnsDecimal(string hex, int expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.HexadecimalToDecimal(hex));
    }

    [Fact]
    public void HexadecimalToBinary_ReturnsBinary()
    {
        Assert.Equal("1010", NumberSystemConverterHelper.HexadecimalToBinary("A"));
    }

    [Fact]
    public void HexadecimalToOctal_ReturnsOctal()
    {
        Assert.Equal("12", NumberSystemConverterHelper.HexadecimalToOctal("A"));
    }

    [Theory]
    [InlineData("12", 10)]
    [InlineData("7", 7)]
    public void OctalToDecimal_ReturnsDecimal(string octal, int expected)
    {
        Assert.Equal(expected, NumberSystemConverterHelper.OctalToDecimal(octal));
    }

    [Fact]
    public void OctalToBinary_ReturnsBinary()
    {
        Assert.Equal("1010", NumberSystemConverterHelper.OctalToBinary("12"));
    }

    [Fact]
    public void OctalToHexadecimal_ReturnsHex()
    {
        Assert.Equal("A", NumberSystemConverterHelper.OctalToHexadecimal("12"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("102")]
    [InlineData("abc")]
    public void BinaryToDecimal_InvalidInput_Throws(string binary)
    {
        Assert.Throws<ArgumentException>(() => NumberSystemConverterHelper.BinaryToDecimal(binary));
    }

    [Theory]
    [InlineData("")]
    [InlineData("8")]
    public void OctalToDecimal_InvalidInput_Throws(string octal)
    {
        Assert.Throws<ArgumentException>(() => NumberSystemConverterHelper.OctalToDecimal(octal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("GG")]
    public void HexadecimalToDecimal_InvalidInput_Throws(string hex)
    {
        Assert.Throws<ArgumentException>(() => NumberSystemConverterHelper.HexadecimalToDecimal(hex));
    }
}
