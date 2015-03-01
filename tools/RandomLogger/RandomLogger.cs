using System;
using System.IO;
using System.Threading;

namespace RandomLogger
{
    class RandomLogger
    {
        private readonly string _loggerName;
        private readonly int _writeIntervalFromMs;
        private readonly int _writeIntervalToMs;
        private readonly string _logFileName;
        private readonly Random _rnd;

        public RandomLogger(string directory, string loggerName, string logFileName = null, int writeIntervalFromMs = 500, int writeIntervalToMs = 1000)
        {
            _loggerName = loggerName;
            _writeIntervalFromMs = writeIntervalFromMs;
            _writeIntervalToMs = writeIntervalToMs;
            _logFileName = Path.Combine(directory, logFileName ?? string.Format("{0:yyyyMMdd_HHmmss}_{1}.log", DateTime.Now, DateTime.Now.Ticks));
            _rnd = new Random((int)DateTime.Now.Ticks);
        }

        public string LogFileName
        {
            get { return _logFileName; }
        }

        public void StartWriting()
        {
            while (true)
            {
                File.AppendAllLines(_logFileName, new []{GetNewLogRecord()});
                Thread.Sleep(_rnd.Next(_writeIntervalFromMs, _writeIntervalToMs));
            }
// ReSharper disable once FunctionNeverReturns
        }


        string GetNewLogRecord()
        {
            return GetNewLogRecord((LogLevel)_rnd.Next(0, 3));
        }

        string GetNewLogRecord(LogLevel level)
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}:{1}:{2}:the message{3:n}\nmsggggggg{3:n}xxxx", 
                DateTime.Now,
                level,
                _loggerName,
                Guid.NewGuid()
                );
        }

        enum LogLevel
        {
// ReSharper disable UnusedMember.Local
            Debug, Info, Warn, Error 
// ReSharper restore UnusedMember.Local
        }
    }
}