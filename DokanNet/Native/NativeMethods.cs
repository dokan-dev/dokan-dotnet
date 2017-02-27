using System;
using System.Runtime.InteropServices;

/// <summary>
/// Namespace for structures and classes related to native API.
/// </summary>
namespace DokanNet.Native
{
    /// <summary>
    /// Native API to the kernel Dokan driver.
    /// </summary>
    internal static class NativeMethods
    {
        private const string DOKAN_DLL = "dokan1.dll";

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// If the mount fail, it will directly return \ref DokanMain error.
        /// </summary>
        /// <param name="options">A <see cref="DOKAN_OPTIONS"/> that describe the mount.</param>
        /// <param name="operations">Instance of <see cref="DOKAN_OPERATIONS"/> that will be called for each request made by the kernel.</param>
        /// <returns>\ref DokanMain status.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern int DokanMain(ref DOKAN_OPTIONS options, ref DOKAN_OPERATIONS operations);

        /// <summary>
        /// Unmount a dokan device from a driver letter.
        /// </summary>
        /// <param name="driveLetter">Dokan driver letter to unmount.</param>
        /// <returns><c>True</c> if device was unmount or <c>false</c> in case of failure or device not found.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern bool DokanUnmount(char driveLetter);

        /// <summary>
        /// Get the version of Dokan.
        /// The returned <see cref="uint"/> is the version number without the dots.
        /// </summary>
        /// <returns>The version of Dokan</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanVersion();

        /// <summary>
        /// Get the version of the Dokan driver.
        /// The returned <see cref="uint"/> is the version number without the dots.
        /// </summary>
        /// <returns>The version of Dokan driver.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanDriverVersion();

        /// <summary>
        /// Unmount a dokan device from a mount point
        /// </summary>
        /// <param name="mountPoint">MountPoint Mount point to unmount ("<c>Z</c>", 
        /// "<c>Z:</c>", "<c>Z:\\</c>", "<c>Z:\MyMountPoint</c>").</param>
        /// <returns><c>True</c> if device was unmount or <c>false</c> in case of failure or device not found.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern bool DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="timeout">Extended time in milliseconds requested.</param>
        /// <param name="rawFileInfo"><see cref="DokanFileInfo"/> of the operation to extend.</param>
        /// <returns>If the operation was successful.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DokanResetTimeout(uint timeout, DokanFileInfo rawFileInfo);

        /// <summary>
        /// Get the handle to Access Token.
        /// This method needs be called in <see cref="IDokanOperations.CreateFile"/> callback.
        /// The caller must call <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211(v=vs.85).aspx">CloseHandle (MSDN)</a> for the returned handle.
        /// </summary>
        /// <param name="rawFileInfo">
        /// A <see cref="DokanFileInfo"/> of the operation to extend.
        /// </param>
        /// <returns>
        /// A handle to the account token for the user on whose behalf the code is running.
        /// </returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern IntPtr DokanOpenRequestorToken(DokanFileInfo rawFileInfo);

        /// <summary>
        /// Convert <see cref="DokanOperationProxy.ZwCreateFileDelegate"/> parameters to <see cref="IDokanOperations.CreateFile"/> parameters.
        /// </summary>
        /// <param name="fileAttributes">FileAttributes from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="createOptions">CreateOptions from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="createDisposition">CreateDisposition from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="outFileAttributesAndFlags">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwFlagsAndAttributes.</param>
        /// <param name="outCreationDisposition">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwCreationDisposition.</param>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile function (MSDN)</a>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanMapKernelToUserCreateFileFlags(
            uint fileAttributes,
            uint createOptions,
            uint createDisposition,
            ref int outFileAttributesAndFlags,
            ref int outCreationDisposition);

        /*
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/
    }
}