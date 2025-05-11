using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
#if NETFRAMEWORK
using DiscUtils.Streams.Compatibility;
#endif
using DokanNet.Logging;
using DokanNet;
using LTRData.Extensions.Native.Memory;
using static DokanNet.FormatProviders;
using NativeFileAccess = DokanNet.FileAccess;
using FileAccess = System.IO.FileAccess;

namespace DokanNetMirror;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal class Mirror : IDokanOperations2
{
    int IDokanOperations2.DirectoryListingTimeoutResetIntervalMs => 0;

    private readonly string path;

    private const NativeFileAccess DataAccess = NativeFileAccess.ReadData | NativeFileAccess.WriteData | NativeFileAccess.AppendData |
                                          NativeFileAccess.Execute |
                                          NativeFileAccess.GenericExecute | NativeFileAccess.GenericWrite |
                                          NativeFileAccess.GenericRead;

    private const NativeFileAccess DataWriteAccess = NativeFileAccess.WriteData | NativeFileAccess.AppendData |
                                               NativeFileAccess.Delete |
                                               NativeFileAccess.GenericWrite;

    private readonly ILogger _logger;

    public Mirror(ILogger logger, string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException("Path not found", nameof(path));
        }

        _logger = logger;
        this.path = path;
    }

#if NETCOREAPP
    protected string GetPath(ReadOnlyNativeMemory<char> fileName) => string.Concat(path, fileName.Span);
#else
    protected string GetPath(ReadOnlyNativeMemory<char> fileName) => path + fileName.ToString();
