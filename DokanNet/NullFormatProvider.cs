using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DokanNet
{
    public class FormatProviders : IFormatProvider, ICustomFormatter
    {
        public static readonly FormatProviders DefaultFormatProvider = new FormatProviders();
        public static readonly String NullStringRapresentation = "<null>";

        public static String DokanFormat(FormattableString formattable)
             => formattable.ToString(DefaultFormatProvider);

        private FormatProviders() { }

        public object GetFormat(Type service)
        {
            if (service == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        public string Format(string format, object arg, IFormatProvider provider)
        {
            if (arg == null) return NullStringRapresentation;
            var formattable = arg as IFormattable;
            if (formattable != null)
                return formattable.ToString(format, provider);
            
            return arg.ToString();
        }
    }
}
