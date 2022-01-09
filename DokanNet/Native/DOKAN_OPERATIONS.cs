using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// Dokan API callbacks interface
    /// 
    /// A struct of callbacks that describe all Dokan API operation
    /// that will be called when Windows access to the filesystem.
    /// 
    /// If an error occurs, return <see cref="NtStatus"/>.
    /// 
    /// All this callbacks can be set to <c>null</c> or return <see cref="NtStatus.NotImplemented"/>
    /// if you dont want to support one of them. Be aware that returning such value to important callbacks
    /// such <see cref="ZwCreateFile"/>/<see cref="ReadFile"/>/... would make the filesystem not working or unstable.
    /// 
    /// Se <see cref="IDokanOperations"/> for more information about the fields.
    /// </summary>
    /// <remarks>This is the same struct as <c>_DOKAN_OPERATIONS</c> (dokan.h) in the C version of Dokan.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal sealed class DOKAN_OPERATIONS
    {
        public DokanOperationProxy.ZwCreateFileDelegate ZwCreateFile;
        public DokanOperationProxy.CleanupDelegate Cleanup;
        public DokanOperationProxy.CloseFileDelegate CloseFile;
        public DokanOperationProxy.ReadFileDelegate ReadFile;
        public DokanOperationProxy.WriteFileDelegate WriteFile;
        public DokanOperationProxy.FlushFileBuffersDelegate FlushFileBuffers;
        public DokanOperationProxy.GetFileInformationDelegate GetFileInformation;
        public DokanOperationProxy.FindFilesDelegate FindFiles;

        public DokanOperationProxy.FindFilesWithPatternDelegate FindFilesWithPattern;

        public DokanOperationProxy.SetFileAttributesDelegate SetFileAttributes;
        public DokanOperationProxy.SetFileTimeDelegate SetFileTime;
        public DokanOperationProxy.DeleteFileDelegate DeleteFile;
        public DokanOperationProxy.DeleteDirectoryDelegate DeleteDirectory;
        public DokanOperationProxy.MoveFileDelegate MoveFile;
        public DokanOperationProxy.SetEndOfFileDelegate SetEndOfFile;
        public DokanOperationProxy.SetAllocationSizeDelegate SetAllocationSize;

        // Lockfile & Unlockfile are only used if dokan option UserModeLock is enabled
        public DokanOperationProxy.LockFileDelegate LockFile;
        public DokanOperationProxy.UnlockFileDelegate UnlockFile;

        public DokanOperationProxy.GetDiskFreeSpaceDelegate GetDiskFreeSpace;
        public DokanOperationProxy.GetVolumeInformationDelegate GetVolumeInformation;
        public DokanOperationProxy.MountedDelegate Mounted;
        public DokanOperationProxy.UnmountedDelegate Unmounted;

        public DokanOperationProxy.GetFileSecurityDelegate GetFileSecurity;
        public DokanOperationProxy.SetFileSecurityDelegate SetFileSecurity;

        public DokanOperationProxy.FindStreamsDelegate FindStreams;
    }
}