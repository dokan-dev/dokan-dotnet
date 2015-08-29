using DokanNet.Native;

namespace DokanNet
{
    public static class Dokan
    {
        #region Dokan Driver Options

        private const ushort DOKAN_VERSION = 740; // ver 0.7.4
        /*
        private const uint DOKAN_OPTION_DEBUG = 1;
        private const uint DOKAN_OPTION_STDERR = 2;
        private const uint DOKAN_OPTION_ALT_STREAM = 4;
        private const uint DOKAN_OPTION_KEEP_ALIVE = 8;
        private const uint DOKAN_OPTION_NETWORK = 16;
        private const uint DOKAN_OPTION_REMOVABLE = 32;
        */

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
            Mount(operations, mountPoint, DokanOptions.FixedDrive, 0, DOKAN_VERSION);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions)
        {
            Mount(operations, mountPoint, mountOptions, 0, DOKAN_VERSION);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, DOKAN_VERSION);
        }

        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version)
        {
            var dokanOperationProxy = new DokanOperationProxy(operations);

            var dokanOptions = new DOKAN_OPTIONS
            {
                Version = (ushort)version,
                MountPoint = mountPoint,
                ThreadCount = (ushort)threadCount,
                Options = (uint)mountOptions,
            };

            /*    dokanOptions.Options |= options.RemovableDrive ? DOKAN_OPTION_REMOVABLE : 0;
                dokanOptions.Options |= options.DebugMode ? DOKAN_OPTION_DEBUG : 0;
                dokanOptions.Options |= options.UseStandardError ? DOKAN_OPTION_STDERR : 0;
                dokanOptions.Options |= options.UseAlternativeStreams ? DOKAN_OPTION_ALT_STREAM : 0;
                dokanOptions.Options |= options.UseKeepAlive ? DOKAN_OPTION_KEEP_ALIVE : 0;
                dokanOptions.Options |= options.NetworkDrive ? DOKAN_OPTION_NETWORK : 0;*/

            var dokanOperations = new DOKAN_OPERATIONS
            {
                CreateFile = dokanOperationProxy.CreateFileProxy,
                OpenDirectory = dokanOperationProxy.OpenDirectoryProxy,
                CreateDirectory = dokanOperationProxy.CreateDirectoryProxy,
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
                EnumerateNamedStreams = dokanOperationProxy.EnumerateNamedStreamsProxy,
            };

            int status = NativeMethods.DokanMain(ref dokanOptions, ref dokanOperations);

            switch (status)
            {
                case DOKAN_ERROR:
                    throw new DokanException(status, "Dokan error");
                case DOKAN_DRIVE_LETTER_ERROR:
                    throw new DokanException(status, "Bad drive letter");
                case DOKAN_DRIVER_INSTALL_ERROR:
                    throw new DokanException(status, "Can't install driver");
                case DOKAN_MOUNT_ERROR:
                    throw new DokanException(status, "Can't assign a drive letter or mount point");
                case DOKAN_START_ERROR:
                    throw new DokanException(status, "Something's wrong with Dokan driver");
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