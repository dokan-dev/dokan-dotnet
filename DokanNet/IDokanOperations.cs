using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        /// <summary>
        /// Create or Open file/Directory.
        /// CreateFile is called each time a request is made on a file.
        /// If the file requested is a directory and DokanFileInfo.IsDirectory is not set
        /// <see cref="DokanFileInfo.IsDirectory"/> has to be set to true for informing the kernel.
        /// <see cref="DokanFileInfo.Context"/> can be use to store Data (like FileStream)
        /// that can be retrieved in all other request related to the Context
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="access"><see cref="FileAccess"/> to the file.</param>
        /// <param name="share"><see cref="FileShare"/> of other request on the same file</param>
        /// <param name="mode">Open <see cref="FileMode"/> for the file</param>
        /// <param name="options">Additional <see cref="FileOptions"/> for the open</param>
        /// <param name="attributes"><see cref="FileAttributes"/> to set after open</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
            FileOptions options, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// Cleanup request before CloseFile is called.
        /// When <see cref="DokanFileInfo.DeleteOnClose"/> is true, you must delete the file in Cleanup.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="info">Dokan file request informations</param>
        void Cleanup(string fileName, DokanFileInfo info);

        /// <summary>
        /// CloseFile is called at the end of the life of the Context.
        /// Context has to be clear before return.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="info">Dokan file request informations</param>
        void CloseFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// Read event on the file previously opened in <see cref="CreateFile"/>
        /// ReadFile can be called by different thread at the same time.
        /// Therefor the read has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="buffer">Read buffer that has to be fill with the read result.
        /// The buffer size depend of the read size requested by the kernel.</param>
        /// <param name="bytesRead">Total byte that has been read</param>
        /// <param name="offset">Offset from where the read has to be proceed</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
            DokanFileInfo info);

        /// <summary>
        /// Write event on the file previously opened in <see cref="CreateFile"/>
        /// WriteFile can be called by different thread at the same time.
        /// Therefor the write has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="buffer">Data that has to be written</param>
        /// <param name="bytesWritten">Total byte that has been write</param>
        /// <param name="offset">Offset from where the read has to be proceed</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten,
            long offset, DokanFileInfo info);

        /// <summary>
        /// Clears buffers for this context and causes any buffered data to be written to the file.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info);

        /// <summary>
        /// FileInformation struct has to be fill by the file informations requested.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="fileInfo">FileInformation struct to fill</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        /// <summary>
        /// List all files in the path requested
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="files">FileInformation list to fill</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        /// <summary>
        /// Like FindFiles but with a search pattern
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="files">FileInformation list to fill</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
            DokanFileInfo info);

        /// <summary>
        /// Set file attributes on a specific file
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="attributes">FileAttributes to set on file</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// Set file times on a specific file.
        /// If DateTime is null, this should not be updated.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="creationTime">Creation DateTime</param>
        /// <param name="lastWriteTime">LastAccessTime DateTime</param>
        /// <param name="lastAccessTime">LastWrite DateTime</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
            DateTime? lastWriteTime, DokanFileInfo info);

        /// <summary>
        /// Check if file can be delete.
        /// The file should not be delete in DeleteFile but in <see cref="Cleanup"/>.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return DokanResult.Success if file can be delete or NtStatus appropriate.</returns>
        NtStatus DeleteFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// Check if Directory can be delete.
        /// The directory should not be delete in DeleteFile but in <see cref="Cleanup"/>.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return DokanResult.Success if file can be delete or NtStatus appropriate.</returns>
        NtStatus DeleteDirectory(string fileName, DokanFileInfo info);

        /// <summary>
        /// Move a file or directory to his new destination
        /// </summary>
        /// <param name="oldName">Source file path to move.</param>
        /// <param name="newName">Destination file path</param>
        /// <param name="replace">Can replace or not if destination already exist</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        /// <summary>
        /// SetEndOfFile is used to truncate or extend a file (physical file size).
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// SetAllocationSize is used to truncate or extend a file.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// Lock file at a specific offset and data length.
        /// This is only used if DokanOptions.UserModeLock is enabled.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="offset"></param>
        /// <param name="length">Data length to lock</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// Unlock file at a specific offset and data length.
        /// This is only used if DokanOptions.UserModeLock is enabled.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="offset"></param>
        /// <param name="length">Data length to lock</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// GetDiskFreeSpace request device space informations
        /// </summary>
        /// <param name="freeBytesAvailable">Amount of available space.</param>
        /// <param name="totalNumberOfBytes">Total size of storage space.</param>
        /// <param name="totalNumberOfFreeBytes">Amount of free space.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes,
            out long totalNumberOfFreeBytes,
            DokanFileInfo info);

        /// <summary>
        /// GetVolumeInformation request volume informations
        /// </summary>
        /// <param name="volumeLabel">Volume name</param>
        /// <param name="features">Features enabled on the volume.</param>
        /// <param name="fileSystemName">FileSystem name.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info);

        /// <summary>
        /// Get ACL information on the requested file.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="security">Security information result.</param>
        /// <param name="sections">Access sections requested.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Set ACL information on the requested file.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="security">Security information to set.</param>
        /// <param name="sections">Access sections on which.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Mount is called when Dokan succeed to mount the volume.
        /// </summary>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus Mounted(DokanFileInfo info);

        /// <summary>
        /// Unmounted is called when Dokan is unmounting the volume.
        /// </summary>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus Unmounted(DokanFileInfo info);

        /// <summary>
        /// Retrieve all FileStreams informations on the file.
        /// This is only called if DokanOptions.AltStream is enabled.
        /// </summary>
        /// <param name="fileName">File path associate to the request.</param>
        /// <param name="streams">List of FileInformation for each streams present on the file.</param>
        /// <param name="info">Dokan file request informations</param>
        /// <returns>Return NtStatus or DokanResult appropriate to the request result.</returns>
        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info);
    }
}