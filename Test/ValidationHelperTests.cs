using System.Net;
using System.Text.Json;
using Utils.Validate;

namespace Utils.Test;

public class ValidationHelperTests
{
    private enum SampleEnum
    {
        First = 1,
        Second = 2
    }

    [Theory]
    [InlineData("First", true)]
    [InlineData("first", true)]
    [InlineData("2", true)]
    [InlineData("Third", false)]
    public void IsEnum_String_ValidatesNameOrDefinedNumber(string value, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsEnum<SampleEnum>(value));
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("[]", true)]
    [InlineData("null", true)]
    [InlineData("", false)]
    [InlineData("{", false)]
    public void IsValidJson_ReturnsExpected(string json, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidJson(json));
    }

    [Fact]
    public void IsValidType_ValidatesJsonElementAgainstType()
    {
        using var doc = JsonDocument.Parse("""{"text":"abc","number":42,"flag":true,"guid":"11111111-1111-1111-1111-111111111111","nothing":null}""");
        var root = doc.RootElement;

        Assert.True(ValidationHelper.IsValidType(root.GetProperty("text"), typeof(string)));
        Assert.True(ValidationHelper.IsValidType(root.GetProperty("number"), typeof(int)));
        Assert.False(ValidationHelper.IsValidType(root.GetProperty("number"), typeof(string)));
        Assert.True(ValidationHelper.IsValidType(root.GetProperty("flag"), typeof(bool)));
        Assert.True(ValidationHelper.IsValidType(root.GetProperty("guid"), typeof(Guid)));
        Assert.True(ValidationHelper.IsValidType(root.GetProperty("nothing"), typeof(int?)));
        Assert.False(ValidationHelper.IsValidType(root.GetProperty("nothing"), typeof(int)));
        Assert.False(ValidationHelper.IsValidType(root.GetProperty("flag"), typeof(object)));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.Created, true)]
    [InlineData(HttpStatusCode.NoContent, true)]
    [InlineData(HttpStatusCode.MovedPermanently, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public void IsSuccessStatusCode_ReturnsExpected(HttpStatusCode code, bool expected)
    {
        Assert.Equal(expected, code.IsSuccessStatusCode());
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com/path?q=1", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("not a url", false)]
    public void IsValidUrl_ReturnsExpected(string url, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidUrl(url));
    }

    [Theory]
    [InlineData("+20123456789", true)]
    [InlineData("(555) 123-4567", true)]
    [InlineData("123", false)]
    public void IsValidPhone_ReturnsExpected(string phone, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidPhone(phone));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(150, true)]
    [InlineData(0, false)]
    [InlineData(151, false)]
    public void IsValidAge_ReturnsExpected(int age, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidAge(age));
    }

    [Fact]
    public void IsValidBirthDate_ReturnsExpected()
    {
        Assert.True(ValidationHelper.IsValidBirthDate(DateTime.Today.AddYears(-30)));
        Assert.False(ValidationHelper.IsValidBirthDate(DateTime.Today.AddDays(1)));
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("a.b+tag@sub.example.co.uk", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void IsValidEmail_ReturnsExpected(string email, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidEmail(email));
    }

    [Fact]
    public void IsPatternValid_ReturnsExpected()
    {
        Assert.True(ValidationHelper.IsPatternValid(@"\d+"));
        Assert.False(ValidationHelper.IsPatternValid("["));
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("0", true)]
    [InlineData("-1", false)]
    [InlineData("1.2", false)]
    public void IsValidInteger_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidInteger(value));
    }

    [Theory]
    [InlineData("12.3", true)]
    [InlineData("12.", true)]
    [InlineData(".5", true)]
    [InlineData("-1.2", false)]
    public void IsValidFloat_ReturnsExpected(string value, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidFloat(value));
    }

    [Fact]
    public void IsNumber_ReturnsExpected()
    {
        Assert.True(ValidationHelper.IsNumber("123"));
        Assert.True(ValidationHelper.IsNumber("12.3"));
        Assert.False(ValidationHelper.IsNumber("-1"));
    }

    [Theory]
    [InlineData("Str0ng!Pass", true)]
    [InlineData("weak", false)]
    [InlineData("NoSpecial123", false)]
    [InlineData("", false)]
    public void IsValidStrongPassword_ReturnsExpected(string password, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidStrongPassword(password));
    }

    [Theory]
    [InlineData("+20123456789", true)]
    [InlineData("20123456789", true)]
    [InlineData("0123456789", false)]
    [InlineData("", false)]
    public void IsValidPhoneNumber_ReturnsExpected(string phone, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsValidPhoneNumber(phone));
    }

    [Theory]
    [InlineData('a', true)]
    [InlineData('E', true)]
    [InlineData('z', false)]
    public void IsVowel_ReturnsExpected(char c, bool expected)
    {
        Assert.Equal(expected, ValidationHelper.IsVowel(c));
    }

    [Fact]
    public void IsValidDateRange_ReturnsExpected()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        Assert.True(ValidationHelper.IsValidDateRange(new DateTime(2024, 6, 1), start, end));
        Assert.False(ValidationHelper.IsValidDateRange(new DateTime(2025, 1, 1), start, end));
        Assert.True(ValidationHelper.IsValidDateRange(start, end));
        Assert.False(ValidationHelper.IsValidDateRange(end, start));
    }

    [Theory]
    [InlineData("")]
    [InlineData("102")]
    public void ValidateBinary_InvalidInput_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => ValidationHelper.ValidateBinary(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("8")]
    public void ValidateOctal_InvalidInput_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => ValidationHelper.ValidateOctal(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("GG")]
    public void ValidateHexadecimal_InvalidInput_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => ValidationHelper.ValidateHexadecimal(input));
    }

    [Fact]
    public void ValidateNumberSystems_AcceptValidInput()
    {
        ValidationHelper.ValidateBinary("-1010");
        ValidationHelper.ValidateOctal("-777");
        ValidationHelper.ValidateHexadecimal("-ff");
    }
}
