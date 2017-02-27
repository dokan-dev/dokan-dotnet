using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DokanNet.Native
{
    /// <summary>
    /// Contains information about the file that is found by the FindFirstFile, FindFirstFileEx, or FindNextFile function.
    /// </summary>
    /// <remarks>
    /// If a file has a long file name, the complete name appears in the cFileName member, and the 8.3 format truncated version
    /// of the name appears in the <see cref="cAlternateFileName"/>member. Otherwise,<see cref="cAlternateFileName"/> is empty. If the FindFirstFileEx function
    /// was called with a value of FindExInfoBasic in the fInfoLevelId parameter, the <see cref="cAlternateFileName"/> member will always contain
    /// a NULL string value. This remains true for all subsequent calls to the FindNextFile function. As an alternative method of
    /// retrieving the 8.3 format version of a file name, you can use the GetShortPathName function. For more information about
    /// file names, see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx">Naming Files, Paths, and Namespaces (MSDN)</a>.
    /// 
    /// Not all file systems can record creation and last access times, and not all file systems record them in the same manner.
    /// For example, on the FAT file system, create time has a resolution of 10 milliseconds, write time has a resolution of
    /// 2 seconds, and access time has a resolution of 1 day. The NTFS file system delays updates to the last access time for
    /// a file by up to 1 hour after the last access.For more information, see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724290(v=vs.85).aspx">File Times (MSDN)</a>.
    /// </remarks>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365740%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396">WIN32_FIND_DATA structure (MSDN)</a>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct WIN32_FIND_DATA
    {
        /// <summary>
        /// The file attributes of a file.
        /// 
        /// For possible values and their descriptions, see <see cref="FileAttributes"/>.
        /// The <see cref="FileAttributes.SparseFile"/> attribute on the file is set if any of 
        /// the streams of the file have ever been sparse.
        /// </summary>
        public FileAttributes dwFileAttributes;

        /// <summary>
        /// A <see cref="FILETIME"/> structure that specifies when a file or directory was created.
        /// If the underlying file system does not support creation time, this member is zero.
        /// </summary>
        public FILETIME ftCreationTime;

        /// <summary>
        /// A <see cref="FILETIME"/> structure.
        /// 
        /// For a file, the structure specifies when the file was last read from, written to, or for executable files, run.
        /// 
        /// For a directory, the structure specifies when the directory is created.
        /// If the underlying file system does not support last access time, this member is zero.
        /// 
        /// On the FAT file system, the specified date for both files and directories is correct, 
        /// but the time of day is always set to midnight.
        /// </summary>
        public FILETIME ftLastAccessTime;

        /// <summary>
        /// A <see cref="FILETIME"/> structure.
        /// 
        /// For a file, the structure specifies when the file was last written to, 
        /// truncated, or overwritten, for example, when WriteFile or SetEndOfFile 
        /// are used. The date and time are not updated when file attributes or 
        /// security descriptors are changed.
        /// 
        /// For a directory, the structure specifies when the directory is created.
        /// If the underlying file system does not support last write time, 
        /// this member is zero.
        /// </summary>
        public FILETIME ftLastWriteTime;

        /// <summary>
        /// The high-order DWORD value of the file size, in bytes.
        /// 
        /// This value is zero unless the file size is greater than MAXDWORD.
        /// 
        /// The size of the file is equal to (nFileSizeHigh* (MAXDWORD+1)) + nFileSizeLow.
        /// </summary>
        public uint nFileSizeHigh;

        /// <summary>
        /// The low-order DWORD value of the file size, in bytes.
        /// </summary>
        public uint nFileSizeLow;

        /// <summary>
        /// If the <see cref="dwFileAttributes"/> member includes the <see cref="FileAttributes.ReparsePoint"/> attribute, 
        /// this member specifies the reparse point tag. 
        /// Otherwise, this value is undefined and should not be used.
        /// </summary>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365511(v=vs.85).aspx">Reparse Point Tags (MSDN)</a>
        private readonly uint dwReserved0;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        private readonly uint dwReserved1;

        /// <summary>
        /// The name of the file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;

        /// <summary>
        /// An alternative name for the file.
        /// This name is in the classic 8.3 file name format.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        private readonly string cAlternateFileName;
    }
}