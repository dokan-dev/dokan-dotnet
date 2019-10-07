using System;
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
        private const ushort DOKAN_VERSION = 130;

        #endregion Dokan Driver Options

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
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
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, 0, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="threadCount">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, DOKAN_VERSION, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="threadCount">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, TimeSpan.FromSeconds(20), string.Empty,
                512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="threadCount">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, string.Empty, 512, 512, logger);
        }

        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="threadCount">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, string uncName, ILogger logger = null)
        {
            Mount(operations, mountPoint, mountOptions, threadCount, version, timeout, uncName, 512, 512, logger);
        }


        /// <summary>
        /// Mount a new %Dokan Volume.
        /// This function block until the device is unmount.
        /// </summary>
        /// <param name="operations">Instance of <see cref="IDokanOperations"/> that will be called for each request made by the kernel.</param>
        /// <param name="mountPoint">Mount point. Can be <c>M:\\</c> (drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).</param>
        /// <param name="mountOptions"><see cref="DokanOptions"/> features enable for the mount.</param>
        /// <param name="threadCount">Number of threads to be used internally by %Dokan library. More thread will handle more event at the same time.</param>
        /// <param name="version">Version of the dokan features requested (Version "123" is equal to %Dokan version 1.2.3).</param>
        /// <param name="timeout">Max timeout in ms of each request before dokan give up.</param>
        /// <param name="uncName">UNC name used for network volume.</param>
        /// <param name="allocationUnitSize">Allocation Unit Size of the volume. This will behave on the file size.</param>
        /// <param name="sectorSize">Sector Size of the volume. This will behave on the file size.</param>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        /// <exception cref="DokanException">If the mount fails.</exception>
        public static void Mount(this IDokanOperations operations, string mountPoint, DokanOptions mountOptions,
            int threadCount, int version, TimeSpan timeout, string uncName = null, int allocationUnitSize = 512,
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
                ThreadCount = (ushort)threadCount,
                Options = (uint)mountOptions,
                Timeout = (uint)timeout.TotalMilliseconds,
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

            DokanStatus status = (DokanStatus)NativeMethods.DokanMain(ref dokanOptions, ref dokanOperations);
            if (status != DokanStatus.Success)
            {
                throw new DokanException(status);
            }
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
        /// Methods that notify the kernel of something filesystem-related without going
        /// through the normal filesystem flow.
        /// </summary>
        /// <remarks>All methods in this class take full file paths, not filenames as
        /// presented by Dokan-dotnet.  This means that you must prepend the mount
        /// point of any affected file (after removing any trailing <see cref="System.IO.Path.PathSeparator"/>)
        /// to the filename that dokan-dotnet hands your <see cref="IDokanOperations"/>
        /// implementation.</remarks>
        /// <example>When DotNetMirror runs, things can happen on the original filesystem that aren't immediately
        /// displayed on the mirrored filesystem.  This is because Dokany doesn't know what the filesystem
        /// implementation is actually doing, and doesn't understand that events that happen outside of the
        /// mounted filesystem actually relate to what it should do.  DotNetMirror can set up a
        /// <see cref="System.IO.FileSystemWatcher"/> to be told of what happens on the original filesystem,
        /// which events can then notify the kernel so that the kernel can notify Explorer that it needs to
        /// refresh the directory listing that it's showing for the mirrored drive.</example>
        public class NotifyTheKernel
        {
            /// <summary>
            /// Notify Dokan that a file or directory has been created outside the normal filesystem execution flow.
            /// </summary>
            /// <param name="FilePath">Path within the filesystem to the file or directory.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if the notification succeeded.</returns>
            public static bool DokanNotifyCreate(string FilePath, bool isDirectory)
            {
                return NativeMethods.DokanNotifyCreate(FilePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been deleted outside the normal filesystem execution flow.
            /// </summary>
            /// <param name="FilePath">Full path to the file or directory.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool DokanNotifyDelete(string FilePath, bool isDirectory)
            {
                return NativeMethods.DokanNotifyDelete(FilePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that file or directory attributes have changed outside the normal filesystem execution flow.
            /// </summary>
            /// <param name="FilePath">Full path to the file or directory.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool DokanNotifyUpdate(string FilePath)
            {
                return NativeMethods.DokanNotifyUpdate(FilePath);
            }

            /// <summary>
            /// Notify Dokan that file or directory extended attributes have changed outside the normal filesystem execution flow.
            /// </summary>
            /// <param name="FilePath">Full path to the file or directory.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool DokanNotifyXAttrUpdate(string FilePath)
            {
                return NativeMethods.DokanNotifyXAttrUpdate(FilePath);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been renamed outside the normal filesystem execution flow.
            /// </summary>
            /// <remarks>This method supports in-place rename for file/directory within the same parent.</remarks>
            /// <param name="OldPath">Full path to the old name of the file or directory.</param>
            /// <param name="NewPath">Full path to the new name of the file or directory.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <param name="isInSameDirectory">Indicates if OldPath and NewPath have the same parent.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool DokanNotifyRename(string OldPath, string NewPath, bool isDirectory, bool isInSameDirectory)
            {
                return NativeMethods.DokanNotifyRename(OldPath,
                    NewPath,
                    isDirectory,
                    isInSameDirectory);
            }
        }
    }
}
