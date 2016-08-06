using System;
using DokanNet.Logging;
using DokanNet.Native;
using DokanNet.Properties;

namespace DokanNet
{
    public static class Dokan
    {
        #region Dokan Driver Options

        /// <summary>
        /// DokanNet compatible version 1.0.0
        /// </summary>
        private const ushort DOKAN_VERSION = 100;

        #endregion Dokan Driver Options

        #region Dokan Driver Errors

        /// <summary>
        /// Dokan mount succeed
        /// </summary>
        private const int DOKAN_SUCCESS = 0;

        /// <summary>
        /// Dokan mount error
        /// </summary>
        private const int DOKAN_ERROR = -1;

        /// <summary>
        /// Dokan mount failed - Bad drive letter
        /// </summary>
        private const int DOKAN_DRIVE_LETTER_ERROR = -2;

        /// <summary>
        /// Dokan mount failed - Can't install driver 
        /// </summary>
        private const int DOKAN_DRIVER_INSTALL_ERROR = -3;

        /// <summary>
        /// Dokan mount failed - Driver answer that something is wrong
        /// </summary>
        private const int DOKAN_START_ERROR = -4;

        /// <summary>
        /// Dokan mount failed
        /// Can't assign a drive letter or mount point
        /// Probably already used by another volume
        /// </summary>
        private const int DOKAN_MOUNT_ERROR = -5;

        /// <summary>
        /// Dokan mount failed
        /// Mount point is invalid
        /// </summary>
        private const int DOKAN_MOUNT_POINT_ERROR = -6;

        /// <summary>
        /// Dokan mount failed
        /// Requested an incompatible version
        /// </summary>
        private const int DOKAN_VERSION_ERROR = -7;

        #endregion Dokan Driver Errors

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// If the mount fail a exception is trowed.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, ILogger logger = null)
        {
            Mount(operations, mountPoint, DokanOptions.FixedDrive, logger);
        }

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, 0, logger);
        }

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, DOKAN_VERSION, logger);
        }

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to Dokan version 1.2.3) (Version "123" is equal to Dokan version 1.2.3).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, TimeSpan.FromSeconds(20), string.Empty,
                512, 512, logger);
        }

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to Dokan version 1.2.3)</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, string.Empty, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to Dokan version 1.2.3)</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, string uncName, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, uncName, 512, 512, logger);
        }


        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// if the mount fail throw is fiered with error information.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount</param>
        /// <param name="threadCount">Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to Dokan version 1.2.3)</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume</param>
        /// <param name="allocationUnitSize">Allocation Unit Size of the volume. This will behave on the file size.</param>
        /// <param name="sectorSize">Sector Size of the volume. This will behave on the file size.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, string uncName = null, int allocationUnitSize = 512,
            int sectorSize = 512, ILogger logger = null)
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
                Version = (ushort) version,
                MountPoint = mountPoint,
                UNCName = string.IsNullOrEmpty(uncName) ? null : uncName,
                ThreadCount = (ushort) threadCount,
                Options = (uint) mountOptions,
                Timeout = (uint) timeout.Milliseconds,
                AllocationUnitSize = (uint) allocationUnitSize,
                SectorSize = (uint) sectorSize
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

            var status = NativeMethods.DokanMain(ref dokanOptions, ref dokanOperations);

            switch (status)
            {
                case DOKAN_ERROR:
                    throw new DokanException(status, Resource.ErrorDokan);
                case DOKAN_DRIVE_LETTER_ERROR:
                    throw new DokanException(status, Resource.ErrorBadDriveLetter);
                case DOKAN_DRIVER_INSTALL_ERROR:
                    throw new DokanException(status, Resource.ErrorDriverInstall);
                case DOKAN_MOUNT_ERROR:
                    throw new DokanException(status, Resource.ErrorAssignDriveLetter);
                case DOKAN_START_ERROR:
                    throw new DokanException(status, Resource.ErrorStart);
                case DOKAN_MOUNT_POINT_ERROR:
                    throw new DokanException(status, Resource.ErrorMountPointInvalid);
                case DOKAN_VERSION_ERROR:
                    throw new DokanException(status, Resource.ErrorVersion);
            }
        }

        /// <summary>
        /// Unmount a dokan device from a driver letter
        /// </summary>
        /// <param name="driveLetter">Driver letter to unmount</param>
        /// <returns>Return if volume has been unmount</returns>
        public static bool Unmount(char driveLetter)
        {
            return NativeMethods.DokanUnmount(driveLetter);
        }

        /// <summary>
        /// Unmount a dokan device from a mount point
        /// </summary>
        /// <param name="mountPoint">Mount point to unmount ("Z", "Z:", "Z:\", Z:\MyMountPoint).</param>
        /// <returns>Return if volume has been unmount</returns>
        public static bool RemoveMountPoint(string mountPoint)
        {
            return NativeMethods.DokanRemoveMountPoint(mountPoint);
        }

        /// <summary>
        /// Retrieve native dokan dll version supported
        /// </summary>
        /// <returns>Return native dokan dll version supported</returns>
        public static int Version => (int) NativeMethods.DokanVersion();

        /// <summary>
        /// Retrieve native dokan driver version supported
        /// </summary>
        /// <returns>Return native dokan driver version supported</returns>
        public static int DriverVersion => (int) NativeMethods.DokanDriverVersion();
    }
}