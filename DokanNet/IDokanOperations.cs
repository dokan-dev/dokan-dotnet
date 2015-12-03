using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                              FileOptions options, FileAttributes attributes, DokanFileInfo info);

        void Cleanup(string fileName, DokanFileInfo info);

        void CloseFile(string fileName, DokanFileInfo info);

        NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
                            DokanFileInfo info);

        NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten,
                             long offset, DokanFileInfo info);

        NtStatus FlushFileBuffers(string fileName, DokanFileInfo info);

        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        NtStatus FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                               DateTime? lastWriteTime, DokanFileInfo info);

        NtStatus DeleteFile(string fileName, DokanFileInfo info);

        NtStatus DeleteDirectory(string fileName, DokanFileInfo info);

        NtStatus MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        NtStatus SetEndOfFile(string fileName, long length, DokanFileInfo info);

        NtStatus SetAllocationSize(string fileName, long length, DokanFileInfo info);

        NtStatus LockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatus UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
                                    DokanFileInfo info);

        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                        out string fileSystemName, DokanFileInfo info);

        NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatus Unmount(DokanFileInfo info);

        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, DokanFileInfo info);
    }
}