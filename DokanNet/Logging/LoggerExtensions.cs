using System;
using System.Text;

namespace DokanNet.Logging
{
    using System.Globalization;

    public static class LoggerExtensions
    {
        public static string FormatMessageForLogging(this string message, string category = null, string addLoggerName = "")
        {
            return message.FormatMessageForLogging(null, category, addLoggerName);
        }

        public static string FormatMessageForLogging(this string message, DateTime? dateTime, string category = null, string loggerName = "")
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
