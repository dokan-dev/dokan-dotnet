using DokanNet.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DokanNet
{
    internal sealed class DokanOperationProxy
    {
        #region Delegates

        public delegate int CleanupDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate int CloseFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate int CreateDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int CreateFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, uint rawAccessMode, uint rawShare,
            uint rawCreationDisposition, uint rawFlagsAndAttributes,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo dokanFileInfo);

        public delegate int DeleteDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int DeleteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int FindFilesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        /*     public delegate int FindFilesWithPatternDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawSearchPattern,
            IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);*/

        public delegate int FlushFileBuffersDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int GetDiskFreeSpaceDelegate(
            ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate int GetFileInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName, ref BY_HANDLE_FILE_INFORMATION handleFileInfo,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo fileInfo);

        public delegate int GetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int GetVolumeInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize,
            ref uint rawVolumeSerialNumber, ref uint rawMaximumComponentLength, ref FileSystemFeatures rawFileSystemFlags,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawFileSystemNameBuffer,
            uint rawFileSystemNameSize, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate int LockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int MoveFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawNewFileName,
            [MarshalAs(UnmanagedType.Bool)] bool rawReplaceIfExisting,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate int OpenDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo fileInfo);

        public delegate int ReadFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] byte[] rawBuffer, uint rawBufferLength,
            ref int rawReadLength, long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int SetAllocationSizeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int SetEndOfFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int SetFileAttributesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, uint rawAttributes,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int SetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawSecurityInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int SetFileTimeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            ref FILETIME rawCreationTime, ref FILETIME rawLastAccessTime, ref FILETIME rawLastWriteTime,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int UnlockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int UnmountDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate int WriteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] rawBuffer, uint rawNumberOfBytesToWrite,
            ref int rawNumberOfBytesWritten, long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate int EnumerateNamedStreamsDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawEnumContext,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawStreamName, ref uint rawStreamNameLength, ref long rawStreamSize,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Delegates

        private readonly IDokanOperations _operations;

        private readonly uint _serialNumber;

        public DokanOperationProxy(IDokanOperations operations)
        {
            _operations = operations;
            _serialNumber = (uint)_operations.GetHashCode();
        }

        private void DbgPrint(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public int CreateFileProxy(string rawFileName, uint rawAccessMode,
                                   uint rawShare, uint rawCreationDisposition, uint rawFlagsAndAttributes,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nCreateFileProxy : " + rawFileName);
                DbgPrint("\tCreationDisposition\t" + (FileMode)rawCreationDisposition);
                DbgPrint("\tFileAccess\t" + (FileAccess)rawAccessMode);
                DbgPrint("\tFileShare\t" + (FileShare)rawShare);
                DbgPrint("\tFileOptions\t" + (FileOptions)(rawFlagsAndAttributes & 0xffffc000));
                DbgPrint("\tFileAttributes\t" + (FileAttributes)(rawFlagsAndAttributes & 0x3fff));
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.CreateFile(rawFileName, (FileAccess)rawAccessMode, (FileShare)rawShare,
                                                    (FileMode)rawCreationDisposition,
                                                    (FileOptions)(rawFlagsAndAttributes & 0xffffc000), //& 0xffffc000
                                                    (FileAttributes)(rawFlagsAndAttributes & 0x3fff), rawFileInfo);
                //& 0x3ffflower 14 bits i think are file atributes and rest are file options WRITE_TROUGH etc.

                DbgPrint("CreateFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("CreateFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int OpenDirectoryProxy(string rawFileName,
                                      DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nOpenDirectoryProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.OpenDirectory(rawFileName, rawFileInfo);

                DbgPrint("OpenDirectoryProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("OpenDirectoryProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int CreateDirectoryProxy(string rawFileName,
                                        DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nCreateDirectoryProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.CreateDirectory(rawFileName, rawFileInfo);

                DbgPrint("CreateDirectoryProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("CreateDirectoryProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int CleanupProxy(string rawFileName,
                               DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nCleanupProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.Cleanup(rawFileName, rawFileInfo);

                DbgPrint("CleanupProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("CleanupProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int CloseFileProxy(string rawFileName,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nCloseFileProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.CloseFile(rawFileName, rawFileInfo);

                DbgPrint("CloseFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("CloseFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int ReadFileProxy(string rawFileName, byte[] rawBuffer,
                                 uint rawBufferLength, ref int rawReadLength, long rawOffset,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nReadFileProxy : " + rawFileName);
                DbgPrint("\tBufferLength\t" + rawBufferLength);
                DbgPrint("\tOffset\t" + rawOffset);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.ReadFile(rawFileName, rawBuffer, out rawReadLength, rawOffset,
                                                  rawFileInfo);

                DbgPrint("ReadFileProxy : " + rawFileName + " Return : " + result + " ReadLength : " + rawReadLength);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("ReadFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int WriteFileProxy(string rawFileName, byte[] rawBuffer,
                                  uint rawNumberOfBytesToWrite, ref int rawNumberOfBytesWritten, long rawOffset,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nWriteFileProxy : " + rawFileName);
                DbgPrint("\tNumberOfBytesToWrite\t" + rawNumberOfBytesToWrite);
                DbgPrint("\tOffset\t" + rawOffset);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.WriteFile(rawFileName, rawBuffer,
                                                   out rawNumberOfBytesWritten, rawOffset,
                                                   rawFileInfo);

                DbgPrint("WriteFileProxy : " + rawFileName + " Return : " + result + " NumberOfBytesWritten : " + rawNumberOfBytesWritten);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("WriteFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int FlushFileBuffersProxy(string rawFileName,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nFlushFileBuffersProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.FlushFileBuffers(rawFileName, rawFileInfo);

                DbgPrint("FlushFileBuffersProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("FlushFileBuffersProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int GetFileInformationProxy(string rawFileName,
                                           ref BY_HANDLE_FILE_INFORMATION rawHandleFileInformation,
                                           DokanFileInfo rawFileInfo)
        {
            FileInformation fi;
            try
            {
                DbgPrint("\nGetFileInformationProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.GetFileInformation(rawFileName, out fi, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(fi.FileName != null);
                    DbgPrint("\tFileName\t" + fi.FileName);
                    DbgPrint("\tAttributes\t" + fi.Attributes);
                    DbgPrint("\tCreationTime\t" + fi.CreationTime);
                    DbgPrint("\tLastAccessTime\t" + fi.LastAccessTime);
                    DbgPrint("\tLastWriteTime\t" + fi.LastWriteTime);
                    DbgPrint("\tLength\t" + fi.Length);

                    rawHandleFileInformation.dwFileAttributes = (uint)fi.Attributes /* + FILE_ATTRIBUTE_VIRTUAL*/;

                    long ctime = fi.CreationTime.ToFileTime();
                    long atime = fi.LastAccessTime.ToFileTime();
                    long mtime = fi.LastWriteTime.ToFileTime();
                    rawHandleFileInformation.ftCreationTime.dwHighDateTime = (int)(ctime >> 32);
                    rawHandleFileInformation.ftCreationTime.dwLowDateTime = (int)(ctime & 0xffffffff);

                    rawHandleFileInformation.ftLastAccessTime.dwHighDateTime = (int)(atime >> 32);
                    rawHandleFileInformation.ftLastAccessTime.dwLowDateTime = (int)(atime & 0xffffffff);

                    rawHandleFileInformation.ftLastWriteTime.dwHighDateTime = (int)(mtime >> 32);
                    rawHandleFileInformation.ftLastWriteTime.dwLowDateTime = (int)(mtime & 0xffffffff);

                    rawHandleFileInformation.dwVolumeSerialNumber = _serialNumber;

                    rawHandleFileInformation.nFileSizeLow = (uint)(fi.Length & 0xffffffff);
                    rawHandleFileInformation.nFileSizeHigh = (uint)(fi.Length >> 32);
                    rawHandleFileInformation.dwNumberOfLinks = 1;
                    rawHandleFileInformation.nFileIndexHigh = 0;
                    rawHandleFileInformation.nFileIndexLow = (uint)fi.FileName.GetHashCode();
                }

                DbgPrint("GetFileInformationProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("GetFileInformationProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int FindFilesProxy(string rawFileName, IntPtr rawFillFindData,
                                  // function pointer
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                IList<FileInformation> files;

                DbgPrint("\nFindFilesProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.FindFiles(rawFileName, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
#if DEBUG
                    foreach (FileInformation fi in files)
                    {
                        DbgPrint("\n\tFileName\t" + fi.FileName);
                        DbgPrint("\tAttributes\t" + fi.Attributes);
                        DbgPrint("\tCreationTime\t" + fi.CreationTime);
                        DbgPrint("\tLastAccessTime\t" + fi.LastAccessTime);
                        DbgPrint("\tLastWriteTime\t" + fi.LastWriteTime);
                        DbgPrint("\tLength\t" + fi.Length);
                    }
#endif

                    var fill =
                   (FILL_FIND_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_DATA));
                    // Used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)

                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                DbgPrint("FindFilesProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("FindFilesProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        private static void Addto(FILL_FIND_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!String.IsNullOrEmpty(fi.FileName));
            long ctime = fi.CreationTime.ToFileTime();
            long atime = fi.LastAccessTime.ToFileTime();
            long mtime = fi.LastWriteTime.ToFileTime();
            var data = new WIN32_FIND_DATA
            {
                dwFileAttributes = fi.Attributes,
                ftCreationTime =
                {
                    dwHighDateTime = (int) (ctime >> 32),
                    dwLowDateTime = (int) (ctime & 0xffffffff)
                },
                ftLastAccessTime =
                {
                    dwHighDateTime = (int) (atime >> 32),
                    dwLowDateTime = (int) (atime & 0xffffffff)
                },
                ftLastWriteTime =
                {
                    dwHighDateTime = (int) (mtime >> 32),
                    dwLowDateTime = (int) (mtime & 0xffffffff)
                },
                nFileSizeLow = (uint)(fi.Length & 0xffffffff),
                nFileSizeHigh = (uint)(fi.Length >> 32),
                cFileName = fi.FileName
            };
            //ZeroMemory(&data, sizeof(WIN32_FIND_DATAW));

            fill(ref data, rawFileInfo);
        }

        ////

        public int SetEndOfFileProxy(string rawFileName, long rawByteOffset,
                                     DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nSetEndOfFileProxy : " + rawFileName);
                DbgPrint("\tByteOffset\t" + rawByteOffset);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.SetEndOfFile(rawFileName, rawByteOffset, rawFileInfo);

                DbgPrint("SetEndOfFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("SetEndOfFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int SetAllocationSizeProxy(string rawFileName, long rawLength,
                                          DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nSetAllocationSizeProxy : " + rawFileName);
                DbgPrint("\tLength\t" + rawLength);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.SetAllocationSize(rawFileName, rawLength, rawFileInfo);

                DbgPrint("SetAllocationSizeProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("SetAllocationSizeProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int SetFileAttributesProxy(string rawFileName, uint rawAttributes,
                                          DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nSetFileAttributesProxy : " + rawFileName);
                DbgPrint("\tAttributes\t" + (FileAttributes)rawAttributes);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.SetFileAttributes(rawFileName, (FileAttributes)rawAttributes, rawFileInfo);

                DbgPrint("SetFileAttributesProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("SetFileAttributesProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int SetFileTimeProxy(string rawFileName,
                                    ref FILETIME rawCreationTime, ref FILETIME rawLastAccessTime, ref FILETIME rawLastWriteTime,
                                    DokanFileInfo rawFileInfo)
        {
            var ctime = (rawCreationTime.dwLowDateTime != 0 || rawCreationTime.dwHighDateTime != 0) && (rawCreationTime.dwLowDateTime != -1 || rawCreationTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long)rawCreationTime.dwHighDateTime << 32) | (uint)rawCreationTime.dwLowDateTime)
                : (DateTime?)null;
            var atime = (rawLastAccessTime.dwLowDateTime != 0 || rawLastAccessTime.dwHighDateTime != 0) && (rawLastAccessTime.dwLowDateTime != -1 || rawLastAccessTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long)rawLastAccessTime.dwHighDateTime << 32) | (uint)rawLastAccessTime.dwLowDateTime)
                : (DateTime?)null;
            var mtime = (rawLastWriteTime.dwLowDateTime != 0 || rawLastWriteTime.dwHighDateTime != 0) && (rawLastWriteTime.dwLowDateTime != -1 || rawLastWriteTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long)rawLastWriteTime.dwHighDateTime << 32) | (uint)rawLastWriteTime.dwLowDateTime)
                : (DateTime?)null;

            try
            {
                DbgPrint("\nSetFileTimeProxy : " + rawFileName);
                DbgPrint("\tCreateTime\t" + ctime);
                DbgPrint("\tAccessTime\t" + atime);
                DbgPrint("\tWriteTime\t" + mtime);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.SetFileTime(rawFileName, ctime, atime, mtime, rawFileInfo);

                DbgPrint("SetFileTimeProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("SetFileTimeProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int DeleteFileProxy(string rawFileName,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nDeleteFileProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.DeleteFile(rawFileName, rawFileInfo);

                DbgPrint("DeleteFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("DeleteFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int DeleteDirectoryProxy(string rawFileName,
                                        DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nDeleteDirectoryProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.DeleteDirectory(rawFileName, rawFileInfo);

                DbgPrint("DeleteDirectoryProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("DeleteDirectoryProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int MoveFileProxy(string rawFileName,
                                 string rawNewFileName, bool rawReplaceIfExisting,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nMoveFileProxy : " + rawFileName);
                DbgPrint("\tNewFileName\t" + rawNewFileName);
                DbgPrint("\tReplaceIfExisting\t" + rawReplaceIfExisting);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.MoveFile(rawFileName, rawNewFileName, rawReplaceIfExisting,
                                                  rawFileInfo);

                DbgPrint("MoveFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("MoveFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int LockFileProxy(string rawFileName,
                                 long rawByteOffset, long rawLength,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nLockFileProxy : " + rawFileName);
                DbgPrint("\tByteOffset\t" + rawByteOffset);
                DbgPrint("\tLength\t" + rawLength);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.LockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                DbgPrint("LockFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("LockFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int UnlockFileProxy(string rawFileName,
                                   long rawByteOffset, long rawLength,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nUnlockFileProxy : " + rawFileName);
                DbgPrint("\tByteOffset\t" + rawByteOffset);
                DbgPrint("\tLength\t" + rawLength);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.UnlockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                DbgPrint("UnlockFileProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("UnlockFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        ////

        public int GetDiskFreeSpaceProxy(ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nGetDiskFreeSpaceProxy");
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.GetDiskFreeSpace(out rawFreeBytesAvailable, out rawTotalNumberOfBytes,
                                                          out rawTotalNumberOfFreeBytes,
                                                          rawFileInfo);

                DbgPrint("\tFreeBytesAvailable\t" + rawFreeBytesAvailable);
                DbgPrint("\tTotalNumberOfBytes\t" + rawTotalNumberOfBytes);
                DbgPrint("\tTotalNumberOfFreeBytes\t" + rawTotalNumberOfFreeBytes);
                DbgPrint("GetDiskFreeSpaceProxy Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("GetDiskFreeSpaceProxy Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int GetVolumeInformationProxy(StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize, ref uint rawVolumeSerialNumber,
                                             ref uint rawMaximumComponentLength, ref FileSystemFeatures rawFileSystemFlags,
                                             StringBuilder rawFileSystemNameBuffer, uint rawFileSystemNameSize,
                                             DokanFileInfo rawFileInfo)
        {
            rawMaximumComponentLength = 256;
            rawVolumeSerialNumber = _serialNumber;
            string label;
            string name;
            try
            {
                DbgPrint("\nGetVolumeInformationProxy");
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.GetVolumeInformation(out label, out rawFileSystemFlags, out name, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(!String.IsNullOrEmpty(name));
                    Debug.Assert(!String.IsNullOrEmpty(label));
                    rawVolumeNameBuffer.Append(label);
                    rawFileSystemNameBuffer.Append(name);

                    DbgPrint("\tVolumeNameBuffer\t" + rawVolumeNameBuffer);
                    DbgPrint("\tFileSystemNameBuffer\t" + rawFileSystemNameBuffer);
                    DbgPrint("\tVolumeSerialNumber\t" + rawVolumeSerialNumber);
                    DbgPrint("\tFileSystemFlags\t" + rawFileSystemFlags);
                }

                DbgPrint("GetVolumeInformationProxy Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("GetVolumeInformationProxy Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int UnmountProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\nUnmountProxy");
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.Unmount(rawFileInfo);

                DbgPrint("UnmountProxy Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("UnmountProxy Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int GetFileSecurityProxy(string rawFileName,
                                        ref SECURITY_INFORMATION rawRequestedInformation,
                                        IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
                                        ref uint rawSecurityDescriptorLengthNeeded,
                                        DokanFileInfo rawFileInfo)
        {
            FileSystemSecurity sec;

            var sect = AccessControlSections.None;
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Owner;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Group;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_DACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_DACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Access;
            }
            if (rawRequestedInformation.HasFlag(SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_SACL_SECURITY_INFORMATION) ||
                rawRequestedInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_SACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Audit;
            }
            try
            {
                DbgPrint("\nGetFileSecurityProxy : " + rawFileName);
                DbgPrint("\tFileSystemSecurity\t" + sect);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.GetFileSecurity(rawFileName, out sec, sect, rawFileInfo);
                if (result == DokanResult.Success /*&& sec != null*/)
                {
                    Debug.Assert(sec != null);
                    DbgPrint("\tFileSystemSecurity Result : " + sec);
                    var buffer = sec.GetSecurityDescriptorBinaryForm();
                    rawSecurityDescriptorLengthNeeded = (uint)buffer.Length;
                    if (buffer.Length > rawSecurityDescriptorLength)
                        return (int)DokanResult.InsufficientBuffer;

                    Marshal.Copy(buffer, 0, rawSecurityDescriptor, buffer.Length);
                }

                DbgPrint("GetFileSecurityProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("GetFileSecurityProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int SetFileSecurityProxy(string rawFileName, ref SECURITY_INFORMATION rawSecurityInformation,
                                        IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
                                        DokanFileInfo rawFileInfo)
        {
            var sect = AccessControlSections.None;
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Owner;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Group;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_DACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_DACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Access;
            }
            if (rawSecurityInformation.HasFlag(SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.PROTECTED_SACL_SECURITY_INFORMATION) ||
                rawSecurityInformation.HasFlag(SECURITY_INFORMATION.UNPROTECTED_SACL_SECURITY_INFORMATION))
            {
                sect |= AccessControlSections.Audit;
            }
            var buffer = new byte[rawSecurityDescriptorLength];
            try
            {
                Marshal.Copy(rawSecurityDescriptor, buffer, 0, (int)rawSecurityDescriptorLength);
                var sec = rawFileInfo.IsDirectory ? (FileSystemSecurity)new DirectorySecurity() : new FileSecurity();
                sec.SetSecurityDescriptorBinaryForm(buffer);

                DbgPrint("\nSetFileSecurityProxy : " + rawFileName);
                DbgPrint("\tAccessControlSections\t" + sect);
                DbgPrint("\tFileSystemSecurity\t" + sec);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                DokanResult result = _operations.SetFileSecurity(rawFileName, sec, sect, rawFileInfo);

                DbgPrint("SetFileSecurityProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("SetFileSecurityProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

        public int EnumerateNamedStreamsProxy(string rawFileName,
                                              IntPtr rawEnumContext,
                                              StringBuilder rawStreamName, ref uint rawStreamNameLength,
                                              ref long rawStreamSize,
                                              DokanFileInfo rawFileInfo)
        {
            try
            {
                DbgPrint("\tEnumerateNamedStreamsProxy : " + rawFileName);
                DbgPrint("\tContext\t" + (rawFileInfo.Context != null ? rawFileInfo.Context.GetType().Name : "<null>"));

                string name;
                DokanResult result = _operations.EnumerateNamedStreams(rawFileName, rawEnumContext, out name, out rawStreamSize , rawFileInfo);
                if (result == DokanResult.Success)
                {
                    rawStreamName.Append(name);
                    rawStreamNameLength = (uint)name.Length;
                }

                DbgPrint("EnumerateNamedStreamsProxy : " + rawFileName + " Return : " + result);
                return (int)result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                DbgPrint("EnumerateNamedStreamsProxy : " + rawFileName + " Throw : " + ex.Message);
                return (int)DokanResult.ExceptionInService;
            }
        }

#region Nested type: FILL_FIND_DATA

        private delegate int FILL_FIND_DATA(
            ref WIN32_FIND_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

#endregion Nested type: FILL_FIND_DATA
    }
}