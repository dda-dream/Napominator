using System.Diagnostics;

namespace Napominator
{

    public interface ILogger
    {
        void Log(string message, EventLogEntryType level, string USERNAME);
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
    }

    class LogController
    {
        private string USERNAME;

        private TextBoxLogger _textBoxLogger;
        private EventViewerLogger _eventViewerLogger;
        private FileLogger _fileLogger;



        public void Log(string message, EventLogEntryType level)
        {
            if (_textBoxLogger != null)
                _textBoxLogger.Log(message, level, USERNAME);
            if (_eventViewerLogger != null)
                _eventViewerLogger.Log(message, level, USERNAME);
            if (_fileLogger != null)
                _fileLogger.Log(message, level, USERNAME);
        }



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



        public void Add_textBox_Log(string _text, bool _writeToLogFileOrRegistry = true, EventLogEntryType _mode = EventLogEntryType.Information)
        {
            this.Log(_text, _mode);
        }




    }
}
