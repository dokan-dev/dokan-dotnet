using System;

namespace DokanNet.Logging
{
    /// <summary>
    /// Log to the console.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly string _loggerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="loggerName">Optional name to be added to each log line.</param>
        public ConsoleLogger(string loggerName = "")
        {
            _loggerName = loggerName;
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            WriteToConsole(Console.ForegroundColor, message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            WriteToConsole(Console.ForegroundColor, message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            WriteToConsole(ConsoleColor.DarkYellow, message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            WriteToConsole(ConsoleColor.Red, message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            WriteToConsole(ConsoleColor.Red, message, args);
        }

        private void WriteToConsole(ConsoleColor newColor, string message, params object[] args)
        {
            var origForegroundColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = newColor;
                if (args.Length > 0)
                    message = string.Format(message, args);
                Console.WriteLine(message.FormatMessageForLogging(true, loggerName: _loggerName));
            }
            finally
            {
                Console.ForegroundColor = origForegroundColor;
            }
        }
    }
}