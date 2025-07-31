namespace Utilities.Utils.Random
{
    public static class RandomHelper
    {
        private static readonly Random _random = new Random();
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Generates a random DateTime in the past up to specified number of years
        /// </summary>
        /// <param name="years">Maximum years in the past (default: 10)</param>
        /// <returns>Random DateTime in the past</returns>
        public static DateTime GetPastDate(int years = 10)
        {
            if (years <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(years), "Years must be greater than 0");
            }

            lock (_syncLock)
            {
                return DateTime.Now.AddDays(-_random.Next(1, 365 * years + 1));
            }
        }

        /// <summary>
        /// Generates a random DateTime within a specified range
        /// </summary>
        /// <param name="startDate">Start date of range</param>
        /// <param name="endDate">End date of range</param>
        /// <returns>Random DateTime within range</returns>
        public static DateTime GetDateBetween(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            lock (_syncLock)
            {
                TimeSpan timeSpan = endDate - startDate;
                int totalDays = (int)timeSpan.TotalDays;
                int randomDays = _random.Next(totalDays + 1);
                return startDate.AddDays(randomDays);
            }
        }

        /// <summary>
        /// Picks a random item from a list
        /// </summary>
        /// <typeparam name="T">Type of items in list</typeparam>
        /// <param name="list">List to pick from</param>
        /// <returns>Random item from list</returns>
        public static T GetItem<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List cannot be null or empty");
            }

            lock (_syncLock)
            {
                return list[_random.Next(list.Count)];
            }
        }

        /// <summary>
        /// Picks a random enum value
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Random enum value</returns>
        public static T GetEnumValue<T>() where T : Enum
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            lock (_syncLock)
            {
                return values[_random.Next(values.Length)];
            }
        }

        /// <summary>
        /// Generates a random boolean value
        /// </summary>
        /// <param name="probability">Probability of true (0.0 to 1.0)</param>
        /// <returns>Random boolean</returns>
        public static bool GetBool(double probability = 0.5)
        {
            if (probability < 0 || probability > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0.0 and 1.0");
            }

            lock (_syncLock)
            {
                return _random.NextDouble() < probability;
            }
        }

        /// <summary>
        /// Generates a random integer within a range
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive)</param>
        /// <returns>Random integer</returns>
        public static int GetInt(int min = 1, int max = 100)
        {
            lock (_syncLock)
            {
                return _random.Next(min, max);
            }
        }

        /// <summary>
        /// Generates a random double within a range
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Random double</returns>
        public static double GetDouble(double min = 0.0, double max = 1.0)
        {
            if (min >= max)
            {
                throw new ArgumentException("Min must be less than max");
            }

            lock (_syncLock)
            {
                return min + _random.NextDouble() * (max - min);
            }
        }

        /// <summary>
        /// Generates a random string of specified length
        /// </summary>
        /// <param name="length">Length of string</param>
        /// <param name="chars">Character set to use (default: a-z, A-Z, 0-9)</param>
        /// <returns>Random string</returns>
        public static string GetString(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive");
            }

            if (string.IsNullOrEmpty(chars))
            {
                throw new ArgumentException("Character set cannot be null or empty");
            }

            lock (_syncLock)
            {
                return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
            }
        }

        /// <summary>
        /// Generates a random alphanumeric string
        /// </summary>
        /// <param name="length">Length of string</param>
        /// <returns>Random alphanumeric string</returns>
        public static string GetAlphanumeric(int length) => GetString(length);

        /// <summary>
        /// Generates a random color
        /// </summary>
        /// <returns>Random color</returns>
        public static System.Drawing.Color GetColor()
        {
            lock (_syncLock)
            {
                return System.Drawing.Color.FromArgb(
                    _random.Next(256),
                    _random.Next(256),
                    _random.Next(256));
            }
        }

        /// <summary>
        /// Shuffles a list in place using Fisher-Yates algorithm
        /// </summary>
        /// <typeparam name="T">Type of items in list</typeparam>
        /// <param name="list">List to shuffle</param>
        public static void Shuffle<T>(IList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            lock (_syncLock)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = _random.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }

        /// <summary>
        /// Returns a shuffled copy of the input list
        /// </summary>
        /// <typeparam name="T">Type of items in list</typeparam>
        /// <param name="list">List to shuffle</param>
        /// <returns>New shuffled list</returns>
        public static IList<T> Shuffled<T>(IList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var newList = new List<T>(list);
            Shuffle(newList);
            return newList;
        }

        /// <summary>
        /// Picks a random item from a list with weighted probabilities
        /// </summary>
        /// <typeparam name="T">Type of items in list</typeparam>
        /// <param name="items">List of items</param>
        /// <param name="weights">List of weights corresponding to items</param>
        /// <returns>Randomly selected item based on weights</returns>
        public static T GetWeighted<T>(IList<T> items, IList<double> weights)
        {
            if (items == null || weights == null)
            {
                throw new ArgumentNullException(items == null ? nameof(items) : nameof(weights));
            }
            if (items.Count != weights.Count)
            {
                throw new ArgumentException("Items and weights must have the same count");
            }
            if (items.Count == 0)
            {
                throw new ArgumentException("Items list cannot be empty");
            }
            if (weights.Any(w => w < 0))
            {
                throw new ArgumentException("Weights cannot be negative");
            }

            double totalWeight = weights.Sum();
            if (totalWeight <= 0)
            {
                throw new ArgumentException("Total weight must be positive");
            }

            lock (_syncLock)
            {
                double randomValue = _random.NextDouble() * totalWeight;
                double cumulative = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    cumulative += weights[i];
                    if (randomValue <= cumulative)
                    {
                        return items[i];
                    }
                }
            }

            return items.Last();
        }

    }
}
