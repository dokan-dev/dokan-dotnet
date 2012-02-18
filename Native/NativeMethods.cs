using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    internal static class NativeMethods
    {
        [DllImport("dokan.dll",ExactSpelling = true)]
        public static extern int DokanMain(ref DOKAN_OPTIONS options, ref DOKAN_OPERATIONS operations);

        [DllImport("dokan.dll",ExactSpelling = true,CharSet = CharSet.Auto)]
        public static extern int DokanUnmount(char driveLetter);

        [DllImport("dokan.dll",ExactSpelling = true)]
        public static extern uint DokanVersion();

        [DllImport("dokan.dll",ExactSpelling = true)]
        public static extern uint DokanDriverVersion();

        [DllImport("dokan.dll",ExactSpelling = true,CharSet = CharSet.Auto)]
        public static extern int DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

        [DllImport("dokan.dll",ExactSpelling = true) ][return :MarshalAs(UnmanagedType.Bool)]
        public static extern bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);

        [DllImport("dokan.dll",ExactSpelling = true)]
        public static extern IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);


        /*
        [DllImport("dokan.dll", CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/
    }
}