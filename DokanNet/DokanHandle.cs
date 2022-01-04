using System.Runtime.InteropServices;
using DokanNet.Native;
using Microsoft.Win32.SafeHandles;

namespace DokanNet
{
    /// <summary>
    /// This class wraps a native DOKAN_HANDLE.
    /// 
    /// Since this class derives form SafeHandle, it is automatically marshalled as
    /// the native handle it represents to and from native code in for example P/Invoke
    /// calls. It also uses reference counting and guaranteed to stay alive during such calls.
    /// <see cref="SafeHandle"/>
    /// </summary>
    internal class DokanHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Initializes a new empty instance, specifying whether the handle is to be reliably released.
        /// Used internally by native marshaller and not intended to be used directly from user code.
        /// </summary>
        /// <param name="ownsHandle">true to reliably release the handle during the finalization phase; false to prevent
        /// reliable release (not recommended).</param>
        public DokanHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        /// <summary>
        /// Initializes an empty instance. Used internally by native marshaller and
        /// not intended to be used directly from user code.
        /// </summary>
        public DokanHandle() : base(ownsHandle: true)
        {
        }

        /// <summary>
        /// Releases the native DOKAN_HANDLE wrapped by this instance.
        /// </summary>
        /// <returns>Always returns true</returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.DokanCloseHandle(handle);
            return true;
        }
    }
}
