using System.Text.RegularExpressions;

namespace Utilities
{
    public class clsValidate
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

            if (cleanInput.Any(c => !(char.IsDigit(c) || (c >= 'A' && c <= 'F'))))
                throw new ArgumentException("Input contains invalid hexadecimal characters");
        }


    }
}
