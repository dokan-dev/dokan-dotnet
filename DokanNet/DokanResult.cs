namespace DokanNet
{
	/// <summary>
	/// Defines result status codes for Dokan operations.
	/// </summary>
	public enum DokanResult
	{
		/// <summary>
		/// The operation completed successfully.
		/// </summary>
		Success = 0,

		/// <summary>
		/// Incorrect function.
		/// </summary>
		Error = -1,
		
		/// <summary>
		/// The system cannot find the file specified.
		/// </summary>
		FileNotFound = -2,

		/// <summary>
		/// The system cannot find the path specified.
		/// </summary>
		PathNotFound = -3,

		/// <summary>
		/// Access is denied.
		/// </summary>
		AccessDenied = -5,

		/// <summary>
		/// The device is not ready.
		/// </summary>
		NotReady = -21,
		
		/// <summary>
		/// The process cannot access the file because it is being used by another process.
		/// </summary>
		SharingViolation = -32,

		/// <summary>
		/// The file exists.
		/// </summary>
		FileExists = -80,

		/// <summary>
		/// There is not enough space on the disk.
		/// </summary>
		DiskFull = -112,

		/// <summary>
		/// This function is not supported on this system.
		/// </summary>
		NotImplemented = -120,
		
		/// <summary>
		/// The filename, directory name, or volume label syntax is incorrect.
		/// </summary>
		InvalidName = -123,

		/// <summary>
		/// The directory is not empty.
		/// </summary>
		DirNotEmpty = -145,

		/// <summary>
		/// Cannot create a file when that file already exists.
		/// </summary>
		AlreadyExists = -183,
		
		/// <summary>
		/// An exception occurred in the service when handling the control request.
		/// </summary>
		ExceptionInService = -1064,

		/// <summary>
		/// A required privilege is not held by the client.
		/// </summary>
		PrivilegeNotHeld = -1314,

		Undefined = int.MaxValue
	}
}