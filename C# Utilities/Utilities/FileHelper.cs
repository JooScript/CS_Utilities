using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace Utilities
{
    public static class FileHelper
    {
        #region Logging

        #region Locks

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(); //Per File Lock
        private static readonly object _cleanupLock = new();
        private static readonly object _logDirLock = new object();
        private static readonly object _logLevelLock = new object();

        #endregion

        public enum enLogLevel
        {
            Info = 0,
            Warn = 1,
            Error = 2,
        }
        public static enLogLevel LogLevel
        {
            get
            {
                lock (_logLevelLock)
                {
                    return _LogLevel;
                }
            }
            set
            {
                lock (_logLevelLock)
                {
                    _LogLevel = value;
                }
            }
        }

        private static enLogLevel _LogLevel = enLogLevel.Info;

        private static string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Logs");

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
                        Helper.CreateFolderIfDoesNotExist(normalizedPath);
                        _logDirectory = normalizedPath;
                    }
                }
                catch (Exception ex) when (ex is PathTooLongException or NotSupportedException or IOException) // Known filesystem errors
                {
                    throw new ArgumentException($"Invalid log directory path: {ex.Message}", nameof(value));
                }
            }
        }

        private const int _MaxLogAgeDays = 7;

        private static void _CleanOldLogs()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_MaxLogAgeDays);
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

                Helper.CreateFolderIfDoesNotExist(_logDirectory);

                string logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{_LogLevel.ToString()}] {message}{Environment.NewLine}";
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
                Helper.CreateFolderIfDoesNotExist(_logDirectory);

                string logFilePath = Path.Combine(_logDirectory, LogFileName);

                var logEntry = new
                {
                    Timestamp = DateTime.Now,
                    Level = _LogLevel.ToString(),
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
                Helper.ErrorLogger(ex);
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
                Helper.ErrorLogger(ex);
                return false;
            }
        }

        #endregion

        public static async Task<bool> StoreToFileAsync(string Content, string sourceFile = "File.txt", string DestinationFolder = null, bool Replace = true)
        {
            if (string.IsNullOrEmpty(DestinationFolder))
            {
                DestinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (!Helper.CreateFolderIfDoesNotExist(DestinationFolder))
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
                Helper.ErrorLogger(logEx);
                return false;
            }
        }

        public static bool StoreToFile(string Content, string sourceFile = "File.txt", string DestinationFolder = null, bool Replace = true)
        {
            return StoreToFileAsync(Content, sourceFile, DestinationFolder, Replace).GetAwaiter().GetResult();
        }

        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            return Helper.GenerateGUID() + new FileInfo(sourceFile).Extension;
        }

        public static bool HandleFileToFolder(string destinationFolder, ref string sourceFile, bool replaceWithGuid, Func<string, string, bool> fileOperation)
        {
            if (!Helper.CreateFolderIfDoesNotExist(destinationFolder))
                return false;

            try
            {
                string fileName;

                if (replaceWithGuid)
                {
                    fileName = ReplaceFileNameWithGUID(sourceFile);
                }
                else
                {
                    fileName = Path.GetFileName(sourceFile);
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string ext = Path.GetExtension(fileName);
                    string destinationFile = Path.Combine(destinationFolder, fileName);

                    int counter = 1;
                    while (File.Exists(destinationFile))
                    {
                        string newFileName = $"{nameWithoutExt} ({counter}){ext}";
                        destinationFile = Path.Combine(destinationFolder, newFileName);
                        counter++;
                    }

                    fileName = Path.GetFileName(destinationFile);
                }

                string finalDestination = Path.Combine(destinationFolder, fileName);

                if (!fileOperation(sourceFile, finalDestination))
                {
                    return false;
                }

                sourceFile = finalDestination;
                return true;
            }
            catch (IOException iox)
            {
                Helper.ErrorLogger(iox);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Helper.ErrorLogger(ex);
                return false;
            }
        }

        public static bool CopyFileToFolder(string destinationFolder, ref string sourceFile, bool replaceWithGuid = false)
        {
            return HandleFileToFolder(destinationFolder, ref sourceFile, replaceWithGuid, (src, dest) =>
            {
                File.Copy(src, dest);
                return true;
            });
        }

        public static bool MoveFileToFolder(string destinationFolder, ref string sourceFile, bool replaceWithGuid = false)
        {
            return HandleFileToFolder(destinationFolder, ref sourceFile, replaceWithGuid, (src, dest) =>
            {
                File.Move(src, dest);
                return true;
            });
        }

        public static bool DeleteFile(string? fileLocation)
        {
            if (string.IsNullOrWhiteSpace(fileLocation))
            {
                Helper.ErrorLogger(new ArgumentNullException(nameof(fileLocation), "File path cannot be null or whitespace."));
                return false;
            }

            try
            {
                if (!File.Exists(fileLocation))
                {
                    Helper.ErrorLogger(new FileNotFoundException("File not found", fileLocation));
                    return false;
                }

                File.Delete(fileLocation);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                Helper.ErrorLogger(ex);
                return false;
            }
        }

        /// <summary>
        /// Waits until the specified file is available for exclusive access.
        /// </summary>
        /// <param name="filePath">The path of the file to check.</param>
        /// <param name="maxAttempts">Maximum retry attempts.</param>
        /// <param name="delayMs">Delay in milliseconds between attempts.</param>
        /// <returns>True if the file becomes available; otherwise, false.</returns>
        public static bool WaitForFileAvailable(string filePath, int maxAttempts = 5, int delayMs = 1000)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                        return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(delayMs);
                }
            }
            return false;
        }

    }
}