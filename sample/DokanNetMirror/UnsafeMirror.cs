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
                stream.SafeFileHandle.SetFilePointer(innerOffset);
                stream.SafeFileHandle.ReadFile(innerBuffer, innerBufferLength, out innerBytesRead);
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
                    var bytesToCopy = (uint)GetNumOfBytesToCopy((int)bufferLength, offset, info, stream);
                    DoWrite(stream, buffer, bytesToCopy, out bytesWritten, offset);
                }
            }
            else
            {
                var stream = info.Context as FileStream;
                lock (stream) //Protect from overlapped write
                {
                    var bytesToCopy = (uint)GetNumOfBytesToCopy((int)bufferLength, offset, info, stream);
                    DoWrite(stream, buffer, bytesToCopy, out bytesWritten, offset);
                }
            }

            return Trace($"Unsafe{nameof(WriteFile)}", fileName, info, DokanResult.Success, "out " + bytesWritten.ToString(),
                offset.ToString(CultureInfo.InvariantCulture));

            void DoWrite(FileStream stream, IntPtr innerBuffer, uint innerBufferLength, out int innerBytesWritten, long innerOffset)
            {
                stream.SafeFileHandle.SetFilePointer(innerOffset);
                stream.SafeFileHandle.WriteFile(innerBuffer, innerBufferLength, out innerBytesWritten);
            }
        }
    }
}
