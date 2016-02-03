using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokanNet.Logging
{
    using System.Globalization;

    public static class LoggerExtensions
    {
        public static string FormatMessageForLogging(this string message, string category = null, bool addLoggerName = false)
        {
            return message.FormatMessageForLogging(null, category, addLoggerName);
        }

        public static string FormatMessageForLogging(this string message, DateTime? dateTime, string category = null, bool addLoggerName = false)
        {
            var stringBuilder = new StringBuilder();
            if (dateTime.HasValue)
            {
                stringBuilder.AppendFormat("{0}" + " - ", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            }

            if (addLoggerName)
            {
                stringBuilder.Append("[DokanNet] ");
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
