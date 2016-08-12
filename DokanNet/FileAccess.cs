using System;

namespace DokanNet
{
    /// <summary>
    /// Defines standard, specific, and generic rights. 
    /// These rights are used in access control entries (ACEs) and are the primary means of 
    /// specifying the requested or granted access to an object. 
    /// </summary>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa374896(v=vs.85).aspx">Access Mask Format (MSDN)</seealso>
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa374892(v=vs.85).aspx">ACCESS_MASK (MSDN)</seealso>
    [Flags]
    public enum FileAccess : long
    {
        /// <summary>
        /// Read access right to an object
        /// </summary>
        ReadData = 0x00000001,

        /// <summary>
        /// Write access right to an object
        /// </summary>
        WriteData = 0x00000002,

        /// <summary>
        /// For a file object, the right to append data to the file.
        /// </summary>
        AppendData = 0x00000004,

        /// <summary>
        /// The right to read extended file attributes.
        /// </summary>
        ReadExtendedAttributes = 0x00000008,

        /// <summary>
        /// The right to write extended file attributes.
        /// </summary>
        WriteExtendedAttributes = 0x00000010,

        /// <summary>
        /// For a native code file, the right to execute the file.
        /// This access right given to scripts may cause the script to be executable, depending on the script interpreter.
        /// </summary>
        Execute = 0x00000020,

        /// <summary>
        /// The right to read file attributes.
        /// </summary>
        ReadAttributes = 0x00000080,

        /// <summary>
        /// The right to write file attributes.
        /// </summary>
        WriteAttributes = 0x00000100,

        /// <summary>
        /// The right to delete the object.
        /// </summary>
        Delete = 0x00010000,

        /// <summary>
        /// The right to read the information in the object's security descriptor, 
        /// not including the information in the system access control list (SACL).
        /// </summary>
        ReadPermissions = 0x00020000,

        /// <summary>
        /// The right to modify the discretionary access control list (DACL) in 
        /// the object's security descriptor.
        /// </summary>
        ChangePermissions = 0x00040000,

        /// <summary>
        /// The right to change the owner in the object's security descriptor.
        /// </summary>
        SetOwnership = 0x00080000,

        /// <summary>
        /// The right to use the object for synchronization. 
        /// This enables a thread to wait until the object is in the signaled state. 
        /// Some object types do not support this access right.
        /// </summary>
        Synchronize = 0x00100000,

        /// <summary>
        /// Access system security (ACCESS_SYSTEM_SECURITY). 
        /// It is used to indicate access to a system access control list (SACL). 
        /// This type of access requires the calling process to have the SE_SECURITY_NAME 
        /// (Manage auditing and security log) privilege. If this flag is set in 
        /// the access mask of an audit access ACE (successful or unsuccessful access), 
        /// the SACL access will be audited.
        /// </summary>
        Reserved = 0x01000000,

        //MaximumAllowed        = 0x02000000,
        //GenericAll            = 0x10000000,

        /// <summary>
        /// Generic execute access
        /// </summary>
        GenericExecute = 0x20000000,

        /// <summary>
        /// Generic write access
        /// </summary>
        GenericWrite = 0x40000000,

        /// <summary>
        /// Generic read access
        /// </summary>
        GenericRead = 0x80000000,
    }
}