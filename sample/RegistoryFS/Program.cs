using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using DokanNet;
using System.Security.AccessControl;

namespace RegistoryFS
{
    class RFS : IDokanOperations
    {
        #region DokanOperations member

        private Dictionary<string, RegistryKey> TopDirectory;

        public RFS()
        {
            TopDirectory = new Dictionary<string, RegistryKey>();
            TopDirectory["ClassesRoot"] = Registry.ClassesRoot;
            TopDirectory["CurrentUser"] = Registry.CurrentUser;
            TopDirectory["CurrentConfig"] = Registry.CurrentConfig;
            TopDirectory["LocalMachine"] = Registry.LocalMachine;
            TopDirectory["Users"] = Registry.Users;
        }

        public DokanError Cleanup(string filename, DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError CloseFile(string filename, DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError CreateDirectory(string filename, DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError CreateFile(
            string filename,
            FileAccess access,
            System.IO.FileShare share,
            System.IO.FileMode mode,
            System.IO.FileOptions options,
            System.IO.FileAttributes attributes,
            DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError DeleteDirectory(string filename, DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError DeleteFile(string filename, DokanFileInfo info)
        {
            return DokanError.Error;
        }


        private RegistryKey GetRegistoryEntry(string name)
        {
            Console.WriteLine("GetRegistoryEntry : {0}", name);
            int top = name.IndexOf('\\', 1) -1;
            if (top < 0)
                top = name.Length - 1;

            string topname = name.Substring(1, top);
            int sub = name.IndexOf('\\', 1);

            if (TopDirectory.ContainsKey(topname))
            {
                if (sub == -1)
                    return TopDirectory[topname];
                else
                    return TopDirectory[topname].OpenSubKey(name.Substring(sub+1));
            }
            return null;
        }

        public DokanError FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError FindFiles(
            string filename,
            out IList<FileInformation> files,
            DokanFileInfo info)
        {
            files = new List<FileInformation>();
            if (filename == "\\")
            {
                foreach (string name in TopDirectory.Keys)
                {
                    FileInformation finfo = new FileInformation();
                    finfo.FileName = name;
                    finfo.Attributes = System.IO.FileAttributes.Directory;
                    finfo.LastAccessTime = DateTime.Now;
                    finfo.LastWriteTime = DateTime.Now;
                    finfo.CreationTime = DateTime.Now;
                    files.Add(finfo);
                }
                return DokanError.Success;
            }
            else
            {
                RegistryKey key = GetRegistoryEntry(filename);
                if (key == null)
                    return DokanError.Error;
                foreach (string name in key.GetSubKeyNames())
                {
                    FileInformation finfo = new FileInformation();
                    finfo.FileName = name;
                    finfo.Attributes = System.IO.FileAttributes.Directory;
                    finfo.LastAccessTime = DateTime.Now;
                    finfo.LastWriteTime = DateTime.Now;
                    finfo.CreationTime = DateTime.Now;
                    files.Add(finfo);
                }
                foreach (string name in key.GetValueNames())
                {
                    FileInformation finfo = new FileInformation();
                    finfo.FileName = name;
                    finfo.Attributes = System.IO.FileAttributes.Normal;
                    finfo.LastAccessTime = DateTime.Now;
                    finfo.LastWriteTime = DateTime.Now;
                    finfo.CreationTime = DateTime.Now;
                    files.Add(finfo);
                }
                return DokanError.Success;
            }
        }


        public DokanError GetFileInformation(
            string filename,
            out FileInformation fileinfo,
            DokanFileInfo info)
        {
            fileinfo = new FileInformation();
            if (filename == "\\")
            {
                fileinfo.Attributes = System.IO.FileAttributes.Directory;
                fileinfo.LastAccessTime = DateTime.Now;
                fileinfo.LastWriteTime = DateTime.Now;
                fileinfo.CreationTime = DateTime.Now;

                return DokanError.Success;
            }

            RegistryKey key = GetRegistoryEntry(filename);
            if (key == null)
                return DokanError.Error;

            fileinfo.Attributes = System.IO.FileAttributes.Directory;
            fileinfo.LastAccessTime = DateTime.Now;
            fileinfo.LastWriteTime = DateTime.Now;
            fileinfo.CreationTime = DateTime.Now;

            return DokanError.Success;
        }

        public DokanError LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError MoveFile(
            string filename,
            string newname,
            bool replace,
            DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError OpenDirectory(string filename, DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError ReadFile(
            string filename,
            byte[] buffer,
            out int readBytes,
            long offset,
            DokanFileInfo info)
        {
            readBytes = 0;
            return DokanError.Error;
        }

        public DokanError SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError SetFileAttributes(
            string filename,
            System.IO.FileAttributes attr,
            DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError SetFileTime(
            string filename,
            DateTime? ctime,
            DateTime? atime,
            DateTime? mtime,
            DokanFileInfo info)
        {
            return DokanError.Error;
        }

        public DokanError UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError Unmount(DokanFileInfo info)
        {
            return DokanError.Success;
        }

        public DokanError GetDiskFreeSpace(
           out long freeBytesAvailable,
           out long totalBytes,
           out long totalFreeBytes,
           DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return DokanError.Success;
        }

        public DokanError WriteFile(
            string filename,
            byte[] buffer,
            out int writtenBytes,
            long offset,
            DokanFileInfo info)
        {
            writtenBytes = 0;
            return DokanError.Error;
        }

        public DokanError GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "RFS";
            features = FileSystemFeatures.None;
            fileSystemName = String.Empty;
            return DokanError.Error;
        }

        public DokanError GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            security = null;
            return DokanError.Error;
        }

        public DokanError SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            return DokanError.Error;
        }

        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RFS rfs = new RFS();
                rfs.Mount("r:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
                Console.WriteLine("Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
