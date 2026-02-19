using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Napominator
{
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
    }



    public class LogMediator : ILogMediator
    {
        public event EventHandler<LogEventArgs> subscriber;

        public void AddLog(string _message, EventLogEntryType _level, string _USERNAME)
        {
            LogEventArgs a = new LogEventArgs(_message, _level, _USERNAME);

            subscriber.Invoke(this, a);
        }
    }
}
