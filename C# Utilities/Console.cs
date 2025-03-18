using System.Data;

namespace Utilities
{
    public class clsConsoleUtil
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
            foreach (T item in list)
            {
                Console.WriteLine(item);
            }
        }

    }
}
