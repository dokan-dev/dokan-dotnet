using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using DokanNet;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror
{
    /// <summary>
    /// Implementation of IDokanOperationsUnsafe to demonstrate usage.
    /// </summary>
    internal class UnsafeMirror : Mirror, IDokanOperationsUnsafe
    {
        /// <summary>
        /// Constructs a new unsafe mirror for the specified root path.
        /// </summary>
        /// <param name="path">Root path of mirror.</param>
        public UnsafeMirror(string path) : base(path) { }

        /// <summary>
        /// Read from file using unmanaged buffers.
        /// </summary>
        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            if (info.Context == null) // memory mapped read
            {
                using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Read))
                {
                    DoRead(stream, buffer, bufferLength, out bytesRead, offset);
                }
            }
            else // normal read
            {
                var stream = info.Context as FileStream;
                lock (stream) //Protect from overlapped read
                {
                    DoRead(stream, buffer, bufferLength, out bytesRead, offset);
                }
            }

            return Trace($"Unsafe{nameof(ReadFile)}", fileName, info, DokanResult.Success, "out " + bytesRead.ToString(),
                offset.ToString(CultureInfo.InvariantCulture));

            void DoRead(FileStream stream, IntPtr innerBuffer, uint innerBufferLength, out int innerBytesRead, long innerOffset)
            {
                NativeMethods.SetFilePointer(stream.SafeFileHandle, innerOffset);
                NativeMethods.ReadFile(stream.SafeFileHandle, innerBuffer, innerBufferLength, out innerBytesRead);
            }
        }

        /// <summary>
        /// Write to file using unmanaged buffers.
        /// </summary>
        public NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            if (info.Context == null)
            {
                using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Write))
                {
                    DoWrite(stream, buffer, bufferLength, out bytesWritten, offset);
                }
            }
            else
            {
                var stream = info.Context as FileStream;
                lock (stream) //Protect from overlapped write
                {
                    DoWrite(stream, buffer, bufferLength, out bytesWritten, offset);
                }
            }

            return Trace($"Unsafe{nameof(WriteFile)}", fileName, info, DokanResult.Success, "out " + bytesWritten.ToString(),
                offset.ToString(CultureInfo.InvariantCulture));

            void DoWrite(FileStream stream, IntPtr innerBuffer, uint innerBufferLength, out int innerBytesWritten, long innerOffset)
            {
                NativeMethods.SetFilePointer(stream.SafeFileHandle, innerOffset);
                NativeMethods.WriteFile(stream.SafeFileHandle, innerBuffer, innerBufferLength, out innerBytesWritten);
            }
        }

        /// <summary>
        /// kernel32 file method wrappers.
        /// </summary>
        private class NativeMethods
        {
            public static void SetFilePointer(SafeFileHandle fileHandle, long offset)
            {
                if (!SetFilePointerEx(fileHandle, offset, IntPtr.Zero, FILE_BEGIN))
                {
                    throw new Win32Exception();
                }
            }

            public static void ReadFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out int bytesRead)
            {
                if (!ReadFile(fileHandle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
                {
                    throw new Win32Exception();
                }
            }

            public static void WriteFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToWrite, out int bytesWritten)
            {
                if (!WriteFile(fileHandle, buffer, bytesToWrite, out bytesWritten, IntPtr.Zero))
                {
                    throw new Win32Exception();
                }
            }

            private const uint FILE_BEGIN = 0;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out int lpNumberOfBytesRead, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);
        }
    }
}
