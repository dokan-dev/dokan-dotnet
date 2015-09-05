using DokanNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using FileAccess = DokanNet.FileAccess;

namespace DokanNetMirror
{
    internal class Mirror : IDokanOperations
    {
        private readonly string _path;

        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        private int streamCount;

        private IDictionary<Stream, int> streamIds = new ConcurrentDictionary<Stream, int>();

        public Mirror(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("path");
            _path = path;
        }

        private string GetPath(string fileName)
        {
            return _path + fileName;
        }

        private string ToTrace(DokanFileInfo info)
        {
            var contextStream = info.Context as FileStream;
            string context = contextStream != null ? streamIds[contextStream].ToString() : "<null>";
            return $"{{{context}, {info.DeleteOnClose}, {info.IsDirectory}, {info.NoCache}, {info.PagingIo}, #{info.ProcessId}, {info.SynchronousIo}, {info.WriteToEndOfFile}}}";
        }

        private DokanResult Trace(string method, string fileName, DokanFileInfo info, DokanResult result, params string[] parameters)
        {
            var extraParameters = parameters != null && parameters.Length > 0 ? ", " + string.Join(", ", parameters) : string.Empty;

            Console.WriteLine($"{method}('{fileName}', {ToTrace(info)}{extraParameters}) -> {result}");

            return result;
        }

        private DokanResult Trace(string method, string fileName, DokanFileInfo info, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanResult result)
        {
            Console.WriteLine($"{method}('{fileName}', {ToTrace(info)}, [{access}], [{share}], [{mode}], [{options}], [{attributes}]) -> {result}");

            return result;
        }

        #region Implementation of IDokanOperations

