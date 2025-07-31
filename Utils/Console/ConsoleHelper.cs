using System.Data;
using Utilities.Utils.Format;

namespace Utilities.Utils.Console
{
    public class ConsoleHelper
    {

        /// <summary>
        /// Prints a colored status message with icon based on success/failure
        /// </summary>
        /// <param name="success">True for success, false for failure</param>
        /// <param name="customSuccessMessage">Optional custom success message</param>
        /// <param name="customFailureMessage">Optional custom failure message</param>
        public static void PrintStatus(bool success, string customSuccessMessage = null, string customFailureMessage = null)
        {
            ConsoleColor color = success ? ConsoleColor.Green : ConsoleColor.Red;
            string icon = success ? "✓" : "✗";
            string defaultMessage = success ? "Done" : "Failed";
            string message = $"  {icon} {(success ? customSuccessMessage ?? defaultMessage : customFailureMessage ?? defaultMessage)}";

            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message.PadRight(Console.WindowWidth - 1));

            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Extended version with elapsed time display
        /// </summary>
        public static void PrintStatus(bool success, TimeSpan elapsedTime, string customSuccessMessage = null, string customFailureMessage = null)
        {
            ConsoleColor color = success ? ConsoleColor.Green : ConsoleColor.Red;
            string icon = success ? "✓" : "✗";
            string defaultMessage = success ? "Completed" : "Failed";
            string message = $"  {icon} {(success ? customSuccessMessage ?? defaultMessage : customFailureMessage ?? defaultMessage)}";
            string timeInfo = $" [{elapsedTime.TotalMilliseconds}ms]";

            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(timeInfo.PadRight(Console.WindowWidth - message.Length - 1));

            Console.ForegroundColor = originalColor;
        }

        public static void DataTableConsolePrinting(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                Console.WriteLine("No data found.");
                return;
            }

            foreach (DataColumn column in dataTable.Columns)
            {
                Console.Write($"{column.ColumnName,-30}");
            }
            Console.WriteLine();

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Console.Write($"{item,-30}");
                }
                Console.WriteLine();
            }
        }

        public static void ListConsolePrinting<T>(List<T> list, bool listStyle = true)
        {
            if (list == null || list.Count == 0)
            {
                Console.WriteLine("The list is empty or null.");
                return;
            }

            if (listStyle)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Console.WriteLine($"[{FormatHelper.FormatNumbers(i + 1, list.Count)}] {list[i]}");
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Console.Write($"[{FormatHelper.FormatNumbers(i + 1, list.Count)}] {list[i]}  ");
                }
            }


        }

        public static void PrintColoredMessage(string message, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
