using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Napominator
{

    public interface ILogger
    {
        void Log(string message, LogLevel level);
    }

    public class FileLogger : ILogger
    {
        void ILogger.Log(string message, LogLevel level)
        {
            throw new NotImplementedException();
        }
    }
    public class EventViewerLogger : ILogger
    {
        public void Log(string message, LogLevel level)
        {
            throw new NotImplementedException();
        }
    }
    public class TextBoxLogger : ILogger
    {
        void ILogger.Log(string message, LogLevel level)
        {
            throw new NotImplementedException();
        }
    }

    class LogController
    {
        private static LogController logController;
        private TextBox textBox_Log;
        private string USERNAME;

        public static LogController GetInstance(TextBox textBox, string USERNAME)
        {
            if (logController == null)
            {
                logController = new LogController();
                logController.textBox_Log = textBox;
                logController.USERNAME = USERNAME;
            }
            return logController;
        }

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

    }
}
