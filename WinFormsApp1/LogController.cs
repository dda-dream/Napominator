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
        TextBox textBox_Log;
        
        public TextBoxLogger(TextBox textBox)
        {
            textBox_Log = textBox;
        }
        public void Log(string message, EventLogEntryType level, string USERNAME)
        {
            string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";

            if (textBox_Log.Lines.Length > 100)
                textBox_Log.Text = "";

            textBox_Log.AppendText(timeStr + message + Environment.NewLine);
            textBox_Log.SelectionStart = textBox_Log.Text.Length;
            textBox_Log.SelectionLength = 0;
            textBox_Log.ScrollToCaret();

        }
    }

    class LogController
    {
        //private static LogController logController;
        //private TextBox textBox_Log;
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

        /*
        public static LogController GetInstance(TextBox textBox, string USERNAME)
        {
            LazyInitializer.EnsureInitialized(ref logController);

            logController.textBox_Log = textBox;
            logController.USERNAME = USERNAME;

            return logController;
        }
        */
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

        public LogController AddTextBoxLogger(TextBox textBox)
        {
            _textBoxLogger = new TextBoxLogger(textBox);
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

        /*
        public void Add_textBox_Log(string _text, bool _writeToLogFileOrRegistry = true, EventLogEntryType _mode = EventLogEntryType.Information)
        {
            string timeStr = DateTime.Now.ToString("dd-MM-yyyy") + " " + DateTime.Now.ToLongTimeString() + ": ";

            if (_writeToLogFileOrRegistry)
            {
                WriteToRegistryLog(_text, _mode, USERNAME);
                string filename = "log-" + USERNAME + ".txt";
                try
                {
                    var file = File.AppendText(filename);
                    file.AutoFlush = false;
                    file.WriteLine(timeStr + _text);
                    file.Close();
                }
                catch (Exception e)
                {
                    WriteToRegistryLog(USERNAME + " " + e.ToString(), EventLogEntryType.Error, USERNAME);
                }
            }

            if (textBox_Log.Lines.Length > 100)
                textBox_Log.Text = "";

            textBox_Log.AppendText(timeStr + _text + Environment.NewLine);
            textBox_Log.SelectionStart = textBox_Log.Text.Length;
            textBox_Log.SelectionLength = 0;
            textBox_Log.ScrollToCaret();
        }


        public void WriteToRegistryLog(string _text, EventLogEntryType _mode, string _USERNAME)
        {
            EventLog eventLog = new EventLog();

            if (!EventLog.Exists("NAPOMINATOR")) // RUN AS ADMIN FIRST TIME
            {
                MessageBox.Show(" RUN AS ADMIN FIRST TIME to allow create EventLog.CreateEventSource(NAPOMINATOR)");
                EventLog.CreateEventSource("NAPOMINATOR", "NAPOMINATOR");
            }
            eventLog.Source = "NAPOMINATOR";

            eventLog.WriteEntry(_USERNAME + " : " + _text, _mode, 1, 1);
        }
        */
    }
}
