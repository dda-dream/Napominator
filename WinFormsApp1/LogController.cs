using Serilog;
using Serilog.Sinks.Grafana.Loki;
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
    LogMediator _logMediator;

    public TextBoxLogger(LogMediator logHandler)
    {
        _logMediator = logHandler;
    }
    public void Log(string message, EventLogEntryType level, string USERNAME)
    {
        _logMediator.AddLog(message, level, USERNAME);
    }

    public void LogClear()
    {
        _logMediator.LogClear();   
    }
}
public class LokiLogger : ILogger
{
    public LokiLogger(string ip)
    {
        Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Machine", Environment.MachineName)
                .Enrich.WithProperty("User", Environment.UserName)
                .Enrich.WithProperty("OS", Environment.OSVersion.VersionString)
                .WriteTo.GrafanaLoki("http://10.66.66.49:3100",
                        batchPostingLimit: 1,
                        period: TimeSpan.FromSeconds(60),
                        textFormatter: new LokiJsonTextFormatter(),
                        labels: new[]
                        {
                            new LokiLabel { Key = "app", Value = Path.GetFileName(Application.ExecutablePath) },
                            new LokiLabel { Key = "ip", Value = ip }
                        },
                        propertiesAsLabels: new[] { "Level" }
                )
                .CreateLogger();

        //Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
        //Log.Information("NAPOMINATOR");
        //Log.CloseAndFlush();
    }
    public void Log(string message, EventLogEntryType level, string USERNAME)
    {
        var logEntry = message;
        if(level == EventLogEntryType.Information)
            Serilog.Log.Information(logEntry);
        if (level == EventLogEntryType.Error)
            Serilog.Log.Error(logEntry);
        if (level == EventLogEntryType.Warning)
            Serilog.Log.Warning(logEntry);
    }

    public void LogClear()
    {
    }


}







public class LogController : IDisposable
{
    private string USERNAME;

    private TextBoxLogger _textBoxLogger;
    private EventViewerLogger _eventViewerLogger;
    private FileLogger _fileLogger;
    private LokiLogger _lokiLogger;


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
    public LogController AddSerilogLoki(string ip)
    {
        _lokiLogger = new LokiLogger(ip);
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

        if(_lokiLogger != null)
            _lokiLogger.Log(_message, _level, USERNAME);

    }

    public void Dispose()
    {
        Serilog.Log.CloseAndFlush();
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
