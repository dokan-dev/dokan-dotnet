using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        DokanResult CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                              FileOptions options, FileAttributes attributes, DokanFileInfo info);

        DokanResult OpenDirectory(string fileName, DokanFileInfo info);

        DokanResult CreateDirectory(string fileName, DokanFileInfo info);

        DokanResult Cleanup(string fileName, DokanFileInfo info);

        DokanResult CloseFile(string fileName, DokanFileInfo info);

        DokanResult ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
                            DokanFileInfo info);

        DokanResult WriteFile(string fileName, byte[] buffer, out int bytesWritten,
                             long offset, DokanFileInfo info);

        DokanResult FlushFileBuffers(string fileName, DokanFileInfo info);

        DokanResult GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        DokanResult FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        DokanResult SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        DokanResult SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                               DateTime? lastWriteTime, DokanFileInfo info);

        DokanResult DeleteFile(string fileName, DokanFileInfo info);

        DokanResult DeleteDirectory(string fileName, DokanFileInfo info);

        DokanResult MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        DokanResult SetEndOfFile(string fileName, long length, DokanFileInfo info);

        DokanResult SetAllocationSize(string fileName, long length, DokanFileInfo info);

        DokanResult LockFile(string fileName, long offset, long length, DokanFileInfo info);

        DokanResult UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        DokanResult GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
                                    DokanFileInfo info);

        DokanResult GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                        out string fileSystemName, DokanFileInfo info);

        DokanResult GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        DokanResult SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        DokanResult Unmount(DokanFileInfo info);

        DokanResult EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName, out long streamSize,
            DokanFileInfo info);
    }
}