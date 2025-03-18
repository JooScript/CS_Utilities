using System.Text.RegularExpressions;

namespace Utilities
{
    public class clsValidatoin
    {
        public static bool ValidateEmail(string emailAddress)
        {
            var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";
            var regex = new Regex(pattern);
            return regex.IsMatch(emailAddress);
        }

        public static bool ValidateInteger(string Number)
        {
            var pattern = @"^[0-9]*$";
            var regex = new Regex(pattern);
            return regex.IsMatch(Number);
        }

        public static bool ValidateFloat(string Number)
        {
            var pattern = @"^[0-9]*(?:\.[0-9]*)?$";
            var regex = new Regex(pattern);
            return regex.IsMatch(Number);
        }

        public static bool IsNumber(string Number)
        {
            return (ValidateInteger(Number) || ValidateFloat(Number));
        }

        public static bool ValidateStrongPassword(string password)
        {
            const int MinLength = 8;
            Regex hasUpperCase = new Regex(@"[A-Z]");
            Regex hasLowerCase = new Regex(@"[a-z]");
            Regex hasDigit = new Regex(@"[0-9]");
            Regex hasSpecialChar = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            return !string.IsNullOrWhiteSpace(password) && password.Length >= MinLength && hasUpperCase.IsMatch(password) && hasLowerCase.IsMatch(password) && hasDigit.IsMatch(password) && hasSpecialChar.IsMatch(password);
        }

        public static bool IsVowel(char c)
        {
            return "aeiouAEIOU".Contains(c);
        }

    }
}
