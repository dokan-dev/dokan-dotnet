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

        /// <summary>
        /// Get the version of Dockan.
        /// The returned <see cref="uint"/> is the version number without the dots.
        /// </summary>
        /// <returns>The version of dockan</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanVersion();

        /// <summary>
        /// Get the version of Dockan driver
        /// The returned <see cref="uint"/> is the version number without the dots.
        /// </summary>
        /// <returns>The version of dockan driver</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanDriverVersion();

        [DllImport(DOKAN_DLL, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to extend with</param>
        /// <param name="rawFileInfo">The <see cref="DokanFileInfo"/> to operand on</param>
        /// <returns>If the operation was successful</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);

        /// <summary>
        /// Get the handle to the account token.
        /// This method needs be called in <see cref="IDokanOperations.CreateFile"/>, OpenDirectory or CreateDirectly
        /// callback.
        /// </summary>
        /// <returns>The account token for the user on whose behalf the code is running.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);

        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanMapKernelToUserCreateFileFlags(uint fileAttributes, uint createOptions,
            uint createDisposition, ref int outFileAttributesAndFlags, ref int outCreationDisposition);

        /*
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/
    }
}