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
using System.Runtime.Versioning;
using DokanNet;
using DokanNet.Logging;
using Microsoft.Win32.SafeHandles;
using LTRData.Extensions.Native.Memory;
using FileAccess = System.IO.FileAccess;

namespace DokanNetMirror;

/// <summary>
/// Implementation of IDokanOperationsUnsafe to demonstrate usage.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal class UnsafeMirror : Mirror
{
    static void DoRead(SafeFileHandle handle, nint buffer, int bufferLength, out int bytesRead, long offset)
    {
        handle.SetFilePointer(offset);
        handle.ReadFile(buffer, (uint)bufferLength, out bytesRead);
    }

    static void DoWrite(SafeFileHandle handle, nint buffer, int bufferLength, out int bytesWritten, long offset)
    {
        var newpos = Extensions.SetFilePointer(handle, offset);
        handle.WriteFile(buffer, (uint)bufferLength, out bytesWritten);
    }

    /// <summary>
    /// Constructs a new unsafe mirror for the specified root path.
    /// </summary>
    /// <param name="path">Root path of mirror.</param>
    public UnsafeMirror(ILogger logger, string path) : base(logger, path) { }

    /// <summary>
    /// Read from file using unmanaged buffers.
    /// </summary>
    public override NtStatus ReadFile(ReadOnlyNativeMemory<char> fileName, NativeMemory<byte> buffer, out int bytesRead, long offset, ref DokanFileInfo info)
    {
        if (info.Context is not FileStream stream) // memory mapped read
        {
            using (stream = new FileStream(GetPath(fileName), FileMode.Open, System.IO.FileAccess.Read))
            {
                DoRead(stream.SafeFileHandle, buffer.Address, buffer.Length, out bytesRead, offset);
            }
        }
        else // normal read
        {
#pragma warning disable CA2002
            lock (stream) //Protect from overlapped read
#pragma warning restore CA2002
            {
                DoRead(stream.SafeFileHandle, buffer.Address, buffer.Length, out bytesRead, offset);
            }
        }

        return Trace($"Unsafe{nameof(ReadFile)}", fileName, info, DokanResult.Success, "out " + bytesRead.ToString(),
            offset.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Write to file using unmanaged buffers.
    /// </summary>
    public override NtStatus WriteFile(ReadOnlyNativeMemory<char> fileName, ReadOnlyNativeMemory<byte> buffer, out int bytesWritten, long offset, ref DokanFileInfo info)
    {
        if (info.Context is not FileStream stream)
        {
            using (stream = new FileStream(GetPath(fileName), FileMode.Open, FileAccess.Write))
            {
                var bytesToCopy = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
                DoWrite(stream.SafeFileHandle, buffer.Address, bytesToCopy, out bytesWritten, offset);
            }
        }
        else
        {
#pragma warning disable CA2002
            lock (stream) //Protect from overlapped write
#pragma warning restore CA2002
            {
                var bytesToCopy = GetNumOfBytesToCopy(buffer.Length, offset, info, stream);
                DoWrite(stream.SafeFileHandle, buffer.Address, bytesToCopy, out bytesWritten, offset);
            }
        }

        return Trace($"Unsafe{nameof(WriteFile)}", fileName, info, DokanResult.Success, $"out {bytesWritten}",
            offset.ToString(CultureInfo.InvariantCulture));
    }
}
