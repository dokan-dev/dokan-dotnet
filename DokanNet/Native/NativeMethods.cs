using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    internal delegate int DokanMain(ref DOKAN_OPTIONS options, ref DOKAN_OPERATIONS operations);
    internal delegate int DokanUnmount(char driveLetter);
    internal delegate uint DokanVersion();
    internal delegate uint DokanDriverVersion();
    internal delegate int DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);
    [return:MarshalAs(UnmanagedType.Bool)]
    internal delegate bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);
    internal delegate IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);
    internal delegate void DokanMapKernelToUserCreateFileFlags(uint fileAttributes,
        uint createOptions,
        uint createDisposition,
        ref int outFileAttributesAndFlags,
        ref int outCreationDisposition);

    internal class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string library);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr library, string name);
        
        public NativeMethods(string dll = "dokan1.dll")
        {
            var library = LoadLibrary(dll);
            if (library == IntPtr.Zero) throw new DllNotFoundException($"Couldn't load {dll}");
            foreach (var field in GetType().GetFields())
            {
                var addr = GetProcAddress(library, field.Name);
                if (addr == IntPtr.Zero)
                    throw new DllNotFoundException($"Couldn't load {field.Name} in {dll}");
                field.SetValue(this, Marshal.GetDelegateForFunctionPointer(addr, field.FieldType));
            }
        }

#pragma warning disable 649
        public readonly DokanMain DokanMain;
        public readonly DokanUnmount DokanUnmount;
        public readonly DokanVersion DokanVersion;
        public readonly DokanDriverVersion DokanDriverVersion;
        public readonly DokanRemoveMountPoint DokanRemoveMountPoint;
        public readonly DokanResetTimeout DokanResetTimeout;
        public readonly DokanOpenRequestorToken DokanOpenRequestorToken;
        public readonly DokanMapKernelToUserCreateFileFlags DokanMapKernelToUserCreateFileFlags;
#pragma warning restore 649
    }
}