        public DokanResult CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
                                      DokanFileInfo info)
        {
            var path = GetPath(fileName);

            bool pathExists = true;
            bool pathIsDirectory = false;

            bool readWriteAttributes = (access & DataAccess) == 0;

            bool readAccess = (access & DataWriteAccess) == 0;

            try
            {
                pathIsDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (IOException)
            {
                pathExists = false;
            }

            switch (mode)
            {
                case FileMode.Open:

                    if (pathExists)
                    {
                        if (readWriteAttributes || pathIsDirectory)
                        //check if only wants to read attributes,security info or open directory
                        {
                            info.IsDirectory = pathIsDirectory;
                            info.Context = new object();
                            // Must set it to someting if you return DokanError.Success

                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.Success);
                        }
                    }
                    else
                    {
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.FileNotFound);
                    }
                    break;

                case FileMode.CreateNew:
                    if (pathExists)
                        return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.AlreadyExists);
                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.FileNotFound);

                    break;

                default:
                    break;
            }

            try
            {
                info.Context = new FileStream(path, mode, readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share, 4096, options);
                streamIds.Add((Stream)info.Context, Interlocked.Increment(ref streamCount));
            }
            catch (UnauthorizedAccessException) // Don't have access rights
            {
                return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.AccessDenied);
            }

            return Trace(nameof(CreateFile), fileName, info, access, share, mode, options, attributes, DokanResult.Success);
        }

        public DokanResult OpenDirectory(string fileName, DokanFileInfo info)
        {
            string path = GetPath(fileName);
            if (!Directory.Exists(path))
            {
                return Trace(nameof(OpenDirectory), fileName, info, DokanResult.PathNotFound);
            }

            try
            {
                new DirectoryInfo(path).EnumerateFileSystemInfos().Any(); // You can't list directory
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(nameof(OpenDirectory), fileName, info, DokanResult.AccessDenied);
            }
            return Trace(nameof(OpenDirectory), fileName, info, DokanResult.Success);
        }

        public DokanResult CreateDirectory(string fileName, DokanFileInfo info)
        {
            if (Directory.Exists(GetPath(fileName)))
                return Trace(nameof(CreateDirectory), fileName, info, DokanResult.AlreadyExists);

            try
            {
                Directory.CreateDirectory(GetPath(fileName));
                return Trace(nameof(CreateDirectory), fileName, info, DokanResult.Success);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(nameof(CreateDirectory), fileName, info, DokanResult.AccessDenied);
            }
        }

        public DokanResult Cleanup(string fileName, DokanFileInfo info)
        {
            Trace(nameof(Cleanup) + "(in)", fileName, info, DokanResult.Undefined);
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
                streamIds.Remove((Stream)info.Context);
            }
            info.Context = null;

            if (info.DeleteOnClose)
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
            return Trace(nameof(Cleanup), fileName, info, DokanResult.Success);
        }

        public DokanResult CloseFile(string fileName, DokanFileInfo info)
        {
            Trace(nameof(CloseFile) + "(in)", fileName, info, DokanResult.Undefined);
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
                streamIds.Remove((Stream)info.Context);
            }
            info.Context = null;
            return Trace(nameof(CloseFile), fileName, info, DokanResult.Success); // could recreate cleanup code hear but this is not called sometimes
        }

        public DokanResult ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            if (info.Context == null) // memory mapped read
            {
                Trace(nameof(ReadFile) + "(no Context)", fileName, info, DokanResult.Undefined, offset.ToString(CultureInfo.InvariantCulture));
                using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Read))
                {
                    stream.Position = offset;
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
            else // normal read
            {
                var stream = info.Context as FileStream;
                stream.Position = offset;
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            return Trace(nameof(ReadFile), fileName, info, DokanResult.Success, $"out {bytesRead}", offset.ToString(CultureInfo.InvariantCulture));
        }

        public DokanResult WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            if (info.Context == null)
            {
                Trace(nameof(WriteFile) + "(no Context)", fileName, info, DokanResult.Undefined, offset.ToString(CultureInfo.InvariantCulture));
                using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Write))
                {
                    stream.Position = offset;
                    stream.Write(buffer, 0, buffer.Length);
                    bytesWritten = buffer.Length;
                }
            }
            else
            {
                var stream = info.Context as FileStream;
                stream.Write(buffer, 0, buffer.Length);
                bytesWritten = buffer.Length;
            }
            return Trace(nameof(WriteFile), fileName, info, DokanResult.Success, $"out {bytesWritten}", offset.ToString(CultureInfo.InvariantCulture));
        }

        public DokanResult FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Flush();
                return Trace(nameof(FlushFileBuffers), fileName, info, DokanResult.Success);
            }
            catch (IOException)
            {
                return Trace(nameof(FlushFileBuffers), fileName, info, DokanResult.DiskFull);
            }
        }

        public DokanResult GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            // may be called with info.Context=null , but usually it isn't
            string path = GetPath(fileName);
            FileSystemInfo finfo = new FileInfo(path);
            if (!finfo.Exists)
                finfo = new DirectoryInfo(path);

            fileInfo = new FileInformation
            {
                FileName = fileName,
                Attributes = finfo.Attributes,
                CreationTime = finfo.CreationTime,
                LastAccessTime = finfo.LastAccessTime,
                LastWriteTime = finfo.LastWriteTime,
                Length = (finfo is FileInfo) ? ((FileInfo)finfo).Length : 0,
            };
            return Trace(nameof(GetFileInformation), fileName, info, DokanResult.Success);
        }

        public DokanResult FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new DirectoryInfo(GetPath(fileName))
                .GetFileSystemInfos()
                .Select(finfo => new FileInformation
                {
                    Attributes = finfo.Attributes,
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Length = (finfo is FileInfo) ? ((FileInfo)finfo).Length : 0,
                    FileName = finfo.Name
                }).ToArray();

            return Trace(nameof(FindFiles), fileName, info, DokanResult.Success);
        }

        public DokanResult SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            try
            {
                File.SetAttributes(GetPath(fileName), attributes);
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

        public DokanResult SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            try
            {
                string path = GetPath(fileName);
                if (creationTime.HasValue)
                    File.SetCreationTime(path, creationTime.Value);

                if (lastAccessTime.HasValue)
                    File.SetLastAccessTime(path, lastAccessTime.Value);

                if (lastWriteTime.HasValue)
                    File.SetLastWriteTime(path, lastWriteTime.Value);

                return Trace(nameof(SetFileTime), fileName, info, DokanResult.Success, creationTime.ToString(), lastAccessTime.ToString(), lastWriteTime.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(nameof(SetFileTime), fileName, info, DokanResult.AccessDenied, creationTime.ToString(), lastAccessTime.ToString(), lastWriteTime.ToString());
            }
            catch (FileNotFoundException)
            {
                return Trace(nameof(SetFileTime), fileName, info, DokanResult.FileNotFound, creationTime.ToString(), lastAccessTime.ToString(), lastWriteTime.ToString());
            }
        }

        public DokanResult DeleteFile(string fileName, DokanFileInfo info)
        {
            return Trace(nameof(DeleteFile), fileName, info, File.Exists(GetPath(fileName)) ? DokanResult.Success : DokanResult.FileNotFound);
            // we just check here if we could delete file the true deletion is in Cleanup
        }

        public DokanResult DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return Trace(nameof(DeleteDirectory), fileName, info, Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any() ? DokanResult.DirNotEmpty : DokanResult.Success);
            // if dir is not empty could not delete
        }

        public DokanResult MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            string oldpath = GetPath(oldName);
            string newpath = GetPath(newName);
            if (!File.Exists(newpath))
            {
                if (info.Context != null)
                    streamIds.Remove((Stream)info.Context);
                info.Context = null;

                File.Move(oldpath, newpath);
                return Trace(nameof(MoveFile), oldName, info, DokanResult.Success, newName, replace.ToString(CultureInfo.InvariantCulture));
            }
            else if (replace)
            {
                if (info.Context != null)
                    streamIds.Remove((Stream)info.Context);
                info.Context = null;

                if (!info.IsDirectory)
                    File.Delete(newpath);
                File.Move(oldpath, newpath);
                return Trace(nameof(MoveFile), oldName, info, DokanResult.Success, newName, replace.ToString(CultureInfo.InvariantCulture));
            }
            return Trace(nameof(MoveFile), oldName, info, DokanResult.FileExists, newName, replace.ToString(CultureInfo.InvariantCulture));
        }

        public DokanResult SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return Trace(nameof(SetEndOfFile), fileName, info, DokanResult.Success, length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace(nameof(SetEndOfFile), fileName, info, DokanResult.DiskFull, length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DokanResult SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return Trace(nameof(SetAllocationSize), fileName, info, DokanResult.Success, length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace(nameof(SetAllocationSize), fileName, info, DokanResult.DiskFull, length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DokanResult LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Lock(offset, length);
                return Trace(nameof(LockFile), fileName, info, DokanResult.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace(nameof(LockFile), fileName, info, DokanResult.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DokanResult UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Unlock(offset, length);
                return Trace(nameof(UnlockFile), fileName, info, DokanResult.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace(nameof(UnlockFile), fileName, info, DokanResult.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DokanResult GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            var dinfo = DriveInfo.GetDrives().Where(di => di.RootDirectory.Name == Path.GetPathRoot(_path + "\\")).Single();

            used = dinfo.AvailableFreeSpace;
            total = dinfo.TotalSize;
            free = dinfo.TotalFreeSpace;
            return Trace(nameof(GetDiskFreeSpace), null, info, DokanResult.Success, $"out {free}", $"out {total}", $"out {used}");
        }

        public DokanResult GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                                out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "DOKAN";
            fileSystemName = "DOKAN";

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;

            return Trace(nameof(GetVolumeInformation), null, info, DokanResult.Success, $"out {volumeLabel}", $"out {features}", $"out {fileSystemName}");
        }

        public DokanResult GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            try
            {
                security = info.IsDirectory
                               ? (FileSystemSecurity)Directory.GetAccessControl(GetPath(fileName))
                               : File.GetAccessControl(GetPath(fileName));
                return Trace(nameof(GetFileSecurity), fileName, info, DokanResult.Success, $"{sections}");
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return Trace(nameof(GetFileSecurity), fileName, info, DokanResult.AccessDenied, $"{sections}");
            }
        }

        public DokanResult SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            try
            {
                if (info.IsDirectory)
                {
                    Directory.SetAccessControl(GetPath(fileName), (DirectorySecurity)security);
                }
                else
                {
                    File.SetAccessControl(GetPath(fileName), (FileSecurity)security);
                }
                return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.Success, sections.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(nameof(SetFileAttributes), fileName, info, DokanResult.AccessDenied, sections.ToString());
            }
        }

        public DokanResult Unmount(DokanFileInfo info)
        {
            return Trace(nameof(Unmount), null, info, DokanResult.Success);
        }

        public DokanResult EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return Trace(nameof(EnumerateNamedStreams), fileName, info, DokanResult.Error, enumContext.ToString(), $"out {streamName}", $"out {streamSize}");
        }

        #endregion Implementation of IDokanOperations
    }
}