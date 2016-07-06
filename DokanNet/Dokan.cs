using System;
using DokanNet.Native;

namespace DokanNet
{

    using DokanNet.Logging;

    public static class Dokan
    {
        #region Dokan Driver Options

        private const ushort DOKAN_VERSION = 100; // ver 1.0.0

        #endregion Dokan Driver Options

        #region Dokan Driver Errors

        private const int DOKAN_SUCCESS = 0;
        private const int DOKAN_ERROR = -1;                 /* General Error */
        private const int DOKAN_DRIVE_LETTER_ERROR = -2;    /* Bad Drive letter */
        private const int DOKAN_DRIVER_INSTALL_ERROR = -3;  /* Can't install driver */
        private const int DOKAN_START_ERROR = -4;           /* Driver something wrong */
        private const int DOKAN_MOUNT_ERROR = -5;           /* Can't assign a drive letter or mount point */
        private const int DOKAN_MOUNT_POINT_ERROR = -6;     /* Mount point is invalid */
        private const int DOKAN_VERSION_ERROR = -7;         /* Requested an incompatible version */

        #endregion Dokan Driver Errors

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, ILogger logger = null)
        {
            Mount(operations, mountPoint, DokanOptions.FixedDrive, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, 0, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, DOKAN_VERSION, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library</param>
        /// <param name="version">Supported Dokan Version, ex. "530" (Dokan ver 0.5.3).</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, TimeSpan.FromSeconds(20), String.Empty, 512, 512, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library</param>
        /// <param name="version">Supported Dokan Version, ex. "530" (Dokan ver 0.5.3).</param>
        /// <param name="timeout">Max time to mount the virtual filesystem</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version, TimeSpan timeout, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, String.Empty, 512, 512, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library</param>
        /// <param name="version">Supported Dokan Version, ex. "530" (Dokan ver 0.5.3).</param>
        /// <param name="timeout">Max time to mount the virtual filesystem</param>
        /// <param name="uncName">UNC provider name</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version, TimeSpan timeout, string uncName, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, uncName, 512, 512, logger);
        }

        /// <summary>
        /// A blocking function that mount the virtual file system
        /// </summary>
        /// <param name="operations">An <see cref="IDokanOperations"/></param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions">Combination of DOKAN_OPTIONS_*</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library</param>
        /// <param name="version">Supported Dokan Version, ex. "530" (Dokan ver 0.5.3).</param>
        /// <param name="timeout">Max time to mount the virtual filesystem</param>
        /// <param name="uncName">UNC provider name</param>
        /// <param name="allocationUnitSize">Device allocation size.</param>
        /// <param name="sectorSize">Device sector size.</param>
        /// <param name="logger">An <see cref="ILogger"/> for logging</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions, int threadCount, int version, TimeSpan timeout, string uncName = null, int allocationUnitSize = 512, int sectorSize = 512, ILogger logger = null)
        {
#if TRACE
            if(logger == null){
                logger = new ConsoleLogger("[DokanNet] ");
            }
#endif
            if (logger == null)
            {
                logger = new NullLogger();
            }

            var dokanOperationProxy = new DokanOperationProxy(operations, logger);

            var dokanOptions = new DOKAN_OPTIONS
            {
                Version = (ushort)version,
                MountPoint = mountPoint,
                UNCName = string.IsNullOrEmpty(uncName) ? null : uncName,
                ThreadCount = (ushort)threadCount,
                Options = (uint)mountOptions,
                Timeout = (uint)timeout.Milliseconds,
                AllocationUnitSize = (uint)allocationUnitSize,
                SectorSize = (uint)sectorSize
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
                FindFilesWithPattern = dokanOperationProxy.FindFilesWithPatternProxy,
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
                Mounted = dokanOperationProxy.MountedProxy,
                Unmounted = dokanOperationProxy.UnmountedProxy,
                GetFileSecurity = dokanOperationProxy.GetFileSecurityProxy,
                SetFileSecurity = dokanOperationProxy.SetFileSecurityProxy,
                FindStreams = dokanOperationProxy.FindStreamsProxy
            };

            int status = NativeMethods.DokanMain(ref dokanOptions, ref dokanOperations);

            switch (status)
            {
                case DOKAN_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorDokan);
                case DOKAN_DRIVE_LETTER_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorBadDriveLetter);
                case DOKAN_DRIVER_INSTALL_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorDriverInstall);
                case DOKAN_MOUNT_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorAssignDriveLetter);
                case DOKAN_START_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorStart);
                case DOKAN_MOUNT_POINT_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorMountPointInvalid);
                case DOKAN_VERSION_ERROR:
                    throw new DokanException(status, Properties.Resource.ErrorVersion);
            }
        }

        public static bool Unmount(char driveLetter)
        {
            return NativeMethods.DokanUnmount(driveLetter);
        }

        public static bool RemoveMountPoint(string mountPoint)
        {
            return NativeMethods.DokanRemoveMountPoint(mountPoint);
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
