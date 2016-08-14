using System;

namespace DokanNet
{
    /// <summary>
    /// Provide support to format object with <c>null</c>
    /// </summary>
    public class FormatProviders : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// A Singleton instanse of this class.
        /// </summary>
        public static readonly FormatProviders DefaultFormatProvider = new FormatProviders();

        /// <summary>
        /// A constant string that representet what to use if the formated object is <c>null</c>.
        /// </summary>
        public static readonly string NullStringRepresentation = "<null>";

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
        /// Prevent instantiation of the class
        /// </summary>
        private FormatProviders()
        {
        }

        /// <inheritdoc />
        public object GetFormat(Type service)
        {
            return service == typeof(ICustomFormatter) ? this : null;
        }

        /// <inheritdoc />
        public string Format(string format, object arg, IFormatProvider provider)
        {
            if (arg == null) return NullStringRepresentation;
            var formattable = arg as IFormattable;
            if (formattable != null)
                return formattable.ToString(format, provider);

            return arg.ToString();
        }
    }
}