using DokanNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using FileAccess = DokanNet.FileAccess;

namespace DokanNetMirror
{
    internal class Mirror : IDokanOperations
    {
        private readonly string path;

        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite | FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        public Mirror(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("path");
            this.path = path;
        }

        private string GetPath(string fileName)
        {
            return path + fileName;
        }

        private string ToTrace(DokanFileInfo info)
        {
            var context = info.Context != null ? "<" + info.Context.GetType().Name + ">" : "<null>";

            return string.Format(CultureInfo.InvariantCulture, "{{{0}, {1}, {2}, {3}, {4}, #{5}, {6}, {7}}}",
                context, info.DeleteOnClose, info.IsDirectory, info.NoCache, info.PagingIo, info.ProcessId, info.SynchronousIo, info.WriteToEndOfFile);
        }

        private string ToTrace(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString(CultureInfo.CurrentCulture) : "<null>";
        }

        private NtStatus Trace(string method, string fileName, DokanFileInfo info, NtStatus result, params string[] parameters)
        {
            var extraParameters = parameters != null && parameters.Length > 0 ? ", " + string.Join(", ", parameters) : string.Empty;

#if TRACE
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}('{1}', {2}{3}) -> {4}",
                method, fileName, ToTrace(info), extraParameters, result));
#endif

            return result;
        }

        private NtStatus Trace(string method, string fileName, DokanFileInfo info,
                                  FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
                                  NtStatus result)
        {
#if TRACE
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}('{1}', {2}, [{3}], [{4}], [{5}], [{6}], [{7}]) -> {8}",
                method, fileName, ToTrace(info), access, share, mode, options, attributes, result));
#endif

            return result;
        }

        #region Implementation of IDokanOperations

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes,
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
                        // check if driver only wants to read attributes, security info, or open directory
                        {
                            info.IsDirectory = pathIsDirectory;
                            info.Context = new object();
                            // must set it to someting if you return DokanError.Success

                            return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.Success);
                        }
                    }
                    else
                    {
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.ObjectNameNotFound);
                    }
                    break;

                case FileMode.CreateNew:
                    if (pathExists)
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.ObjectNameCollision);
                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                        return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.ObjectNameNotFound);
                    break;

                default:
                    break;
            }

            try
            {
                info.Context = new FileStream(path, mode, readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite, share, 4096, options);
            }
            catch (UnauthorizedAccessException) // don't have access rights
            {
                return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.AccessDenied);
            }

            return Trace("CreateFile", fileName, info, access, share, mode, options, attributes, NtStatus.Success);
        }

        public NtStatus OpenDirectory(string fileName, DokanFileInfo info)
        {
            string path = GetPath(fileName);
            if (!Directory.Exists(path))
            {
                return Trace("OpenDirectory", fileName, info, NtStatus.ObjectPathNotFound);
            }

            try
            {
                new DirectoryInfo(path).EnumerateFileSystemInfos().Any(); // you can't list the directory
            }
            catch (UnauthorizedAccessException)
            {
                return Trace("OpenDirectory", fileName, info, NtStatus.AccessDenied);
            }
            return Trace("OpenDirectory", fileName, info, NtStatus.Success);
        }

        public NtStatus CreateDirectory(string fileName, DokanFileInfo info)
        {
            if (Directory.Exists(GetPath(fileName)))
                return Trace("CreateDirectory", fileName, info, NtStatus.ObjectNameCollision);

            try
            {
                Directory.CreateDirectory(GetPath(fileName));
                return Trace("CreateDirectory", fileName, info, NtStatus.Success);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace("CreateDirectory", fileName, info, NtStatus.AccessDenied);
            }
        }

        public void Cleanup(string fileName, DokanFileInfo info)
        {
#if TRACE
            if (info.Context != null)
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}('{1}', {2} - entering",
                    "Cleanup", fileName, ToTrace(info)));
#endif

            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
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
            Trace("Cleanup", fileName, info, NtStatus.Success);
        }

        public void CloseFile(string fileName, DokanFileInfo info)
        {
#if TRACE
            if (info.Context != null)
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0}('{1}', {2} - entering",
                    "CloseFile", fileName, ToTrace(info)));
