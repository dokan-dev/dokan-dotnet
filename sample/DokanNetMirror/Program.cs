using System;
using System.Collections;
using DokanNet;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace DokaNetMirror
{
    class Mirror : IDokanOperations
    {
        private string root_;
        private ulong count_;
        public Mirror(string root)
        {
            root_ = root;
            count_ = 1;
        }

        private string GetPath(string filename)
        {
            string path = root_ + filename;
            Console.Error.WriteLine("GetPath : {0}", path);
            return path;
        }

        public DokanError CreateFile(string filename, FileAccess access, System.IO.FileShare share,
            System.IO.FileMode mode, System.IO.FileOptions options, System.IO.FileAttributes attributes, DokanFileInfo info)
        {
            string path = GetPath(filename);
            info.Context = count_++;
            if (System.IO.File.Exists(path))
            {
                return DokanError.ErrorSuccess;
            }
            else if(System.IO.Directory.Exists(path))
            {
                info.IsDirectory = true;
                return DokanError.ErrorSuccess;
            }
            else
            {
                return DokanError.ErrorFileNotFound;
            }
        }

        public DokanError OpenDirectory(string filename, DokanFileInfo info)
        {
            info.Context = count_++;
            if (System.IO.Directory.Exists(GetPath(filename)))
                return 0;
            else
                return DokanError.ErrorPathNotFound;
        }

        public DokanError CreateDirectory(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError Cleanup(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError CloseFile(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError ReadFile(string filename, byte[] buffer, out int bytesRead,
            long offset, DokanFileInfo info)
        {
            try
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(GetPath(filename));
                fs.Seek(offset, System.IO.SeekOrigin.Begin);
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                return DokanError.ErrorSuccess;
            }
            catch (Exception)
            {
                bytesRead = 0;
                return DokanError.ErrorError;
            }
        }

        public DokanError WriteFile(string filename, byte[] buffer,
            out int bytesWritten, long offset, DokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanError.ErrorError;
        }

        public DokanError FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError GetFileInformation(string filename, out FileInformation fileinfo, DokanFileInfo info)
        {
            string path = GetPath(filename);
            fileinfo = new FileInformation();
            if (System.IO.File.Exists(path))
            {
                System.IO.FileInfo f = new System.IO.FileInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = f.Length;
                return DokanError.ErrorSuccess;
            }
            else if (System.IO.Directory.Exists(path))
            {
                System.IO.DirectoryInfo f = new System.IO.DirectoryInfo(path);

                fileinfo.Attributes = f.Attributes;
                fileinfo.CreationTime = f.CreationTime;
                fileinfo.LastAccessTime = f.LastAccessTime;
                fileinfo.LastWriteTime = f.LastWriteTime;
                fileinfo.Length = 0;// f.Length;
                return DokanError.ErrorSuccess;
            }
            else
            {
                return DokanError.ErrorError;
            }
        }

        public DokanError FindFiles(string filename, out IList<FileInformation> files, DokanFileInfo info)
        {
            string path = GetPath(filename);
            files = new List<FileInformation>();
            if (System.IO.Directory.Exists(path))
            {
                System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(path);
                System.IO.FileSystemInfo[] entries = d.GetFileSystemInfos();
                foreach (System.IO.FileSystemInfo f in entries)
                {
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = (f is System.IO.DirectoryInfo) ? 0 : ((System.IO.FileInfo)f).Length;
                    fi.FileName = f.Name;
                    files.Add(fi);
                }
                return DokanError.ErrorSuccess;
            }
            else
            {
                return DokanError.ErrorError;
            }
        }

        public DokanError SetFileAttributes(string filename, System.IO.FileAttributes attr, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError SetFileTime(string filename, DateTime? creationTime,
                DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError DeleteFile(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError DeleteDirectory(string filename, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError UnlockFile(String filename, long offset, long length, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError GetDiskFreeSpace(out long freeBytesAvailable, out long totalBytes,
            out long totalFreeBytes, DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return DokanError.ErrorSuccess;
        }

        public DokanError GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = String.Empty;
            features = FileSystemFeatures.None;
            fileSystemName = String.Empty;
            return DokanError.ErrorError;
        }

        public DokanError GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            security = null;
            return DokanError.ErrorError;
        }

        public DokanError SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            return DokanError.ErrorError;
        }

        public DokanError Unmount(DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        static void Main(string[] args)
        {
            try
            {
                Mirror mirror = new Mirror("C:");
                mirror.Mount("n:\\", DokanOptions.DebugMode, 5);

                Console.WriteLine("Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
