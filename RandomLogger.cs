using System;
using System.IO;
using System.Threading;

namespace LogtailR
{
    class RandomLogger
    {
        private readonly int _writeIntervalMs;
        private readonly string _logFileName;
        private readonly Random _rnd;

        public RandomLogger(string directory, int writeIntervalMs = 1000)
        {
            _writeIntervalMs = writeIntervalMs;
            _logFileName = Path.Combine(directory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
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