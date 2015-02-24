using System;
using System.IO;
using System.Threading;

namespace RandomLogger
{
    class RandomLogger
    {
        private readonly int _writeIntervalMs;
        private readonly string _logFileName;
        private readonly Random _rnd;

        public RandomLogger(string directory, string logFileName = null, int writeIntervalMs = 1000)
        {
            _writeIntervalMs = writeIntervalMs;
            _logFileName = Path.Combine(directory, logFileName ?? string.Format("{0:yyyyMMdd_HHmmss}_{1}.log", DateTime.Now, DateTime.Now.Ticks));
            _rnd = new Random();
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
                Thread.Sleep(_writeIntervalMs);
            }
// ReSharper disable once FunctionNeverReturns
        }


        string GetNewLogRecord()
        {
            return GetNewLogRecord((LogLevel)_rnd.Next(0, 3));
        }

        string GetNewLogRecord(LogLevel level)
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}:{1}:{2:n}", 
                DateTime.Now,
                level,
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