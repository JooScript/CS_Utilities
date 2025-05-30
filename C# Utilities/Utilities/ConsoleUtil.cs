using System.Data;

namespace Utilities
{
    public class ConsoleUtil
    {
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

        public static void ListConsolePrinting<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Console.WriteLine("The list is empty or null.");
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"[{FormatUtil.FormatNumbers(i + 1, list.Count)}] {list[i]}");
            }
        }

        public static void PrintColoredMessage(string message, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine();
            Console.ResetColor();
        }

    }
}
