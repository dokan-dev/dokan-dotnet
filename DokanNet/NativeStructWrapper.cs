using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DokanNet
{
    internal class NativeStructWrapper<T> : SafeBuffer where T : class
    {
        public NativeStructWrapper(T obj) : base(ownsHandle: true)
        {
            var size = Marshal.SizeOf(obj);
            SetHandle(Marshal.AllocHGlobal(size));
            Initialize((ulong)size);
            Object = obj;
            Marshal.StructureToPtr(obj, handle, false);
        }

        public NativeStructWrapper() : base(ownsHandle: true)
        {
        }

        public T Object { get; }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
