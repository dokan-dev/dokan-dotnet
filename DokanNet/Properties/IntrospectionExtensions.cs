#if NET10 || NET11 || NET20 || NET30 || NET35 || NET40
#define NET40_OR_LESS
#endif

#if NET40_OR_LESS

namespace System.Reflection
{
    /// <summary>Contains methods for converting <see cref="T:System.Type" /> objects.</summary>
    /// <remarks>This extension is missing in .NET framework 4.0 and below.</remarks>
    internal static class IntrospectionExtensions
    {
        /// <summary>In .NET Framework 4.5 and over, this method return a <see cref="T:System.Reflection.TypeInfo" /> representation of the specified type.
        ///          In .NET Framework 4.0 and below, all information in <see cref="T:System.Reflection.TypeInfo" /> is in <see cref="System.Type"/>, 
        ///          and this method returns <paramref name="type"/> with no changes.</summary>
        /// <returns><paramref name="type"/> with no changes.</returns>
        /// <param name="type">The type to return.</param>
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
}
#endif