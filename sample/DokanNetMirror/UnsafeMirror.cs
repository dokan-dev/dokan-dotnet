/*
There is various way we could have implemented this, for example we could have used the Filestream.Seek to move the 
filesystem pointer, but for didactic reasons we want to use as much pInvoke as possible.
Also in .NET6 any call through the stream seems to move the pointer back to the position memorized by the stream
https://github.com/dotnet/docs/blob/12473973716efbf21470ecbc96de0bd5e1f65e3b/docs/core/compatibility/core-libraries/6.0/filestream-doesnt-sync-offset-with-os.md
hence we want to avoid touching the filestream once obtained the safefilehandle
*/

using System;
using System.Globalization;
using System.IO;
using DokanNet;
using DokanNet.Logging;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror
{
    /// <summary>
    /// Implementation of IDokanOperationsUnsafe to demonstrate usage.
    /// </summary>
    internal class UnsafeMirror : Mirror, IDokanOperationsUnsafe
    {
        static void DoRead(SafeFileHandle handle, IntPtr buffer, uint bufferLength, out int bytesRead, long offset)
        {
            handle.SetFilePointer(offset);
            handle.ReadFile(buffer, bufferLength, out bytesRead);
        }

        static void DoWrite(SafeFileHandle handle, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset)
        {
            var newpos = Extensions.SetFilePointer(handle, offset);
            handle.WriteFile(buffer, bufferLength, out bytesWritten);
        }
        /// <summary>
        /// Constructs a new unsafe mirror for the specified root path.
        /// </summary>
        /// <param name="path">Root path of mirror.</param>
        public UnsafeMirror(ILogger logger, string path) : base(logger, path) { }

        /// <summary>
        /// Read from file using unmanaged buffers.
        /// </summary>
        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            if (info.Context == null) // memory mapped read
            {
                using (var stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Read))
                {
                    DoRead(stream.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
                }
            }
            else // normal read
            {
                var stream = info.Context as FileStream;
#pragma warning disable CA2002
                lock (stream) //Protect from overlapped read
#pragma warning restore CA2002
                {
                    DoRead(stream.SafeFileHandle, buffer, bufferLength, out bytesRead, offset);
                }
            }

            return Trace($"Unsafe{nameof(ReadFile)}", fileName, info, DokanResult.Success, "out " + bytesRead.ToString(),
                offset.ToString(CultureInfo.InvariantCulture));
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
                    DoWrite(stream.SafeFileHandle, buffer, bytesToCopy, out bytesWritten, offset);
                }
            }
            else
            {
                var stream = info.Context as FileStream;
#pragma warning disable CA2002
                lock (stream) //Protect from overlapped write
#pragma warning restore CA2002
                {
                    var bytesToCopy = (uint)GetNumOfBytesToCopy((int)bufferLength, offset, info, stream);
                    DoWrite(stream.SafeFileHandle, buffer, bytesToCopy, out bytesWritten, offset);
                }
            }

            return Trace($"Unsafe{nameof(WriteFile)}", fileName, info, DokanResult.Success, "out " + bytesWritten.ToString(),
                offset.ToString(CultureInfo.InvariantCulture));
        }
    }
}
