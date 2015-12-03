using DokanNet.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DokanNet
{
    internal sealed class DokanOperationProxy
    {
        #region Delegates

        public delegate NtStatus ZwCreateFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr SecurityContext, uint rawDesiredAccess, uint rawFileAttributes,
            uint rawShareAccess, uint rawCreateDisposition, uint rawCreateOptions,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo dokanFileInfo);

        public delegate void CleanupDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate void CloseFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus ReadFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), Out] byte[] rawBuffer, uint rawBufferLength,
            ref int rawReadLength, long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus WriteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] rawBuffer, uint rawNumberOfBytesToWrite,
            ref int rawNumberOfBytesWritten, long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FlushFileBuffersDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName, ref BY_HANDLE_FILE_INFORMATION handleFileInfo,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo fileInfo);

        public delegate NtStatus FindFilesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        /*public delegate NtStatus FindFilesWithPatternDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawSearchPattern,
            IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);*/

        public delegate NtStatus SetFileAttributesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, uint rawAttributes,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileTimeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            ref FILETIME rawCreationTime, ref FILETIME rawLastAccessTime, ref FILETIME rawLastWriteTime,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus DeleteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);
        
        public delegate NtStatus DeleteDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus MoveFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawNewFileName,
            [MarshalAs(UnmanagedType.Bool)] bool rawReplaceIfExisting,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetEndOfFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetAllocationSizeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus LockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnlockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetDiskFreeSpaceDelegate(
            ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

         public delegate NtStatus GetVolumeInformationDelegate(
             [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize,
             ref uint rawVolumeSerialNumber, ref uint rawMaximumComponentLength, ref FileSystemFeatures rawFileSystemFlags,
             [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawFileSystemNameBuffer,
             uint rawFileSystemNameSize, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawSecurityInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FindStreamsDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In/*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnmountDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Delegates

        private readonly IDokanOperations operations;

        private readonly uint serialNumber;

        public DokanOperationProxy(IDokanOperations operations)
        {
            this.operations = operations;
            serialNumber = (uint)this.operations.GetHashCode();
        }

        private void Trace(string message)
        {
#if TRACE
            Console.WriteLine(message);
            Thread.Sleep(500);
#endif
        }

        private string ToTrace(DokanFileInfo info)
        {
            var context = info.Context != null ? info.Context.GetType().Name : "<null>";

            return string.Format(CultureInfo.InvariantCulture, "{{{0}, {1}, {2}, {3}, {4}, #{5}, {6}, {7}}}",
                context, info.DeleteOnClose, info.IsDirectory, info.NoCache, info.PagingIo, info.ProcessId, info.SynchronousIo, info.WriteToEndOfFile);
        }

        public NtStatus ZwCreateFileProxy(string rawFileName, IntPtr SecurityContext, uint rawDesiredAccess, uint rawFileAttributes,
            uint rawShareAccess, uint rawCreateDisposition, uint rawCreateOptions,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                int FileAttributesAndFlags = 0;
                int CreationDisposition = 0;
                NativeMethods.DokanMapKernelToUserCreateFileFlags(rawFileAttributes, rawCreateOptions, rawCreateDisposition, ref FileAttributesAndFlags, ref CreationDisposition);

                Trace("\nCreateFileProxy : " + rawFileName);
                Trace("\tCreationDisposition\t" + (FileMode)CreationDisposition);
                Trace("\tFileAccess\t" + (FileAccess)rawDesiredAccess);
                Trace("\tFileShare\t" + (FileShare)rawShareAccess);
                Trace("\tFileOptions\t" + (FileOptions)(FileAttributesAndFlags & 0xffffc000));
                Trace("\tFileAttributes\t" + (FileAttributes)(FileAttributesAndFlags & 0x3fff));
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.CreateFile(rawFileName,
                                                    (FileAccess)rawDesiredAccess,
                                                    (FileShare)rawShareAccess,
                                                    (FileMode)CreationDisposition,
                                                    (FileOptions)(FileAttributesAndFlags & 0xffffc000),
                                                    (FileAttributes)(FileAttributesAndFlags & 0x3fff),
                                                    rawFileInfo);

                Trace("CreateFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("CreateFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.Unsuccessful;
            }
        }

        ////

        public void CleanupProxy(string rawFileName,
                                DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nCleanupProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                operations.Cleanup(rawFileName, rawFileInfo);

                Trace("CleanupProxy : " + rawFileName);
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("CleanupProxy : " + rawFileName + " Throw : " + ex.Message);
            }
        }

        ////

        public void CloseFileProxy(string rawFileName,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nCloseFileProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                operations.CloseFile(rawFileName, rawFileInfo);

                Trace("CloseFileProxy : " + rawFileName);
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("CloseFileProxy : " + rawFileName + " Throw : " + ex.Message);
            }
        }

        ////

        public NtStatus ReadFileProxy(string rawFileName,
                                 byte[] rawBuffer, uint rawBufferLength, ref int rawReadLength, long rawOffset,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nReadFileProxy : " + rawFileName);
                Trace("\tBufferLength\t" + rawBufferLength);
                Trace("\tOffset\t" + rawOffset);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.ReadFile(rawFileName, rawBuffer, out rawReadLength, rawOffset, rawFileInfo);

                Trace("ReadFileProxy : " + rawFileName + " Return : " + result + " ReadLength : " + rawReadLength);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("ReadFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus WriteFileProxy(string rawFileName,
                                  byte[] rawBuffer, uint rawNumberOfBytesToWrite, ref int rawNumberOfBytesWritten, long rawOffset,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nWriteFileProxy : " + rawFileName);
                Trace("\tNumberOfBytesToWrite\t" + rawNumberOfBytesToWrite);
                Trace("\tOffset\t" + rawOffset);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.WriteFile(rawFileName, rawBuffer, out rawNumberOfBytesWritten, rawOffset, rawFileInfo);

                Trace("WriteFileProxy : " + rawFileName + " Return : " + result + " NumberOfBytesWritten : " + rawNumberOfBytesWritten);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("WriteFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus FlushFileBuffersProxy(string rawFileName,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nFlushFileBuffersProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.FlushFileBuffers(rawFileName, rawFileInfo);

                Trace("FlushFileBuffersProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("FlushFileBuffersProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus GetFileInformationProxy(string rawFileName,
                                           ref BY_HANDLE_FILE_INFORMATION rawHandleFileInformation,
                                           DokanFileInfo rawFileInfo)
        {
            FileInformation fi;
            try
            {
                Trace("\nGetFileInformationProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.GetFileInformation(rawFileName, out fi, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(fi.FileName != null);
                    Trace("\tFileName\t" + fi.FileName);
                    Trace("\tAttributes\t" + fi.Attributes);
                    Trace("\tCreationTime\t" + fi.CreationTime);
                    Trace("\tLastAccessTime\t" + fi.LastAccessTime);
                    Trace("\tLastWriteTime\t" + fi.LastWriteTime);
                    Trace("\tLength\t" + fi.Length);

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

                    rawHandleFileInformation.dwVolumeSerialNumber = serialNumber;

                    rawHandleFileInformation.nFileSizeLow = (uint)(fi.Length & 0xffffffff);
                    rawHandleFileInformation.nFileSizeHigh = (uint)(fi.Length >> 32);
                    rawHandleFileInformation.dwNumberOfLinks = 1;
                    rawHandleFileInformation.nFileIndexHigh = 0;
                    rawHandleFileInformation.nFileIndexLow = (uint)fi.FileName.GetHashCode();
                }

                Trace("GetFileInformationProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("GetFileInformationProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus FindFilesProxy(string rawFileName,
                                  IntPtr rawFillFindData,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                IList<FileInformation> files;

                Trace("\nFindFilesProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.FindFiles(rawFileName, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (FileInformation fi in files)
                    {
                        Trace("\n\tFileName\t" + fi.FileName);
                        Trace("\tAttributes\t" + fi.Attributes);
                        Trace("\tCreationTime\t" + fi.CreationTime);
                        Trace("\tLastAccessTime\t" + fi.LastAccessTime);
                        Trace("\tLastWriteTime\t" + fi.LastWriteTime);
                        Trace("\tLength\t" + fi.Length);
                    }

                var fill =
                   (FILL_FIND_FILE_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_FILE_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)
                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                Trace("FindFilesProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("FindFilesProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static void Addto(FILL_FIND_FILE_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
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

        public NtStatus FindStreamsProxy(string rawFileName,
                          IntPtr rawFillFindData,
                          DokanFileInfo rawFileInfo)
        {
            try
            {
                IList<FileInformation> files;

                Trace("\nFindStreamsProxy: " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.FindStreams(rawFileName, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (FileInformation fi in files)
                    {
                        Trace("\n\tFileName\t" + fi.FileName);
                        Trace("\tLength\t" + fi.Length);
                    }

                    var fill =
                       (FILL_FIND_STREAM_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_STREAM_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)
                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                Trace("FindStreamsProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("FindStreamsProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static void Addto(FILL_FIND_STREAM_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!String.IsNullOrEmpty(fi.FileName));
            var data = new WIN32_FIND_STREAM_DATA
            {
                StreamSize = fi.Length,
                cStreamName = fi.FileName
            };
            //ZeroMemory(&data, sizeof(WIN32_FIND_DATAW));

            fill(ref data, rawFileInfo);
        }

        ////

        public NtStatus SetEndOfFileProxy(string rawFileName,
                                     long rawByteOffset,
                                     DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nSetEndOfFileProxy : " + rawFileName);
                Trace("\tByteOffset\t" + rawByteOffset);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.SetEndOfFile(rawFileName, rawByteOffset, rawFileInfo);

                Trace("SetEndOfFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("SetEndOfFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus SetAllocationSizeProxy(string rawFileName, long rawLength,
                                          DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nSetAllocationSizeProxy : " + rawFileName);
                Trace("\tLength\t" + rawLength);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.SetAllocationSize(rawFileName, rawLength, rawFileInfo);

                Trace("SetAllocationSizeProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("SetAllocationSizeProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus SetFileAttributesProxy(string rawFileName,
                                          uint rawAttributes,
                                          DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nSetFileAttributesProxy : " + rawFileName);
                Trace("\tAttributes\t" + (FileAttributes)rawAttributes);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileAttributes(rawFileName, (FileAttributes)rawAttributes, rawFileInfo);

                Trace("SetFileAttributesProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("SetFileAttributesProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus SetFileTimeProxy(string rawFileName,
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
                Trace("\nSetFileTimeProxy : " + rawFileName);
                Trace("\tCreateTime\t" + ctime);
                Trace("\tAccessTime\t" + atime);
                Trace("\tWriteTime\t" + mtime);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileTime(rawFileName, ctime, atime, mtime, rawFileInfo);

                Trace("SetFileTimeProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("SetFileTimeProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteFileProxy(string rawFileName,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nDeleteFileProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.DeleteFile(rawFileName, rawFileInfo);

                Trace("DeleteFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("DeleteFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteDirectoryProxy(string rawFileName,
                                        DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nDeleteDirectoryProxy : " + rawFileName);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.DeleteDirectory(rawFileName, rawFileInfo);

                Trace("DeleteDirectoryProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("DeleteDirectoryProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus MoveFileProxy(string rawFileName,
                                 string rawNewFileName, bool rawReplaceIfExisting,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nMoveFileProxy : " + rawFileName);
                Trace("\tNewFileName\t" + rawNewFileName);
                Trace("\tReplaceIfExisting\t" + rawReplaceIfExisting);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.MoveFile(rawFileName, rawNewFileName, rawReplaceIfExisting, rawFileInfo);

                Trace("MoveFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("MoveFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus LockFileProxy(string rawFileName,
                                 long rawByteOffset, long rawLength,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nLockFileProxy : " + rawFileName);
                Trace("\tByteOffset\t" + rawByteOffset);
                Trace("\tLength\t" + rawLength);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.LockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                Trace("LockFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("LockFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus UnlockFileProxy(string rawFileName,
                                   long rawByteOffset, long rawLength,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nUnlockFileProxy : " + rawFileName);
                Trace("\tByteOffset\t" + rawByteOffset);
                Trace("\tLength\t" + rawLength);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.UnlockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                Trace("UnlockFileProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("UnlockFileProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus GetDiskFreeSpaceProxy(ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nGetDiskFreeSpaceProxy");
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.GetDiskFreeSpace(out rawFreeBytesAvailable, out rawTotalNumberOfBytes, out rawTotalNumberOfFreeBytes, rawFileInfo);

                Trace("\tFreeBytesAvailable\t" + rawFreeBytesAvailable);
                Trace("\tTotalNumberOfBytes\t" + rawTotalNumberOfBytes);
                Trace("\tTotalNumberOfFreeBytes\t" + rawTotalNumberOfFreeBytes);
                Trace("GetDiskFreeSpaceProxy Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("GetDiskFreeSpaceProxy Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus GetVolumeInformationProxy(StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize,
                                             ref uint rawVolumeSerialNumber, ref uint rawMaximumComponentLength, ref FileSystemFeatures rawFileSystemFlags,
                                             StringBuilder rawFileSystemNameBuffer, uint rawFileSystemNameSize,
                                             DokanFileInfo rawFileInfo)
        {
            rawMaximumComponentLength = 256;
            rawVolumeSerialNumber = serialNumber;
            string label;
            string name;
            try
            {
                Trace("\nGetVolumeInformationProxy");
                Trace("\tContext\t" + ToTrace(rawFileInfo));
                NtStatus result = operations.GetVolumeInformation(out label, out rawFileSystemFlags, out name, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(!String.IsNullOrEmpty(name));
                    Debug.Assert(!String.IsNullOrEmpty(label));
                    rawVolumeNameBuffer.Append(label);
                    rawFileSystemNameBuffer.Append(name);

                    Trace("\tVolumeNameBuffer\t" + rawVolumeNameBuffer);
                    Trace("\tFileSystemNameBuffer\t" + rawFileSystemNameBuffer);
                    Trace("\tVolumeSerialNumber\t" + rawVolumeSerialNumber);
                    Trace("\tFileSystemFlags\t" + rawFileSystemFlags);
                }

                Trace("GetVolumeInformationProxy Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("GetVolumeInformationProxy Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus UnmountProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                Trace("\nUnmountProxy");
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.Unmount(rawFileInfo);

                Trace("UnmountProxy Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("UnmountProxy Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus GetFileSecurityProxy(string rawFileName,
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
                Trace("\nGetFileSecurityProxy : " + rawFileName);
                Trace("\tFileSystemSecurity\t" + sect);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.GetFileSecurity(rawFileName, out sec, sect, rawFileInfo);
                if (result == DokanResult.Success /*&& sec != null*/)
                {
                    Debug.Assert(sec != null);
                    Trace("\tFileSystemSecurity Result : " + sec);
                    var buffer = sec.GetSecurityDescriptorBinaryForm();
                    rawSecurityDescriptorLengthNeeded = (uint)buffer.Length;
                    if (buffer.Length > rawSecurityDescriptorLength)
                        return DokanResult.BufferOverflow;

                    Marshal.Copy(buffer, 0, rawSecurityDescriptor, buffer.Length);
                }

                Trace("GetFileSecurityProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("GetFileSecurityProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus SetFileSecurityProxy(string rawFileName,
                                        ref SECURITY_INFORMATION rawSecurityInformation, IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
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

                Trace("\nSetFileSecurityProxy : " + rawFileName);
                Trace("\tAccessControlSections\t" + sect);
                Trace("\tFileSystemSecurity\t" + sec);
                Trace("\tContext\t" + ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileSecurity(rawFileName, sec, sect, rawFileInfo);

                Trace("SetFileSecurityProxy : " + rawFileName + " Return : " + result);
                return result;
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
                Trace("SetFileSecurityProxy : " + rawFileName + " Throw : " + ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

#region Nested type: FILL_FIND_FILE_DATA

        private delegate long FILL_FIND_FILE_DATA(
            ref WIN32_FIND_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

#endregion Nested type: FILL_FIND_FILE_DATA

#region Nested type: FILL_FIND_FILE_DATA

        private delegate long FILL_FIND_STREAM_DATA(
            ref WIN32_FIND_STREAM_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

#endregion Nested type: FILL_FIND_FILE_DATA
    }
}