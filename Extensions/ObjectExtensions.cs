namespace Utils.Extensions;

public static class ObjectExtensions
{
    public static bool TryParseString(this object? value, out string result)
    {
        result = string.Empty;

        if (value is null)
            return false;

        var str = value.ToString();

        if (string.IsNullOrWhiteSpace(str))
            return false;

        result = str;
        return true;
    }

}
