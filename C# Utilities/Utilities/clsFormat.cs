using System.Text;

namespace Utilities
{
    public class clsFormat
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
            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            if (IsSingle(word))
            {
                return word;
            }

            Dictionary<string, string> irregularSingulars = new Dictionary<string, string>
        {
            { "children", "child" },
            { "feet", "foot" },
            { "teeth", "tooth" },
            { "mice", "mouse" },
            { "people", "person" },
            { "geese", "goose" },
            { "men", "man" },
            { "women", "woman" },
            { "leaves", "leaf" },
            { "knives", "knife" },
            { "lives", "life" },
            { "elves", "elf" },
            { "loaves", "loaf" },
            { "potatoes", "potato" },
            { "tomatoes", "tomato" },
            { "cacti", "cactus" },
            { "foci", "focus" },
            { "fungi", "fungus" },
            { "analyses", "analysis" },
            { "crises", "crisis" },
            { "phenomena", "phenomenon" },
            { "criteria", "criterion" }
        };

            if (irregularSingulars.ContainsKey(word.ToLower()))
            {
                return irregularSingulars[word.ToLower()];
            }

            if (word.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                if (word.EndsWith("ses", StringComparison.OrdinalIgnoreCase) || word.EndsWith("xes", StringComparison.OrdinalIgnoreCase) || word.EndsWith("zes", StringComparison.OrdinalIgnoreCase) || word.EndsWith("ches", StringComparison.OrdinalIgnoreCase) || word.EndsWith("shes", StringComparison.OrdinalIgnoreCase))
                {
                    return word.Substring(0, word.Length - 2);
                }
                else if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                {
                    return word.Substring(0, word.Length - 3) + "y";
                }
            }
            else if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return word.Substring(0, word.Length - 1);
            }
            else if (word.EndsWith("ves", StringComparison.OrdinalIgnoreCase))
            {
                return word.Substring(0, word.Length - 3) + "f";
            }

            return word;
        }

        public static bool IsSingle(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            // List of words that are the same in singular and plural
            var sameSingularPlural = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "series", "species", "deer", "sheep", "fish", "aircraft",
        "offspring", "moose", "swine", "trout", "salmon"
    };

            // Check if word is same in both forms
            if (sameSingularPlural.Contains(word))
            {
                return true;
            }

            // Check irregular singulars (reverse of your irregular plurals)
            var irregularPlurals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "child", "foot", "tooth", "mouse", "person", "goose",
        "man", "woman", "leaf", "knife", "life", "elf", "loaf",
        "potato", "tomato", "cactus", "focus", "fungus",
        "analysis", "crisis", "phenomenon", "criterion"
    };

            if (irregularPlurals.Contains(word))
            {
                return true;
            }

            // Check regular singular patterns (opposite of plural patterns)
            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                word.Length > 1 &&
                !clsValidate.IsVowel(word[word.Length - 2]))
            {
                return false;
            }

            if (word.EndsWith("f", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // If none of the above, it's likely singular
            return true;
        }

        public static string? Pluralize(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            if (IsPlural(word))
            {
                return word;
            }

            var irregularPlurals = new Dictionary<string, string>
        {
            { "child", "children" },
            { "foot", "feet" },
            { "tooth", "teeth" },
            { "mouse", "mice" },
            { "person", "people" },
            { "goose", "geese" },
            { "man", "men" },
            { "woman", "women" },
            { "leaf", "leaves" },
            { "knife", "knives" },
            { "life", "lives" },
            { "elf", "elves" },
            { "loaf", "loaves" },
            { "potato", "potatoes" },
            { "tomato", "tomatoes" },
            { "cactus", "cacti" },
            { "focus", "foci" },
            { "fungus", "fungi" },
            { "analysis", "analyses" },
            { "crisis", "crises" },
            { "phenomenon", "phenomena" },
            { "criterion", "criteria" }
        };

            // Check if the word is in the irregular plurals dictionary
            if (irregularPlurals.ContainsKey(word.ToLower()))
            {
                return irregularPlurals[word.ToLower()];
            }

            // Handle common pluralization rules
            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                return word + "es";
            }
            else if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                        !clsValidate.IsVowel(word[word.Length - 2]))
            {
                return word.Substring(0, word.Length - 1) + "ies";
            }
            else if (word.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                return word.Substring(0, word.Length - 1) + "ves";
            }
            else if (word.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
            {
                return word.Substring(0, word.Length - 2) + "ves";
            }
            else
            {
                return word + "s";
            }
        }

        public static bool IsPlural(string? word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return false;
            }

            var irregularPlurals = new Dictionary<string, string>
    {
        { "children", "child" },
        { "feet", "foot" },
        { "teeth", "tooth" },
        { "mice", "mouse" },
        { "people", "person" },
        { "geese", "goose" },
        { "men", "man" },
        { "women", "woman" },
        { "leaves", "leaf" },
        { "knives", "knife" },
        { "lives", "life" },
        { "elves", "elf" },
        { "loaves", "loaf" },
        { "potatoes", "potato" },
        { "tomatoes", "tomato" },
        { "cacti", "cactus" },
        { "foci", "focus" },
        { "fungi", "fungus" },
        { "analyses", "analysis" },
        { "crises", "crisis" },
        { "phenomena", "phenomenon" },
        { "criteria", "criterion" }
    };

            // Check if word is an irregular plural
            if (irregularPlurals.ContainsKey(word.ToLower()))
            {
                return true;
            }

            // Check regular plural endings
            if (word.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                // Check for words ending with s, x, z, ch, sh (which add "es")
                var stem = word.Substring(0, word.Length - 2);
                if (stem.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                    stem.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                    stem.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                    stem.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                    stem.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
            {
                // Check for words ending with consonant + y (which become "ies")
                var stem = word.Substring(0, word.Length - 3);
                if (stem.Length > 0 && !clsValidate.IsVowel(stem[stem.Length - 1]))
                {
                    return true;
                }
            }
            else if (word.EndsWith("ves", StringComparison.OrdinalIgnoreCase))
            {
                // Check for words ending with f/fe (which become "ves")
                var stem = word.Substring(0, word.Length - 3);
                if (stem.EndsWith("f", StringComparison.OrdinalIgnoreCase) ||
                    stem.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
                     !word.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            {
                // Regular plural ending with "s" (but not words that already end with "s")
                return true;
            }

            return false;
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

        public static string FormatId(string? input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (input.EndsWith("id", StringComparison.OrdinalIgnoreCase) && input.Length >= 2)
            {
                char[] chars = input.ToCharArray();

                chars[^2] = 'I'; // Using index from end operator (C# 8.0+)
                chars[^1] = 'd';

                return new string(chars);
            }

            return input;
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
