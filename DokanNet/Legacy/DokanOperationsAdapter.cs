using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using DokanNet.Logging;
using LTRData.Extensions.Native.Memory;

namespace DokanNet.Legacy;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal class DokanOperationsAdapter(IDokanOperations operations, ILogger logger) : IDokanOperations2
{
    public int DirectoryListingTimeoutResetIntervalMs => 0;

    public void Cleanup(ReadOnlyNativeMemory<char> fileNamePtr, ref DokanFileInfo info)
        => operations.Cleanup(fileNamePtr.ToString(), new DokanFileInfoAdapter(ref info));

    public void CloseFile(ReadOnlyNativeMemory<char> fileNamePtr, ref DokanFileInfo info)
        => operations.CloseFile(fileNamePtr.ToString(), new DokanFileInfoAdapter(ref info));

    public NtStatus CreateFile(ReadOnlyNativeMemory<char> fileNamePtr, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, ref DokanFileInfo info)
        => operations.CreateFile(fileNamePtr.ToString(), access, share, mode, options, attributes, new DokanFileInfoAdapter(ref info));

    public NtStatus DeleteDirectory(ReadOnlyNativeMemory<char> fileNamePtr, ref DokanFileInfo info)
        => operations.DeleteDirectory(fileNamePtr.ToString(), new DokanFileInfoAdapter(ref info));

    public NtStatus DeleteFile(ReadOnlyNativeMemory<char> fileNamePtr, ref DokanFileInfo info)
        => operations.DeleteFile(fileNamePtr.ToString(), new DokanFileInfoAdapter(ref info));

    public NtStatus FindFiles(ReadOnlyNativeMemory<char> fileNamePtr, out IEnumerable<FindFileInformation> files, ref DokanFileInfo info)
    {
        var status = operations.FindFiles(fileNamePtr.ToString(), out var list, new DokanFileInfoAdapter(ref info));

        files = list.Select(item => new FindFileInformation
        {
            Attributes = item.Attributes,
            CreationTime = item.CreationTime,
            LastAccessTime = item.LastAccessTime,
            LastWriteTime = item.LastWriteTime,
            Length = item.Length,
            FileName = item.FileName.AsMemory()
        });

        return status;
    }

    public NtStatus FindFilesWithPattern(ReadOnlyNativeMemory<char> fileNamePtr, ReadOnlyNativeMemory<char> searchPatternPtr, out IEnumerable<FindFileInformation> files, ref DokanFileInfo info)
    {
        var status = operations.FindFilesWithPattern(fileNamePtr.ToString(), searchPatternPtr.ToString(), out var list, new DokanFileInfoAdapter(ref info));

        files = list.Select(item => new FindFileInformation
        {
            Attributes = item.Attributes,
            CreationTime = item.CreationTime,
            LastAccessTime = item.LastAccessTime,
            LastWriteTime = item.LastWriteTime,
            Length = item.Length,
            FileName = item.FileName.AsMemory()
        });

        return status;
    }

    public NtStatus FindStreams(ReadOnlyNativeMemory<char> fileNamePtr, out IEnumerable<FindFileInformation> streams, ref DokanFileInfo info)
    {
        var status = operations.FindStreams(fileNamePtr.ToString(), out var list, new DokanFileInfoAdapter(ref info));

        streams = list.Select(item => new FindFileInformation
        {
            Attributes = item.Attributes,
            CreationTime = item.CreationTime,
            LastAccessTime = item.LastAccessTime,
            LastWriteTime = item.LastWriteTime,
            Length = item.Length,
            FileName = item.FileName.AsMemory()
        });

        return status;
    }

    public NtStatus FlushFileBuffers(ReadOnlyNativeMemory<char> fileNamePtr, ref DokanFileInfo info)
        => operations.FlushFileBuffers(fileNamePtr.ToString(), new DokanFileInfoAdapter(ref info));

