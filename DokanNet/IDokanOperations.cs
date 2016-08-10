using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        /// <summary>
        /// CreateFile is called each time a request is made on a file.
        /// 
        /// In case <see cref="FileMode.OpenOrCreate"/> & <see cref="FileMode.Create"/> are opening successfully a already
        /// existing file, you have to SetLastError(ERROR_ALREADY_EXISTS).
        /// 
        /// You should set TRUE on <see cref="DokanFileInfo.IsDirectory"/> when file is a directory.
        /// 
        /// <see cref="DokanFileInfo.Context"/> can be use to store Data (like FileStream)
        /// that can be retrieved in all other request related to the Context
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="access">A <see cref="FileAccess"/> with permissions for file or directory.</param>
        /// <param name="share">Type of share access to other threads, which is specified as
        /// <see cref="FileShare.None"/> or any combination of <see cref="FileShare"/>.
        /// Device and intermediate drivers usually set ShareAccess to zero,
        /// which gives the caller exclusive access to the open file.</param>
        /// <param name="mode">Specifies how the operating system should open a file. See https://msdn.microsoft.com/en-us/library/system.io.filemode(v=vs.110).aspx </param>
        /// <param name="options">Represents advanced options for creating a FileStream object. See https://msdn.microsoft.com/en-us/library/system.io.fileoptions(v=vs.110).aspx </param>
        /// <param name="attributes">Provides attributes for files and directories. See https://msdn.microsoft.com/en-us/library/system.io.fileattributes(v=vs.110).aspx </param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
            FileOptions options, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// Cleanup request before CloseFile is called.
        /// </summary>
        /// <remarks>
        /// When <see cref="DokanFileInfo.DeleteOnClose"/> is true, you must delete the file in <see cref="Cleanup"/>.
        /// Refer to comment at <see cref="DeleteFile"/> for explanation.
        /// </remarks>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        void Cleanup(string fileName, DokanFileInfo info);

        /// <summary>
        /// CloseFile is called at the end of the life of the Context.
        /// Context has to be clear before return.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        void CloseFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// Read event on the file previously opened in <see cref="CreateFile"/>
        /// It can be called by different thread at the same time.
        /// Therefor the read has to be thread safe.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="buffer">Read buffer that has to be fill with the read result.
        /// The buffer size depend of the read size requested by the kernel.</param>
        /// <param name="bytesRead">Total byte that has been read</param>
        /// <param name="offset">Offset from where the read has to be proceed</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
            DokanFileInfo info);

        /// <summary>
        /// Write event on the file previously opened in <see cref="CreateFile"/>
        /// WriteFile can be called by different thread at the same time.
        /// Therefor the write has to be thread safe.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="buffer">Data that has to be written</param>
        /// <param name="bytesWritten">Total byte that has been write</param>
        /// <param name="offset">Offset from where the write has to be proceed</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten,
            long offset, DokanFileInfo info);

        /// <summary>
        /// Clears buffers for this context and causes any buffered data to be written to the file.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info);

        /// <summary>
        /// Get specific informations on a file.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="fileInfo"><see cref="FileInformation"/> struct to fill</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        /// <summary>
        /// List all files in the path requested
        /// 
        /// <see cref="FindFilesWithPattern"/> is checking first. If it is not implemented or
        /// returns <see cref="NtStatus.NotImplemented"/>, then FindFiles is called, if implemented.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="files"><see cref="FileInformation"/> list to fill</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        /// <summary>
        /// Same as <see cref="FindFiles"/> but with a search pattern
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="files"><see cref="FileInformation"/> list to fill</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
            DokanFileInfo info);

        /// <summary>
        /// Set file attributes on a specific file
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="attributes">FileAttributes to set on file</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// Set file times on a specific file.
        /// If DateTime is null, this should not be updated.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="creationTime">Creation DateTime</param>
        /// <param name="lastWriteTime">LastAccessTime DateTime</param>
        /// <param name="lastAccessTime">LastWrite DateTime</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
            DateTime? lastWriteTime, DokanFileInfo info);

        /// <summary>
        /// Check if it is possible to delete a file.
        /// The file should not be delete in DeleteFile but in <see cref="Cleanup"/>.
        /// </summary>
        /// <remarks>
        /// You should not delete the file on <see cref="DeleteFile"/> or <see cref="DeleteDirectory"/>, but instead
        /// you must only check whether you can delete the file or not,
        /// and return <see cref="NtStatus.Success"/> (when you can delete it) or appropriate error
        /// codes such as <see cref="NtStatus.AccessDenied"/>, <see cref="NtStatus.ObjectPathNotFound"/>,
        /// <see cref="NtStatus.ObjectNameNotFound"/>.
        /// When you return <see cref="NtStatus.Success"/>, you get a <see cref="Cleanup"/> call afterwards with
        /// <see cref="DokanFileInfo.DeleteOnClose"/> set to TRUE and only then you have to actually
        /// delete the file being closed.
        /// </remarks>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="DokanResult.Success"/> if file can be delete or <see cref="NtStatus"/> appropriate.</returns>
        NtStatus DeleteFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// Check if it is possible to delete a directory.
        /// The file should not be delete in DeleteFile but in <see cref="Cleanup"/>.
        /// </summary>
        /// <remarks>
        /// You should not delete the file on <see cref="DeleteFile"/> or <see cref="DeleteDirectory"/>, but instead
        /// you must only check whether you can delete the file or not,
        /// and return <see cref="NtStatus.Success"/> (when you can delete it) or appropriate error
        /// codes such as <see cref="NtStatus.AccessDenied"/>, <see cref="NtStatus.ObjectPathNotFound"/>,
        /// <see cref="NtStatus.ObjectNameNotFound"/>.
        /// When you return <see cref="NtStatus.Success"/>, you get a <see cref="Cleanup"/> call afterwards with
        /// <see cref="DokanFileInfo.DeleteOnClose"/> set to TRUE and only then you have to actually
        /// delete the file being closed.
        /// </remarks>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="DokanResult.Success"/> if file can be delete or <see cref="NtStatus"/> appropriate.</returns>
        NtStatus DeleteDirectory(string fileName, DokanFileInfo info);

        /// <summary>
        /// Move a file or directory to his new destination
        /// </summary>
        /// <param name="oldName">Source file path to move.</param>
        /// <param name="newName">Destination file path</param>
        /// <param name="replace">Can replace or not if destination already exist</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        /// <summary>
        /// SetEndOfFile is used to truncate or extend a file (physical file size).
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// SetAllocationSize is used to truncate or extend a file.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// Lock file at a specific offset and data length.
        /// This is only used if DokanOptions.UserModeLock is enabled.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="offset">Offset from where the lock has to be proceed</param>
        /// <param name="length">Data length to lock</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// Unlock file at a specific offset and data length.
        /// This is only used if DokanOptions.UserModeLock is enabled.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="offset">Offset from where the unlock has to be proceed</param>
        /// <param name="length">Data length to lock</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// GetDiskFreeSpace request device space informations
        /// </summary>
        /// <param name="freeBytesAvailable">Amount of available space.</param>
        /// <param name="totalNumberOfBytes">Total size of storage space.</param>
        /// <param name="totalNumberOfFreeBytes">Amount of free space.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes,
            out long totalNumberOfFreeBytes,
            DokanFileInfo info);

        /// <summary>
        /// GetVolumeInformation request volume informations
        /// </summary>
        /// <param name="volumeLabel">Volume name</param>
        /// <param name="features">Features enabled on the volume.</param>
        /// <param name="fileSystemName">FileSystem name.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info);

        /// <summary>
        /// Get ACL information on the requested file.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="security">Security information result.</param>
        /// <param name="sections">Access sections requested.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Set ACL information on the requested file.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="security">Security information to set.</param>
        /// <param name="sections">Access sections on which.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Mount is called when Dokan succeed to mount the volume.
        /// </summary>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus Mounted(DokanFileInfo info);

        /// <summary>
        /// Unmounted is called when Dokan is unmounting the volume.
        /// </summary>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus Unmounted(DokanFileInfo info);

        /// <summary>
        /// Retrieve all FileStreams informations on the file.
        /// This is only called if <see cref="DokanOptions.AltStream"/> is enabled.
        /// </summary>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="streams">List of <see cref="FileInformation"/> for each streams present on the file.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info);
    }
}