#endif

            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
            }
            info.Context = null;
            Trace("CloseFile", fileName, info, NtStatus.Success); // could recreate cleanup code here but this is not called sometimes
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            if (info.Context == null) // memory mapped read
            {
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
            return Trace("ReadFile", fileName, info, NtStatus.Success, "out " + bytesRead.ToString(), offset.ToString(CultureInfo.InvariantCulture));
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            if (info.Context == null)
            {
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
            return Trace("WriteFile", fileName, info, NtStatus.Success, "out " + bytesWritten.ToString(), offset.ToString(CultureInfo.InvariantCulture));
        }

        public NtStatus FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Flush();
                return Trace("FlushFileBuffers", fileName, info, NtStatus.Success);
            }
            catch (IOException)
            {
                return Trace("FlushFileBuffers", fileName, info, NtStatus.DiskFull);
            }
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            // may be called with info.Context == null, but usually it isn't
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
            return Trace("GetFileInformation", fileName, info, NtStatus.Success);
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
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

            return Trace("FindFiles", fileName, info, NtStatus.Success);
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            try
            {
                File.SetAttributes(GetPath(fileName), attributes);
                return Trace("SetFileAttributes", fileName, info, NtStatus.Success, attributes.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return Trace("SetFileAttributes", fileName, info, NtStatus.AccessDenied, attributes.ToString());
            }
            catch (FileNotFoundException)
            {
                return Trace("SetFileAttributes", fileName, info, NtStatus.ObjectNameNotFound, attributes.ToString());
            }
            catch (DirectoryNotFoundException)
            {
                return Trace("SetFileAttributes", fileName, info, NtStatus.ObjectPathNotFound, attributes.ToString());
            }
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
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

                return Trace("SetFileTime", fileName, info, NtStatus.Success, ToTrace(creationTime), ToTrace(lastAccessTime), ToTrace(lastWriteTime));
            }
            catch (UnauthorizedAccessException)
            {
                return Trace("SetFileTime", fileName, info, NtStatus.AccessDenied, ToTrace(creationTime), ToTrace(lastAccessTime), ToTrace(lastWriteTime));
            }
            catch (FileNotFoundException)
            {
                return Trace("SetFileTime", fileName, info, NtStatus.ObjectNameNotFound, ToTrace(creationTime), ToTrace(lastAccessTime), ToTrace(lastWriteTime));
            }
        }

        public NtStatus DeleteFile(string fileName, DokanFileInfo info)
        {
            return Trace("DeleteFile", fileName, info, File.Exists(GetPath(fileName)) ? NtStatus.Success : NtStatus.ObjectNameNotFound);
            // we just check here if we could delete the file - the true deletion is in Cleanup
        }

        public NtStatus DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return Trace("DeleteDirectory", fileName, info, Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any() ? NtStatus.DirectoryNotEmpty : NtStatus.Success);
            // if dir is not empty it can't be deleted
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            string oldpath = GetPath(oldName);
            string newpath = GetPath(newName);
            if (!File.Exists(newpath))
            {
                info.Context = null;

                File.Move(oldpath, newpath);
                return Trace("MoveFile", oldName, info, NtStatus.Success, newName, replace.ToString(CultureInfo.InvariantCulture));
            }
            else if (replace)
            {
                info.Context = null;

                if (!info.IsDirectory)
                    File.Delete(newpath);
                File.Move(oldpath, newpath);
                return Trace("MoveFile", oldName, info, NtStatus.Success, newName, replace.ToString(CultureInfo.InvariantCulture));
            }
            return Trace("MoveFile", oldName, info, NtStatus.ObjectNameCollision, newName, replace.ToString(CultureInfo.InvariantCulture));
        }

        public NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return Trace("SetEndOfFile", fileName, info, NtStatus.Success, length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("SetEndOfFile", fileName, info, NtStatus.DiskFull, length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return Trace("SetAllocationSize", fileName, info, NtStatus.Success, length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("SetAllocationSize", fileName, info, NtStatus.DiskFull, length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Lock(offset, length);
                return Trace("LockFile", fileName, info, NtStatus.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("LockFile", fileName, info, NtStatus.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Unlock(offset, length);
                return Trace("UnlockFile", fileName, info, NtStatus.Success, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
            catch (IOException)
            {
                return Trace("UnlockFile", fileName, info, NtStatus.AccessDenied, offset.ToString(CultureInfo.InvariantCulture), length.ToString(CultureInfo.InvariantCulture));
            }
        }

        public NtStatus GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            var dinfo = DriveInfo.GetDrives().Where(di => di.RootDirectory.Name == Path.GetPathRoot(path + "\\")).Single();

            used = dinfo.AvailableFreeSpace;
            total = dinfo.TotalSize;
            free = dinfo.TotalFreeSpace;
            return Trace("GetDiskFreeSpace", null, info, NtStatus.Success, "out " + free.ToString(), "out " + total.ToString(), "out " + used.ToString());
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                                out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "DOKAN";
            fileSystemName = "DOKAN";

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;

            return Trace("GetVolumeInformation", null, info, NtStatus.Success, "out " + volumeLabel, "out " + features.ToString(), "out " + fileSystemName);
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            try
            {
                security = info.IsDirectory
                               ? (FileSystemSecurity)Directory.GetAccessControl(GetPath(fileName))
                               : File.GetAccessControl(GetPath(fileName));
                return Trace("GetFileSecurity", fileName, info, NtStatus.Success, sections.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return Trace("GetFileSecurity", fileName, info, NtStatus.AccessDenied, sections.ToString());
            }
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
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
                return Trace("SetFileSecurity", fileName, info, NtStatus.Success, sections.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return Trace("SetFileSecurity", fileName, info, NtStatus.AccessDenied, sections.ToString());
            }
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            return Trace("Unmount", null, info, NtStatus.Success);
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return Trace("EnumerateNamedStreams", fileName, info, NtStatus.Error, enumContext.ToString(), "out " + streamName, "out " + streamSize.ToString());
        }

        #endregion Implementation of IDokanOperations
    }
}