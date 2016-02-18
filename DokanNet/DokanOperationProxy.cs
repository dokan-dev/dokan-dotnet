using DokanNet.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DokanNet
{
    using DokanNet.Logging;

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

        public delegate NtStatus FindFilesWithPatternDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawSearchPattern,
            IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

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

        public delegate NtStatus MountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnmountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Delegates

        private readonly IDokanOperations operations;

        private readonly ILogger logger;

        private readonly uint serialNumber;

        public DokanOperationProxy(IDokanOperations operations, ILogger logger)
        {
            this.operations = operations;
            this.logger = logger;
            serialNumber = (uint)this.operations.GetHashCode();
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
                FileOptions fileOptions = 0;
                FileAttributes fileAttributes = 0;
                int FileAttributesAndFlags = 0;
                int CreationDisposition = 0;
                Dokan.Methods.DokanMapKernelToUserCreateFileFlags(rawFileAttributes, rawCreateOptions, rawCreateDisposition, ref FileAttributesAndFlags, ref CreationDisposition);

                foreach (FileOptions fileOption in Enum.GetValues(typeof(FileOptions)))
                {
                    if (((FileOptions)(FileAttributesAndFlags & 0xffffc000) & fileOption) == fileOption)
                        fileOptions |= fileOption;
                }

                foreach (FileAttributes fileAttribute in Enum.GetValues(typeof(FileAttributes)))
                {
                    if (((FileAttributes)(FileAttributesAndFlags & 0x3fff) & fileAttribute) == fileAttribute)
                        fileAttributes |= fileAttribute;
                }
                
                this.logger.Debug("CreateFileProxy : {0}", rawFileName);
                this.logger.Debug("\tCreationDisposition\t{0}", (FileMode)CreationDisposition);
                this.logger.Debug("\tFileAccess\t{0}", (FileAccess)rawDesiredAccess);
                this.logger.Debug("\tFileShare\t{0}", (FileShare)rawShareAccess);
                this.logger.Debug("\tFileOptions\t{0}", fileOptions);
                this.logger.Debug("\tFileAttributes\t{0}", fileAttributes);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));
                NtStatus result = operations.CreateFile(rawFileName,
                                                    (FileAccess)rawDesiredAccess,
                                                    (FileShare)rawShareAccess,
                                                    (FileMode)CreationDisposition,
                                                    fileOptions,
                                                    fileAttributes,
                                                    rawFileInfo);

                this.logger.Debug("CreateFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("CreateFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.Unsuccessful;
            }
        }

        ////
        
        public void CleanupProxy(string rawFileName,
                                DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("CleanupProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                operations.Cleanup(rawFileName, rawFileInfo);

                this.logger.Debug("CleanupProxy : {0}", rawFileName);
            }
            catch (Exception ex)
            {
                this.logger.Error("CleanupProxy : {0} Throw : {1}", rawFileName, ex.Message);
            }
        }

        ////

        public void CloseFileProxy(string rawFileName,
                                  DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("CloseFileProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                operations.CloseFile(rawFileName, rawFileInfo);

                this.logger.Debug("CloseFileProxy : {0}", rawFileName);
            }
            catch (Exception ex)
            {
                this.logger.Error("CloseFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
            }
        }

        ////

        public NtStatus ReadFileProxy(string rawFileName,
                                 byte[] rawBuffer, uint rawBufferLength, ref int rawReadLength, long rawOffset,
                                 DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("ReadFileProxy : " + rawFileName);
                this.logger.Debug("\tBufferLength\t" + rawBufferLength);
                this.logger.Debug("\tOffset\t" + rawOffset);
                this.logger.Debug("\tContext\t" + this.ToTrace(rawFileInfo));

                NtStatus result = operations.ReadFile(rawFileName, rawBuffer, out rawReadLength, rawOffset, rawFileInfo);

                this.logger.Debug("ReadFileProxy : " + rawFileName + " Return : " + result + " ReadLength : " + rawReadLength);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("ReadFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("WriteFileProxy : {0}", rawFileName);
                this.logger.Debug("\tNumberOfBytesToWrite\t{0}", rawNumberOfBytesToWrite);
                this.logger.Debug("\tOffset\t{0}", rawOffset);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.WriteFile(rawFileName, rawBuffer, out rawNumberOfBytesWritten, rawOffset, rawFileInfo);

                this.logger.Debug("WriteFileProxy : {0} Return : {1} NumberOfBytesWritten : {2}", rawFileName, result, rawNumberOfBytesWritten);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("WriteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus FlushFileBuffersProxy(string rawFileName,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("FlushFileBuffersProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.FlushFileBuffers(rawFileName, rawFileInfo);

                this.logger.Debug("FlushFileBuffersProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("FlushFileBuffersProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("GetFileInformationProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.GetFileInformation(rawFileName, out fi, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(fi.FileName != null);
                    this.logger.Debug("\tFileName\t{0}", fi.FileName);
                    this.logger.Debug("\tAttributes\t{0}", fi.Attributes);
                    this.logger.Debug("\tCreationTime\t{0}", fi.CreationTime);
                    this.logger.Debug("\tLastAccessTime\t{0}", fi.LastAccessTime);
                    this.logger.Debug("\tLastWriteTime\t{0}", fi.LastWriteTime);
                    this.logger.Debug("\tLength\t{0}", fi.Length);

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

                this.logger.Debug("GetFileInformationProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("GetFileInformationProxy : {0} Throw : {1}", rawFileName, ex.Message);
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

                this.logger.Debug("FindFilesProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.FindFiles(rawFileName, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (FileInformation fi in files)
                    {
                        this.logger.Debug("\tFileName\t{0}", fi.FileName);
                        this.logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        this.logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        this.logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        this.logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        this.logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                var fill =
                   (FILL_FIND_FILE_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_FILE_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)
                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                this.logger.Debug("FindFilesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("FindFilesProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus FindFilesWithPatternProxy(string rawFileName,
                          string rawSearchPattern,
                          IntPtr rawFillFindData,
                          DokanFileInfo rawFileInfo)
        {
            try
            {
                IList<FileInformation> files;

                this.logger.Debug("FindFilesWithPatternProxy : {0}", rawFileName);
                this.logger.Debug("\trawSearchPattern\t{0}", rawSearchPattern);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.FindFilesWithPattern(rawFileName, rawSearchPattern, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (FileInformation fi in files)
                    {
                        this.logger.Debug("\tFileName\t{0}", fi.FileName);
                        this.logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        this.logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        this.logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        this.logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        this.logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill =
                       (FILL_FIND_FILE_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_FILE_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)
                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                this.logger.Debug("FindFilesWithPatternProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("FindFilesWithPatternProxy : {0} Throw : {1}", rawFileName, ex.Message);
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

                this.logger.Debug("FindStreamsProxy: {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.FindStreams(rawFileName, out files, rawFileInfo);

                Debug.Assert(!(result == DokanResult.NotImplemented && files == null));
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (FileInformation fi in files)
                    {
                        this.logger.Debug("\tFileName\t{0}", fi.FileName);
                        this.logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill =
                       (FILL_FIND_STREAM_DATA)Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_STREAM_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    for (int index = 0; index < files.Count; index++)
                    {
                        Addto(fill, rawFileInfo, files[index]);
                    }
                }

                this.logger.Debug("FindStreamsProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("FindStreamsProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("SetEndOfFileProxy : {0}", rawFileName);
                this.logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.SetEndOfFile(rawFileName, rawByteOffset, rawFileInfo);

                this.logger.Debug("SetEndOfFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("SetEndOfFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus SetAllocationSizeProxy(string rawFileName, long rawLength,
                                          DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("SetAllocationSizeProxy : {0}", rawFileName);
                this.logger.Debug("\tLength\t{0}", rawLength);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.SetAllocationSize(rawFileName, rawLength, rawFileInfo);

                this.logger.Debug("SetAllocationSizeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("SetAllocationSizeProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("SetFileAttributesProxy : {0}", rawFileName);
                this.logger.Debug("\tAttributes\t{0}", (FileAttributes)rawAttributes);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileAttributes(rawFileName, (FileAttributes)rawAttributes, rawFileInfo);

                this.logger.Debug("SetFileAttributesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("SetFileAttributesProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("SetFileTimeProxy : {0}", rawFileName);
                this.logger.Debug("\tCreateTime\t{0}", ctime);
                this.logger.Debug("\tAccessTime\t{0}", atime);
                this.logger.Debug("\tWriteTime\t{0}", mtime);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileTime(rawFileName, ctime, atime, mtime, rawFileInfo);

                this.logger.Debug("SetFileTimeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("SetFileTimeProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteFileProxy(string rawFileName,
                                   DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("DeleteFileProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.DeleteFile(rawFileName, rawFileInfo);

                this.logger.Debug("DeleteFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("DeleteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteDirectoryProxy(string rawFileName,
                                        DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("DeleteDirectoryProxy : {0}", rawFileName);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.DeleteDirectory(rawFileName, rawFileInfo);

                this.logger.Debug("DeleteDirectoryProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("DeleteDirectoryProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("MoveFileProxy : {0}", rawFileName);
                this.logger.Debug("\tNewFileName\t{0}", rawNewFileName);
                this.logger.Debug("\tReplaceIfExisting\t{0}", rawReplaceIfExisting);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.MoveFile(rawFileName, rawNewFileName, rawReplaceIfExisting, rawFileInfo);

                this.logger.Debug("MoveFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("MoveFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("LockFileProxy : {0}", rawFileName);
                this.logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                this.logger.Debug("\tLength\t{0}", rawLength);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.LockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                this.logger.Debug("LockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("LockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                this.logger.Debug("UnlockFileProxy : {0}", rawFileName);
                this.logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                this.logger.Debug("\tLength\t{0}", rawLength);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.UnlockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                this.logger.Debug("UnlockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("UnlockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus GetDiskFreeSpaceProxy(ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
                                         DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("GetDiskFr{0}eeSpaceProxy", "ARG0");
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.GetDiskFreeSpace(out rawFreeBytesAvailable, out rawTotalNumberOfBytes, out rawTotalNumberOfFreeBytes, rawFileInfo);

                this.logger.Debug("\tFreeBytesAvailable\t{0}", rawFreeBytesAvailable);
                this.logger.Debug("\tTotalNumberOfBytes\t{0}", rawTotalNumberOfBytes);
                this.logger.Debug("\tTotalNumberOfFreeBytes\t{0}", rawTotalNumberOfFreeBytes);
                this.logger.Debug("GetDiskFreeSpaceProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("GetDiskFreeSpaceProxy Throw : {0}", ex.Message);
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
                this.logger.Debug("GetVolumeInformationProxy");
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));
                NtStatus result = operations.GetVolumeInformation(out label, out rawFileSystemFlags, out name, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(!String.IsNullOrEmpty(name));
                    Debug.Assert(!String.IsNullOrEmpty(label));
                    rawVolumeNameBuffer.Append(label);
                    rawFileSystemNameBuffer.Append(name);

                    this.logger.Debug("\tVolumeNameBuffer\t{0}", rawVolumeNameBuffer);
                    this.logger.Debug("\tFileSystemNameBuffer\t{0}", rawFileSystemNameBuffer);
                    this.logger.Debug("\tVolumeSerialNumber\t{0}", rawVolumeSerialNumber);
                    this.logger.Debug("\tFileSystemFlags\t{0}", rawFileSystemFlags);
                }

                this.logger.Debug("GetVolumeInformationProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("GetVolumeInformationProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus MountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("MountedProxy");
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.Mounted(rawFileInfo);

                this.logger.Debug("MountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("MountedProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus UnmountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                this.logger.Debug("UnmountedProxy");
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.Unmounted(rawFileInfo);

                this.logger.Debug("UnmountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("UnmountedProxy Throw : {0}", ex.Message);
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
                this.logger.Debug("GetFileSecurityProxy : {0}", rawFileName);
                this.logger.Debug("\tFileSystemSecurity\t{0}", sect);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.GetFileSecurity(rawFileName, out sec, sect, rawFileInfo);
                if (result == DokanResult.Success /*&& sec != null*/)
                {
                    Debug.Assert(sec != null);
                    this.logger.Debug("\tFileSystemSecurity Result : {0}", sec);
                    var buffer = sec.GetSecurityDescriptorBinaryForm();
                    rawSecurityDescriptorLengthNeeded = (uint)buffer.Length;
                    if (buffer.Length > rawSecurityDescriptorLength)
                        return DokanResult.BufferOverflow;

                    Marshal.Copy(buffer, 0, rawSecurityDescriptor, buffer.Length);
                }

                this.logger.Debug("GetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("GetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
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

                this.logger.Debug("SetFileSecurityProxy : {0}", rawFileName);
                this.logger.Debug("\tAccessControlSections\t{0}", sect);
                this.logger.Debug("\tFileSystemSecurity\t{0}", sec);
                this.logger.Debug("\tContext\t{0}", this.ToTrace(rawFileInfo));

                NtStatus result = operations.SetFileSecurity(rawFileName, sec, sect, rawFileInfo);

                this.logger.Debug("SetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                this.logger.Error("SetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        #region Nested type: FILL_FIND_FILE_DATA

        private delegate long FILL_FIND_FILE_DATA(
            ref WIN32_FIND_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Nested type: FILL_FIND_FILE_DATA

        #region Nested type: FILL_FIND_STREAM_DATA

        private delegate long FILL_FIND_STREAM_DATA(
            ref WIN32_FIND_STREAM_DATA rawFindData, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Nested type: FILL_FIND_STREAM_DATA
    }
}