#endif

    protected static NtStatus Trace(string method, ReadOnlyNativeMemory<char> fileName, in DokanFileInfo info, NtStatus result,
        params object?[] parameters)
    {
#if CONSOLE_LOGGING
        var extraParameters = parameters != null && parameters.Length > 0
            ? ", " + string.Join(", ", parameters.Select(x => string.Format(DefaultFormatProvider, "{0}", x)))
            : string.Empty;

        logger.Debug(DokanFormat($"{method}('{fileName.ToString()}', {info}{extraParameters}) -> {result}"));
#endif

        return result;
    }

    private static NtStatus Trace(string method, ReadOnlyNativeMemory<char> fileName, in DokanFileInfo info,
        NativeFileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
        NtStatus result)
    {
#if CONSOLE_LOGGING
        logger.Debug(
            DokanFormat(
                $"{method}('{fileName.ToString()}', {info}, [{access}], [{share}], [{mode}], [{options}], [{attributes}]) -> {result}"));
#endif

        return result;
    }

    protected static int GetNumOfBytesToCopy(int bufferLength, long offset, in DokanFileInfo info, FileStream stream)
    {
        if (info.PagingIo)
        {
            var longDistanceToEnd = stream.Length - offset;
            var isDistanceToEndMoreThanInt = longDistanceToEnd > int.MaxValue;
            if (isDistanceToEndMoreThanInt)
            {
                return bufferLength;
            }

            var distanceToEnd = (int)longDistanceToEnd;
            if (distanceToEnd < bufferLength)
            {
                return distanceToEnd;
            }

            return bufferLength;
        }

        return bufferLength;
    }

    #region Implementation of IDokanOperations

    public NtStatus CreateFile(ReadOnlyNativeMemory<char> fileName, NativeFileAccess access, FileShare share, FileMode mode,
        FileOptions options, FileAttributes attributes, ref DokanFileInfo info)
    {
        var result = DokanResult.Success;
        var filePath = GetPath(fileName);

        if (info.IsDirectory)
        {
            try
            {
                switch (mode)
                {
                    case FileMode.Open:
                        if (!Directory.Exists(filePath))
                        {
                            try
                            {
                                if (!File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
                                {
                                    return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                        attributes, DokanResult.NotADirectory);
                                }
                            }
                            catch (Exception)
                            {
                                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                    attributes, DokanResult.FileNotFound);
                            }

                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                attributes, DokanResult.PathNotFound);
                        }

                        _ = new DirectoryInfo(filePath).EnumerateFileSystemInfos().Any();
                        // you can't list the directory
                        break;

                    case FileMode.CreateNew:
                        if (Directory.Exists(filePath))
                        {
                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                attributes, DokanResult.FileExists);
                        }

                        try
                        {
                            File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                attributes, DokanResult.AlreadyExists);
                        }
                        catch (IOException)
                        {
                        }

                        Directory.CreateDirectory(GetPath(fileName));
                        break;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                    DokanResult.AccessDenied);
            }
        }
        else
        {
            var pathExists = true;
            var pathIsDirectory = false;

            var readWriteAttributes = (access & DataAccess) == 0;
            var readAccess = (access & DataWriteAccess) == 0;

            try
            {
                pathExists = (Directory.Exists(filePath) || File.Exists(filePath));
                pathIsDirectory = pathExists && File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
            }
            catch (IOException)
            {
            }

            switch (mode)
            {
                case FileMode.Open:

                    if (pathExists)
                    {
                        // check if driver only wants to read attributes, security info, or open directory
                        if (readWriteAttributes || pathIsDirectory)
                        {
                            if (pathIsDirectory && (access & NativeFileAccess.Delete) == NativeFileAccess.Delete
                                && (access & NativeFileAccess.Synchronize) != NativeFileAccess.Synchronize)
                            {
                                //It is a DeleteFile request on a directory
                                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                    attributes, DokanResult.AccessDenied);
                            }

                            info.IsDirectory = pathIsDirectory;

                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options,
                                attributes, DokanResult.Success);
                        }
                    }
                    else
                    {
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                            DokanResult.FileNotFound);
                    }

                    break;

                case FileMode.CreateNew:
                    if (pathExists)
                    {
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                            DokanResult.FileExists);
                    }

                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                    {
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                            DokanResult.FileNotFound);
                    }

                    break;
            }

            try
            {
                var streamAccess = readAccess ? FileAccess.Read : FileAccess.ReadWrite;

                if (mode == FileMode.CreateNew && readAccess)
                {
                    streamAccess = FileAccess.ReadWrite;
                }

                info.Context = new FileStream(filePath, mode,
                    streamAccess, share, 4096, options);

                if (pathExists && (mode == FileMode.OpenOrCreate
                                   || mode == FileMode.Create))
                {
                    result = DokanResult.AlreadyExists;
                }

                bool fileCreated = mode == FileMode.CreateNew || mode == FileMode.Create || (!pathExists && mode == FileMode.OpenOrCreate);
                
                if (fileCreated)
                {
                    FileAttributes new_attributes = attributes;
                    new_attributes |= FileAttributes.Archive; // Files are always created as Archive
                    // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set.
                    new_attributes &= ~FileAttributes.Normal;
                    File.SetAttributes(filePath, new_attributes);
                }
            }
            catch (UnauthorizedAccessException) // don't have access rights
            {
                if (info.Context is FileStream fileStream)
                {
                    // returning AccessDenied cleanup and close won't be called,
                    // so we have to take care of the stream now
                    fileStream.Dispose();
                    info.Context = null;
                }

                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                    DokanResult.AccessDenied);
            }
            catch (DirectoryNotFoundException)
            {
                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                    DokanResult.PathNotFound);
            }
            catch (Exception ex)
            {
                var hr = (uint)Marshal.GetHRForException(ex);
                switch (hr)
                {
                    case 0x80070020: //Sharing violation
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
                            DokanResult.SharingViolation);
                    default:
                        throw;
                }
            }
        }

        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes,
            result);
    }

    public void Cleanup(ReadOnlyNativeMemory<char> fileName, ref DokanFileInfo info)
    {
#if TRACE
        if (info.Context != null)
        {
            Console.WriteLine(DokanFormat($"{nameof(Cleanup)}('{fileName}', {info} - entering"));
        }
#endif

        (info.Context as FileStream)?.Dispose();
        info.Context = null;

        if (info.DeletePending)
        {
            if (info.IsDirectory)
            {
                Directory.Delete(GetPath(fileName));
            }
            else
            {
                File.Delete(GetPath(fileName));
            }
        }

        Trace(nameof(Cleanup), fileName, info, DokanResult.Success);
    }

    public void CloseFile(ReadOnlyNativeMemory<char> fileName, ref DokanFileInfo info)
    {
#if TRACE
        if (info.Context != null)
        {
            Console.WriteLine(DokanFormat($"{nameof(CloseFile)}('{fileName}', {info} - entering"));
        }
#endif

        (info.Context as FileStream)?.Dispose();
        info.Context = null;
        Trace(nameof(CloseFile), fileName, info, DokanResult.Success);
        // could recreate cleanup code here but this is not called sometimes
    }

    public virtual NtStatus ReadFile(ReadOnlyNativeMemory<char> fileName, NativeMemory<byte> buffer, out int bytesRead, long offset, ref DokanFileInfo info)
    {
        if (info.Context is Stream stream) // normal read
        {
#pragma warning disable CA2002 // Do not lock on objects with weak identity
            lock (stream) //Protect from overlapped read
            {
                stream.Position = offset;
                bytesRead = stream.Read(buffer.Span);
            }
#pragma warning restore CA2002 // Do not lock on objects with weak identity
        }
        else // memory mapped read
        {
            using var fstream = new FileStream(GetPath(fileName), FileMode.Open, FileAccess.Read);
            fstream.Position = offset;
            bytesRead = fstream.Read(buffer.Span);
        }

        return Trace(nameof(ReadFile), fileName, info, DokanResult.Success, $"out {bytesRead}",
            offset.ToString(CultureInfo.InvariantCulture));
    }

    public virtual NtStatus WriteFile(ReadOnlyNativeMemory<char> fileName, ReadOnlyNativeMemory<byte> buffer, out int bytesWritten, long offset, ref DokanFileInfo info)
    {
        var append = offset == -1;

        if (info.Context is Stream stream)
        {
#pragma warning disable CA2002 // Do not lock on objects with weak identity
            lock (stream) //Protect from overlapped write
            {
                if (append)
                {
                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.End);
                    }
                    else
                    {
                        bytesWritten = 0;
                        return Trace(nameof(WriteFile), fileName, info, DokanResult.Error, $"out {bytesWritten}",
                            offset.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    stream.Position = offset;
                }

                stream.Write(buffer.Span);
            }
#pragma warning restore CA2002 // Do not lock on objects with weak identity

            bytesWritten = buffer.Length;
        }
        else
        {
            using var fstream = new FileStream(GetPath(fileName), append ? FileMode.Append : FileMode.Open, FileAccess.Write);
            if (!append) // Offset of -1 is an APPEND: https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-writefile
            {
                fstream.Position = offset;
            }

            fstream.Write(buffer.Span);
            bytesWritten = buffer.Length;
        }

        return Trace(nameof(WriteFile), fileName, info, DokanResult.Success, $"out {bytesWritten}",
            offset.ToString(CultureInfo.InvariantCulture));
    }

    public NtStatus FlushFileBuffers(ReadOnlyNativeMemory<char> fileName, ref DokanFileInfo info)
    {
        try
        {
            if (info.Context is Stream stream)
            {
                stream.Flush();
            }

            return Trace(nameof(FlushFileBuffers), fileName, info, DokanResult.Success);
        }
        catch (IOException)
        {
            return Trace(nameof(FlushFileBuffers), fileName, info, DokanResult.DiskFull);
        }
    }

    public NtStatus GetFileInformation(ReadOnlyNativeMemory<char> fileName, out ByHandleFileInformation fileInfo, ref DokanFileInfo info)
    {
        // may be called with info.Context == null, but usually it isn't
        var filePath = GetPath(fileName);
        FileSystemInfo finfo = new FileInfo(filePath);
        if (!finfo.Exists)
        {
            finfo = new DirectoryInfo(filePath);
        }

        fileInfo = new ByHandleFileInformation
        {
            Attributes = finfo.Attributes,
            CreationTime = finfo.CreationTime,
            LastAccessTime = finfo.LastAccessTime,
            LastWriteTime = finfo.LastWriteTime,
            Length = (finfo as FileInfo)?.Length ?? 0,
        };

        return Trace(nameof(GetFileInformation), fileName, info, DokanResult.Success);
    }

    public NtStatus FindFiles(ReadOnlyNativeMemory<char> fileName, out IEnumerable<FindFileInformation> files, ref DokanFileInfo info)
    {
        // This function is not called because FindFilesWithPattern is implemented
        // Return DokanResult.NotImplemented in FindFilesWithPattern to make FindFiles called
        files = FindFilesHelper(fileName, "*");

        return Trace(nameof(FindFiles), fileName, info, DokanResult.Success);
    }

    public NtStatus SetFileAttributes(ReadOnlyNativeMemory<char> fileName, FileAttributes attributes, ref DokanFileInfo info)
    {
        try
        {
            // MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
            // because a value of 0x00000000 in the FileAttributes field means that the file attributes for this file MUST NOT be changed when setting basic information for the file
            if (attributes != 0)
            {
                File.SetAttributes(GetPath(fileName), attributes);
            }

            return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.Success, attributes.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.AccessDenied, attributes.ToString());
        }
        catch (FileNotFoundException)
        {
            return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.FileNotFound, attributes.ToString());
        }
        catch (DirectoryNotFoundException)
        {
            return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.PathNotFound, attributes.ToString());
        }
    }

    public NtStatus SetFileTime(ReadOnlyNativeMemory<char> fileName, DateTime? creationTime, DateTime? lastAccessTime,
        DateTime? lastWriteTime, ref DokanFileInfo info)
    {
        try
        {
            if (info.Context is FileStream stream)
            {
                var ct = creationTime?.ToFileTime() ?? 0;
                var lat = lastAccessTime?.ToFileTime() ?? 0;
                var lwt = lastWriteTime?.ToFileTime() ?? 0;
                
                if (NativeMethods.SetFileTime(stream.SafeFileHandle, ref ct, ref lat, ref lwt))
                {
                    return DokanResult.Success;
                }

                throw new Win32Exception();
            }

            var filePath = GetPath(fileName);

            if (creationTime.HasValue)
            {
                File.SetCreationTime(filePath, creationTime.Value);
            }

            if (lastAccessTime.HasValue)
            {
                File.SetLastAccessTime(filePath, lastAccessTime.Value);
            }

            if (lastWriteTime.HasValue)
            {
                File.SetLastWriteTime(filePath, lastWriteTime.Value);
            }

            return Trace(nameof(SetFileTime), fileName, info, DokanResult.Success, creationTime, lastAccessTime,
                lastWriteTime);
        }
        catch (UnauthorizedAccessException)
        {
            return Trace(nameof(SetFileTime), fileName, info, DokanResult.AccessDenied, creationTime, lastAccessTime,
                lastWriteTime);
        }
        catch (FileNotFoundException)
        {
            return Trace(nameof(SetFileTime), fileName, info, DokanResult.FileNotFound, creationTime, lastAccessTime,
                lastWriteTime);
        }
    }

    public NtStatus DeleteFile(ReadOnlyNativeMemory<char> fileName, ref DokanFileInfo info)
    {
        var filePath = GetPath(fileName);

        if (Directory.Exists(filePath))
        {
            return Trace(nameof(DeleteFile), fileName, info, DokanResult.AccessDenied);
        }

        if (!File.Exists(filePath))
        {
            return Trace(nameof(DeleteFile), fileName, info, DokanResult.FileNotFound);
        }

        if (File.GetAttributes(filePath).HasFlag(FileAttributes.Directory))
        {
            return Trace(nameof(DeleteFile), fileName, info, DokanResult.AccessDenied);
        }

        return Trace(nameof(DeleteFile), fileName, info, DokanResult.Success);
        // we just check here if we could delete the file - the true deletion is in Cleanup
    }

    public NtStatus DeleteDirectory(ReadOnlyNativeMemory<char> fileName, ref DokanFileInfo info)
    {
        return Trace(nameof(DeleteDirectory), fileName, info,
            Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
                ? DokanResult.DirectoryNotEmpty
                : DokanResult.Success);
        // if dir is not empty it can't be deleted
    }

    public NtStatus MoveFile(ReadOnlyNativeMemory<char> oldName, ReadOnlyNativeMemory<char> newName, bool replace, ref DokanFileInfo info)
    {
        var oldpath = GetPath(oldName);
        var newpath = GetPath(newName);

        (info.Context as FileStream)?.Dispose();
        info.Context = null;

        var exist = info.IsDirectory ? Directory.Exists(newpath) : File.Exists(newpath);

        try
        {

            if (!exist)
            {
                info.Context = null;
                if (info.IsDirectory)
                {
                    Directory.Move(oldpath, newpath);
                }
                else
                {
                    File.Move(oldpath, newpath);
                }

                return Trace(nameof(MoveFile), oldName, info, DokanResult.Success, newName,
                    replace.ToString(CultureInfo.InvariantCulture));
            }
            else if (replace)
            {
                info.Context = null;

                if (info.IsDirectory) //Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
                {
                    return Trace(nameof(MoveFile), oldName, info, DokanResult.AccessDenied, newName,
                        replace.ToString(CultureInfo.InvariantCulture));
                }

                File.Delete(newpath);
                File.Move(oldpath, newpath);
                return Trace(nameof(MoveFile), oldName, info, DokanResult.Success, newName,
                    replace.ToString(CultureInfo.InvariantCulture));
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Trace(nameof(MoveFile), oldName, info, DokanResult.AccessDenied, newName,
                replace.ToString(CultureInfo.InvariantCulture));
        }

        return Trace(nameof(MoveFile), oldName, info, DokanResult.FileExists, newName,
            replace.ToString(CultureInfo.InvariantCulture));
    }

    public NtStatus SetEndOfFile(ReadOnlyNativeMemory<char> fileName, long length, ref DokanFileInfo info)
    {
        try
        {
            (info.Context as FileStream)?.SetLength(length);
            return Trace(nameof(SetEndOfFile), fileName, info, DokanResult.Success,
                length.ToString(CultureInfo.InvariantCulture));
        }
        catch (IOException)
        {
            return Trace(nameof(SetEndOfFile), fileName, info, DokanResult.DiskFull,
                length.ToString(CultureInfo.InvariantCulture));
        }
    }

    public NtStatus SetAllocationSize(ReadOnlyNativeMemory<char> fileName, long length, ref DokanFileInfo info)
    {
        try
        {
            (info.Context as FileStream)?.SetLength(length);
            return Trace(nameof(SetAllocationSize), fileName, info, DokanResult.Success,
                length.ToString(CultureInfo.InvariantCulture));
        }
        catch (IOException)
        {
            return Trace(nameof(SetAllocationSize), fileName, info, DokanResult.DiskFull,
                length.ToString(CultureInfo.InvariantCulture));
        }
    }

    public NtStatus LockFile(ReadOnlyNativeMemory<char> fileName, long offset, long length, ref DokanFileInfo info)
    {
        try
        {
            (info.Context as FileStream)?.Lock(offset, length);
            return Trace(nameof(LockFile), fileName, info, DokanResult.Success,
                offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
        }
        catch (IOException)
        {
            return Trace(nameof(LockFile), fileName, info, DokanResult.AccessDenied,
                offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
        }
    }

    public NtStatus UnlockFile(ReadOnlyNativeMemory<char> fileName, long offset, long length, ref DokanFileInfo info)
    {
        try
        {
            ((FileStream?)info.Context)?.Unlock(offset, length);
            return Trace(nameof(UnlockFile), fileName, info, DokanResult.Success,
                offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
        }
        catch (IOException)
        {
            return Trace(nameof(UnlockFile), fileName, info, DokanResult.AccessDenied,
                offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
        }
    }

    public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, ref DokanFileInfo info)
    {
        var dinfo = DriveInfo.GetDrives().Single(di => string.Equals(di.RootDirectory.Name, Path.GetPathRoot(path + "\\"), StringComparison.OrdinalIgnoreCase));

        freeBytesAvailable = dinfo.TotalFreeSpace;
        totalNumberOfBytes = dinfo.TotalSize;
        totalNumberOfFreeBytes = dinfo.AvailableFreeSpace;

        return Trace(nameof(GetDiskFreeSpace), default, info, DokanResult.Success, $"out {freeBytesAvailable}",
            $"out {totalNumberOfBytes}", $"out {totalNumberOfFreeBytes}");
    }

    public NtStatus GetVolumeInformation(NativeMemory<char> volumeLabel, out FileSystemFeatures features,
        NativeMemory<char> fileSystemName, out uint maximumComponentLength, ref uint volumeSerialNumber, ref DokanFileInfo info)
    {
        volumeLabel.SetString("DOKAN");
        fileSystemName.SetString("NTFS");
        maximumComponentLength = 256;

        features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                   FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                   FileSystemFeatures.UnicodeOnDisk;

        return Trace(nameof(GetVolumeInformation), default, info, DokanResult.Success, $"out {volumeLabel}",
            $"out {features}", $"out {fileSystemName}");
    }

    public NtStatus GetFileSecurity(ReadOnlyNativeMemory<char> fileName, out FileSystemSecurity? security, AccessControlSections sections,
        ref DokanFileInfo info)
    {
        try
        {
#if NET5_0_OR_GREATER
            security = info.IsDirectory
                ? (FileSystemSecurity)new DirectoryInfo(GetPath(fileName)).GetAccessControl()
                : new FileInfo(GetPath(fileName)).GetAccessControl();
#else
            security = info.IsDirectory
                ? (FileSystemSecurity)Directory.GetAccessControl(GetPath(fileName))
                : File.GetAccessControl(GetPath(fileName));
#endif
            return Trace(nameof(GetFileSecurity), fileName, info, DokanResult.Success, sections.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            security = null;
            return Trace(nameof(GetFileSecurity), fileName, info, DokanResult.AccessDenied, sections.ToString());
        }
    }

    public NtStatus SetFileSecurity(ReadOnlyNativeMemory<char> fileName, FileSystemSecurity security, AccessControlSections sections,
        ref DokanFileInfo info)
    {
        try
        {
#if NET5_0_OR_GREATER
            if (info.IsDirectory)
            {
                new DirectoryInfo(GetPath(fileName)).SetAccessControl((DirectorySecurity)security);
            }
            else
            {
                new FileInfo(GetPath(fileName)).SetAccessControl((FileSecurity)security);
            }
#else
            if (info.IsDirectory)
            {
                Directory.SetAccessControl(GetPath(fileName), (DirectorySecurity)security);
            }
            else
            {
                File.SetAccessControl(GetPath(fileName), (FileSecurity)security);
            }
#endif
            return Trace(nameof(SetFileSecurity), fileName, info, DokanResult.Success, sections.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            return Trace(nameof(SetFileSecurity), fileName, info, DokanResult.AccessDenied, sections.ToString());
        }
    }

    public NtStatus Mounted(ReadOnlyNativeMemory<char> mountPoint, ref DokanFileInfo info)
    {
        return Trace(nameof(Mounted), default, info, DokanResult.Success);
    }

    public NtStatus Unmounted(ref DokanFileInfo info)
    {
        var ntStatus = Trace(nameof(Unmounted), default, info, DokanResult.Success);

        (_logger as IDisposable)?.Dispose();

        return ntStatus;
    }

    public NtStatus FindStreams(ReadOnlyNativeMemory<char> fileName, out IEnumerable<FindFileInformation> streams, ref DokanFileInfo info)
    {
        streams = [];
        return Trace(nameof(FindStreams), fileName, info, DokanResult.NotImplemented);
    }

    public IEnumerable<FindFileInformation> FindFilesHelper(ReadOnlyNativeMemory<char> fileName, string searchPattern)
    {
        var files = new DirectoryInfo(GetPath(fileName))
            .EnumerateFileSystemInfos()
            .Where(finfo => DokanHelper.DokanIsNameInExpression(searchPattern.AsSpan(), finfo.Name.AsSpan(), true))
            .Select(finfo => new FindFileInformation
            {
                Attributes = finfo.Attributes,
                CreationTime = finfo.CreationTime,
                LastAccessTime = finfo.LastAccessTime,
                LastWriteTime = finfo.LastWriteTime,
                Length = (finfo as FileInfo)?.Length ?? 0,
                FileName = finfo.Name.AsMemory()
            });

        return files;
    }

    public NtStatus FindFilesWithPattern(ReadOnlyNativeMemory<char> fileName, ReadOnlyNativeMemory<char> searchPattern, out IEnumerable<FindFileInformation> files,
        ref DokanFileInfo info)
    {
        files = FindFilesHelper(fileName, searchPattern.ToString());

        return Trace(nameof(FindFilesWithPattern), fileName, info, DokanResult.Success);
    }

    #endregion Implementation of IDokanOperations
}
