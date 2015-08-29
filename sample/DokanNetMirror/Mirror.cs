using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using FileAccess = DokanNet.FileAccess;

namespace DokanNetMirror
{
    internal class Mirror : IDokanOperations
    {
        private readonly string _path;

        private const FileAccess DataAccess = FileAccess.ReadData |
                                              FileAccess.WriteData |
                                              FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute |
                                              FileAccess.GenericWrite |
                                              FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData |
                                                   FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

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

        #region Implementation of IDokanOperations

        public DokanResult CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                                     FileOptions options, FileAttributes attributes, DokanFileInfo info)
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

                            return DokanResult.Success;
                        }
                    }
                    else
                    {
                        return DokanResult.FileNotFound;
                    }
                    break;

                case FileMode.CreateNew:
                    if (pathExists)
                        return DokanResult.AlreadyExists;
                    break;

                case FileMode.Truncate:
                    if (!pathExists)
                        return DokanResult.FileNotFound;

                    break;

                default:
                    break;
            }

            try
            {
                info.Context = new FileStream(path, mode,
                                              readAccess
                                                  ? System.IO.FileAccess.Read
                                                  : System.IO.FileAccess.ReadWrite, share, 4096, options);
            }
            catch (UnauthorizedAccessException) // Don't have access rights
            {
                return DokanResult.AccessDenied;
            }

            return DokanResult.Success;
        }

        public DokanResult OpenDirectory(string fileName, DokanFileInfo info)
        {
            string path = GetPath(fileName);
            if (!Directory.Exists(path))
            {
                return DokanResult.PathNotFound;
            }

            try
            {
                new DirectoryInfo(path).EnumerateFileSystemInfos().Any(); // You can't list directory
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            return DokanResult.Success;
        }

        public DokanResult CreateDirectory(string fileName, DokanFileInfo info)
        {
            if (Directory.Exists(GetPath(fileName)))
                return DokanResult.AlreadyExists;

            try
            {
                Directory.CreateDirectory(GetPath(fileName));
                return DokanResult.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
        }

        public DokanResult Cleanup(string fileName, DokanFileInfo info)
        {
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
            return DokanResult.Success;
        }

        public DokanResult CloseFile(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
            }
            info.Context = null;
            return DokanResult.Success; // could recreate cleanup code hear but this is not called sometimes
        }

        public DokanResult ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
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
            return DokanResult.Success;
        }

        public DokanResult WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset,
                                    DokanFileInfo info)
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
            return DokanResult.Success;
        }

        public DokanResult FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Flush();
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
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
            return DokanResult.Success;
        }

        public DokanResult FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new DirectoryInfo(GetPath(fileName))
                .GetFileSystemInfos()
                .Select(finfo => new FileInformation
                {
                    Attributes =
                        finfo.
                        Attributes,
                    CreationTime =
                        finfo.
                        CreationTime,
                    LastAccessTime =
                        finfo.
                        LastAccessTime,
                    LastWriteTime =
                        finfo.
                        LastWriteTime,
                    Length =
                        (finfo is
                         FileInfo)
                            ? ((
                               FileInfo
                               )finfo
                              ).
                                  Length
                            : 0,
                    FileName =
                        finfo.Name,
                }).ToArray();

            return DokanResult.Success;
        }

        public DokanResult SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            try
            {
                File.SetAttributes(GetPath(fileName), attributes);
                return DokanResult.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
            }
        }

        public DokanResult SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                                      DateTime? lastWriteTime, DokanFileInfo info)
        {
            try
            {
                string path = GetPath(fileName);
                if (creationTime.HasValue)
                {
                    File.SetCreationTime(path, creationTime.Value);
                }
                if (lastAccessTime.HasValue)
                {
                    File.SetCreationTime(path, lastAccessTime.Value);
                }
                if (lastWriteTime.HasValue)
                {
                    File.SetCreationTime(path, lastWriteTime.Value);
                }
                return DokanResult.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
        }

        public DokanResult DeleteFile(string fileName, DokanFileInfo info)
        {
            return File.Exists(GetPath(fileName)) ? DokanResult.Success : DokanResult.FileNotFound;
            // we just check here if we could delete file the true deletion is in Cleanup
        }

        public DokanResult DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
                       ? DokanResult.DirNotEmpty
                       : DokanResult.Success; // if dir is not empdy could not delete
        }

        public DokanResult MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            string oldpath = GetPath(oldName);
            string newpath = GetPath(newName);
            if (!File.Exists(newpath))
            {
                info.Context = null;

                File.Move(oldpath, newpath);
                return DokanResult.Success;
            }
            else if (replace)
            {
                info.Context = null;

                if (!info.IsDirectory)
                    File.Delete(newpath);
                File.Move(oldpath, newpath);
                return DokanResult.Success;
            }
            return DokanResult.FileExists;
        }

        public DokanResult SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }

        public DokanResult SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).SetLength(length);
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.DiskFull;
            }
        }

        public DokanResult LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Lock(offset, length);
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
        }

        public DokanResult UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Unlock(offset, length);
                return DokanResult.Success;
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
        }

        public DokanResult GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            var dinfo = DriveInfo.GetDrives()
                .Where(di => di.RootDirectory.Name == Path.GetPathRoot(_path + "\\")).Single();

            used = dinfo.AvailableFreeSpace;
            total = dinfo.TotalSize;
            free = dinfo.TotalFreeSpace;
            return DokanResult.Success;
        }

        public DokanResult GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                               out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "DOKAN";

            fileSystemName = "DOKAN";

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;

            return DokanResult.Success;
        }

        public DokanResult GetFileSecurity(string fileName, out FileSystemSecurity security,
                                          AccessControlSections sections, DokanFileInfo info)
        {
            try
            {
                security = info.IsDirectory
                               ? (FileSystemSecurity)Directory.GetAccessControl(GetPath(fileName))
                               : File.GetAccessControl(GetPath(fileName));
                return DokanResult.Success;
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return DokanResult.AccessDenied;
            }
        }

        public DokanResult SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                          DokanFileInfo info)
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
                return DokanResult.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
        }

        public DokanResult Unmount(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public DokanResult EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return DokanResult.Error;
        }

        #endregion Implementation of IDokanOperations
    }
}