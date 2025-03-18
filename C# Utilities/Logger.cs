namespace Utilities
{
    internal class clsLogger
    {
        public delegate void LogAction(string Msg);
        private LogAction _logAction;

        public clsLogger(LogAction action)
        {
            _logAction = action;
        }

        public void Log(string Msg)
        {
            _logAction(Msg);
        }
    }
}