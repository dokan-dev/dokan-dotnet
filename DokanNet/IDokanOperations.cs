using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        /// <summary>
        /// Called when a file or directory i to be created.
        /// 
        /// In case OPEN_ALWAYS & CREATE_ALWAYS are opening successfully a already
        /// existing file, you have to SetLastError(<see cref="NtStatus.ObjectNameCollision"/>).
        /// 
        /// If file is a directory, CreateFile (not OpenDirectory) may be called.
        /// In this case, CreateFile should return <see cref="NtStatus.Success"/> when that directory
        /// can be opened.
        /// 
        /// You should set TRUE on <see cref="DokanFileInfo.IsDirectory"/> when file is a directory.
        /// </summary>
        /// <param name="fileName">File or directory name</param>
        /// <param name="access">A <see cref="FileAccess"/> with permissions for file or directory.</param>
        /// <param name="share">Type of share access to other threads, which is specified as <see cref="FileShare.None"/> 
        /// or any combination of <see cref="FileShare"/>. 
        /// 
        /// Device and intermediate drivers usually set ShareAccess to zero, which gives the caller 
        /// exclusive access to the open file.</param>
        /// <param name="mode">Specifies how the operating system should open a file. See https://msdn.microsoft.com/en-us/library/system.io.filemode(v=vs.110).aspx </param>
        /// <param name="options">Represents advanced options for creating a FileStream object. See https://msdn.microsoft.com/en-us/library/system.io.fileoptions(v=vs.110).aspx </param>
        /// <param name="attributes">Provides attributes for files and directories. See https://msdn.microsoft.com/en-us/library/system.io.fileattributes(v=vs.110).aspx </param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns></returns>
        NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                              FileOptions options, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// TODO
        /// </summary>
        /// <remarks>
        /// When <see cref="DokanFileInfo.DeleteOnClose"/> is true, you must delete the file in <see cref="Cleanup"/>.
        /// Refer to comment at <see cref="DeleteFile"/> for explanation.
        /// </remarks>
        /// <param name="fileName">File or directory name</param>
        /// <param name="info">A <see cref="DokanFileInfo"/></param>
        void Cleanup(string fileName, DokanFileInfo info);

        void CloseFile(string fileName, DokanFileInfo info);

        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
                            DokanFileInfo info);

        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten,
                             long offset, DokanFileInfo info);

        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info);

        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, DokanFileInfo info);

        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                               DateTime? lastWriteTime, DokanFileInfo info);

        /// <summary>
        /// Check if it is possible to delete a file or directory.
        /// </summary>
        /// <remarks>
        /// You should not delete the file on <see cref="DeleteFile"/> or <see cref="DeleteDirectory"/>, but instead
        /// you must only check whether you can delete the file or not,
        /// and return <see cref="NtStatus.Success"/> (when you can delete it) or appropriate error
        /// codes such as <see cref="NtStatus.AccessDenied"/>, <see cref="NtStatus.ObjectPathNotFound"/>,
        /// <see cref="NtStatus.ObjectNameNotFound"/>.
        /// When you return <see cref="NtStatus.Success"/>, you get a <see cref="Cleanup "/> call afterwards with
        /// <see cref="DokanFileInfo.DeleteOnClose"/> set to TRUE and only then you have to actually
        /// delete the file being closed.
        /// </remarks>
        /// <param name="fileName">File name</param>
        /// <param name="info">A <see cref="DokanFileInfo"/></param>
        /// <returns>An <see cref="NtStatus"/></returns>
        NtStatus DeleteFile(string fileName, DokanFileInfo info);

        NtStatus DeleteDirectory(string fileName, DokanFileInfo info);

        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info);

        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info);

        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
                                    DokanFileInfo info);

        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                        out string fileSystemName, DokanFileInfo info);

        NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatus Mounted(DokanFileInfo info);

        NtStatus Unmounted(DokanFileInfo info);

        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info);
    }
}