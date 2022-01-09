using System;
using System.Runtime.InteropServices;
using System.Text;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Helper and extension methods to %Dokan.
    /// </summary>
    public static class Dokan
    {
        #region Dokan Driver Options

        /// <summary>
        /// The %Dokan version that DokanNet is compatible with. Currently it is version 1.0.0.
        /// </summary>
        /// <see cref="DOKAN_OPTIONS.Version"/>
        private const ushort DOKAN_VERSION = 200;

        #endregion Dokan Driver Options

        /// <summary>
        /// Initialize all required Dokan internal resources.
        /// 
        /// This needs to be called only once before trying to use <see cref="Mount"/> or <see cref="CreateFileSystem"/> for the first time.
        /// Otherwise both will fail and raise an exception.
        /// </summary>
        public static void Init()
        {
            NativeMethods.DokanInit();
        }

        /// <summary>
        /// Release all allocated resources by <see cref="Init"/> when they are no longer needed.
        ///
        /// This should be called when the application no longer expects to create a new FileSystem with
        /// <see cref="Mount"/> or <see cref="CreateFileSystem"/> and after all devices are unmount.
        /// </summary>
        public static void Shutdown()
        {
            NativeMethods.DokanShutdown();
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, ILogger logger = null)
        {
            Mount(operations, mountPoint, DokanOptions.FixedDrive, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, false, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, singleThread, DOKAN_VERSION, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, singleThread, version, TimeSpan.FromSeconds(20), string.Empty,
                512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, singleThread, version, timeout, string.Empty, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, string uncName, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, singleThread, version, timeout, uncName, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="allocationUnitSize">Allocation Unit Size of the volume. This will behave on the file size.</param>
        /// <param name="sectorSize">Sector Size of the volume. This will behave on the file size.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, string uncName = null, int allocationUnitSize = 512,
            int sectorSize = 512, ILogger logger = null)
        {
            if (logger == null)
            {
#if TRACE
                logger = new ConsoleLogger("[DokanNet] ");
#else
                logger = new NullLogger();
#endif
            }

            var dokanOperationProxy = new DokanOperationProxy(operations, logger);

            var dokanOptions = new DOKAN_OPTIONS
            {
                Version = (ushort)version,
                MountPoint = mountPoint,
                UNCName = string.IsNullOrEmpty(uncName) ? null : uncName,
                SingleThread = singleThread,
                Options = (uint)mountOptions,
                Timeout = (uint)timeout.TotalMilliseconds,
                AllocationUnitSize = (uint)allocationUnitSize,
                SectorSize = (uint)sectorSize,
                VolumeSecurityDescriptorLength = 0
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

            DokanStatus status = NativeMethods.DokanMain(dokanOptions, dokanOperations);
            if (status != DokanStatus.Success)
            {
                throw new DokanException(status);
            }
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, DokanOptions.FixedDrive, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, mountOptions, false, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, mountOptions, singleThread, DOKAN_VERSION, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, mountOptions, singleThread, version, TimeSpan.FromSeconds(20), string.Empty,
                512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, mountOptions, singleThread, version, timeout, string.Empty, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, string uncName, ILogger logger = null)
        {
            return CreateFileSystem(operations, mountPoint, mountOptions, singleThread, version, timeout, uncName, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="WaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="singleThread">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="allocationUnitSize">Allocation Unit Size of the volume. This will behave on the file size.</param>
        /// <param name="sectorSize">Sector Size of the volume. This will behave on the file size.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        /// <returns>Dokan mount instance context that can be used for related instance calls like <see cref="IsFileSystemRunning"/></returns>
        public static DokanInstance CreateFileSystem(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            bool singleThread, int version, TimeSpan timeout, string uncName = null, int allocationUnitSize = 512,
            int sectorSize = 512, ILogger logger = null)
        {
            if (logger == null)
            {
#if TRACE
                logger = new ConsoleLogger("[DokanNet] ");
#else
                logger = new NullLogger();
#endif
            }

            DokanInstance instance = new DokanInstance();

            var dokanOperationProxy = new DokanOperationProxy(operations, logger);

            var dokanOptions = new DOKAN_OPTIONS
            {
                Version = (ushort)version,
                MountPoint = mountPoint,
                UNCName = string.IsNullOrEmpty(uncName) ? null : uncName,
                SingleThread = singleThread,
                Options = (uint)mountOptions,
                Timeout = (uint)timeout.TotalMilliseconds,
                AllocationUnitSize = (uint)allocationUnitSize,
                SectorSize = (uint)sectorSize,
                VolumeSecurityDescriptorLength = 0
            };

            instance.DokanOptions = new NativeStructWrapper<DOKAN_OPTIONS>(dokanOptions);

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

            instance.DokanOperations = new NativeStructWrapper<DOKAN_OPERATIONS>(dokanOperations);

            DokanStatus status = NativeMethods.DokanCreateFileSystem(instance.DokanOptions, instance.DokanOperations, out instance.DokanHandle);
            if (status != DokanStatus.Success)
            {
                throw new DokanException(status);
            }

            return instance;
        }

        /// <summary>
        /// Check if the FileSystem is still running or not.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="CreateFileSystem"/>.</param>
        /// <returns>Whether the FileSystem is still running or not.</returns>
        public static bool IsFileSystemRunning(this DokanInstance dokanInstance)
        {
            return NativeMethods.DokanIsFileSystemRunning(dokanInstance.DokanHandle);
        }

        /// <summary>
        /// Wait until the FileSystem is unmount.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="CreateFileSystem"/>.</param>
        /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <param name="milliSeconds"> is zero,
        /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <param name="milliSeconds"> is INFINITE, the function will return only when the object is signaled.</param>
        /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
        public static uint WaitForFileSystemClosed(this DokanInstance dokanInstance, uint milliSeconds)
        {
            return NativeMethods.DokanWaitForFileSystemClosed(dokanInstance.DokanHandle, milliSeconds);
        }

        /// <summary>
        /// Unmount a dokan device from a driver letter.
        /// </summary>
        /// <param name="driveLetter">Driver letter to unmount.</param>
        /// <returns><c>true</c> if device was unmount 
        /// -or- <c>false</c> in case of failure or device not found.</returns>
        public static bool Unmount(char driveLetter)
        {
            return NativeMethods.DokanUnmount(driveLetter);
        }

        /// <summary>
        /// Unmount a dokan device from a mount point.
        /// </summary>
        /// <param name="mountPoint">Mount point to unmount (<c>Z</c>, <c>Z:</c>, <c>Z:\\</c>, <c>Z:\\MyMountPoint</c>).</param>
        /// <returns><c>true</c> if device was unmount 
        /// -or- <c>false</c> in case of failure or device not found.</returns>
        public static bool RemoveMountPoint(string mountPoint)
        {
            return NativeMethods.DokanRemoveMountPoint(mountPoint);
        }

        /// <summary>
        /// Retrieve native dokan dll version supported.
        /// </summary>
        /// <returns>Return native dokan dll version supported.</returns>
        public static int Version => (int)NativeMethods.DokanVersion();

        /// <summary>
        /// Retrieve native dokan driver version supported.
        /// </summary>
        /// <returns>Return native dokan driver version supported.</returns>
        public static int DriverVersion => (int)NativeMethods.DokanDriverVersion();

        /// <summary>
        /// Dokan User FS file-change notifications
        /// </summary>
        /// <remarks> If <see cref="DokanOptions.EnableNotificationAPI"/> is passed to <see cref="Dokan.Mount"/>,
        /// the application implementing the user file system can notify
        /// the Dokan kernel driver of external file- and directory-changes.
        /// 
        /// For example, the mirror application can notify the driver about
        /// changes made in the mirrored directory so that those changes will
        /// be automatically reflected in the implemented mirror file system.
        /// 
        /// This requires the filePath passed to the respective methods
        /// to include the absolute path of the changed file including the drive-letter
        /// and the path to the mount point, e.g. "C:\Dokan\ChangedFile.txt".
        /// 
        /// These functions SHOULD NOT be called from within the implemented
        /// file system and thus be independent of any Dokan file system operation.
        ///</remarks>
        public class Notify
        {
            /// <summary>
            /// Notify Dokan that a file or directory has been created.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if the notification succeeded.</returns>
            public static bool Create(DokanInstance dokanInstance, string filePath, bool isDirectory)
            {
                return NativeMethods.DokanNotifyCreate(dokanInstance.DokanHandle, filePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been deleted.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if notification succeeded.</returns>
            /// <remarks><see cref="DokanOptions.EnableNotificationAPI"/> must be set in the mount options for this to succeed.</remarks>
            public static bool Delete(DokanInstance dokanInstance, string filePath, bool isDirectory)
            {
                return NativeMethods.DokanNotifyDelete(dokanInstance.DokanHandle, filePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that file or directory attributes have changed.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <returns>true if notification succeeded.</returns>
            /// <remarks><see cref="DokanOptions.EnableNotificationAPI"/> must be set in the mount options for this to succeed.</remarks>
            public static bool Update(DokanInstance dokanInstance, string filePath)
            {
                return NativeMethods.DokanNotifyUpdate(dokanInstance.DokanHandle, filePath);
            }

            /// <summary>
            /// Notify Dokan that file or directory extended attributes have changed.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <returns>true if notification succeeded.</returns>
            /// <remarks><see cref="DokanOptions.EnableNotificationAPI"/> must be set in the mount options for this to succeed.</remarks>
            public static bool XAttrUpdate(DokanInstance dokanInstance, string filePath)
            {
                return NativeMethods.DokanNotifyXAttrUpdate(dokanInstance.DokanHandle, filePath);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been renamed.
            /// This method supports in-place rename for file/directory within the same parent.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
            /// <param name="oldPath">Old, absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="newPath">New, absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <param name="isInSameDirectory">Indicates if the file or directory have the same parent directory.</param>
            /// <returns>true if notification succeeded.</returns>
            /// <remarks><see cref="DokanOptions.EnableNotificationAPI"/> must be set in the mount options for this to succeed.</remarks>
            public static bool Rename(DokanInstance dokanInstance, string oldPath, string newPath, bool isDirectory, bool isInSameDirectory)
            {
                return NativeMethods.DokanNotifyRename(dokanInstance.DokanHandle, oldPath,
                    newPath,
                    isDirectory,
                    isInSameDirectory);
            }
        }
    }
}
