using Humanizer;
using System.Text.RegularExpressions;
using Utils.FileActions;
using Utils.General;

namespace Utils.Validate;

public static class ValidationHelper
{
    public static async Task<bool> HasInternetConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient? client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            using HttpRequestMessage? request = new HttpRequestMessage(
                HttpMethod.Head,
                "https://www.cloudflare.com/cdn-cgi/trace");

            using HttpResponseMessage? response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

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
        {
            return false;
        }

        var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";
        return Regex.IsMatch(email, pattern);
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