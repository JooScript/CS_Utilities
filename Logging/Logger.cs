namespace Utils.Logging;

internal class Logger
{
    public delegate void LogAction(string Msg);
    private LogAction _logAction;

    public Logger(LogAction action)
    {
        _logAction = action;
    }

    public void Log(string Msg)
    {
        _logAction(Msg);
    }
}