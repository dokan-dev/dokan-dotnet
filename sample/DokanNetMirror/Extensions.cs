using System;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror
{
    internal static class Extensions
    {
        public static void SetFilePointer(this SafeFileHandle fileHandle, long offset)
        {
            if (!NativeMethods.SetFilePointerEx(fileHandle, offset, IntPtr.Zero, FILE_BEGIN))
            {
                throw new Win32Exception();
            }
        }

        public static void ReadFile(this SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out int bytesRead)
        {
            if (!NativeMethods.ReadFile(fileHandle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
        }

        public static void WriteFile(this SafeFileHandle fileHandle, IntPtr buffer, uint bytesToWrite, out int bytesWritten)
        {
            if (!NativeMethods.WriteFile(fileHandle, buffer, bytesToWrite, out bytesWritten, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
        }

        private const uint FILE_BEGIN = 0;
    }
}
