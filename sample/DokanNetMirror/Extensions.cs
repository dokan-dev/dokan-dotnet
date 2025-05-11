using System;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror;

public static class Extensions
{
    /// <returns>returns the new position of the file pointer</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Win32Exception"></exception>
    public static long SetFilePointer(this SafeFileHandle fileHandle, long offset, System.IO.SeekOrigin seekOrigin = System.IO.SeekOrigin.Begin)
    {
        if (fileHandle.IsInvalid)
        {
            throw new InvalidOperationException("can't set pointer on an invalid handle");
        }

        var wasSet = NativeMethods.SetFilePointerEx(fileHandle, offset, out var newposition, seekOrigin);
        if (!wasSet)
        {
            throw new Win32Exception();
        }

        return newposition;
    }

    public static void ReadFile(this SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out int bytesRead)
    {
        if (fileHandle.IsInvalid)
        {
            throw new InvalidOperationException("can't set pointer on an invalid handle");
        }

        if (!NativeMethods.ReadFile(fileHandle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
        {
            throw new Win32Exception();
        }
    }

    public static void WriteFile(this SafeFileHandle fileHandle, IntPtr buffer, uint bytesToWrite, out int bytesWritten)
    {
        if (fileHandle.IsInvalid)
        {
            throw new InvalidOperationException("can't set pointer on an invalid handle");
        }

        if (!NativeMethods.WriteFile(fileHandle, buffer, bytesToWrite, out bytesWritten, IntPtr.Zero))
        {
            throw new Win32Exception();
        }
    }
}
