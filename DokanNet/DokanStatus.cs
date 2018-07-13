using System;
using System.Collections.Generic;
using System.Text;

namespace DokanNet
{
    /// <summary>
    /// Error codes returned by DokanMain.
    /// </summary>
    public enum DokanStatus : int
    {
        /// <summary>
        /// Dokan mount succeed.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Dokan mount error.
        /// </summary>
        Error = -1,

        /// <summary>
        /// Dokan mount failed - Bad drive letter.
        /// </summary>
        DriveLetterError = -2,

        /// <summary>
        /// Dokan mount failed - Can't install driver.
        /// </summary>
        DriverInstallError = -3,

        /// <summary>
        /// Dokan mount failed - Driver answer that something is wrong.
        /// </summary>
        StartError = -4,

        /// <summary>
        /// Dokan mount failed.
        /// Can't assign a drive letter or mount point.
        /// Probably already used by another volume.
        /// </summary>
        MountError = -5,

        /// <summary>
        /// Dokan mount failed.
        /// Mount point is invalid.
        /// </summary>
        MountPointError = -6,

        /// <summary>
        /// Dokan mount failed.
        /// Requested an incompatible version.
        /// </summary>
        VersionError = -7
    }
}
