using System;

namespace DokanNet
{
    /// <summary>
    /// Provide support to format object with <see langword="null" />
    /// </summary>
    public class FormatProviders : IFormatProvider, ICustomFormatter
    {
        public static readonly FormatProviders DefaultFormatProvider = new FormatProviders();
        public static readonly string NullStringRepresentation = "<null>";

        public static string DokanFormat(FormattableString formattable)
            => formattable.ToString(DefaultFormatProvider);

        private FormatProviders()
        {
        }

        public object GetFormat(Type service)
        {
            return service == typeof(ICustomFormatter) ? this : null;
        }

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