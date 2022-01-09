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
        private const string DOKAN_DLL = "dokan2.dll";

        /// <summary>
        /// Initialize all required Dokan internal resources.
        /// 
        /// This needs to be called only once before trying to use <see cref="DokanMain"/> or <see cref="DokanCreateFileSystem"/> for the first time.
        /// Otherwise both will fail and raise an exception.
        /// </summary>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanInit();

        /// <summary>
        /// Release all allocated resources by <see cref="DokanInit"/> when they are no longer needed.
        ///
        /// This should be called when the application no longer expects to create a new FileSystem with
        /// <see cref="DokanMain"/> or <see cref="DokanCreateFileSystem"/> and after all devices are unmount.
        /// </summary>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanShutdown();

        /// <summary>
        /// Mount a new Dokan Volume.
        /// This function block until the device is unmount.
        /// If the mount fail, it will directly return an error.
        /// </summary>
        /// <param name="options">A <see cref="DOKAN_OPTIONS"/> that describe the mount.</param>
        /// <param name="operations">Instance of <see cref="DOKAN_OPERATIONS"/> that will be called for each request made by the kernel.</param>
        /// <returns><see cref="DokanStatus"/></returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern DokanStatus DokanMain([In] DOKAN_OPTIONS options, [In] DOKAN_OPERATIONS operations);

        /// <summary>
        /// Mount a new Dokan Volume.
        /// 
        /// It is mandatory to have called <see cref="DokanInit"/> previously to use this API.
        /// This function returns directly on device mount or on failure.
        /// <see cref="DokanWaitForFileSystemClosed"/> can be used to wait until the device is unmount.
        /// </summary>
        /// <param name="options">A <see cref="NativeStructWrapper&lt;DOKAN_OPTIONS&gt;"/> that describe the mount.</param>
        /// <param name="operations">Instance of <see cref="NativeStructWrapper&lt;DOKAN_OPERATIONS&gt;"/> that will be called for each request made by the kernel.</param>
        /// <param name="dokanInstance">Dokan mount instance context that can be used for related instance calls like <see cref="DokanIsFileSystemRunning"/>.</param>
        /// <returns><see cref="DokanStatus"/></returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern DokanStatus DokanCreateFileSystem(SafeBuffer options, SafeBuffer operations, out DokanHandle dokanInstance);

        /// <summary>
        /// Check if the FileSystem is still running or not.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
        /// <returns>Whether the FileSystem is still running or not.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern bool DokanIsFileSystemRunning(DokanHandle dokanInstance);

        /// <summary>
        /// Wait until the FileSystem is unmount.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
        /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <param name="milliSeconds"> is zero,
        /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <param name="milliSeconds"> is INFINITE, the function will return only when the object is signaled.</param>
        /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern uint DokanWaitForFileSystemClosed(DokanHandle dokanInstance, uint milliSeconds);

        /// <summary>
        /// Unmount and wait until all resources of the \c DokanInstance are released.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanCloseHandle(IntPtr dokanInstance);

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
        /// <param name="desiredAccess">DesiredAccess from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="fileAttributes">FileAttributes from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="createOptions">CreateOptions from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="createDisposition">CreateDisposition from <see cref="DokanOperationProxy.ZwCreateFileDelegate"/>.</param>
        /// <param name="outDesiredAccess">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwDesiredAccess.</param>
        /// <param name="outFileAttributesAndFlags">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwFlagsAndAttributes.</param>
        /// <param name="outCreationDisposition">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwCreationDisposition.</param>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile function (MSDN)</a>
        [DllImport(DOKAN_DLL, ExactSpelling = true)]
        public static extern void DokanMapKernelToUserCreateFileFlags(
            uint desiredAccess,
            uint fileAttributes,
            uint createOptions,
            uint createDisposition,
            ref uint outDesiredAccess,
            ref int outFileAttributesAndFlags,
            ref int outCreationDisposition);

        /*
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanIsNameInExpression([MarshalAs(UnmanagedType.LPWStr)] string expression,
                                                          // matching pattern
                                                          [MarshalAs(UnmanagedType.LPWStr)] string name, // file name
                                                          [MarshalAs(UnmanagedType.Bool)] bool ignoreCase);*/

        /// <summary>
        /// Notify Dokan that a file or directory has been created.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
        /// <param name="filePath">Full path to the file or directory, including mount point.</param>
        /// <param name="isDirectory">Indicates if the path is a directory.</param>
        /// <returns>true if the notification succeeded.</returns>
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanNotifyCreate(DokanHandle dokanInstance,
                                                    [MarshalAs(UnmanagedType.LPWStr)] string filePath,
                                                    [MarshalAs(UnmanagedType.Bool)] bool isDirectory);

        /// <summary>
        /// Notify Dokan that a file or directory has been deleted.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
        /// <param name="filePath">Full path to the file or directory, including mount point.</param>
        /// <param name="isDirectory">Indicates if the path is a directory.</param>
        /// <returns>true if notification succeeded.</returns>
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanNotifyDelete(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath,
                                                    [MarshalAs(UnmanagedType.Bool)] bool isDirectory);

        /// <summary>
        /// Notify Dokan that file or directory attributes have changed.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
        /// <param name="filePath">Full path to the file or directory, including mount point.</param>
        /// <returns>true if notification succeeded.</returns>
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanNotifyUpdate(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath);

        /// <summary>
        /// Notify Dokan that file or directory extended attributes have changed.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
        /// <param name="filePath">Full path to the file or directory, including mount point.</param>
        /// <returns>true if notification succeeded.</returns>
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanNotifyXAttrUpdate(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath);

        /// <summary>
        /// Notify Dokan that a file or directory has been renamed.
        /// </summary>
        /// <remarks>This method supports in-place rename for file/directory within the same parent.</remarks>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
        /// <param name="OldPath">Old path to the file or directory, including mount point.</param>
        /// <param name="newPath">New path to the file or directory, including mount point.</param>
        /// <param name="isDirectory">Indicates if the path is a directory.</param>
        /// <param name="isInSameDirectory">Indicates if OldPath and NewPath have the same parent.</param>
        /// <returns>true if notification succeeded.</returns>
        [DllImport(DOKAN_DLL, CharSet = CharSet.Unicode)]
        public static extern bool DokanNotifyRename(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string OldPath,
                                                    [MarshalAs(UnmanagedType.LPWStr)] string newPath,
                                                    [MarshalAs(UnmanagedType.Bool)] bool isDirectory,
                                                    [MarshalAs(UnmanagedType.Bool)] bool isInSameDirectory);

    }
}