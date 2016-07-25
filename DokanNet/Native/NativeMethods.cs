using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    internal static class NativeMethods
    {
        private const string DOKAN_DLL = "dokan1.dll";

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern int DokanMain(ref DOKAN_OPTIONS options, ref DOKAN_OPERATIONS operations);

        [DllImport(DOKAN_DLL, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DokanUnmount(char driveLetter);

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanVersion();

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanDriverVersion();

        [DllImport(DOKAN_DLL, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanMapKernelToUserCreateFileFlags(uint fileAttributes, uint createOptions, uint createDisposition, ref int outFileAttributesAndFlags, ref int outCreationDisposition);

        /*
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/
    }
}
