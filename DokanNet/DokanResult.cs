namespace DokanNet
{
    /// <summary>
    /// Defines common result status codes in <see cref="NtStatus"/> for %Dokan
    /// operations.
    /// </summary>
    public static class DokanResult
    {
        /// <summary>
        /// Success - The operation completed successfully.
        /// </summary>
        public const NtStatus Success = NtStatus.Success;

        /// <summary>
        /// Error - Incorrect function.
        /// </summary>
        public const NtStatus Error = NtStatus.Error;

        /// <summary>
        /// Error - The system cannot find the file specified.
        /// </summary>
        public const NtStatus FileNotFound = NtStatus.ObjectNameNotFound;

        /// <summary>
        /// Error - The system cannot find the path specified.
        /// </summary>
        public const NtStatus PathNotFound = NtStatus.ObjectPathNotFound;

        /// <summary>
        /// Error - Access is denied.
        /// </summary>
        public const NtStatus AccessDenied = NtStatus.AccessDenied;

        /// <summary>
        /// Error - The handle is invalid.
        /// </summary>
        public const NtStatus InvalidHandle = NtStatus.InvalidHandle;

        /// <summary>
        /// Warning - The device is not ready.
        /// </summary>
        public const NtStatus NotReady = NtStatus.DeviceBusy;

        /// <summary>
        /// Error - The process cannot access the file because it is being used
        /// by another process.
        /// </summary>
        public const NtStatus SharingViolation = NtStatus.SharingViolation;

        /// <summary>
        /// Error - The file exists.
        /// </summary>
        public const NtStatus FileExists = NtStatus.ObjectNameCollision;

        /// <summary>
        /// Error - There is not enough space on the disk.
        /// </summary>
        public const NtStatus DiskFull = NtStatus.DiskFull;

        /// <summary>
        /// Error - This function is not supported on this system.
        /// </summary>
        public const NtStatus NotImplemented = NtStatus.NotImplemented;

        /// <summary>
        /// Error - The data area passed to a system call is too small.
        /// </summary>
        public const NtStatus BufferTooSmall = NtStatus.BufferTooSmall;

        /// <summary>
        /// Warning - The data area passed to a system call is too small.
        /// </summary>
        public const NtStatus BufferOverflow = NtStatus.BufferOverflow;

        /// <summary>
        /// Error - The filename, directory name, or volume label syntax is
        /// incorrect.
        /// </summary>
        public const NtStatus InvalidName = NtStatus.ObjectNameInvalid;

        /// <summary>
        /// Error - The directory is not empty.
        /// </summary>
        public const NtStatus DirectoryNotEmpty = NtStatus.DirectoryNotEmpty;

        /// <summary>
        /// Error - Cannot create a file when that file already exists.
        /// </summary>
        public const NtStatus AlreadyExists = NtStatus.ObjectNameCollision;

        /// <summary>
        /// Error - An exception occurred in the service when handling the
        /// control request.
        /// </summary>
        public const NtStatus InternalError = NtStatus.InternalError;

        /// <summary>
        /// Error - A required privilege is not held by the client.
        /// </summary>
        public const NtStatus PrivilegeNotHeld = NtStatus.PrivilegeNotHeld;

        /// <summary>
        /// Error - The requested operation was unsuccessful.
        /// </summary>
        public const NtStatus Unsuccessful = NtStatus.Unsuccessful;

        /// <summary>
        /// Error - The parameter is incorrect.
        /// </summary>
        public const NtStatus InvalidParameter = NtStatus.InvalidParameter;
    }
}