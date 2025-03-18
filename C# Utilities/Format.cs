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

        public static string Singularize(string word)
        {
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

            // Handle common singularization rules
            if (word.EndsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                // Remove "es" for words ending in s, x, z, ch, sh
                if (word.EndsWith("ses", StringComparison.OrdinalIgnoreCase) ||
                    word.EndsWith("xes", StringComparison.OrdinalIgnoreCase) ||
                    word.EndsWith("zes", StringComparison.OrdinalIgnoreCase) ||
                    word.EndsWith("ches", StringComparison.OrdinalIgnoreCase) ||
                    word.EndsWith("shes", StringComparison.OrdinalIgnoreCase))
                {
                    return word.Substring(0, word.Length - 2);
                }
                // Remove "es" for words ending in "ies" (e.g., "cities" -> "city")
                else if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                {
                    return word.Substring(0, word.Length - 3) + "y";
                }
            }
            else if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                // Remove "s" for most words
                return word.Substring(0, word.Length - 1);
            }
            else if (word.EndsWith("ves", StringComparison.OrdinalIgnoreCase))
            {
                // Handle words ending in "ves" (e.g., "leaves" -> "leaf")
                return word.Substring(0, word.Length - 3) + "f";
            }

            // If no rules match, return the word as-is
            return word;
        }

        public static string Pluralize(string word)
        {
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
                        !clsValidatoin.IsVowel(word[word.Length - 2]))
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

        public static string CapitalizeFirstChar(string input)
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

    }
}
