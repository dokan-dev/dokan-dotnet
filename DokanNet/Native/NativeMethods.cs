using LTRData.Extensions.Native.Memory;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DokanNet.Native;

#pragma warning disable IDE0079 // Remove unnecessary suppression

internal delegate void NativeWaitOrTimerCallback(nint state, bool timedOut);

/// <summary>
/// Native API to the kernel Dokan driver.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal static partial class NativeMethods
{
    private const string DOKAN_DLL = "dokan2.dll";

#if NET7_0_OR_GREATER
    /// <summary>
    /// Initialize all required Dokan internal resources.
    /// 
    /// This needs to be called only once before trying to use <see cref="DokanMain"/> or <see cref="DokanCreateFileSystem"/> for the first time.
    /// Otherwise both will fail and raise an exception.
    /// </summary>
    [LibraryImport(DOKAN_DLL)]
    public static partial void DokanInit();

    /// <summary>
    /// Release all allocated resources by <see cref="DokanInit"/> when they are no longer needed.
    ///
    /// This should be called when the application no longer expects to create a new FileSystem with
    /// <see cref="DokanMain"/> or <see cref="DokanCreateFileSystem"/> and after all devices are unmount.
    /// </summary>
    [LibraryImport(DOKAN_DLL)]
    public static partial void DokanShutdown();

    /// <summary>
    /// Converts native Win32 error code to NTSTATUS
    /// </summary>
    /// <param name="win32Error"></param>
    /// <returns></returns>
    [LibraryImport(DOKAN_DLL)]
    public static partial NtStatus DokanNtStatusFromWin32(int win32Error);

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
    [LibraryImport(DOKAN_DLL)]
    public static partial DokanStatus DokanCreateFileSystem(SafeBuffer options, SafeBuffer operations, out DokanHandle dokanInstance);

    /// <summary>
    /// Register a callback for when the FileSystem is unmounted.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    /// <param name="handle">Wait handle representing the registered wait. Needs to be freed by calling <see cref="DokanUnregisterWaitForFileSystemClosed"/>.</param>
    /// <param name="callback">Callback function to be called when file system is unmounted or timeout occurs.</param>
    /// <param name="context">Parameter to send to callback function.</param>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <paramref name="milliSeconds" /> is zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <paramref name="milliSeconds" /> is INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
    [LibraryImport(DOKAN_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanRegisterWaitForFileSystemClosed(DokanHandle? dokanInstance, out nint handle, NativeWaitOrTimerCallback callback, nint context, uint milliSeconds);

    /// <summary>
    /// Unregister a callback for when the FileSystem is unmounted.
    /// </summary>
    /// <param name="handle">Handle returned in handle parameter in previous call to <see cref="DokanRegisterWaitForFileSystemClosed"/></param>.
    /// <param name="waitForCallbacks">Indicates whether to wait for callbacks to complete before returning. Normally true unless called from same thread as callback function.</param>
    /// <returns></returns>
    [LibraryImport(DOKAN_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanUnregisterWaitForFileSystemClosed(nint handle, [MarshalAs(UnmanagedType.Bool)] bool waitForCallbacks);

    /// <summary>
    /// Check if the FileSystem is still running or not.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    /// <returns>Whether the FileSystem is still running or not.</returns>
    [LibraryImport(DOKAN_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanIsFileSystemRunning(DokanHandle dokanInstance);

    /// <summary>
    /// Wait until the FileSystem is unmount.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <paramref name="milliSeconds" /> is zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <paramref name="milliSeconds" /> is INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
    [LibraryImport(DOKAN_DLL)]
    public static partial uint DokanWaitForFileSystemClosed(DokanHandle dokanInstance, uint milliSeconds);

    /// <summary>
    /// Unmount and wait until all resources of the \c DokanInstance are released.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    [LibraryImport(DOKAN_DLL)]
    public static partial void DokanCloseHandle(nint dokanInstance);

    /// <summary>
    /// Unmount a dokan device from a driver letter.
    /// </summary>
    /// <param name="driveLetter">Dokan driver letter to unmount.</param>
    /// <returns><c>True</c> if device was unmount or <c>false</c> in case of failure or device not found.</returns>
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanUnmount(char driveLetter);

    /// <summary>
    /// Get the version of Dokan.
    /// The returned <see cref="uint"/> is the version number without the dots.
    /// </summary>
    /// <returns>The version of Dokan</returns>
    [LibraryImport(DOKAN_DLL)]
    public static partial uint DokanVersion();

    /// <summary>
    /// Get the version of the Dokan driver.
    /// The returned <see cref="uint"/> is the version number without the dots.
    /// </summary>
    /// <returns>The version of Dokan driver.</returns>
    [LibraryImport(DOKAN_DLL)]
    public static partial uint DokanDriverVersion();

    /// <summary>
    /// Unmount a dokan device from a mount point
    /// </summary>
    /// <param name="mountPoint">MountPoint Mount point to unmount ("<c>Z</c>", 
    /// "<c>Z:</c>", "<c>Z:\\</c>", "<c>Z:\MyMountPoint</c>").</param>
    /// <returns><c>True</c> if device was unmount or <c>false</c> in case of failure or device not found.</returns>
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanRemoveMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint);

    /// <summary>
    /// Convert <see cref="DokanOperationProxy.ZwCreateFileProxy"/> parameters to <see cref="IDokanOperations.CreateFile"/> parameters.
    /// </summary>
    /// <param name="desiredAccess">DesiredAccess from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="fileAttributes">FileAttributes from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="createOptions">CreateOptions from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="createDisposition">CreateDisposition from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="outDesiredAccess">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwDesiredAccess.</param>
    /// <param name="outFileAttributesAndFlags">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwFlagsAndAttributes.</param>
    /// <param name="outCreationDisposition">New <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile (MSDN)</a> dwCreationDisposition.</param>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx">CreateFile function (MSDN)</a>
    [LibraryImport(DOKAN_DLL)]
    public static partial void DokanMapKernelToUserCreateFileFlags(
        uint desiredAccess,
        uint fileAttributes,
        uint createOptions,
        uint createDisposition,
        out uint outDesiredAccess,
        out int outFileAttributesAndFlags,
        out int outCreationDisposition);

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
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanNotifyCreate(DokanHandle dokanInstance,
                                                [MarshalAs(UnmanagedType.LPWStr)] string filePath,
                                                [MarshalAs(UnmanagedType.Bool)] bool isDirectory);

    /// <summary>
    /// Notify Dokan that a file or directory has been deleted.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
    /// <param name="filePath">Full path to the file or directory, including mount point.</param>
    /// <param name="isDirectory">Indicates if the path is a directory.</param>
    /// <returns>true if notification succeeded.</returns>
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanNotifyDelete(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath,
                                                [MarshalAs(UnmanagedType.Bool)] bool isDirectory);

    /// <summary>
    /// Notify Dokan that file or directory attributes have changed.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
    /// <param name="filePath">Full path to the file or directory, including mount point.</param>
    /// <returns>true if notification succeeded.</returns>
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanNotifyUpdate(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath);

    /// <summary>
    /// Notify Dokan that file or directory extended attributes have changed.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/></param>
    /// <param name="filePath">Full path to the file or directory, including mount point.</param>
    /// <returns>true if notification succeeded.</returns>
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanNotifyXAttrUpdate(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string filePath);

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
    [LibraryImport(DOKAN_DLL, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DokanNotifyRename(DokanHandle dokanInstance, [MarshalAs(UnmanagedType.LPWStr)] string OldPath,
                                                [MarshalAs(UnmanagedType.LPWStr)] string newPath,
                                                [MarshalAs(UnmanagedType.Bool)] bool isDirectory,
                                                [MarshalAs(UnmanagedType.Bool)] bool isInSameDirectory);
#else
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
    /// Converts native Win32 error code to NTSTATUS
    /// </summary>
    /// <param name="win32Error"></param>
    /// <returns></returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    public static extern NtStatus DokanNtStatusFromWin32(int win32Error);

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
    public static extern bool DokanIsFileSystemRunning(DokanHandle? dokanInstance);

    /// <summary>
    /// Wait until the FileSystem is unmount.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <paramref name="milliSeconds" /> is zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <paramref name="milliSeconds" /> is INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    public static extern uint DokanWaitForFileSystemClosed(DokanHandle? dokanInstance, uint milliSeconds);

    /// <summary>
    /// Register a callback for when the FileSystem is unmounted.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    /// <param name="handle">Wait handle representing the registered wait. Needs to be freed by calling <see cref="DokanUnregisterWaitForFileSystemClosed"/>.</param>
    /// <param name="callback">Callback function to be called when file system is unmounted or timeout occurs.</param>
    /// <param name="context">Parameter to send to callback function.</param>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <paramref name="milliSeconds" /> is zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <paramref name="milliSeconds" /> is INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    public static extern bool DokanRegisterWaitForFileSystemClosed(DokanHandle? dokanInstance, out nint handle, NativeWaitOrTimerCallback callback, nint context, uint milliSeconds);

    /// <summary>
    /// Unregister a callback for when the FileSystem is unmounted.
    /// </summary>
    /// <param name="handle">Handle returned in handle parameter in previous call to <see cref="DokanRegisterWaitForFileSystemClosed" /></param>.
    /// <param name="waitForCallbacks">Indicates whether to wait for callbacks to complete before returning. Normally true unless called from same thread as callback function.</param>
    /// <returns></returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    public static extern bool DokanUnregisterWaitForFileSystemClosed(nint handle, bool waitForCallbacks);

    /// <summary>
    /// Unmount and wait until all resources of the \c DokanInstance are released.
    /// </summary>
    /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanCreateFileSystem"/>.</param>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    public static extern void DokanCloseHandle(nint dokanInstance);

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
    /// Convert <see cref="DokanOperationProxy.ZwCreateFileProxy"/> parameters to <see cref="IDokanOperations.CreateFile"/> parameters.
    /// </summary>
    /// <param name="desiredAccess">DesiredAccess from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="fileAttributes">FileAttributes from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="createOptions">CreateOptions from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
    /// <param name="createDisposition">CreateDisposition from <see cref="DokanOperationProxy.ZwCreateFileProxy"/>.</param>
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
        out uint outDesiredAccess,
        out int outFileAttributesAndFlags,
        out int outCreationDisposition);

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
#endif

    /// <summary>
    /// Extends the time out of the current IO operation in driver.
    /// </summary>
    /// <param name="timeout">Extended time in milliseconds requested.</param>
    /// <param name="rawFileInfo"><see cref="DokanFileInfo"/> of the operation to extend.</param>
    /// <returns>If the operation was successful.</returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Types not supported")]
    public static extern bool DokanResetTimeout(uint timeout, in DokanFileInfo rawFileInfo);

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
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Types not supported")]
    public static extern SafeAccessTokenHandle DokanOpenRequestorToken(in DokanFileInfo rawFileInfo);

    /// <summary>
    /// Mount a new Dokan Volume.
    /// This function block until the device is unmount.
    /// If the mount fail, it will directly return an error.
    /// </summary>
    /// <param name="options">A <see cref="DOKAN_OPTIONS"/> that describe the mount.</param>
    /// <param name="operations">Instance of <see cref="DOKAN_OPERATIONS"/> that will be called for each request made by the kernel.</param>
    /// <returns><see cref="DokanStatus"/></returns>
    [DllImport(DOKAN_DLL, ExactSpelling = true)]
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Types not supported")]
    public static extern DokanStatus DokanMain([In] DOKAN_OPTIONS options, [In] DOKAN_OPERATIONS operations);
}
