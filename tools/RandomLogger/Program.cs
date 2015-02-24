using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RandomLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            const string logDir = @"c:\temp\logtailrlogs";
            const int logFilesCount = 3;
            const int writeIntervalMs = 500;

            var loggers = new List<RandomLogger>();
            for (int i = 0; i < logFilesCount; i++)
            {
                var logger = new RandomLogger(logDir, writeIntervalMs: writeIntervalMs);
                loggers.Add(logger);
                Console.WriteLine("Will write to {0}", logger.LogFileName);
                Task.Factory.StartNew(logger.StartWriting, TaskCreationOptions.LongRunning);
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
