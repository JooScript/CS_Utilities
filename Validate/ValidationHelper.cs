using Humanizer;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utils.FileActions;
using Utils.General;
using static System.Net.Mime.MediaTypeNames;

namespace Utils.Validate;

public static class ValidationHelper
{
    public static async Task<bool> HasInternetConnectionAsync(CancellationToken cancellationToken = default)
    {
        string[] _testUrls =
            {
            "https://www.google.com/generate_204",
            "https://www.cloudflare.com/cdn-cgi/trace",
            "https://www.microsoft.com",
            "https://www.apple.com"
        };

        using HttpClient? client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        foreach (string url in _testUrls)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);

                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
                FileHelper.WarnLogger($"Failed to connect to {url}. Trying next URL...");
            }
        }

        return false;
    }

    public static bool IsEnum<TEnum>(string value)
        where TEnum : struct, Enum
        => (Enum.TryParse<TEnum>(value, true, out var stringResult)) ||
            (int.TryParse(value, out int intValue) && Enum.IsDefined(typeof(TEnum), intValue));

    public static bool IsEnum<TEnum>(JsonElement value)
        where TEnum : struct, Enum
        => (value.ValueKind == JsonValueKind.String && Enum.TryParse<TEnum>(value.GetString(), true, out var _)) ||
        (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue) && Enum.IsDefined(typeof(TEnum), intValue));

    public static bool IsEnum(JsonElement value, Type enumType)
    {
        if (!enumType.IsEnum)
            return false;

        return value.ValueKind switch
        {
            JsonValueKind.String =>
                Enum.TryParse(enumType, value.GetString(), true, out _),

            JsonValueKind.Number =>
                value.TryGetInt32(out var number) &&
                Enum.IsDefined(enumType, number),

            _ => false
        };
    }

    public static bool IsValidJson(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(jsonString);
            JsonElement element = doc.RootElement;

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static bool IsValidType(JsonElement value, Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);

        if (underlyingType is not null)
            return value.ValueKind == JsonValueKind.Null || IsValidType(value, underlyingType);

        return type switch
        {
            var t when t == typeof(string) =>
                value.ValueKind is JsonValueKind.String or JsonValueKind.Null,

            var t when t == typeof(char) =>
                value.ValueKind == JsonValueKind.String && value.GetString() is { Length: 1 },

            var t when t == typeof(byte) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetByte(out _),

            var t when t == typeof(short) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetInt16(out _),

            var t when t == typeof(int) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _),

            var t when t == typeof(long) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out _),

            var t when t == typeof(sbyte) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetSByte(out _),

            var t when t == typeof(ushort) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetUInt16(out _),

            var t when t == typeof(uint) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetUInt32(out _),

            var t when t == typeof(ulong) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetUInt64(out _),

            var t when t == typeof(float) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetSingle(out _),

            var t when t == typeof(double) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out _),

            var t when t == typeof(decimal) =>
                value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out _),

            var t when t == typeof(bool) =>
                value.ValueKind is JsonValueKind.True or JsonValueKind.False,

            var t when t == typeof(Guid) =>
                value.ValueKind == JsonValueKind.String && value.TryGetGuid(out _),

            var t when t == typeof(DateTime) =>
                value.ValueKind == JsonValueKind.String && value.TryGetDateTime(out _),

            var t when t == typeof(DateTimeOffset) =>
                value.ValueKind == JsonValueKind.String && value.TryGetDateTimeOffset(out _),

            var t when t == typeof(DateOnly) =>
                value.ValueKind == JsonValueKind.String && value.TryGetDateTime(out _),

            var t when t == typeof(TimeOnly) =>
                value.ValueKind == JsonValueKind.String && value.TryGetDateTime(out _),

            var t when t.IsEnum =>
                IsEnum(value, t),

            _ => false
        };
    }

    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code >= 200 && code < 300;
    }

    public static bool IsValidUrl(string text)
        => Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    public static bool IsValidPhone(string text)
        => Regex.IsMatch(text, @"^\+?[0-9\s\-\(\)]{7,20}$");

    /// <summary>
    /// Validates an age value with optional range constraints
    /// </summary>
    /// <param name="age">The age to validate</param>
    /// <param name="minAge">Minimum allowed age (default 1)</param>
    /// <param name="maxAge">Maximum allowed age (default 150)</param>
    /// <returns>
    /// A tuple where:
    /// - A boolean indicating if the age is valid
    /// </returns>
    public static bool IsValidAge(int age, int minAge = 1, int maxAge = 150)
    {
        return !(age < minAge || age > maxAge);
    }

    /// <summary>
    /// Validates a birth date and calculates age.
    /// </summary>
    /// <param name="birthDate">Date of birth to validate</param>
    /// <param name="minAge">Minimum allowed age (default 1)</param>
    /// <param name="maxAge">Maximum allowed age (default 150)</param>
    /// <returns>
    /// A boolean indicating if the age is within the allowed range.
    /// </returns>
    public static bool IsValidBirthDate(DateTime birthDate, int minAge = 1, int maxAge = 150)
    {
        DateTime today = DateTime.Today;
        int age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age >= minAge && age <= maxAge;
    }

    public static bool IsValidDestinationFolder(string folderPath, bool createIfMissing = false)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Console.WriteLine("Folder path is null or empty.");
            return false;
        }

        try
        {
            string fullPath = Path.GetFullPath(folderPath);

            if (!Directory.Exists(fullPath))
            {
                if (createIfMissing)
                {
                    Helper.CreateFolderIfDoesNotExist(fullPath);
                }
                else
                {
                    return false;
                }
            }

            if (!_HasWritePermission(fullPath))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            FileHelper.ErrorLogger(ex);
            return false;
        }
    }

    private static bool _HasWritePermission(string folderPath)
    {
        try
        {
            string testFile = Path.Combine(folderPath, Path.GetRandomFileName());
            using (FileStream fs = File.Create(testFile, 1, FileOptions.DeleteOnClose))
            {
                // If file creation succeeds, write permission exists
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates an email address using a regular expression.
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";
        return Regex.IsMatch(email, pattern) && MailAddress.TryCreate(email, out _);
    }

    public static bool IsPatternValid(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static bool IsValidInteger(string Number)
    {
        var pattern = @"^[0-9]*$";
        var regex = new Regex(pattern);
        return regex.IsMatch(Number);
    }

    public static bool IsValidFloat(string Number)
    {
        var pattern = @"^[0-9]*(?:\.[0-9]*)?$";
        var regex = new Regex(pattern);
        return regex.IsMatch(Number);
    }

    public static bool IsNumber(string Number)
    {
        return IsValidInteger(Number) || IsValidFloat(Number);
    }

    /// <summary>
    /// Validates password strength (min 8 characters, 1 uppercase, 1 lowercase, 1 digit, 1 special character).
    /// </summary>
    public static bool IsValidStrongPassword(string password)
    {
        const int MinLength = 8;
        Regex hasUpperCase = new Regex(@"[A-Z]");
        Regex hasLowerCase = new Regex(@"[a-z]");
        Regex hasDigit = new Regex(@"[0-9]");
        Regex hasSpecialChar = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        return !string.IsNullOrWhiteSpace(password) && password.Length >= MinLength && hasUpperCase.IsMatch(password) && hasLowerCase.IsMatch(password) && hasDigit.IsMatch(password) && hasSpecialChar.IsMatch(password);
    }

    /// <summary>
    /// Validates a phone number (basic international pattern).
    /// </summary>
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        var pattern = @"^\+?[1-9]\d{1,14}$"; // E.164 format
        return Regex.IsMatch(phoneNumber, pattern);
    }

    public static bool IsPlural(string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return word.Pluralize(false) == word;
    }

    public static bool IsSingle(string? word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        // Words that are the same in singular and plural
        var sameSingularPlural = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "series", "species", "deer", "sheep", "fish", "aircraft",
    "offspring", "moose", "swine", "trout", "salmon"
};

        if (sameSingularPlural.Contains(word))
        {
            return true;
        }

        // If the singularized form equals the original, it is singular
        var singularForm = word.Singularize(false);

        return string.Equals(singularForm, word, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsVowel(char c)
    {
        return "aeiou".IndexOf(char.ToLowerInvariant(c)) >= 0;
    }

    /// <summary>
    /// Validates that a date is within a specific range.
    /// </summary>
    public static bool IsValidDateRange(DateTime date, DateTime minDate, DateTime maxDate)
    {
        return !(date < minDate || date > maxDate);
    }

    public static bool IsValidDateRange(DateTime start, DateTime end)
    {
        return start <= end;
    }

    #region NumberSystemValidation

    public static void ValidateBinary(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty");

        string cleanInput = input.StartsWith("-") ? input.Substring(1) : input;

        if (cleanInput.Any(c => c != '0' && c != '1'))
            throw new ArgumentException("Input contains invalid binary characters");
    }

    public static void ValidateOctal(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty");

        string cleanInput = input.StartsWith("-") ? input.Substring(1) : input;

        if (cleanInput.Any(c => c < '0' || c > '7'))
            throw new ArgumentException("Input contains invalid octal characters");
    }

    public static void ValidateHexadecimal(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty");

        string cleanInput = input.StartsWith("-") ? input.Substring(1) : input;
        cleanInput = cleanInput.ToUpper();

        if (cleanInput.Any(c => !(char.IsDigit(c) || c >= 'A' && c <= 'F')))
            throw new ArgumentException("Input contains invalid hexadecimal characters");
    }

    #endregion

}