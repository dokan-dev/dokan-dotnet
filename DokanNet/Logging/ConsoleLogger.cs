using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokanNet.Logging
{
    using System.Threading;

    public class ConsoleLogger : ILogger
    {
        private static readonly ReaderWriterLockSlim ReadWriteLock = new ReaderWriterLockSlim();

        public void Debug(string message, params object[] args)
        {
            this.WriteToConsole(Console.ForegroundColor, DateTime.Now, message, args);
        }

        public void Info(string message, params object[] args)
        {
            this.WriteToConsole(Console.ForegroundColor, DateTime.Now, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            this.WriteToConsole(ConsoleColor.DarkYellow, DateTime.Now, message, args);
        }

        public void Error(string message, params object[] args)
        {
            this.WriteToConsole(ConsoleColor.Red, DateTime.Now, message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            this.WriteToConsole(ConsoleColor.Red, DateTime.Now, message, args);
        }

        private void WriteToConsole(ConsoleColor newColor, DateTime dateTime, string message, params object[] args)
        {
            ReadWriteLock.EnterWriteLock();
            var origForegroundColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = newColor;
                Console.WriteLine(message.FormatMessageForLogging(dateTime: dateTime, addLoggerName: true));

            }
            finally
            {
                ReadWriteLock.ExitWriteLock();
                Console.ForegroundColor = origForegroundColor;
            }
        }
    }
}
