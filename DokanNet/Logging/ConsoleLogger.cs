using System;

namespace DokanNet.Logging
{
    public class ConsoleLogger : ILogger
    {
        private string loggerName;
        public ConsoleLogger(string loggerName = "")
        {
            this.loggerName = loggerName;
        }

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
            var origForegroundColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = newColor;
                if (args.Length > 0)
                    message = string.Format(message, args);
                Console.WriteLine(message.FormatMessageForLogging(dateTime: dateTime, loggerName: loggerName));

            }
            finally
            {
                Console.ForegroundColor = origForegroundColor;
            }
        }
    }
}
