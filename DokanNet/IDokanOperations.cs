using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

/// <summary>
/// Base namespace for %Dokan.
/// </summary>
namespace DokanNet
{
    /// <summary>
    /// %Dokan API callbacks interface.
    /// 
    /// A interface of callbacks that describe all %Dokan API operation
    /// that will be called when Windows access to the file system.
    /// 
    /// All this callbacks can return <see cref="NtStatus.NotImplemented"/>
    /// if you dont want to support one of them. Be aware that returning such value to important callbacks
    /// such <see cref="CreateFile"/>/<see cref="ReadFile"/>/... would make the filesystem not working or unstable.
    /// </summary>
    /// <remarks>This is the same struct as <c>DOKAN_OPERATIONS</c> (dokan.h) in the C++ version of Dokan.</remarks>
    public interface IDokanOperations
    {
        /// <summary>
        /// CreateFile is called each time a request is made on a file system object.
        /// 
        /// In case <paramref name="mode"/> is <c><see cref="FileMode.OpenOrCreate"/></c> and
        /// <c><see cref="FileMode.Create"/></c> and CreateFile are successfully opening a already
        /// existing file, you have to return <see cref="DokanResult.AlreadyExists"/> instead of <see cref="NtStatus.Success"/>.
        /// 
        /// If the file is a directory, CreateFile is also called.
        /// In this case, CreateFile should return <see cref="NtStatus.Success"/> when that directory
        /// can be opened and <see cref="DokanFileInfo.IsDirectory"/> has to be set to <c>true</c>.
        /// On the other hand, if <see cref="DokanFileInfo.IsDirectory"/> is set to <c>true</c>
        /// but the path target a file, you need to return <see cref="NtStatus.NotADirectory"/>
        /// 
        /// <see cref="DokanFileInfo.Context"/> can be used to store data (like <c><see cref="FileStream"/></c>)
        /// that can be retrieved in all other request related to the context.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="access">A <see cref="FileAccess"/> with permissions for file or directory.</param>
        /// <param name="share">Type of share access to other threads, which is specified as
        /// <see cref="FileShare.None"/> or any combination of <see cref="FileShare"/>.
        /// Device and intermediate drivers usually set ShareAccess to zero,
        /// which gives the caller exclusive access to the open file.</param>
        /// <param name="mode">Specifies how the operating system should open a file. See <a href="https://msdn.microsoft.com/en-us/library/system.io.filemode(v=vs.110).aspx">FileMode Enumeration (MSDN)</a>.</param>
        /// <param name="options">Represents advanced options for creating a FileStream object. See <a href="https://msdn.microsoft.com/en-us/library/system.io.fileoptions(v=vs.110).aspx">FileOptions Enumeration (MSDN)</a>.</param>
        /// <param name="attributes">Provides attributes for files and directories. See <a href="https://msdn.microsoft.com/en-us/library/system.io.fileattributes(v=vs.110).aspx">FileAttributes Enumeration (MSDN></a>.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// \see See <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff566424(v=vs.85).aspx">ZwCreateFile (MSDN)</a> for more information about the parameters of this callback.
        NtStatus CreateFile(
            string fileName,
            FileAccess access,
            FileShare share,
            FileMode mode,
            FileOptions options,
            FileAttributes attributes,
            DokanFileInfo info);

        /// <summary>
        /// Receipt of this request indicates that the last handle for a file object that is associated 
        /// with the target device object has been closed (but, due to outstanding I/O requests, 
        /// might not have been released). 
        /// 
        /// Cleanup is requested before <see cref="CloseFile"/> is called.
        /// </summary>
        /// <remarks>
        /// When <see cref="DokanFileInfo.DeleteOnClose"/> is <c>true</c>, you must delete the file in Cleanup.
        /// Refer to <see cref="DeleteFile"/> and <see cref="DeleteDirectory"/> for explanation.
        /// </remarks>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <seealso cref="DeleteFile"/>
        /// <seealso cref="DeleteDirectory"/>
        /// <seealso cref="CloseFile"/>
        void Cleanup(string fileName, DokanFileInfo info);

