using System;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Created by <see cref="Dokan.CreateFileSystem"/>.
    /// It holds all the resources required to be alive for the time of the mount.
    /// </summary>
    public class DokanInstance : IDisposable
    {
        internal NativeStructWrapper<DOKAN_OPTIONS> DokanOptions { get; private set; }
        internal NativeStructWrapper<DOKAN_OPERATIONS> DokanOperations { get; private set; }
        internal DokanHandle DokanHandle { get; private set; }
        private readonly object _disposeLock;
        private bool _disposed = false;
        public bool IsDisposed
        {
            get { lock (_disposeLock) return _disposed; }
        }

        internal DokanInstance(DOKAN_OPTIONS options, DOKAN_OPERATIONS operations)
        {
            DokanOptions = NativeStructWrapper.Wrap(options);
            DokanOperations = NativeStructWrapper.Wrap(operations);
            _disposeLock = new object();
            var status = NativeMethods.DokanCreateFileSystem(DokanOptions, DokanOperations, out var handle);
            if (status != DokanStatus.Success)
            {
                throw new DokanException(status);
            }
            DokanHandle = handle;
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        // Dispose managed state (managed objects)
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

        ~DokanInstance()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
