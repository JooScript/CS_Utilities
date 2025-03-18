using System.Diagnostics;

namespace Utilities
{
    public class clsWinUtil
    {
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
                clsUtil.ErrorLogger(logEx);
            }
        }

        public static void LogErrorToWinEventLog(string logMessage, string AppName)
        {
            LogToWinEventLog(logMessage, AppName, EventLogEntryType.Error);
        }

    }
}
