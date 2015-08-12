using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using DokanNet;
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

        public DokanError CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
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

                            return DokanError.Success;
                        }
                    }
                    else
                    {
                        return DokanError.FileNotFound;
                    }
                    break;
                case FileMode.CreateNew:
                    if (pathExists)
                        return DokanError.AlreadyExists;
                    break;
                case FileMode.Truncate:
                    if (!pathExists)
                        return DokanError.FileNotFound;

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
                return DokanError.AccessDenied;
            }


            return DokanError.Success;
        }

        public DokanError OpenDirectory(string fileName, DokanFileInfo info)
        {
            string path = GetPath(fileName);
            if (!Directory.Exists(path))
            {
                return DokanError.PathNotFound;
            }

            try
            {
                new DirectoryInfo(path).EnumerateFileSystemInfos().Any(); // You can't list directory
            }
            catch (UnauthorizedAccessException)
            {
                return DokanError.AccessDenied;
            }
            return DokanError.Success;
        }

        public DokanError CreateDirectory(string fileName, DokanFileInfo info)
        {
            if (Directory.Exists(GetPath(fileName)))
                return DokanError.AlreadyExists;

            try
            {
                Directory.CreateDirectory(GetPath(fileName));
                return DokanError.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanError.AccessDenied;
            }
        }

        public DokanError Cleanup(string fileName, DokanFileInfo info)
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
            return DokanError.Success;
        }

        public DokanError CloseFile(string fileName, DokanFileInfo info)
        {
            if (info.Context != null && info.Context is FileStream)
            {
                (info.Context as FileStream).Dispose();
            }
            info.Context = null;
            return DokanError.Success; // could recreate cleanup code hear but this is not called sometimes
        }

        public DokanError ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
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
            return DokanError.Success;
        }

        public DokanError WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset,
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
            return DokanError.Success;
        }

        public DokanError FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            try
            {
                ((FileStream)(info.Context)).Flush();
                return DokanError.Success;
            }
            catch (IOException)
            {
                return DokanError.DiskFull;
            }
        }

        public DokanError GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
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
                               Length = (finfo is FileInfo) ? ((FileInfo) finfo).Length : 0,
                           };
            return DokanError.Success;
        }

        public DokanError FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new DirectoryInfo(GetPath(fileName)).GetFileSystemInfos().Select(finfo => new FileInformation
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
                                                                                                                 ) finfo
                                                                                                                ).
                                                                                                                    Length
                                                                                                              : 0,
                                                                                                      FileName =
                                                                                                          finfo.Name,
                                                                                                  }).ToArray();
            return DokanError.Success;
        }

        public DokanError SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            try
            {
                File.SetAttributes(GetPath(fileName), attributes);
                return DokanError.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanError.AccessDenied;
            }
            catch (FileNotFoundException)
            {
                return DokanError.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanError.PathNotFound;
            }
        }

        public DokanError SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
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
                return DokanError.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanError.AccessDenied;
            }
            catch (FileNotFoundException)
            {
                return DokanError.FileNotFound;
            }
        }

        public DokanError DeleteFile(string fileName, DokanFileInfo info)
        {
            return File.Exists(GetPath(fileName)) ? DokanError.Success : DokanError.FileNotFound;
            // we just check here if we could delete file the true deletion is in Cleanup
        }

        public DokanError DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return Directory.EnumerateFileSystemEntries(GetPath(fileName)).Any()
                       ? DokanError.DirNotEmpty
                       : DokanError.Success; // if dir is not empdy could not delete
        }

        public DokanError MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            string oldpath = GetPath(oldName);
            string newpath = GetPath(newName);
            if (!File.Exists(newpath))
            {
                info.Context = null;

                File.Move(oldpath, newpath);
                return DokanError.Success;
            }
            else if (replace)
            {
                info.Context = null;

                if (!info.IsDirectory)
                    File.Delete(newpath);
                File.Move(oldpath, newpath);
                return DokanError.Success;
            }
            return DokanError.FileExists;
        }

        public DokanError SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream) (info.Context)).SetLength(length);
                return DokanError.Success;
            }
            catch (IOException)
            {
                return DokanError.DiskFull;
            }
        }

        public DokanError SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream) (info.Context)).SetLength(length);
                return DokanError.Success;
            }
            catch (IOException)
            {
                return DokanError.DiskFull;
            }
        }

        public DokanError LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream) (info.Context)).Lock(offset, length);
                return DokanError.Success;
            }
            catch (IOException)
            {
                return DokanError.AccessDenied;
            }
        }

        public DokanError UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            try
            {
                ((FileStream) (info.Context)).Unlock(offset, length);
                return DokanError.Success;
            }
            catch (IOException)
            {
                return DokanError.AccessDenied;
            }
        }

        public DokanError GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            var dinfo =
                DriveInfo.GetDrives().Where(di => di.RootDirectory.Name == Path.GetPathRoot(_path)).Single();

            used = dinfo.AvailableFreeSpace;
            total = dinfo.TotalSize;
            free = dinfo.TotalFreeSpace;
            return DokanError.Success;
        }

        public DokanError GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                               out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "DOKAN";

            fileSystemName = "DOKAN";

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.PersistentAcls | FileSystemFeatures.SupportsRemoteStorage |
                       FileSystemFeatures.UnicodeOnDisk;


            return DokanError.Success;
        }

        public DokanError GetFileSecurity(string fileName, out FileSystemSecurity security,
                                          AccessControlSections sections, DokanFileInfo info)
        {
            try
            {
                security = info.IsDirectory
                               ? (FileSystemSecurity) Directory.GetAccessControl(GetPath(fileName))
                               : File.GetAccessControl(GetPath(fileName));
                return DokanError.Success;
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return DokanError.AccessDenied;
            }
        }

        public DokanError SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                          DokanFileInfo info)
        {
            try
            {
                if (info.IsDirectory)
                {
                    Directory.SetAccessControl(GetPath(fileName), (DirectorySecurity) security);
                }
                else
                {
                    File.SetAccessControl(GetPath(fileName), (FileSecurity) security);
                }
                return DokanError.Success;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanError.AccessDenied;
            }
        }

        public DokanError Unmount(DokanFileInfo info)
        {
            return DokanError.Success;
        }

        #endregion
    }
}