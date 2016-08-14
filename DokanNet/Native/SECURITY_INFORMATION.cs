using System;

namespace DokanNet.Native
{
    /// <summary>
    /// Identifies the object-related security information being set or queried.
    /// This security information includes:
    /// <list type="bullet">
    ///  <item>The owner of an object.</item>
    ///  <item>The primary group of an object.</item>
    ///  <item>The discretionary access control list(DACL) of an object.</item>
    ///  <item>The system access control list(SACL) of an object.</item>
    /// </list>
    /// </summary>
    /// <remarks>Structure taken from <a href="http://www.pinvoke.net/default.aspx/Enums/SECURITY_INFORMATION.html">pinvoke.net</a> </remarks>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379573(v=vs.85).aspx">SECURITY_INFORMATION (MSDN)</a>
    [Flags]
    internal enum SECURITY_INFORMATION : uint
    {
        /// <summary>
        /// The owner identifier of the object is being referenced.
        /// </summary>
        OWNER_SECURITY_INFORMATION = 0x00000001,

        /// <summary>
        /// The primary group identifier of the object is being referenced.
        /// </summary>
        GROUP_SECURITY_INFORMATION = 0x00000002,

        /// <summary>
        /// The DACL of the object is being referenced.
        /// </summary>
        DACL_SECURITY_INFORMATION = 0x00000004,

        /// <summary>
        /// The SACL of the object is being referenced.
        /// </summary>
        SACL_SECURITY_INFORMATION = 0x00000008,

        /// <summary>
        /// The SACL inherits ACEs from the parent object.
        /// </summary>
        /// <remarks>Dokan may not be passing Label ?? 0x00000010</remarks>
        UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,

        /// <summary>
        /// The DACL inherits ACEs from the parent object.
        /// </summary>
        UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,

        /// <summary>
        /// The SACL cannot inherit ACEs.
        /// </summary>
        PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,

        /// <summary>
        /// The DACL cannot inherit access control entries (ACEs).
        /// </summary>
        PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
    }
}