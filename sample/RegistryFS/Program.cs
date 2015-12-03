using DokanNet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace RegistryFS
{
    internal class RFS : IDokanOperations
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

        public void Cleanup(string filename, DokanFileInfo info)
        {
        }

        public void CloseFile(string filename, DokanFileInfo info)
        {
        }

        public NtStatus CreateFile(
            string filename,
            FileAccess access,
            System.IO.FileShare share,
            System.IO.FileMode mode,
            System.IO.FileOptions options,
            System.IO.FileAttributes attributes,
            DokanFileInfo info)
        {
            if (info.IsDirectory && mode == System.IO.FileMode.CreateNew)
                return DokanResult.AccessDenied;
            return DokanResult.Success;
        }

        public NtStatus DeleteDirectory(string filename, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string filename, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        private RegistryKey GetRegistoryEntry(string name)
        {
            Console.WriteLine("GetRegistoryEntry : {0}", name);
            int top = name.IndexOf('\\', 1) - 1;
            if (top < 0)
                top = name.Length - 1;

            string topname = name.Substring(1, top);
            int sub = name.IndexOf('\\', 1);

            if (TopDirectory.ContainsKey(topname))
            {
                if (sub == -1)
                    return TopDirectory[topname];
                else
                    return TopDirectory[topname].OpenSubKey(name.Substring(sub + 1));
            }
            return null;
        }

        public NtStatus FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus FindFiles(
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
                return DokanResult.Success;
            }
            else
            {
                RegistryKey key = GetRegistoryEntry(filename);
                if (key == null)
                    return DokanResult.Error;
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
                return DokanResult.Success;
            }
        }

        public NtStatus GetFileInformation(
            string filename,
            out FileInformation fileinfo,
            DokanFileInfo info)
        {
            fileinfo = new FileInformation();
            fileinfo.FileName = filename;

            if (filename == "\\")
            {
                fileinfo.Attributes = System.IO.FileAttributes.Directory;
                fileinfo.LastAccessTime = DateTime.Now;
                fileinfo.LastWriteTime = DateTime.Now;
                fileinfo.CreationTime = DateTime.Now;

                return DokanResult.Success;
            }

            RegistryKey key = GetRegistoryEntry(filename);
            if (key == null)
                return DokanResult.Error;

            fileinfo.Attributes = System.IO.FileAttributes.Directory;
            fileinfo.LastAccessTime = DateTime.Now;
            fileinfo.LastWriteTime = DateTime.Now;
            fileinfo.CreationTime = DateTime.Now;

            return DokanResult.Success;
        }

        public NtStatus LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus MoveFile(
            string filename,
            string newname,
            bool replace,
            DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus ReadFile(
            string filename,
            byte[] buffer,
            out int readBytes,
            long offset,
            DokanFileInfo info)
        {
            readBytes = 0;
            return DokanResult.Error;
        }

        public NtStatus SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileAttributes(
            string filename,
            System.IO.FileAttributes attr,
            DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus SetFileTime(
            string filename,
            DateTime? ctime,
            DateTime? atime,
            DateTime? mtime,
            DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus Unmount(DokanFileInfo info)
        {
            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(
           out long freeBytesAvailable,
           out long totalBytes,
           out long totalFreeBytes,
           DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return DokanResult.Success;
        }

        public NtStatus WriteFile(
            string filename,
            byte[] buffer,
            out int writtenBytes,
            long offset,
            DokanFileInfo info)
        {
            writtenBytes = 0;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "RFS";
            features = FileSystemFeatures.None;
            fileSystemName = String.Empty;
            return DokanResult.Error;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            security = null;
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            DokanFileInfo info)
        {
            return DokanResult.Error;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize, DokanFileInfo info)
        {
            streamName = String.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info)
        {
            streams = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        #endregion DokanOperations member
    }

    internal class Program
    {
        private static void Main(string[] args)
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