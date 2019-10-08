using System;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Dokan mount options used to describe dokan device behavior. 
    /// </summary>
    /// \if PRIVATE
    /// <seealso cref="DOKAN_OPTIONS.Options"/>
    /// \endif
    [Flags]
    public enum DokanOptions : long
    {
        /// <summary>Fixed Drive.</summary>
        FixedDrive = 0,

        /// <summary>Enable output debug message.</summary>
        DebugMode = 1,

        /// <summary>Enable output debug message to stderr.</summary>
        StderrOutput = 2,

        /// <summary>Use alternate stream.</summary>
        AltStream = 4,

        /// <summary>Enable mount drive as write-protected.</summary>
        WriteProtection = 8,

        /// <summary>Use network drive - Dokan network provider need to be installed.</summary>
        NetworkDrive = 16,

        /// <summary>Use removable drive.</summary>
        RemovableDrive = 32,

        /// <summary>Use mount manager.</summary>
        MountManager = 64,

        /// <summary>Mount the drive on current session only.</summary>
        CurrentSession = 128,

        /// <summary>Enable Lockfile/Unlockfile operations.</summary>
        UserModeLock = 256,

        /// <summary>
        /// Whether DokanNotifyXXX functions should be enabled, which requires this
        /// library to maintain a special handle while the file system is mounted.
        /// Without this flag, the functions always return FALSE if invoked.
        /// </summary>
        EnableNotificationAPI = 512,

        /// <summary>Whether to disable any oplock support on the volume.</summary>
        /// <remarks>Regular range locks are enabled regardless.</remarks>
        DisableOplocks = 1024,

        /// <summary>
        /// Whether Dokan should satisfy a single-entry, name-only directory search
        /// without dispatching to <see cref="IDokanOperations.FindFiles(string, out System.Collections.Generic.IList{FileInformation}, IDokanFileInfo)"/>,
        /// if there is already an open file from which the driver can just copy the
        /// normalized name.  These searches are frequently done inside of CreateFile
        /// calls on Windows 7.
        /// </summary>
        OptimizeSingleNameSearch = 2048,
    }
}
