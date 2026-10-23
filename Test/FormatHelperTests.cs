using Utils.Format;

namespace Utils.Test;

public class FormatHelperTests
{
    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("Version2Name", "version2_name")]
    [InlineData("already_snake", "already_snake")]
    [InlineData("ABC", "abc")]
    [InlineData("", "")]
    public void ToSnakeCase_ConvertsExpectedInput(string input, string expected)
    {
        Assert.Equal(expected, FormatHelper.ToSnakeCase(input));
    }

    [Fact]
    public void FormatException_IncludesAllExceptionLevels()
    {
        var exception = new InvalidOperationException("outer", new ArgumentException("inner"));

        var result = FormatHelper.FormatException(exception);

        Assert.Contains("Exception Level 0", result);
        Assert.Contains("Exception Level 1", result);
        Assert.Contains("outer", result);
        Assert.Contains("inner", result);
        Assert.Contains(nameof(InvalidOperationException), result);
        Assert.Contains(nameof(ArgumentException), result);
    }

    [Fact]
    public void DateToShort_UsesDayMonthYearFormat()
    {
        var result = FormatHelper.DateToShort(new DateTime(2024, 3, 5));

        Assert.Matches(@"^05/\w{3}/2024$", result);
    }

    [Fact]
    public void SanitizeName_ReplacesPathSeparators()
    {
        Assert.Equal("a_b_c", FormatHelper.SanitizeName("a/b\\c"));
    }

    [Fact]
    public void RemovePrefix_WhenPrefixPresent_RemovesIgnoringCase()
    {
        Assert.Equal("Name", FormatHelper.RemovePrefix("PrefixName", "prefix"));
    }

    [Fact]
    public void RemovePrefix_WhenPrefixMissing_ReturnsOriginal()
    {
        Assert.Equal("Name", FormatHelper.RemovePrefix("Name", "prefix"));
    }

    [Fact]
    public void ExceptionToString_ContainsMessageAndStackTraceLabel()
    {
        var exception = new Exception("failure");

        var result = FormatHelper.ExceptionToString(exception);

        Assert.Contains("Message: failure", result);
        Assert.Contains("StackTrace:", result);
    }

    [Theory]
    [InlineData("users", "user")]
    [InlineData("user", "user")]
    [InlineData("series", "series")]
    [InlineData("", "")]
    public void Singularize_ReturnsExpectedWord(string word, string expected)
    {
        Assert.Equal(expected, FormatHelper.Singularize(word));
    }

    [Theory]
    [InlineData("user", "users")]
    [InlineData("users", "users")]
    [InlineData("series", "series")]
    [InlineData("", "")]
    public void Pluralize_ReturnsExpectedWord(string word, string expected)
    {
        Assert.Equal(expected, FormatHelper.Pluralize(word));
    }

    [Fact]
    public void CapitalizeFirstChars_CapitalizesEachWord()
    {
        Assert.Equal("Hello World", FormatHelper.CapitalizeFirstChars("hello world"));
    }

    [Fact]
    public void CapitalizeFirstChars_WhenEmpty_ReturnsInput()
    {
        Assert.Equal("", FormatHelper.CapitalizeFirstChars(""));
    }

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("H", "h")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void SmalizeFirstChar_LowercasesFirstCharacter(string? input, string expected)
    {
        Assert.Equal(expected, FormatHelper.SmalizeFirstChar(input));
    }

    [Fact]
    public void FormatNumbers_ListPadsToMaxDigits()
    {
        var result = FormatHelper.FormatNumbers(new List<int> { 1, 12, 123 });

        Assert.Equal(new[] { "001", "012", "123" }, result);
    }

    [Fact]
    public void FormatNumbers_EmptyList_ReturnsEmptyList()
    {
        Assert.Empty(FormatHelper.FormatNumbers(new List<int>()));
        Assert.Empty(FormatHelper.FormatNumbers(null!));
    }

    [Fact]
    public void FormatNumbers_SingleNumber_PadsToMaxNumberWidth()
    {
        Assert.Equal("005", FormatHelper.FormatNumbers(5, 100));
        Assert.Equal("", FormatHelper.FormatNumbers(5, 0));
    }

    [Theory]
    [InlineData("1010", "1010")]
    [InlineData("101", "0101")]
    [InlineData("10101", "0001 0101")]
    public void FormatBinary_GroupsIntoNibbles(string input, string expected)
    {
        Assert.Equal(expected, FormatHelper.FormatBinary(input));
    }

    [Fact]
    public void FormatBinary_Number_UsesBinaryRepresentation()
    {
        Assert.Equal("1010", FormatHelper.FormatBinary(10));
    }

    [Fact]
    public void FormatBinary_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => FormatHelper.FormatBinary("102"));
    }

    [Theory]
    [InlineData("1a2b", "1A2B")]
    [InlineData("1A2B3", "1 A2B3")]
    public void FormatHexadecimal_GroupsAndUppercases(string input, string expected)
    {
        Assert.Equal(expected, FormatHelper.FormatHexadecimal(input));
    }

    [Fact]
    public void FormatHexadecimal_Number_UsesHexRepresentation()
    {
        Assert.Equal("FF", FormatHelper.FormatHexadecimal(255));
    }

    [Fact]
    public void FormatHexadecimal_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => FormatHelper.FormatHexadecimal("GG"));
    }

    [Theory]
    [InlineData("777", "777")]
    [InlineData("1234", "1 234")]
    public void FormatOctal_GroupsIntoThrees(string input, string expected)
    {
        Assert.Equal(expected, FormatHelper.FormatOctal(input));
    }

    [Fact]
    public void FormatOctal_Number_UsesOctalRepresentation()
    {
        Assert.Equal("10", FormatHelper.FormatOctal(8));
    }

    [Fact]
    public void FormatOctal_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => FormatHelper.FormatOctal("8"));
    }

    [Theory]
    [InlineData("1234567", "1,234,567")]
    [InlineData("1234", "1,234")]
    [InlineData("123", "123")]
    public void FormatDecimal_GroupsThousands(string input, string expected)
    {
        Assert.Equal(expected, FormatHelper.FormatDecimal(input));
    }

    [Fact]
    public void FormatDecimal_Number_UsesThousandsSeparator()
    {
        Assert.Equal("1,234", FormatHelper.FormatDecimal(1234));
    }

    [Fact]
    public void FormatDecimal_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => FormatHelper.FormatDecimal("12A"));
    }
}
