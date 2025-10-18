using System;
using System.Drawing;
using DokanNet.Native;

namespace DokanNet;

/// <summary>
/// Dokan mount options used to describe dokan device behavior. 
/// </summary>
/// \if PRIVATE
/// <seealso cref="DOKAN_OPTIONS.Options"/>
/// \endif
[Flags]
public enum DokanOptions : uint
{
    /// <summary>Fixed Drive.</summary>
    FixedDrive = 0,

    /// <summary>Enable output debug message.</summary>
    DebugMode = 1,

    /// <summary>Enable output debug message to stderr.</summary>
    StderrOutput = (1 << 1),

    /// <summary>Enable the use of alternate stream paths in the form
    /// [file-name]:[stream-name]. If this is not specified then the driver will
    /// fail any attempt to access a path with a colon.</summary>
    AltStream = (1 << 2),

    /// <summary>Enable mount drive as write-protected.</summary>
    WriteProtection = (1 << 3),

    /// <summary>Use network drive - Dokan network provider needs to be installed and a \ref DOKAN_OPTIONS.UNCName provided.</summary>
    NetworkDrive = (1 << 4),

    /// <summary>Use removable drive
    /// Be aware that on some environments, the userland application will be denied
    /// to communicate with the drive which will result in a unwanted unmount.
    /// <a href="https://github.com/dokan-dev/dokany/issues/843">Issue #843</a>.</summary>
    RemovableDrive = (1 << 5),

    /// <summary>Use Windows Mount Manager.
    /// This option is highly recommended to use for better system integration
    /// If a drive letter is used but is busy, Mount manager will assign one for us and 
    /// <see cref="IDokanOperations2.Mounted" /> parameters will contain the new mount point.</summary>
    MountManager = (1 << 6),

    /// <summary>Mount the drive on current session only
    /// Note: As Windows process only have on sessionID which is here used to define what is the current session,
    /// impersonation will not work if someone attend to mount for a user from another one (like system service).
    /// <a href="https://github.com/dokan-dev/dokany/issues/1196">Issue #1196</a>.</summary>
    CurrentSession = (1 << 7),

    /// <summary>Enable Lockfile/Unlockfile operations. Otherwise Dokan will take care of it.</summary>
    UserModeLock = (1 << 8),

    /// <summary>
    /// Enable Case sensitive path.
    /// By default all path are case insensitive.
    /// For case sensitive: \\dir\\File and \\diR\\file are different files
    /// but for case insensitive they are the same.
    /// </summary>
    CaseSensitive = (1 << 9),

    /// <summary>
    /// Allows unmounting of network drive via explorer
    /// </summary>
    EnableNetworkUnmount = (1 << 10),

    /// <summary>
    /// Forward the kernel driver global and volume logs to the userland.
    /// Can be very slow if single thread is enabled.
    /// </summary>
    DispatchDriverLogs = (1 << 11),

    /// <summary>
    /// Pull batches of events from the driver instead of a single one and execute them parallelly.
    /// This option should only be used on computers with low cpu count
    /// and userland filesystem taking time to process requests (like remote storage).
    /// </summary>
    AllowIpcBatching = (1 << 12),
}
