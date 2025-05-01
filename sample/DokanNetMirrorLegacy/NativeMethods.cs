﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Sets the date and time that the specified file or directory was created, last accessed, or last modified.
        /// </summary>
        /// <param name="hFile">A <see cref="SafeFileHandle"/> to the file or directory. 
        /// To get the handler, <see cref="System.IO.FileStream.SafeFileHandle"/> can be used.</param>
        /// <param name="lpCreationTime">A Windows File Time that contains the new creation date and time 
        /// for the file or directory. 
        /// If the application does not need to change this information, set this parameter to 0.</param>
        /// <param name="lpLastAccessTime">A Windows File Time that contains the new last access date and time 
        /// for the file or directory. The last access time includes the last time the file or directory 
        /// was written to, read from, or (in the case of executable files) run. 
        /// If the application does not need to change this information, set this parameter to 0.</param>
        /// <param name="lpLastWriteTime">A Windows File Time that contains the new last modified date and time 
        /// for the file or directory. If the application does not need to change this information, 
        /// set this parameter to 0.</param>
        /// <returns>If the function succeeds, the return value is <c>true</c>.</returns>
        /// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724933">SetFileTime function (MSDN)</a>
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer, [MarshalAs(UnmanagedType.U4)] System.IO.SeekOrigin dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}