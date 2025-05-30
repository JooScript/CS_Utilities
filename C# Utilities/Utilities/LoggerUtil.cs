namespace Utilities
{
    internal class LoggerUtil
    {
        public delegate void LogAction(string Msg);
        private LogAction _logAction;

        public LoggerUtil(LogAction action)
        {
            _logAction = action;
        }

        public void Log(string Msg)
        {
            _logAction(Msg);
        }
    }
}