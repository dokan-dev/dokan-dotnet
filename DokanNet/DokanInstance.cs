using System;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Created by <see cref="DokanInstanceBuilder.Build"/>.
    /// It holds all the resources required to be alive for the time of the mount.
    /// </summary>
    public class DokanInstance : IDisposable
    {
        private static DOKAN_OPERATIONS PrepareOperations(ILogger logger, IDokanOperations operations)
        {
            var dokanOperationProxy = new DokanOperationProxy(logger, operations);

            return new DOKAN_OPERATIONS
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
        }

        internal NativeStructWrapper<DOKAN_OPTIONS> DokanOptions { get; private set; }
        internal NativeStructWrapper<DOKAN_OPERATIONS> DokanOperations { get; private set; }
        internal DokanHandle DokanHandle { get; private set; }
        private readonly object _disposeLock;
        private bool _disposed = false;

        /// <summary>
        /// Whether the object was already disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { lock (_disposeLock) return _disposed; }
        }

        /// <summary>
        /// Mount the filesystem described by <see cref="DOKAN_OPTIONS"/>.
        // <see cref="IDokanOperations"/> will start to received operations from the system and applications for this device.
        /// See <see cref="DokanInstanceBuilder"/>
        /// </summary>
        internal DokanInstance(ILogger logger, DOKAN_OPTIONS options, IDokanOperations operations)
        {
            DokanOptions = NativeStructWrapper.Wrap(options);
            var preparedOperations = PrepareOperations(logger, operations);
            DokanOperations = NativeStructWrapper.Wrap(preparedOperations);
            _disposeLock = new object();
            var status = NativeMethods.DokanCreateFileSystem(DokanOptions, DokanOperations, out var handle);
            if (status != DokanStatus.Success)
            {
                throw new DokanException(status);
            }
            DokanHandle = handle;
        }

        /// <summary>
        /// Dispose the native Dokan Library resources
        /// </summary>
        /// <param name="disposing">Whether this was called from <see cref="Dispose()"/></param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        DokanHandle?.Dispose();     // This calls DokanCloseHandle and waits for dismount
                        DokanOptions?.Dispose();    // After that, it is safe to free unmanaged memory
                        DokanOperations?.Dispose();
                    }

                    // Free unmanaged resources (unmanaged objects) and override finalizer

                    // Set fields to null
                    DokanOptions = null;
                    DokanOperations = null;
                    DokanHandle = null;

                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Destructor that force dispose
        /// </summary>
        ~DokanInstance()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
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
    }
}
