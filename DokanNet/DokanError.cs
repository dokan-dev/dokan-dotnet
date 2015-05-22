namespace DokanNet
{
    public enum DokanError
    {
        // From WinError.h -> http://msdn.microsoft.com/en-us/library/ms819773.aspx
        ErrorFileNotFound = -2, // MessageText: The system cannot find the file specified.
        ErrorPathNotFound = -3, // MessageText: The system cannot find the path specified.
        ErrorAccessDenied = -5, // MessageText: Access is denied.
        ErrorSharingViolation = -32,
        ErrorFileExists = -80,
        ErrorDiskFull = -112, // There is not enough space on the disk.
        ErrorInvalidName = -123,
        ErrorDirNotEmpty = -145, // MessageText: The directory is not empty.

        ErrorAlreadyExists = -183,
        // MessageText: Cannot create a file when that file already exists.

        ErrorExceptionInService = -1064,
        //  An exception occurred in the service when handling thecontrol request. 
        ErrorSuccess = 0,
        ErrorError = -1,
        ErrorNotImplemented = -120,

        ErrorPrivilegeNotHeld = -1314,
        ErrorNotReady         =         -21,


    }
}