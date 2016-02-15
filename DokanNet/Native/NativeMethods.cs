using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    internal static class NativeMethods
    {
        [DllImport("dokan1.dll", ExactSpelling = true)]
        public static extern int DokanMain(ref DOKAN_OPTIONS options, ref DOKAN_OPERATIONS operations);

        [DllImport("dokan1.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int DokanUnmount(char driveLetter);

        [DllImport("dokan1.dll", ExactSpelling = true)]
        public static extern uint DokanVersion();

        [DllImport("dokan1.dll", ExactSpelling = true)]
        public static extern uint DokanDriverVersion();

        [DllImport("dokan1.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

        [DllImport("dokan1.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);

        [DllImport("dokan1.dll", ExactSpelling = true)]
        public static extern IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);

        [DllImport("dokan1.dll", ExactSpelling = true)]
        public static extern void DokanMapKernelToUserCreateFileFlags(uint FileAttributes,
                                                                        uint CreateOptions,
                                                                        uint CreateDisposition,
                                                                        ref int outFileAttributesAndFlags,
                                                                        ref int outCreationDisposition);

        /*
        [DllImport("dokan1.dll", CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/
    }
}
