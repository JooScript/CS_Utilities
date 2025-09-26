using System.Data;
using Utils.DS.TreesDS.BinTreeDS;
using Utils.DS.TreesDS.GenTreeDS;
using Utils.Format;

namespace Utils.ConsoleDisplay;

public static class ConsoleHelper
{
    #region Loaders

    public static void ShowSpinner(CancellationToken token, string word = "Loading")
    {
        string[] sequence = { "|", "/", "-", "\\" };
        int counter = 0;

        while (!token.IsCancellationRequested)
        {
            Console.Write($"\r{sequence[counter++ % sequence.Length]} {word}...");
            Thread.Sleep(100);
        }
    }

    public static void ShowDotsLoader(int durationMs = 4000)
    {
        var endTime = DateTime.Now.AddMilliseconds(durationMs);

        while (DateTime.Now < endTime)
        {
            for (int i = 0; i < 4; i++)
            {
                Console.Write("\rLoading" + new string('.', i));
                Thread.Sleep(500);
            }
        }
        Console.WriteLine("\rDone!      ");
    }

    public static void ShowProgressBar(int total = 20, int delayMs = 100)
    {
        for (int i = 0; i <= total; i++)
        {
            Console.Write($"\r[{new string('#', i)}{new string(' ', total - i)}] {i * 100 / total}%");
            Thread.Sleep(delayMs);
        }
        Console.WriteLine("\nCompleted!");
    }

    #endregion

    #region Tree Printing

    public static void PrintTree<T>(BinaryTreeNode<T> root, int space = 0)
    {
        int COUNT = 10;  // Distance between levels to adjust the visual representation
        if (root == null)
            return;

        space += COUNT;
        PrintTree(root.Right, space); // Print right subtree first, then root, and left subtree last

        Console.WriteLine();
        for (int i = COUNT; i < space; i++)
            Console.Write(" ");
        Console.WriteLine(root.Value); // Print the current node after space

        PrintTree(root.Left, space); // Recur on the left child
    }

    public static void PrintTree<T>(BinaryTree<T> binaryTree, int space = 0) => PrintTree(binaryTree.Root, space);

    public static void PrintTree<T>(TreeNode<T> root, string indent = " ")
    {
        Console.WriteLine(indent + root.Value);
        foreach (var child in root.Children)
        {
            PrintTree(child, indent + "  ");
        }
    }

    public static void PrintTree<T>(Tree<T> genTree, string indent = " ") => PrintTree(genTree.Root, indent);

    #endregion

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
                Console.Write($"{list[i]} ");
            }
        }

    }

    public static void PrintColoredMessage(string message, ConsoleColor color)
    {
        Console.BackgroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteMenuOption(int number, string description)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{number}: ");
        Console.ResetColor();
        Console.WriteLine(description);
    }

    public static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✘ {message}");
        Console.ResetColor();
    }

    public static void PrintSectionHeader(string title)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine(new string('─', 50));
        Console.WriteLine($" {title.ToUpper()}");
        Console.WriteLine(new string('─', 50));
        Console.ForegroundColor = originalColor;
    }

}