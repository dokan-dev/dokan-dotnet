using System;
using System.Runtime.Versioning;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Helper methods to %Dokan.
    /// </summary>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public class Dokan : IDisposable
    {
        private readonly ILogger _logger;
        private bool _disposed;

        /// <summary>
        /// Initialize all required Dokan internal resources.
        /// 
        /// This needs to be called only once before trying to use <see cref="DokanInstance.DokanInstance"/> for the first time.
        /// Otherwise both will fail and raise an exception.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> that will log all DokanNet debug informations.</param>
        public Dokan(ILogger logger)
        {
            _logger = logger;
            NativeMethods.DokanInit();
        }

        /// <summary>
        /// Unmount a dokan device from a driver letter.
        /// </summary>
        /// <param name="driveLetter">Driver letter to unmount.</param>
        /// <returns><c>true</c> if device was unmount 
        /// -or- <c>false</c> in case of failure or device not found.</returns>
        public bool Unmount(char driveLetter)
        {
            return NativeMethods.DokanUnmount(driveLetter);
        }

        /// <summary>
        /// Unmount a dokan device from a mount point.
        /// </summary>
        /// <param name="mountPoint">Mount point to unmount (<c>Z</c>, <c>Z:</c>, <c>Z:\\</c>, <c>Z:\\MyMountPoint</c>).</param>
        /// <returns><c>true</c> if device was unmount 
        /// -or- <c>false</c> in case of failure or device not found.</returns>
        public bool RemoveMountPoint(string mountPoint)
        {
            return NativeMethods.DokanRemoveMountPoint(mountPoint);
        }

        /// <summary>
        /// Retrieve native dokan dll version supported.
        /// </summary>
        /// <returns>Return native dokan dll version supported.</returns>
        public int Version => (int)NativeMethods.DokanVersion();

        /// <summary>
        /// Retrieve native dokan driver version supported.
        /// </summary>
        /// <returns>Return native dokan driver version supported.</returns>
        public int DriverVersion => (int)NativeMethods.DokanDriverVersion();

        /// <summary>
        /// Dokan User FS file-change notifications
        /// </summary>
        /// <remarks>The application implementing the user file system can notify
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
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if the notification succeeded.</returns>
            public static bool Create(DokanInstance dokanInstance, string filePath, bool isDirectory)
            {
                return dokanInstance.NotifyCreate(filePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been deleted.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool Delete(DokanInstance dokanInstance, string filePath, bool isDirectory)
            {
                return dokanInstance.NotifyDelete(filePath, isDirectory);
            }

            /// <summary>
            /// Notify Dokan that file or directory attributes have changed.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool Update(DokanInstance dokanInstance, string filePath)
            {
                return dokanInstance.NotifyUpdate(filePath);
            }

            /// <summary>
            /// Notify Dokan that file or directory extended attributes have changed.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/></param>
            /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool XAttrUpdate(DokanInstance dokanInstance, string filePath)
            {
                return dokanInstance.NotifyXAttrUpdate(filePath);
            }

            /// <summary>
            /// Notify Dokan that a file or directory has been renamed.
            /// This method supports in-place rename for file/directory within the same parent.
            /// </summary>
            /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/></param>
            /// <param name="oldPath">Old, absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="newPath">New, absolute path to the file or directory, including the mount-point of the file system.</param>
            /// <param name="isDirectory">Indicates if the path is a directory.</param>
            /// <param name="isInSameDirectory">Indicates if the file or directory have the same parent directory.</param>
            /// <returns>true if notification succeeded.</returns>
            public static bool Rename(DokanInstance dokanInstance, string oldPath, string newPath, bool isDirectory, bool isInSameDirectory)
            {
                return dokanInstance.NotifyRename(oldPath,
                    newPath,
                    isDirectory,
                    isInSameDirectory);
            }
        }

        /// <summary>
        /// Dispose the native Dokan Library resources
        /// </summary>
        /// <param name="disposing">Whether this was called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }
                //  free unmanaged resources
                NativeMethods.DokanShutdown();
                // set fields to null
                _disposed = true;
            }
        }

        /// <summary>
        /// Dispose the native Dokan Library resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// </summary>
        ~Dokan()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
