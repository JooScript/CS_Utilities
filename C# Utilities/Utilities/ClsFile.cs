﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace Utilities
{
    public static class ClsFile
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
                        ClsUtil.CreateFolderIfDoesNotExist(normalizedPath);
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

                ClsUtil.CreateFolderIfDoesNotExist(_logDirectory);

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
                ClsUtil.CreateFolderIfDoesNotExist(_logDirectory);

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
                ClsUtil.ErrorLogger(ex);
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
                ClsUtil.ErrorLogger(ex);
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

            if (!ClsUtil.CreateFolderIfDoesNotExist(DestinationFolder))
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
                ClsUtil.ErrorLogger(logEx);
                return false;
            }
        }

        public static bool StoreToFile(string Content, string sourceFile = "File.txt", string DestinationFolder = null, bool Replace = true)
        {
            return StoreToFileAsync(Content, sourceFile, DestinationFolder, Replace).GetAwaiter().GetResult();
        }

        public static string ReplaceFileNameWithGUID(string sourceFile)
        {
            return ClsUtil.GenerateGUID() + new FileInfo(sourceFile).Extension;
        }

        public static bool CopyImageToProjectImagesFolder(string DestinationFolder, ref string sourceFile)
        {
            if (!ClsUtil.CreateFolderIfDoesNotExist(DestinationFolder))
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
                ClsUtil.ErrorLogger(iox);
                return false;
            }

            sourceFile = destinationFile;
            return true;
        }

        public static bool DeleteFile(string? imageLocation)
        {
            if (string.IsNullOrWhiteSpace(imageLocation))
            {
                ClsUtil.ErrorLogger(new ArgumentNullException(nameof(imageLocation), "File path cannot be null or whitespace."));
                return false;
            }

            try
            {
                if (!File.Exists(imageLocation))
                {
                    ClsUtil.ErrorLogger(new FileNotFoundException("File not found", imageLocation));
                    return false;
                }

                File.Delete(imageLocation);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                ClsUtil.ErrorLogger(ex);
                return false;
            }
        }

    }
}