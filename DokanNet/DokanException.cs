using System;
using DokanNet.Properties;

namespace DokanNet
{
    /// <summary>
    /// The dokan exception.
    /// </summary>
    [Serializable]
    public class DokanException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DokanException"/> class with a <see cref="Exception.HResult"/>.
        /// </summary>
        /// <param name="status">
        /// The error status also written to <see cref="Exception.HResult"/>.
        /// </param>
        internal DokanException(DokanStatus status)
            : this(status, GetStatusErrorMessage(status)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DokanException"/> class with a <see cref="Exception.HResult"/>.
        /// </summary>
        /// <param name="status">
        /// The error status also written to <see cref="Exception.HResult"/>.
        /// </param>
        /// <param name="message">
        /// The error message.
        /// </param>
        internal DokanException(DokanStatus status, string message)
            : base(message)
        {
            ErrorStatus = status;
            HResult = (int)status;
        }

        private static string GetStatusErrorMessage(DokanStatus status)
        {
            switch (status)
            {
                case DokanStatus.Error:
                    return Resources.ErrorDokan;
                case DokanStatus.DriveLetterError:
                    return Resources.ErrorBadDriveLetter;
                case DokanStatus.DriverInstallError:
                    return Resources.ErrorDriverInstall;
                case DokanStatus.MountError:
                    return Resources.ErrorAssignDriveLetter;
                case DokanStatus.StartError:
                    return Resources.ErrorStart;
                case DokanStatus.MountPointError:
                    return Resources.ErrorMountPointInvalid;
                case DokanStatus.VersionError:
                    return Resources.ErrorVersion;
                default:
                    return Resources.ErrorUnknown;
            }
        }

        public DokanStatus ErrorStatus { get; private set; }
    }
}