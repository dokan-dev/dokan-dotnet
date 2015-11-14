using System;
using DokanNet.Native;

namespace DokanNet
{
    public static class Dokan
    {
        #region Dokan Driver Options

        private const ushort DOKAN_VERSION = 800; // ver 0.8.0

        #endregion Dokan Driver Options

        #region Dokan Driver Errors

        private const int DOKAN_SUCCESS = 0;
        private const int DOKAN_ERROR = -1;
        private const int DOKAN_DRIVE_LETTER_ERROR = -2;
        private const int DOKAN_DRIVER_INSTALL_ERROR = -3;
        private const int DOKAN_START_ERROR = -4;
        private const int DOKAN_MOUNT_ERROR = -5;
        private const int DOKAN_MOUNT_POINT_ERROR = -6;

        #endregion Dokan Driver Errors

        public static void Mount(this IDokanOperations operations, string mountPoint)
        {
            Mount(operations, mountPoint, DokanOptions.FixedDrive);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions)
        {
            Mount(operations, mountPoint, mountOptions, 0);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, DOKAN_VERSION);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, TimeSpan.FromSeconds(20));
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version, TimeSpan timeout)
        {
            var dokanOperationProxy = new DokanOperationProxy(operations);

            var dokanOptions = new DOKAN_OPTIONS
            {
                Version = (ushort)version,
                MountPoint = mountPoint,
                ThreadCount = (ushort)threadCount,
                Options = (uint)mountOptions,
                Timeout = (uint)timeout.Milliseconds
            };

            var dokanOperations = new DOKAN_OPERATIONS
            {
                ZwCreateFile = dokanOperationProxy.ZwCreateFileProxy,
                Cleanup = dokanOperationProxy.CleanupProxy,
                CloseFile = dokanOperationProxy.CloseFileProxy,
                ReadFile = dokanOperationProxy.ReadFileProxy,
                WriteFile = dokanOperationProxy.WriteFileProxy,
                FlushFileBuffers = dokanOperationProxy.FlushFileBuffersProxy,
                GetFileInformation = dokanOperationProxy.GetFileInformationProxy,
                FindFiles = dokanOperationProxy.FindFilesProxy,
                SetFileAttributes = dokanOperationProxy.SetFileAttributesProxy,
                SetFileTime = dokanOperationProxy.SetFileTimeProxy,
                DeleteFile = dokanOperationProxy.DeleteFileProxy,
                DeleteDirectory = dokanOperationProxy.DeleteDirectoryProxy,
                MoveFile = dokanOperationProxy.MoveFileProxy,
                SetEndOfFile = dokanOperationProxy.SetEndOfFileProxy,
                SetAllocationSize = dokanOperationProxy.SetAllocationSizeProxy,
                LockFile = dokanOperationProxy.LockFileProxy,
                UnlockFile = dokanOperationProxy.UnlockFileProxy,
                GetDiskFreeSpace = dokanOperationProxy.GetDiskFreeSpaceProxy,
                GetVolumeInformation = dokanOperationProxy.GetVolumeInformationProxy,
                Unmount = dokanOperationProxy.UnmountProxy,
                GetFileSecurity = dokanOperationProxy.GetFileSecurityProxy,
                SetFileSecurity = dokanOperationProxy.SetFileSecurityProxy,
                FindStreams = dokanOperationProxy.FindStreamsProxy
            };

            int status = NativeMethods.DokanMain(ref dokanOptions, ref dokanOperations);

            switch (status)
            {
                case DOKAN_ERROR:
                    throw new DokanException(status, "Dokan error");
                case DOKAN_DRIVE_LETTER_ERROR:
                    throw new DokanException(status, "Bad drive letter");
                case DOKAN_DRIVER_INSTALL_ERROR:
                    throw new DokanException(status, "Can't install the Dokan driver");
                case DOKAN_MOUNT_ERROR:
                    throw new DokanException(status, "Can't assign a drive letter or mount point");
                case DOKAN_START_ERROR:
                    throw new DokanException(status, "Something's wrong with the Dokan driver");
                case DOKAN_MOUNT_POINT_ERROR:
                    throw new DokanException(status, "Mount point is invalid ");
            }
        }

        public static bool Unmount(char driveLetter)
        {
            return NativeMethods.DokanUnmount(driveLetter) == DOKAN_SUCCESS;
        }

        public static bool RemoveMountPoint(string mountPoint)
        {
            return NativeMethods.DokanRemoveMountPoint(mountPoint) == DOKAN_SUCCESS;
        }

        public static int Version
        {
            get { return (int)NativeMethods.DokanVersion(); }
        }

        public static int DriverVersion
        {
            get { return (int)NativeMethods.DokanDriverVersion(); }
        }
    }
}