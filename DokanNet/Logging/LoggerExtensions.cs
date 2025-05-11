﻿using System;
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
        /// <param name="loggerName">Optional log name to at to the log message.</param>
        /// <param name="dateTimeFormatInfo">An object that supplies format information for DateTime.</param>
        /// <returns>A formated log message.</returns>
        public static string FormatMessageForLogging(
            this string message,
            string? category = null,
            string loggerName = "",
            DateTimeFormatInfo? dateTimeFormatInfo = null)
        {
            return message.FormatMessageForLogging(false, category, loggerName, dateTimeFormatInfo);
        }

        /// <summary>
        /// Format a log message.
        /// </summary>
        /// <param name="message">Message to format.</param>
        /// <param name="addDateTime">If date and time shout be added to the log message.</param>
        /// <param name="category">Optional category to add to the log message.</param>
        /// <param name="loggerName">Optional log name to at to the log message.</param>
        /// <param name="dateTimeFormatInfo">An object that supplies format information for DateTime.</param>
        /// <returns>A formated log message.</returns>
        public static string FormatMessageForLogging(
            this string message,
            bool addDateTime = false,
            string? category = null,
            string loggerName = "",
            DateTimeFormatInfo? dateTimeFormatInfo = null)
        {
            var stringBuilder = new StringBuilder();
            if (addDateTime)
            {
                stringBuilder.AppendFormat("{0} - ", DateTime.Now.ToString((IFormatProvider?)dateTimeFormatInfo ?? CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(loggerName))
            {
                stringBuilder.Append(loggerName);
            }

            if (!string.IsNullOrEmpty(category))
            {
                stringBuilder.Append($"{category} ");
            }

            stringBuilder.Append(message);
            return stringBuilder.ToString();
        }
    }
}