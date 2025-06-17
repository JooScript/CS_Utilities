namespace Utilities
{
    internal class ClsLogger
    {
        public delegate void LogAction(string Msg);
        private LogAction _logAction;

        public ClsLogger(LogAction action)
        {
            _logAction = action;
        }

        public void Log(string Msg)
        {
            _logAction(Msg);
        }
    }
}