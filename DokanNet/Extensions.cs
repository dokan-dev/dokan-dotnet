using System;
using System.IO;
using System.Runtime.InteropServices;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// <see cref="DokanInstance"/> extensions allowing to check whether it is running or to wait until it is unmount.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Check if the FileSystem is still running or not.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/>.</param>
        /// <returns>Whether the FileSystem is still running or not.</returns>
        public static Boolean IsFileSystemRunning(this DokanInstance dokanInstance)
        {
            return NativeMethods.DokanIsFileSystemRunning(dokanInstance.DokanHandle);
        }

        /// <summary>
        /// Wait until the FileSystem is unmount.
        /// </summary>
        /// <param name="dokanInstance">The dokan mount context created by <see cref="DokanInstance.DokanInstance"/>.</param>
        /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If set to zero,
        /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If set to INFINITE, the function will return only when the object is signaled.</param>
        /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
        public static UInt32 WaitForFileSystemClosed(this DokanInstance dokanInstance, UInt32 milliSeconds)
        {
            return NativeMethods.DokanWaitForFileSystemClosed(dokanInstance.DokanHandle, milliSeconds);
        }
    }
}
