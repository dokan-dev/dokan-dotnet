using System;
using System.Globalization;
using System.Text;

namespace DokanNet.Logging
{
    /// <summary>
    /// Extension functions to log messages.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Format a log message.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="category">Optional category to add to the log message.</param>
        /// <param name="addLoggerName">Optional log name to att to the log message.</param>
        /// <returns>A formated log message.</returns>
        public static string FormatMessageForLogging(this string message, string category = null,
            string addLoggerName = "")
        {
            return message.FormatMessageForLogging(null, category, addLoggerName);
        }

        /// <summary>
        /// Format a log message.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="dateTime">If date and time shout be added to the log message.</param>
        /// <param name="category">Optional category to add to the log message.</param>
        /// <param name="loggerName">Optional log name to att to the log message.</param>
        /// <returns>A formated log message.</returns>
        public static string FormatMessageForLogging(this string message, DateTime? dateTime, string category = null,
            string loggerName = "")
        {
            var stringBuilder = new StringBuilder();
            if (dateTime.HasValue)
            {
                stringBuilder.AppendFormat("{0}" + " - ", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(loggerName))
            {
                stringBuilder.Append(loggerName);
            }

            if (!string.IsNullOrEmpty(category))
            {
                stringBuilder.AppendFormat("{0} ", category);
            }

            stringBuilder.Append(message);
            return stringBuilder.ToString();
        }
    }
}