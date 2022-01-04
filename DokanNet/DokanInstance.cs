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
                    // TODO: dispose managed state (managed objects)
                    DokanOptions?.Dispose();
                    DokanOperations?.Dispose();
                    DokanHandle?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer

                // TODO: set large fields to null
                DokanOptions = null;
                DokanOperations = null;
                DokanHandle = null;

                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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
