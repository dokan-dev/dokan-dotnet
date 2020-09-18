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
        /// Enable methods in <see cref="Dokan.Notify"/>, which require this library to maintain a special
        /// handle while the file system is mounted.
        /// Without this flag, the methods in that inner class always return false if invoked.
        /// </summary>
        EnableNotificationAPI = 512,

        /// <summary>Whether to disable any oplock support on the volume.</summary>
        /// <remarks>Regular range locks are enabled regardless.</remarks>
        DisableOplocks = 1024,

        /// <summary>
        /// Enable Fcb Garbage collection
        /// The advantage of the GC approach is that it prevents filter drivers from exponentially
        /// slowing down procedures like zip file extraction due to
        /// repeatedly rebuilding state that they attach to the FCB header.
        /// </summary>
        EnableFCBGC = 2048,

        /// <summary>
        /// Enable Case sensitive path.
        /// By default all path are case insensitive.
        /// For case sensitive: \dir\File & \diR\file are different files
        /// but for case insensitive they are the same.
        /// </summary>
        CaseSensitive = 4096,

        /// <summary>
        /// Enables unmounting of network drives via file explorer
        /// </summary>
        EnableNetworkUnmount = 8192,
    }
}
