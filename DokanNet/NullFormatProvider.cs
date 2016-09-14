using System;

namespace DokanNet
{
    /// <summary>
    /// Provide support to format object with <c>null</c>.
    /// </summary>
    public class FormatProviders : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// A Singleton instance of this class.
        /// </summary>
        public static readonly FormatProviders DefaultFormatProvider = new FormatProviders();

        /// <summary>
        /// A constant string that represents what to use if the formated object is <c>null</c>.
        /// </summary>
        public static readonly string NullStringRepresentation = "<null>";

        /// <summary>
        /// Prevents a default instance of the <see cref="FormatProviders"/> class from being created. 
        /// </summary>
        private FormatProviders()
        {
        }

        /// <summary>
        /// Format a <see cref="FormattableString"/> using <see cref="DefaultFormatProvider"/>.
        /// </summary>
        /// <param name="formattable">The <see cref="FormattableString"/> to format.</param>
        /// <returns>The formated string.</returns>
#pragma warning disable 3001
        public static string DokanFormat(FormattableString formattable)
            => formattable.ToString(DefaultFormatProvider);
#pragma warning restore 3001

        /// <summary>
        /// Returns an object that provides formatting services for the
        /// specified type.
        /// </summary>
        /// <returns>An instance of the object specified by 
        /// <paramref name="formatType" />, if the 
        /// <see cref="T:System.IFormatProvider" /> implementation can supply
        /// that type of object; otherwise, <c>null</c>.</returns>
        /// <param name="formatType">An object that specifies the type of format
        /// object to return. </param>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        /// <summary>
        /// Converts the value of a specified object to an equivalent string
        /// representation using specified format and culture-specific
        /// formatting information.
        /// </summary>
        /// <returns>The string representation of the value of 
        /// <paramref name="arg" />, formatted as specified by 
        /// <paramref name="format" /> and <paramref name="formatProvider" />.
        /// </returns>
        /// <param name="format">A format string containing formatting
        /// specifications. </param>
        /// <param name="arg">An object to format. </param>
        /// <param name="formatProvider">An object that supplies format
        /// information about the current instance. </param>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) return NullStringRepresentation;
            var formattable = arg as IFormattable;
            return formattable?.ToString(format, formatProvider) 
                ?? arg.ToString();
        }
    }
}