using System.Text;
using Humanizer;

namespace Utilities
{
    public class FormatHelper
    {
        public static string DateToShort(DateTime Dt1)
        {
            return Dt1.ToString("dd/MMM/yyyy");
        }

        public static string ExceptionToString(Exception ex)
        {
            StringBuilder ErrorMessage = new StringBuilder();
            ErrorMessage.AppendLine($"[{DateTime.Now}] Error:");
            ErrorMessage.AppendLine($"Message: {ex.Message}");
            ErrorMessage.AppendLine($"StackTrace: {ex.StackTrace}");
            ErrorMessage.AppendLine(new string('-', 50));
            return ErrorMessage.ToString();
        }

        public static string? Singularize(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return null;
            }

            var sameSingularPlural = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "series", "species", "deer", "sheep", "fish", "aircraft",
        "offspring", "moose", "swine", "trout", "salmon"
    };

            if (sameSingularPlural.Contains(word))
            {
                return word;
            }

            if (ValidationHelper.IsSingle(word))
            {
                return word;
            }

            return word.Singularize(false);
        }

        public static string? Pluralize(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return null;
            }

            var sameSingularPlural = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "series", "species", "deer", "sheep", "fish", "aircraft",
        "offspring", "moose", "swine", "trout", "salmon"
    };

            if (sameSingularPlural.Contains(word))
            {
                return word;
            }

            // If already plural, return as is
            if (ValidationHelper.IsPlural(word))
            {
                return word;
            }

            return word.Pluralize(false);
        }

        public static string CapitalizeFirstChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder result = new StringBuilder();
            bool capitalizeNext = true;

            foreach (char c in input)
            {
                if (char.IsWhiteSpace(c))
                {
                    capitalizeNext = true;
                    result.Append(c);
                }
                else if (capitalizeNext)
                {
                    result.Append(char.ToUpper(c));
                    capitalizeNext = false;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public static string SmalizeFirstChar(string? input)
        {
            return string.IsNullOrEmpty(input) ? string.Empty : input.Length == 1 ? input.ToLower() : char.ToLower(input[0]) + input.Substring(1);
        }

        public static List<string> FormatNumbers(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return new List<string>();
            }

            int maxNumber = numbers.Max();

            int digitCount = maxNumber.ToString().Length;

            List<string> formattedNumbers = numbers.Select(num => num.ToString().PadLeft(digitCount, '0')).ToList();

            return formattedNumbers;
        }

        public static string FormatNumbers(int numberToFormat, int maxNumber)
        {
            return maxNumber <= 0 ? string.Empty : numberToFormat.ToString().PadLeft(maxNumber.ToString().Length, '0');
        }



        #region NumberSystemFormat

        public static string FormatBinary(string binary, string separator = " ")
        {
            // Remove any existing whitespace and validate
            string clean = binary.Replace(" ", "");
            if (!clean.All(c => c == '0' || c == '1'))
                throw new ArgumentException("Invalid binary string");

            // Pad with leading zeros to make length a multiple of 4
            int padding = (4 - (clean.Length % 4)) % 4;
            clean = clean.PadLeft(clean.Length + padding, '0');

            // Process in 4-bit chunks from right to left
            StringBuilder result = new StringBuilder();
            for (int i = clean.Length; i > 0; i -= 4)
            {
                int length = Math.Min(4, i);
                string chunk = clean.Substring(Math.Max(0, i - length), length);
                if (result.Length > 0) result.Insert(0, separator);
                result.Insert(0, chunk);
            }

            return result.ToString();
        }

        public static string FormatHexadecimal(string hex, string separator = " ")
        {
            // Remove any existing whitespace and validate
            string clean = hex.Replace(" ", "").ToUpper();
            if (!clean.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
                throw new ArgumentException("Invalid hexadecimal string");

            // Process in 4-character chunks from right to left
            StringBuilder result = new StringBuilder();
            for (int i = clean.Length; i > 0; i -= 4)
            {
                int length = Math.Min(4, i);
                string chunk = clean.Substring(Math.Max(0, i - length), length);
                if (result.Length > 0) result.Insert(0, separator);
                result.Insert(0, chunk);
            }

            return result.ToString();
        }

        public static string FormatOctal(string octal, string separator = " ")
        {
            // Remove any existing whitespace and validate
            string clean = octal.Replace(" ", "");
            if (!clean.All(c => c >= '0' && c <= '7'))
                throw new ArgumentException("Invalid octal string");

            // Process in 3-character chunks from right to left
            StringBuilder result = new StringBuilder();
            for (int i = clean.Length; i > 0; i -= 3)
            {
                int length = Math.Min(3, i);
                string chunk = clean.Substring(Math.Max(0, i - length), length);
                if (result.Length > 0) result.Insert(0, separator);
                result.Insert(0, chunk);
            }

            return result.ToString();
        }

        public static string FormatDecimal(string dec, string separator = ",")
        {
            // Remove any existing formatting and validate
            string clean = dec.Replace(",", "").Replace(" ", "");
            if (!clean.All(char.IsDigit))
                throw new ArgumentException("Invalid decimal string");

            // Process in 3-digit chunks from right to left
            StringBuilder result = new StringBuilder();
            for (int i = clean.Length; i > 0; i -= 3)
            {
                int length = Math.Min(3, i);
                string chunk = clean.Substring(Math.Max(0, i - length), length);
                if (result.Length > 0) result.Insert(0, separator);
                result.Insert(0, chunk);
            }

            return result.ToString();
        }

        public static string FormatBinary(int number) => FormatBinary(Convert.ToString(number, 2));
        public static string FormatHexadecimal(int number) => FormatHexadecimal(number.ToString("X"));
        public static string FormatOctal(int number) => FormatOctal(Convert.ToString(number, 8));
        public static string FormatDecimal(int number) => FormatDecimal(number.ToString());

        #endregion
    }
}
