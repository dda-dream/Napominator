using System.Diagnostics;

namespace Napominator;


public interface ILogger
{
    void Log(string message, EventLogEntryType level, string USERNAME);
    public void LogClear();
}

public class FileLogger : ILogger
{
    public void Log(string message, EventLogEntryType level, string USERNAME)
    {
        string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";
        string filename = "log-" + USERNAME + ".txt";
        var logEntry = timeStr + message + Environment.NewLine;
        try
        {
            File.AppendAllText(filename, logEntry);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FileLogger error: {ex}");
        }
    }

    public void LogClear()
    {
    }
}
public class EventViewerLogger : ILogger
{
    
    public void Log(string message, EventLogEntryType level, string USERNAME)
    {
        EventLog eventLog = new EventLog();

        if (!EventLog.Exists("NAPOMINATOR")) // RUN AS ADMIN FIRST TIME
        {
            MessageBox.Show(" RUN AS ADMIN FIRST TIME to allow create EventLog.CreateEventSource(NAPOMINATOR)");
            EventLog.CreateEventSource("NAPOMINATOR", "NAPOMINATOR");
        }
        eventLog.Source = "NAPOMINATOR";
        eventLog.WriteEntry(USERNAME + " : " + message, level, 1, 1);
    }

    public void LogClear()
    {
    }
}
public class TextBoxLogger : ILogger
{
    LogMediator _logHandler;

    public TextBoxLogger(LogMediator logHandler)
    {
        _logHandler = logHandler;
    }
    public void Log(string message, EventLogEntryType level, string USERNAME)
    {
        _logHandler.AddLog(message, level, USERNAME);
    }

    public void LogClear()
    {
        _logHandler.LogClear();   
    }
}

public class LogController 
{
    private string USERNAME;

    private TextBoxLogger _textBoxLogger;
    private EventViewerLogger _eventViewerLogger;
    private FileLogger _fileLogger;


    public LogController()
    {
    }

    public static LogController Builder()
    {
        return new LogController();
    }
    public LogController AddUSERNAME(string USERNAME)
    {
        this.USERNAME = USERNAME;
        return this;
    }

    public LogController AddTextBoxLogger(LogMediator logHandler)
    {
        _textBoxLogger = new TextBoxLogger(logHandler);
        return this;
    }
    public LogController AddEventViewerLogger()
    {
        _eventViewerLogger = new EventViewerLogger();
        return this;
    }
    public LogController AddFileLogger()
    {
        _fileLogger = new FileLogger();
        return this;
    }


    public void LogClear()
    {
        _textBoxLogger.LogClear();
    }
    public void Log(string _message, EventLogEntryType _level = EventLogEntryType.Information, string _USERNAME = "")
    {
        if (_textBoxLogger != null)
            _textBoxLogger.Log(_message, _level, USERNAME);
        if (_eventViewerLogger != null)
            _eventViewerLogger.Log(_message, _level, USERNAME);
        if (_fileLogger != null)
            _fileLogger.Log(_message, _level, USERNAME);
    }


}




public class LogEventArgs : EventArgs
{
    public string Message { get; set; }
    public EventLogEntryType level { get; set; }
    public string USERNAME { get; set; }

    public LogEventArgs(string _message, EventLogEntryType _level, string _USERNAME)
    {
        Message = _message;
        level = _level;
        USERNAME = _USERNAME;
    }
}



interface ILogMediator
{
    public event EventHandler<LogEventArgs> subscriber;
    public void AddLog(string _message, EventLogEntryType _level, string _USERNAME);
    public void LogClear();
}



public class LogMediator : ILogMediator
{
    public event EventHandler<LogEventArgs> subscriber;

    public void AddLog(string _message, EventLogEntryType _level, string _USERNAME)
    {
        LogEventArgs a = new LogEventArgs(_message, _level, _USERNAME);
        subscriber.Invoke(this, a);
    }

    public void LogClear()
    {

        LogEventArgs a = new LogEventArgs("***CLEAR***LOG***CONTROL***", EventLogEntryType.Information, "");
        subscriber.Invoke(this, a);
    }
}
