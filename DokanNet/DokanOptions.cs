using System;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Dokan mount options used to describe dokan device behavior. 
    /// </summary>
    /// <seealso cref="DOKAN_OPTIONS.Options"/>
    [Flags]
    public enum DokanOptions : long
    {
        /// <summary>Fixed Drive.</summary>
        FixedDrive = 0,

        /// <summary>Enable ouput debug message</summary>
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

        /// <summary>Enable Lockfile/Unlockfile operations</summary>
        UserModeLock = 256
    }
}