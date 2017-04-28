#if NETCOREAPP1_0
using System;

namespace DokanNetMirror
{
    /// <summary>Contains methods that is missing in .NET CORE 1.0.</summary>
    internal static class CoreAppExtensions
    {
        /// <summary>Converts the value of this instance to its equivalent string representation (either "True" or "False").</summary>
        /// <returns>
        /// <see cref="F:System.Boolean.TrueString" /> if the value of this instance is true, 
        /// or <see cref="F:System.Boolean.FalseString" /> if the value of this instance is false.</returns>
        /// <param name="boolean">The <see cref="bool"/> value to convert to get string representation for.</param>
        /// <param name="provider">(Reserved) An <see cref="T:System.IFormatProvider" /> object. </param>
        public static string ToString(this bool boolean, IFormatProvider provider)
        {
            return boolean.ToString();
        }
    }
}
#endif