    public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, ref DokanFileInfo info)
        => operations.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, new DokanFileInfoAdapter(ref info));

    public NtStatus GetFileInformation(ReadOnlyNativeMemory<char> fileNamePtr, out ByHandleFileInformation fileInfo, ref DokanFileInfo info)
    {
        var status = operations.GetFileInformation(fileNamePtr.ToString(), out var fileInfoLegacy, new DokanFileInfoAdapter(ref info));

        fileInfo = new()
        {
            Attributes = fileInfoLegacy.Attributes,
            CreationTime = fileInfoLegacy.CreationTime,
            LastAccessTime = fileInfoLegacy.LastAccessTime,
            LastWriteTime = fileInfoLegacy.LastWriteTime,
            Length = fileInfoLegacy.Length
        };

        return status;
    }

    public NtStatus GetFileSecurity(ReadOnlyNativeMemory<char> fileNamePtr, out FileSystemSecurity? security, AccessControlSections sections, ref DokanFileInfo info)
        => operations.GetFileSecurity(fileNamePtr.ToString(), out security, sections, new DokanFileInfoAdapter(ref info));

    public NtStatus GetVolumeInformation(NativeMemory<char> volumeLabel, out FileSystemFeatures features, NativeMemory<char> fileSystemName, out uint maximumComponentLength, ref uint volumeSerialNumber, ref DokanFileInfo info)
    {
        var status = operations.GetVolumeInformation(out var volumeLabelStr, out features, out var fileSystemNameStr, out maximumComponentLength, new DokanFileInfoAdapter(ref info));
        volumeLabel.SetString(volumeLabelStr);
        fileSystemName.SetString(fileSystemNameStr);
        return status;
    }

    public NtStatus LockFile(ReadOnlyNativeMemory<char> fileNamePtr, long offset, long length, ref DokanFileInfo info)
        => operations.LockFile(fileNamePtr.ToString(), offset, length, new DokanFileInfoAdapter(ref info));

    public NtStatus Mounted(ReadOnlyNativeMemory<char> mountPoint, ref DokanFileInfo info)
        => operations.Mounted(mountPoint.ToString(), new DokanFileInfoAdapter(ref info));

    public NtStatus MoveFile(ReadOnlyNativeMemory<char> oldNamePtr, ReadOnlyNativeMemory<char> newNamePtr, bool replace, ref DokanFileInfo info)
        => operations.MoveFile(oldNamePtr.ToString(), newNamePtr.ToString(), replace, new DokanFileInfoAdapter(ref info));

    public NtStatus ReadFile(ReadOnlyNativeMemory<char> fileNamePtr, NativeMemory<byte> buffer, out int bytesRead, long offset, ref DokanFileInfo info)
    {
        if (operations is IDokanOperationsUnsafe unsafeOperations)
        {
            return unsafeOperations.ReadFile(fileNamePtr.ToString(), buffer.Address, (uint)buffer.Length, out bytesRead, offset, new DokanFileInfoAdapter(ref info));
        }

        // Pool the read buffer and return it to the pool when we're done with it.
        var array = BufferPool.Default.RentBuffer(buffer.Length, logger);
        try
        {
            var status = operations.ReadFile(fileNamePtr.ToString(), array, out bytesRead, offset, new DokanFileInfoAdapter(ref info));
            array.AsSpan().CopyTo(buffer.Span);
            return status;
        }
        finally
        {
            BufferPool.Default.ReturnBuffer(array, logger);
        }
    }

    public NtStatus SetAllocationSize(ReadOnlyNativeMemory<char> fileNamePtr, long length, ref DokanFileInfo info)
        => operations.SetAllocationSize(fileNamePtr.ToString(), length, new DokanFileInfoAdapter(ref info));

    public NtStatus SetEndOfFile(ReadOnlyNativeMemory<char> fileNamePtr, long length, ref DokanFileInfo info)
        => operations.SetEndOfFile(fileNamePtr.ToString(), length, new DokanFileInfoAdapter(ref info));

    public NtStatus SetFileAttributes(ReadOnlyNativeMemory<char> fileNamePtr, FileAttributes attributes, ref DokanFileInfo info)
        => operations.SetFileAttributes(fileNamePtr.ToString(), attributes, new DokanFileInfoAdapter(ref info));

    public NtStatus SetFileSecurity(ReadOnlyNativeMemory<char> fileNamePtr, FileSystemSecurity security, AccessControlSections sections, ref DokanFileInfo info)
        => operations.SetFileSecurity(fileNamePtr.ToString(), security, sections, new DokanFileInfoAdapter(ref info));

    public NtStatus SetFileTime(ReadOnlyNativeMemory<char> fileNamePtr, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, ref DokanFileInfo info)
        => operations.SetFileTime(fileNamePtr.ToString(), creationTime, lastAccessTime, lastWriteTime, new DokanFileInfoAdapter(ref info));

    public NtStatus UnlockFile(ReadOnlyNativeMemory<char> fileNamePtr, long offset, long length, ref DokanFileInfo info)
        => operations.UnlockFile(fileNamePtr.ToString(), offset, length, new DokanFileInfoAdapter(ref info));

    public NtStatus Unmounted(ref DokanFileInfo info)
        => operations.Unmounted(new DokanFileInfoAdapter(ref info));

    public NtStatus WriteFile(ReadOnlyNativeMemory<char> fileNamePtr, ReadOnlyNativeMemory<byte> buffer, out int bytesWritten, long offset, ref DokanFileInfo info)
    {
        if (operations is IDokanOperationsUnsafe unsafeOperations)
        {
            return unsafeOperations.WriteFile(fileNamePtr.ToString(), buffer.Address, (uint)buffer.Length, out bytesWritten, offset, new DokanFileInfoAdapter(ref info));
        }

        // Pool the write buffer and return it to the pool when we're done with it.
        var array = BufferPool.Default.RentBuffer(buffer.Length, logger);
        try
        {
            buffer.Span.CopyTo(array);

            var status = operations.WriteFile(
                fileNamePtr.ToString(),
                array,
                out bytesWritten,
                offset,
                new DokanFileInfoAdapter(ref info));

            return status;
        }
        finally
        {
            BufferPool.Default.ReturnBuffer(array, logger);
        }
    }

    public override string? ToString() => operations.ToString();

    private unsafe class DokanFileInfoAdapter : IDokanFileInfo
    {
        private readonly DokanFileInfo* ptr;

        public DokanFileInfoAdapter(ref DokanFileInfo values)
        {
            ptr = (DokanFileInfo*)Unsafe.AsPointer(ref values);
        }

        public object? Context { get => ptr->Context; set => ptr->Context = value; }

        public bool DeletePending { get => ptr->DeletePending; set => ptr->DeletePending = value; }

        public bool IsDirectory { get => ptr->IsDirectory; set => ptr->IsDirectory = value; }

        public bool NoCache => ptr->NoCache;

        public bool PagingIo => ptr->PagingIo;

        public int ProcessId => ptr->ProcessId;

        public bool SynchronousIo => ptr->SynchronousIo;

        public bool WriteToEndOfFile => ptr->WriteToEndOfFile;

        public WindowsIdentity GetRequestor() => ptr->GetRequestor();

        public bool TryResetTimeout(int milliseconds) => ptr->TryResetTimeout(milliseconds);

        public override string? ToString() => ptr->ToString();
    }
}
