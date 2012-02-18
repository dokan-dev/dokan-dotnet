using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct DOKAN_OPERATIONS
    {
        public DokanOperationProxy.CreateFileDelegate CreateFile;
        public DokanOperationProxy.OpenDirectoryDelegate OpenDirectory;
        public DokanOperationProxy.CreateDirectoryDelegate CreateDirectory;
        public DokanOperationProxy.CleanupDelegate Cleanup;
        public DokanOperationProxy.CloseFileDelegate CloseFile;
        public DokanOperationProxy.ReadFileDelegate ReadFile;
        public DokanOperationProxy.WriteFileDelegate WriteFile;
        public DokanOperationProxy.FlushFileBuffersDelegate FlushFileBuffers;
        public DokanOperationProxy.GetFileInformationDelegate GetFileInformation;
        public DokanOperationProxy.FindFilesDelegate FindFiles;

        public IntPtr FindFilesWithPattern;

        public DokanOperationProxy.SetFileAttributesDelegate SetFileAttributes;
        public DokanOperationProxy.SetFileTimeDelegate SetFileTime;
        public DokanOperationProxy.DeleteFileDelegate DeleteFile;
        public DokanOperationProxy.DeleteDirectoryDelegate DeleteDirectory;
        public DokanOperationProxy.MoveFileDelegate MoveFile;
        public DokanOperationProxy.SetEndOfFileDelegate SetEndOfFile;
        public DokanOperationProxy.SetAllocationSizeDelegate SetAllocationSize;
        public DokanOperationProxy.LockFileDelegate LockFile;
        public DokanOperationProxy.UnlockFileDelegate UnlockFile;
        public DokanOperationProxy.GetDiskFreeSpaceDelegate GetDiskFreeSpace;
        public DokanOperationProxy.GetVolumeInformationDelegate GetVolumeInformation;
        public DokanOperationProxy.UnmountDelegate Unmount;

        public DokanOperationProxy.GetFileSecurityDelegate GetFileSecurity;
        public DokanOperationProxy.SetFileSecurityDelegate SetFileSecurity;
    }
}