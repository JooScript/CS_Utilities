using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace Utilities
{
    public static class clsFile
    {

        #region Logging
        public enum enLogLevel
        {
            Info = 0,
            Warn = 1,
            Error = 2,
        }

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(); //Per File Lock
        private static readonly object _cleanupLock = new();
        private static readonly object _logDirLock = new object();
        private const int MaxLogAgeDays = 7;
        private static string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");
        private static enLogLevel _LogLevel = enLogLevel.Info;

        /// <summary>
        /// Gets or sets the log directory path. Automatically creates the directory when set.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if value is null/empty or invalid.</exception>
        public static string LogDirectory
        {
            get
            {
                lock (_logDirLock)
                {
                    return _logDirectory;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Log directory cannot be null or empty.");
                }

                try
                {
                    var normalizedPath = Path.GetFullPath(value); // Resolves relative paths
                    lock (_logDirLock)
                    {
                        clsUtil.CreateFolderIfDoesNotExist(normalizedPath);
                        _logDirectory = normalizedPath;
                    }
                }
                catch (Exception ex) when (ex is PathTooLongException or NotSupportedException or IOException) // Known filesystem errors
                {
                    throw new ArgumentException($"Invalid log directory path: {ex.Message}", nameof(value));
                }
            }
        }

        private static string _LogLevelStr()
        {
            if (_LogLevel == enLogLevel.Info)
            {
                return "INFO";
            }
            else if (_LogLevel == enLogLevel.Warn)
            {
                return "WARN";
            }
            else
            {
                return "ERROR";
            }
        }

        private static void _CleanOldLogs()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-MaxLogAgeDays);
                var logFiles = Directory.GetFiles(_logDirectory, "AppLog_*.log");

                foreach (var file in logFiles)
                {
                    try
                    {
                        var datePart = Path.GetFileNameWithoutExtension(file).Substring(7);
                        if (DateTime.TryParseExact(datePart, "yyyyMMdd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var fileDate))
                        {
                            if (fileDate < cutoffDate)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                    catch { /* Ignore individual file errors */ }
                }
            }
            catch { /* Ignore cleanup errors */ }
        }

        public static async Task LogToFileAsync(string message)
        {
            try
            {
                string logFileName = $"AppLog_{DateTime.Now:yyyyMMdd}.log";
                string logFilePath = Path.Combine(_logDirectory, logFileName);

                clsUtil.CreateFolderIfDoesNotExist(_logDirectory);

                string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{_LogLevelStr()}] {message}{Environment.NewLine}";
                var fileLock = _fileLocks.GetOrAdd(logFilePath, _ => new SemaphoreSlim(1, 1));

                try
                {
                    await fileLock.WaitAsync();
                    await File.AppendAllTextAsync(logFilePath, logEntry);
                }
                finally
                {
                    fileLock.Release();
                }

                // Only one thread should perform cleanup
                if (DateTime.UtcNow.Hour == 0 && Monitor.TryEnter(_cleanupLock))
                {
                    try
                    {
                        _CleanOldLogs();
                    }
                    finally
                    {
                        Monitor.Exit(_cleanupLock);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public static void LogToFile(string message)
        {
            LogToFileAsync(message).GetAwaiter().GetResult();
        }

        public static void LogToJsonFile(string message)
        {
            string LogFileName = $"AppLog_{DateTime.Now:yyyyMMdd}.json";

            try
            {
                clsUtil.CreateFolderIfDoesNotExist(_logDirectory);

                string logFilePath = Path.Combine(_logDirectory, LogFileName);

                var logEntry = new
                {
                    Timestamp = DateTime.Now,
                    Level = _LogLevelStr(),
                    Message = message,
                    Source = Environment.MachineName
                };

                string jsonEntry = JsonSerializer.Serialize(logEntry) + ",";

                // Append to JSON log file (creates a JSON array over time)
                if (!File.Exists(logFilePath))
                {
                    File.WriteAllText(logFilePath, "[" + jsonEntry);
                }
                else
                {
                    // Remove the last "]" if exists, add new entry, then close the array
                    string content = File.ReadAllText(logFilePath).TrimEnd(']');
                    File.WriteAllText(logFilePath, content + jsonEntry + "]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON logging failed: {ex.Message}");
            }
        }

        #endregion

        #region Username and Password

        public static bool StoreUsernameAndPasswordToFile(string Username, string Password, string FileName = "data.txt", string Splitter = "#//#")
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
                clsUtil.ErrorLogger(ex);
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
                clsUtil.ErrorLogger(ex);
                return false;
            }
        }

        #endregion

        public static async Task<bool> StoreToFileAsync(string Content,string sourceFile = "File.txt", string DestinationFolder = null,  bool Replace = true)
        {
            if (string.IsNullOrEmpty(DestinationFolder))
            {
                DestinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (!clsUtil.CreateFolderIfDoesNotExist(DestinationFolder))
            {
                return false;
            }

            string LogFilePath = Path.Combine(DestinationFolder, sourceFile);
            try
            {
                if (Replace)
                {
                    await File.WriteAllTextAsync(LogFilePath, Content);
                }
                else
                {
                    await File.AppendAllTextAsync(LogFilePath, Content);

                }

                return true;
            }
            catch (Exception logEx)
            {
                clsUtil.ErrorLogger(logEx);
                return false;
            }
        }

        public static bool StoreToFile(string Content,string sourceFile = "File.txt", string DestinationFolder = null,  bool Replace = true)
        {
            return StoreToFileAsync(Content, sourceFile, DestinationFolder, Replace).GetAwaiter().GetResult();
        }

        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            string fileName = sourceFile;
            FileInfo fi = new FileInfo(fileName);
            string extn = fi.Extension;
            return clsUtil.GenerateGUID() + extn;
        }

        public static bool CopyImageToProjectImagesFolder(string DestinationFolder, ref string sourceFile)
        {
            if (!clsUtil.CreateFolderIfDoesNotExist(DestinationFolder))
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
                clsUtil.ErrorLogger(iox);
                return false;
            }

            sourceFile = destinationFile;
            return true;
        }

    }
}