        /// <summary>
        /// CloseFile is called at the end of the life of the context.
        /// 
        /// Receipt of this request indicates that the last handle of the file object that is associated 
        /// with the target device object has been closed and released. All outstanding I/O requests 
        /// have been completed or canceled.
        /// 
        /// CloseFile is requested after <see cref="Cleanup"/> is called.
        /// 
        /// Remainings in <see cref="DokanFileInfo.Context"/> has to be cleared before return.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <seealso cref="Cleanup"/>
        void CloseFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// ReadFile callback on the file previously opened in <see cref="CreateFile"/>.
        /// It can be called by different thread at the same time,
        /// therefor the read has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="buffer">Read buffer that has to be fill with the read result.
        /// The buffer size depend of the read size requested by the kernel.</param>
        /// <param name="bytesRead">Total number of bytes that has been read.</param>
        /// <param name="offset">Offset from where the read has to be proceed.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="WriteFile"/>
        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info);

        /// <summary>
        /// WriteFile callback on the file previously opened in <see cref="CreateFile"/>
        /// It can be called by different thread at the same time,
        /// therefor the write/context has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="buffer">Data that has to be written.</param>
        /// <param name="bytesWritten">Total number of bytes that has been write.</param>
        /// <param name="offset">Offset from where the write has to be proceed.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="ReadFile"/>
        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info);

        /// <summary>
        /// Clears buffers for this context and causes any buffered data to be written to the file.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info);

        /// <summary>
        /// Get specific informations on a file.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="fileInfo"><see cref="FileInformation"/> struct to fill</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        /// <summary>
        /// List all files in the path requested
        /// 
        /// <see cref="FindFilesWithPattern"/> is checking first. If it is not implemented or
        /// returns <see cref="NtStatus.NotImplemented"/>, then FindFiles is called.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="files">A list of <see cref="FileInformation"/> to return.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="FindFilesWithPattern"/>
        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        /// <summary>
        /// Same as <see cref="FindFiles"/> but with a search pattern to filter the result.
        /// </summary>
        /// <param name="fileName">Path requested by the Kernel on the FileSystem.</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="files">A list of <see cref="FileInformation"/> to return.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="FindFiles"/>
        NtStatus FindFilesWithPattern(
            string fileName,
            string searchPattern,
            out IList<FileInformation> files,
            DokanFileInfo info);

        /// <summary>
        /// Set file attributes on a specific file.
        /// </summary>
        /// <remarks>SetFileAttributes and <see cref="SetFileTime"/> are called only if both of them are implemented.</remarks>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="attributes"><see cref="FileAttributes"/> to set on file</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        /// <summary>
        /// Set file times on a specific file.
        /// If <see cref="DateTime"/> is <c>null</c>, this should not be updated.
        /// </summary>
        /// <remarks><see cref="SetFileAttributes"/> and SetFileTime are called only if both of them are implemented.</remarks>
        /// <param name="fileName">File or directory name.</param>
        /// <param name="creationTime"><see cref="DateTime"/> when the file was created.</param>
        /// <param name="lastAccessTime"><see cref="DateTime"/> when the file was last accessed.</param>
        /// <param name="lastWriteTime"><see cref="DateTime"/> when the file was last written to.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetFileTime(
            string fileName,
            DateTime? creationTime,
            DateTime? lastAccessTime,
            DateTime? lastWriteTime,
            DokanFileInfo info);

        /// <summary>
        /// Check if it is possible to delete a file.
        /// </summary>
        /// <remarks>
        /// You should NOT delete the file in DeleteFile, but instead
        /// you must only check whether you can delete the file or not,
        /// and return <see cref="NtStatus.Success"/> (when you can delete it) or appropriate error
        /// codes such as <see cref="NtStatus.AccessDenied"/>, <see cref="NtStatus.ObjectNameNotFound"/>.
        ///
        /// DeleteFile will also be called with <see cref="DokanFileInfo.DeleteOnClose"/> set to <c>false</c>
        /// to notify the driver when the file is no longer requested to be deleted.
        /// 
        /// When you return <see cref="NtStatus.Success"/>, you get a <see cref="Cleanup"/> call afterwards with
        /// <see cref="DokanFileInfo.DeleteOnClose"/> set to <c>true</c> and only then you have to actually
        /// delete the file being closed.
        /// </remarks>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="DokanResult.Success"/> if file can be delete or <see cref="NtStatus"/> appropriate.</returns>
        /// <seealso cref="DeleteDirectory"/>
        /// <seealso cref="Cleanup"/>
        NtStatus DeleteFile(string fileName, DokanFileInfo info);

        /// <summary>
        /// Check if it is possible to delete a directory.
        /// </summary>
        /// <remarks>
        /// You should NOT delete the file in <see cref="DeleteDirectory"/>, but instead
        /// you must only check whether you can delete the file or not,
        /// and return <see cref="NtStatus.Success"/> (when you can delete it) or appropriate error
        /// codes such as <see cref="NtStatus.AccessDenied"/>, <see cref="NtStatus.ObjectPathNotFound"/>,
        /// <see cref="NtStatus.ObjectNameNotFound"/>.
        ///
        /// DeleteFile will also be called with <see cref="DokanFileInfo.DeleteOnClose"/> set to <c>false</c>
        /// to notify the driver when the file is no longer requested to be deleted.
        ///
        /// When you return <see cref="NtStatus.Success"/>, you get a <see cref="Cleanup"/> call afterwards with
        /// <see cref="DokanFileInfo.DeleteOnClose"/> set to <c>true</c> and only then you have to actually
        /// delete the file being closed.
        /// </remarks>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="DokanResult.Success"/> if file can be delete or <see cref="NtStatus"/> appropriate.</returns>
        /// <seealso cref="DeleteFile"/>
        /// <seealso cref="Cleanup"/>
        NtStatus DeleteDirectory(string fileName, DokanFileInfo info);

        /// <summary>
        /// Move a file or directory to a new location.
        /// </summary>
        /// <param name="oldName">Path to the file to move.</param>
        /// <param name="newName">Path to the new location for the file.</param>
        /// <param name="replace">If the file should be replaced if it already exist a file with path <paramref name="newName"/>.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        /// <summary>
        /// SetEndOfFile is used to truncate or extend a file (physical file size).
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// SetAllocationSize is used to truncate or extend a file.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="length">File length to set</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info);

        /// <summary>
        /// Lock file at a specific offset and data length.
        /// This is only used if <see cref="DokanOptions.UserModeLock"/> is enabled.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="offset">Offset from where the lock has to be proceed.</param>
        /// <param name="length">Data length to lock.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="UnlockFile"/>
        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// Unlock file at a specific offset and data length.
        /// This is only used if <see cref="DokanOptions.UserModeLock"/> is enabled.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="offset">Offset from where the unlock has to be proceed.</param>
        /// <param name="length">Data length to lock.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="LockFile"/>
        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        /// <summary>
        /// Retrieves information about the amount of space that is available on a disk volume, which is the total amount of space, 
        /// the total amount of free space, and the total amount of free space available to the user that is associated with the calling thread.
        /// </summary>
        /// <remarks>
        /// Neither GetDiskFreeSpace nor <see cref="GetVolumeInformation"/> save the <see cref="DokanFileInfo.Context"/>.
        /// Before these methods are called, <see cref="CreateFile"/> may not be called. (ditto <see cref="CloseFile"/> and <see cref="Cleanup"/>).
        /// </remarks>
        /// <param name="freeBytesAvailable">Amount of available space.</param>
        /// <param name="totalNumberOfBytes">Total size of storage space.</param>
        /// <param name="totalNumberOfFreeBytes">Amount of free space.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364937(v=vs.85).aspx"> GetDiskFreeSpaceEx function (MSDN)</a>
        /// <seealso cref="GetVolumeInformation"/>
        NtStatus GetDiskFreeSpace(
            out long freeBytesAvailable,
            out long totalNumberOfBytes,
            out long totalNumberOfFreeBytes,
            DokanFileInfo info);

        /// <summary>
        /// Retrieves information about the file system and volume associated with the specified root directory.
        /// </summary>
        /// <remarks>
        /// Neither GetVolumeInformation nor <see cref="GetDiskFreeSpace"/> save the <see cref="DokanFileInfo.Context"/>.
        /// Before these methods are called, <see cref="CreateFile"/> may not be called. (ditto <see cref="CloseFile"/> and <see cref="Cleanup"/>).
        /// 
        /// <see cref="FileSystemFeatures.ReadOnlyVolume"/> is automatically added to the <paramref name="features"/> if <see cref="DokanOptions.WriteProtection"/> was
        /// specified when the volume was mounted.
        /// 
        /// If <see cref="NtStatus.NotImplemented"/> is returned, the %Dokan kernel driver use following settings by default:
        /// | Parameter                    | Default value                                                                                    |
        /// |------------------------------|--------------------------------------------------------------------------------------------------|
        /// | \a rawVolumeNameBuffer       | <c>"DOKAN"</c>                                                                                   |
        /// | \a rawVolumeSerialNumber     | <c>0x19831116</c>                                                                                |
        /// | \a rawMaximumComponentLength | <c>256</c>                                                                                       |
        /// | \a rawFileSystemFlags        | <c>CaseSensitiveSearch \|\| CasePreservedNames \|\| SupportsRemoteStorage \|\| UnicodeOnDisk</c> |
        /// | \a rawFileSystemNameBuffer   | <c>"NTFS"</c>                                                                                    |
        /// </remarks>
        /// <param name="volumeLabel">Volume name</param>
        /// <param name="features"><see cref="FileSystemFeatures"/> with features enabled on the volume.</param>
        /// <param name="fileSystemName">The name of the specified volume.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364993(v=vs.85).aspx"> GetVolumeInformation function (MSDN)</a>
        NtStatus GetVolumeInformation(
            out string volumeLabel,
            out FileSystemFeatures features,
            out string fileSystemName,
            DokanFileInfo info);

        /// <summary>
        /// Get specified information about the security of a file or directory. 
        /// </summary>
        /// \since Supported since version 0.6.0. You must specify the version in <see cref="Dokan.Mount"/> .
        /// 
        /// <param name="fileName">File or directory name.</param>
        /// <param name="security">A <see cref="FileSystemSecurity"/> with security information to return.</param>
        /// <param name="sections">A <see cref="AccessControlSections"/> with access sections to return.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="SetFileSecurity"/>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa446639(v=vs.85).aspx">GetFileSecurity function (MSDN)</a>
        NtStatus GetFileSecurity(
            string fileName,
            out FileSystemSecurity security,
            AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Sets the security of a file or directory object.
        /// </summary>
        /// \since Supported since version 0.6.0. You must specify the version in <see cref="Dokan.Mount"/> .
        /// 
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="security">A <see cref="FileSystemSecurity"/> with security information to set.</param>
        /// <param name="sections">A <see cref="AccessControlSections"/> with access sections on which.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="GetFileSecurity"/>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379577(v=vs.85).aspx">SetFileSecurity function (MSDN)</a>
        NtStatus SetFileSecurity(
            string fileName,
            FileSystemSecurity security,
            AccessControlSections sections,
            DokanFileInfo info);

        /// <summary>
        /// Is called when %Dokan succeed to mount the volume.
        /// </summary>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <see cref="Unmounted"/>
        NtStatus Mounted(DokanFileInfo info);

        /// <summary>
        /// Is called when %Dokan is unmounting the volume.
        /// </summary>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="Mounted"/>
        NtStatus Unmounted(DokanFileInfo info);

        /// <summary>
        /// Retrieve all NTFS Streams informations on the file.
        /// This is only called if <see cref="DokanOptions.AltStream"/> is enabled.
        /// </summary>
        /// <remarks>For files, the first item in <paramref name="streams"/> is information about the 
        /// default data stream <c>"::$DATA"</c>.</remarks>
        /// \since Supported since version 0.8.0. You must specify the version in <see cref="Dokan.Mount"/> .
        /// 
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="streams">List of <see cref="FileInformation"/> for each streams present on the file.</param>
        /// <param name="info">An <see cref="DokanFileInfo"/> with information about the file or directory.</param>
        /// <returns>Return <see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364424(v=vs.85).aspx">FindFirstStreamW function (MSDN)</a>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365993(v=vs.85).aspx">About KTM (MSDN)</a>
        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info);
    }
}

/// <summary>
/// Namespace for AssemblyInfo and resource strings
/// </summary>
namespace DokanNet.Properties
{
    // This is only for documentation of the DokanNet.Properties namespace.
}
