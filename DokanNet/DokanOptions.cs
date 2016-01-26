using System;

namespace DokanNet
{
    [Flags]
    public enum DokanOptions : long
    {
        DebugMode = 1, // ouput debug message
        StderrOutput = 2, // ouput debug message to stderr
        AltStream = 4, // use alternate stream
        WriteProtection = 8, // mount drive as write-protected.
        NetworkDrive = 16, // use network drive, you need to install Dokan network provider.
        RemovableDrive = 32, // use removable drive
        MountManager = 64, // use mount manager
        FixedDrive = 0,
    }
}