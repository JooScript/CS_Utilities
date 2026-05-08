using Microsoft.Win32;
using Utils.FileActions;
using System.Runtime.Versioning;

namespace Utils.Win;

public class WinHelper
{
    [SupportedOSPlatform("windows")]
    public static bool SaveToCurrentUserRegistry(string keyName, string keyValue, string appName)
    {
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("Software name cannot be null or empty.", nameof(appName));

        if (string.IsNullOrWhiteSpace(keyName))
            throw new ArgumentException("Key name cannot be null or empty.", nameof(keyName));

        if (keyValue == null)
            throw new ArgumentNullException(nameof(keyValue), "Key value cannot be null.");

        try
        {
            Registry.SetValue(
                @$"HKEY_CURRENT_USER\SOFTWARE\{appName}",
                keyName,
                keyValue,
                RegistryValueKind.String);

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error saving value to registry.", ex);
        }
    }

    [SupportedOSPlatform("windows")]
    public static bool KeyExists(string keyName, string appName)
    {
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("Software name cannot be null or empty.", nameof(appName));

        if (string.IsNullOrWhiteSpace(keyName))
            throw new ArgumentException("Key name cannot be null or empty.", nameof(keyName));

        try
        {
            object? value = Registry.GetValue(
                @$"HKEY_CURRENT_USER\SOFTWARE\{appName}",
                keyName,
                null);

            return value != null;
        }
        catch
        {
            return false; // if registry access fails, treat as not existing
        }
    }

    [SupportedOSPlatform("windows")]
    public static string GetFromCurrentUserRegistry(string keyName, string appName)
    {
        if (string.IsNullOrWhiteSpace(appName))
            throw new ArgumentException("Software name cannot be null or empty.", nameof(appName));

        if (string.IsNullOrWhiteSpace(keyName))
            throw new ArgumentException("Key name cannot be null or empty.", nameof(keyName));

        try
        {
            if (!KeyExists(keyName, appName))
                return null;

            string? value = Registry.GetValue(
                @$"HKEY_CURRENT_USER\SOFTWARE\{appName}",
                keyName,
                null) as string;

            if (value == null)
                throw new KeyNotFoundException(
                    $"Registry key '{keyName}' for software '{appName}' was not found.");

            return value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error reading value from registry.", ex);
        }
    }

    // [SupportedOSPlatform("windows")]
    // public static void LogToWinEventLog(string logMessage, string AppName, EventLogEntryType Type = EventLogEntryType.Error)
    // {
    //     try
    //     {
    //         if (!EventLog.SourceExists(AppName))
    //         {
    //             EventLog.CreateEventSource(AppName, "Application");
    //         }
    //         EventLog.WriteEntry(AppName, logMessage, Type);
    //     }
    //     catch (Exception logEx)
    //     {
    //         FileHelper.ErrorLogger(logEx);
    //     }
    // }

    // [SupportedOSPlatform("windows")]
    // public static void LogErrorToWinEventLog(string logMessage, string AppName)
    // {
    //     LogToWinEventLog(logMessage, AppName, EventLogEntryType.Error);
    // }

}