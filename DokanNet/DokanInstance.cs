using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Created by <see cref="Dokan.CreateFileSystem"/>.
    /// It holds all the resources required to be alive for the time of the mount.
    /// </summary>
    public class DokanInstance : IDisposable
    {
        internal NativeStructWrapper<DOKAN_OPTIONS> DokanOptions;
        internal NativeStructWrapper<DOKAN_OPERATIONS> DokanOperations;
        internal DokanHandle DokanHandle;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
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

                disposedValue = true;
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
