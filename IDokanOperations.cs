using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        DokanError CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                              FileOptions options, FileAttributes attributes, DokanFileInfo info);

        DokanError OpenDirectory(string fileName, DokanFileInfo info);

        DokanError CreateDirectory(string fileName, DokanFileInfo info);

        DokanError Cleanup(string fileName, DokanFileInfo info);

        DokanError CloseFile(string fileName, DokanFileInfo info);

        DokanError ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
                            DokanFileInfo info);

        DokanError WriteFile(string fileName, byte[] buffer, out int bytesWritten,
                             long offset, DokanFileInfo info);

        DokanError FlushFileBuffers(string fileName, DokanFileInfo info);

        DokanError GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        DokanError FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        DokanError SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        DokanError SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                               DateTime? lastWriteTime, DokanFileInfo info);

        DokanError DeleteFile(string fileName, DokanFileInfo info);

        DokanError DeleteDirectory(string fileName, DokanFileInfo info);

        DokanError MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        DokanError SetEndOfFile(string fileName, long length, DokanFileInfo info);

        DokanError SetAllocationSize(string fileName, long length, DokanFileInfo info);

        DokanError LockFile(string fileName, long offset, long length, DokanFileInfo info);

        DokanError UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        DokanError GetDiskFreeSpace(out long free, out long total, out long used,
                                    DokanFileInfo info);

        DokanError GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                        out string fileSystemName, DokanFileInfo info);

        DokanError GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        DokanError SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        DokanError Unmount(DokanFileInfo info);
    }
}