using DokanNet.Native;
using Microsoft.Win32.SafeHandles;

namespace DokanNet
{
    internal class DokanHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public DokanHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        public DokanHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.DokanCloseHandle(handle);
            return true;
        }
    }
}
