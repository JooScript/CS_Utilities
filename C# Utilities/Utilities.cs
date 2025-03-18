using System.Data;
using System.Diagnostics;
using System.Text;

namespace Utilities
{
    public class clsUtil
    {
        private static readonly string LogFilePath;

        static clsUtil()
        {
            //LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            LogFilePath = Path.Combine("C:\\Users\\yousef\\Desktop\\", "error.log");
        }

        public static bool RememberUsernameAndPasswordToFile(string Username, string Password, string FileName = "data.txt", string Splitter = "#//#")
        {
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath = currentDirectory + $"\\{FileName}";
                if (Username == "" && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                string dataToSave = Username + Splitter + Password;

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(dataToSave);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger(ex);
                return false;
            }
        }

        public static bool GetStoredUsernameAndPasswordFromFile(ref string Username, ref string Password, string FileName = "data.txt", string Splitter = "#//#")
        {
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath = currentDirectory + $"\\{FileName}";

                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                            string[] result = line.Split(new string[] { Splitter }, StringSplitOptions.None);
                            Username = result[0];
                            Password = result[1];
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger(ex);
                return false;
            }
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

        public static void LogToFile(string ErrorMessage)
        {
            try
            {
                File.AppendAllText(LogFilePath, ErrorMessage);
            }
            catch (Exception logEx)
            {
                ErrorLogger(logEx);
            }
        }

        public static async Task LogToFileAsync(string errorMessage)
        {
            try
            {
                await File.AppendAllTextAsync(LogFilePath, errorMessage.ToString());
            }
            catch (Exception logEx)
            {
                ErrorLogger(logEx);
            }
        }

        public static void LogToWinEventLog(string logMessage, string AppName, EventLogEntryType Type = EventLogEntryType.Error)
        {
            try
            {
                if (!EventLog.SourceExists(AppName))
                {
                    EventLog.CreateEventSource(AppName, "Application");
                }
                EventLog.WriteEntry(AppName, logMessage, Type);
            }
            catch (Exception logEx)
            {
                ErrorLogger(logEx);
            }
        }

        public static void LogErrorToWinEventLog(string logMessage, string AppName)
        {
            LogToWinEventLog(logMessage, AppName, EventLogEntryType.Error);
        }

        public static void ErrorLogger(Exception ex)
        {
            clsLogger FileLogger = new clsLogger(LogToFile);
            FileLogger.Log(ExceptionToString(ex));
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
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

        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static bool CreateFolderIfDoesNotExist(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorLogger(ex);
                    return false;
                }
            }
            return true;
        }

        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            string fileName = sourceFile;
            FileInfo fi = new FileInfo(fileName);
            string extn = fi.Extension;
            return GenerateGUID() + extn;
        }

        public static bool CopyImageToProjectImagesFolder(string DestinationFolder, ref string sourceFile)
        {
            if (!CreateFolderIfDoesNotExist(DestinationFolder))
            {
                return false;
            }

            string destinationFile = DestinationFolder + ReplaceFileNameWithGUID(sourceFile);
            try
            {
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (IOException iox)
            {
                ErrorLogger(iox);
                return false;
            }

            sourceFile = destinationFile;
            return true;
        }

        public static string DateToShort(DateTime Dt1)
        {
            return Dt1.ToString("dd/MMM/yyyy");
        }

    }
}
