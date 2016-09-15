using System;

namespace DokanNet
{
    /// <summary>
    /// Defines standard, specific, and generic rights. 
    /// These rights are used in access control entries (ACEs) and are the primary means of 
    /// specifying the requested or granted access to an object. 
    /// </summary>
    /// <remarks>
    /// This extends the <c><see cref="System.IO.FileAccess"/></c> enumerator in .NET that only 
    /// contains flags for <c>Read</c> (<c>0x01</c>) and <c>Write</c> (<c>0x10</c>).
    /// </remarks>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa374896(v=vs.85).aspx">Access Mask Format (MSDN)</a>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa374892(v=vs.85).aspx">ACCESS_MASK (MSDN)</a>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/4z36sx0f.aspx">FileAccess Enumeration (MSDN)</a>
    [Flags]
    public enum FileAccess : long
    {
        /// <summary>
        /// Read access right to an object.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_READ_DATA,0x00000001,File & pipe}
        /// \nativeconst{FILE_LIST_DIRECTORY,0x00000001,Directory}
        /// \endtable
        /// \endnative
        ReadData = 1,

        /// <summary>
        /// Write access right to an object.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_WRITE_DATA,0x00000002,File & pipe}
        /// \nativeconst{FILE_ADD_FILE,0x00000002,Directory}
        /// \endtable
        /// \endnative
        WriteData = 1L << 1,

        /// <summary>
        /// For a file object, the right to append data to the file.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_APPEND_DATA,0x00000004,File}
        /// \nativeconst{FILE_ADD_SUBDIRECTORY,0x00000004,Directory}
        /// \nativeconst{FILE_CREATE_PIPE_INSTANCE,0x00000004,Named pipe}
        /// \endtable
        /// \endnative
        AppendData = 1L << 2,

        /// <summary>
        /// The right to read extended file attributes.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_READ_EA,0x00000008,File & directory}
        /// \endtable
        /// \endnative
        ReadExtendedAttributes = 1L << 3,

        /// <summary>
        /// The right to write extended file attributes.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_WRITE_EA,0x00000010,File & directory}
        /// \endtable
        /// \endnative
        WriteExtendedAttributes = 1L << 4,

        /// <summary>
        /// For a native code file, the right to execute the file.
        /// This access right given to scripts may cause the script to be executable, depending on the script interpreter.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_EXECUTE,0x00000020,File}
        /// \nativeconst{FILE_TRAVERSE,0x00000020,Directory}
        /// \endtable
        /// \endnative
        Execute = 1L << 5,

        /// <summary>
        /// For a directory, the right to delete a directory and all the files it contains, including read-only files.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_DELETE_CHILD,0x00000040,Directory}
        /// \endtable
        /// \endnative
        DeleteChild = 1L << 6,

        /// <summary>
        /// The right to read file attributes.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_READ_ATTRIBUTES,0x00000080,All}
        /// \endtable
        /// \endnative
        ReadAttributes = 1L << 7,

        /// <summary>
        /// The right to write file attributes.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{FILE_WRITE_ATTRIBUTES,0x00000100,All}
        /// \endtable
        /// \endnative
        WriteAttributes = 1L << 8,

        /// <summary>
        /// The right to delete the object.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{DELETE,0x00010000}
        /// \endtable
        /// \endnative
        Delete = 1L << 16,

        /// <summary>
        /// The right to read the information in the object's security descriptor, 
        /// not including the information in the system access control list (SACL).
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{READ_CONTROL,0x00020000}
        /// \endtable
        /// \endnative
        ReadPermissions = 1L << 17,

        /// <summary>
        /// The right to modify the discretionary access control list (DACL) in 
        /// the object's security descriptor.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{WRITE_DAC,0x00040000}
        /// \endtable
        /// \endnative
        ChangePermissions = 1L << 18,

        /// <summary>
        /// The right to change the owner in the object's security descriptor.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{WRITE_OWNER,0x00080000}
        /// \endtable
        /// \endnative
        SetOwnership = 1L << 19,

        /// <summary>
        /// The right to use the object for synchronization. 
        /// This enables a thread to wait until the object is in the signaled state. 
        /// Some object types do not support this access right.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{SYNCHRONIZE,0x00100000}
        /// \endtable
        /// \endnative
        Synchronize = 1L << 20,

        /// <summary>
        /// Obsolete, use <see cref="FileAccess.AccessSystemSecurity"/> instead. 
        /// </summary>
        [Obsolete("Use AccessSystemSecurity instead")]
        Reserved = AccessSystemSecurity,

        /// <summary>
        /// Access system security. 
        /// It is used to indicate access to a system access control list (SACL). 
        /// This type of access requires the calling process to have the <c>SE_SECURITY_NAME</c> 
        /// (Manage auditing and security log) privilege. If this flag is set in 
        /// the access mask of an audit access ACE (successful or unsuccessful access), 
        /// the SACL access will be audited.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{ACCESS_SYSTEM_SECURITY,0x01000000}
        /// \endtable
        /// \endnative
        AccessSystemSecurity = 1L << 24,

        /// <summary>
        /// All the access rights that are valid for the caller.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{MAXIMUM_ALLOWED,0x02000000}
        /// \endtable
        /// \endnative
        MaximumAllowed = 1L << 25,

        /// <summary>
        /// All possible access rights.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{GENERIC_ALL,0x10000000}
        /// \endtable
        /// \endnative
        GenericAll = 1L << 28,

        /// <summary>
        /// Generic execute access. 
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{GENERIC_EXECUTE,0x20000000}
        /// \endtable
        /// \endnative
        GenericExecute = 1L << 29,

        /// <summary>
        /// Generic write access.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{GENERIC_WRITE,0x40000000}
        /// \endtable
        /// \endnative
        GenericWrite = 1L << 30,

        /// <summary>
        /// Generic read access.
        /// </summary>
        /// \native
        /// \table
        /// \nativeconst{GENERIC_READ,0x80000000}
        /// \endtable
        /// \endnative
        GenericRead = 1L << 31
    }
}