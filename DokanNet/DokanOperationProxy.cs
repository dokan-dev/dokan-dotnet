using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using DokanNet.Logging;
using DokanNet.Native;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DokanNet
{
    internal sealed class DokanOperationProxy
    {
        #region Delegates

        public delegate NtStatus ZwCreateFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr securityContext, uint rawDesiredAccess,
            uint rawFileAttributes,
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
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus WriteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] rawBuffer, uint rawNumberOfBytesToWrite,
            ref int rawNumberOfBytesWritten, long rawOffset,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FlushFileBuffersDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName, ref BY_HANDLE_FILE_INFORMATION handleFileInfo,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo fileInfo);

        public delegate NtStatus FindFilesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus FindFilesWithPatternDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawSearchPattern,
            IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileAttributesDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, uint rawAttributes,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileTimeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            ref FILETIME rawCreationTime, ref FILETIME rawLastAccessTime, ref FILETIME rawLastWriteTime,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus DeleteFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus DeleteDirectoryDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus MoveFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string rawNewFileName,
            [MarshalAs(UnmanagedType.Bool)] bool rawReplaceIfExisting,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetEndOfFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetAllocationSizeDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus LockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnlockFileDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, long rawByteOffset, long rawLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetDiskFreeSpaceDelegate(
            ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes, ref long rawTotalNumberOfFreeBytes,
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetVolumeInformationDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize,
            ref uint rawVolumeSerialNumber, ref uint rawMaximumComponentLength,
            ref FileSystemFeatures rawFileSystemFlags,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder rawFileSystemNameBuffer,
            uint rawFileSystemNameSize, [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus GetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus SetFileSecurityDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, [In] ref SECURITY_INFORMATION rawSecurityInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        /// <summary>
        /// Retrieve all FileStreams informations on the file.
        /// This is only called if <see cref="DokanOptions.AltStream"/> is enabled.
        /// </summary>
        /// <remarks>Supported since 0.8.0. 
        /// You must specify the version at <see cref="DOKAN_OPTIONS.Version"/>.</remarks>
        /// <param name="rawFileName">Filename</param>
        /// <param name="rawFillFindData">A <see cref="IntPtr"/> to a <see cref="FILL_FIND_STREAM_DATA"/></param>
        /// <param name="rawFileInfo">A <see cref="DokanFileInfo"/></param>
        /// <returns></returns>
        public delegate NtStatus FindStreamsDelegate(
            [MarshalAs(UnmanagedType.LPWStr)] string rawFileName, IntPtr rawFillFindData, // function pointer
            [MarshalAs(UnmanagedType.LPStruct), In /*, Out*/] DokanFileInfo rawFileInfo);

        public delegate NtStatus MountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        public delegate NtStatus UnmountedDelegate(
            [MarshalAs(UnmanagedType.LPStruct), In] DokanFileInfo rawFileInfo);

        #endregion Delegates

        private readonly IDokanOperations _operations;

        private readonly ILogger _logger;

        private readonly uint _serialNumber;

        public DokanOperationProxy(IDokanOperations operations, ILogger logger)
        {
            _operations = operations;
            _logger = logger;
            _serialNumber = (uint)_operations.GetHashCode();
        }

        /// <summary>
        /// Called when a file is to be created
        /// See https://msdn.microsoft.com/en-us/library/windows/hardware/ff566424(v=vs.85).aspx
        /// </summary>
        public NtStatus ZwCreateFileProxy(string rawFileName, IntPtr securityContext, uint rawDesiredAccess,
            uint rawFileAttributes,
            uint rawShareAccess, uint rawCreateDisposition, uint rawCreateOptions,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                FileOptions fileOptions = 0;
                FileAttributes fileAttributes = 0;
                var fileAttributesAndFlags = 0;
                var creationDisposition = 0;
                NativeMethods.DokanMapKernelToUserCreateFileFlags(rawFileAttributes, rawCreateOptions,
                    rawCreateDisposition, ref fileAttributesAndFlags, ref creationDisposition);

                foreach (FileOptions fileOption in Enum.GetValues(typeof(FileOptions)))
                {
                    if (((FileOptions) (fileAttributesAndFlags & 0xffffc000) & fileOption) == fileOption)
                        fileOptions |= fileOption;
                }

                foreach (FileAttributes fileAttribute in Enum.GetValues(typeof(FileAttributes)))
                {
                    if (((FileAttributes) (fileAttributesAndFlags & 0x3fff) & fileAttribute) == fileAttribute)
                        fileAttributes |= fileAttribute;
                }

                _logger.Debug("CreateFileProxy : {0}", rawFileName);
                _logger.Debug("\tCreationDisposition\t{0}", (FileMode) creationDisposition);
                _logger.Debug("\tFileAccess\t{0}", (FileAccess) rawDesiredAccess);
                _logger.Debug("\tFileShare\t{0}", (FileShare) rawShareAccess);
                _logger.Debug("\tFileOptions\t{0}", fileOptions);
                _logger.Debug("\tFileAttributes\t{0}", fileAttributes);
                _logger.Debug("\tContext\t{0}", rawFileInfo);
                var result = _operations.CreateFile(rawFileName,
                    (FileAccess) rawDesiredAccess,
                    (FileShare) rawShareAccess,
                    (FileMode) creationDisposition,
                    fileOptions,
                    fileAttributes,
                    rawFileInfo);

                _logger.Debug("CreateFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("CreateFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.Unsuccessful;
            }
        }

        ////

        public void CleanupProxy(string rawFileName,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("CleanupProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                _operations.Cleanup(rawFileName, rawFileInfo);

                _logger.Debug("CleanupProxy : {0}", rawFileName);
            }
            catch (Exception ex)
            {
                _logger.Error("CleanupProxy : {0} Throw : {1}", rawFileName, ex.Message);
            }
        }

        ////

        public void CloseFileProxy(string rawFileName,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("CloseFileProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                _operations.CloseFile(rawFileName, rawFileInfo);

                _logger.Debug("CloseFileProxy : {0}", rawFileName);
            }
            catch (Exception ex)
            {
                _logger.Error("CloseFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
            }
        }

        ////

        public NtStatus ReadFileProxy(string rawFileName,
            byte[] rawBuffer, uint rawBufferLength, ref int rawReadLength, long rawOffset,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("ReadFileProxy : " + rawFileName);
                _logger.Debug("\tBufferLength\t" + rawBufferLength);
                _logger.Debug("\tOffset\t" + rawOffset);
                _logger.Debug("\tContext\t" + rawFileInfo);

                var result = _operations.ReadFile(rawFileName, rawBuffer, out rawReadLength, rawOffset, rawFileInfo);

                _logger.Debug("ReadFileProxy : " + rawFileName + " Return : " + result + " ReadLength : " + rawReadLength);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("ReadFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                _logger.Debug("WriteFileProxy : {0}", rawFileName);
                _logger.Debug("\tNumberOfBytesToWrite\t{0}", rawNumberOfBytesToWrite);
                _logger.Debug("\tOffset\t{0}", rawOffset);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.WriteFile(rawFileName, rawBuffer, out rawNumberOfBytesWritten, rawOffset,
                    rawFileInfo);

                _logger.Debug("WriteFileProxy : {0} Return : {1} NumberOfBytesWritten : {2}", rawFileName, result,
                    rawNumberOfBytesWritten);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("WriteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus FlushFileBuffersProxy(string rawFileName,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("FlushFileBuffersProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FlushFileBuffers(rawFileName, rawFileInfo);

                _logger.Debug("FlushFileBuffersProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FlushFileBuffersProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus GetFileInformationProxy(string rawFileName,
            ref BY_HANDLE_FILE_INFORMATION rawHandleFileInformation,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("GetFileInformationProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                FileInformation fi;
                var result = _operations.GetFileInformation(rawFileName, out fi, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(fi.FileName != null);
                    _logger.Debug("\tFileName\t{0}", fi.FileName);
                    _logger.Debug("\tAttributes\t{0}", fi.Attributes);
                    _logger.Debug("\tCreationTime\t{0}", fi.CreationTime);
                    _logger.Debug("\tLastAccessTime\t{0}", fi.LastAccessTime);
                    _logger.Debug("\tLastWriteTime\t{0}", fi.LastWriteTime);
                    _logger.Debug("\tLength\t{0}", fi.Length);

                    rawHandleFileInformation.dwFileAttributes = (uint) fi.Attributes /* + FILE_ATTRIBUTE_VIRTUAL*/;

                    var ctime = ToFileTime(fi.CreationTime);
                    var atime = ToFileTime(fi.LastAccessTime);
                    var mtime = ToFileTime(fi.LastWriteTime);
                    rawHandleFileInformation.ftCreationTime.dwHighDateTime = (int) (ctime >> 32);
                    rawHandleFileInformation.ftCreationTime.dwLowDateTime = (int) (ctime & 0xffffffff);

                    rawHandleFileInformation.ftLastAccessTime.dwHighDateTime = (int) (atime >> 32);
                    rawHandleFileInformation.ftLastAccessTime.dwLowDateTime = (int) (atime & 0xffffffff);

                    rawHandleFileInformation.ftLastWriteTime.dwHighDateTime = (int) (mtime >> 32);
                    rawHandleFileInformation.ftLastWriteTime.dwLowDateTime = (int) (mtime & 0xffffffff);

                    rawHandleFileInformation.dwVolumeSerialNumber = _serialNumber;

                    rawHandleFileInformation.nFileSizeLow = (uint) (fi.Length & 0xffffffff);
                    rawHandleFileInformation.nFileSizeHigh = (uint) (fi.Length >> 32);
                    rawHandleFileInformation.dwNumberOfLinks = 1;
                    rawHandleFileInformation.nFileIndexHigh = 0;
                    rawHandleFileInformation.nFileIndexLow = (uint) fi.FileName.GetHashCode();
                }

                _logger.Debug("GetFileInformationProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetFileInformationProxy : {0} Throw : {1}", rawFileName, ex.Message);
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

                _logger.Debug("FindFilesProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindFiles(rawFileName, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        _logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        _logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        _logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill =
                        (FILL_FIND_FILE_DATA)
                            Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_FILE_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindFilesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindFilesProxy : {0} Throw : {1}", rawFileName, ex.Message);
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

                _logger.Debug("FindFilesWithPatternProxy : {0}", rawFileName);
                _logger.Debug("\trawSearchPattern\t{0}", rawSearchPattern);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindFilesWithPattern(rawFileName, rawSearchPattern, out files, rawFileInfo);

                Debug.Assert(files != null);
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tAttributes\t{0}", fi.Attributes);
                        _logger.Debug("\t\tCreationTime\t{0}", fi.CreationTime);
                        _logger.Debug("\t\tLastAccessTime\t{0}", fi.LastAccessTime);
                        _logger.Debug("\t\tLastWriteTime\t{0}", fi.LastWriteTime);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill =
                        (FILL_FIND_FILE_DATA)
                            Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_FILE_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindFilesWithPatternProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindFilesWithPatternProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static void AddTo(FILL_FIND_FILE_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!string.IsNullOrEmpty(fi.FileName));
            var ctime = ToFileTime(fi.CreationTime);
            var atime = ToFileTime(fi.LastAccessTime);
            var mtime = ToFileTime(fi.LastWriteTime);
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
                nFileSizeLow = (uint) (fi.Length & 0xffffffff),
                nFileSizeHigh = (uint) (fi.Length >> 32),
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

                _logger.Debug("FindStreamsProxy: {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.FindStreams(rawFileName, out files, rawFileInfo);

                Debug.Assert(!(result == DokanResult.NotImplemented && files == null));
                if (result == DokanResult.Success && files.Count != 0)
                {
                    foreach (var fi in files)
                    {
                        _logger.Debug("\tFileName\t{0}", fi.FileName);
                        _logger.Debug("\t\tLength\t{0}", fi.Length);
                    }

                    var fill =
                        (FILL_FIND_STREAM_DATA)
                            Marshal.GetDelegateForFunctionPointer(rawFillFindData, typeof(FILL_FIND_STREAM_DATA));
                    // used a single entry call to speed up the "enumeration" of the list
                    foreach (var t in files)
                    {
                        AddTo(fill, rawFileInfo, t);
                    }
                }

                _logger.Debug("FindStreamsProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("FindStreamsProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        private static void AddTo(FILL_FIND_STREAM_DATA fill, DokanFileInfo rawFileInfo, FileInformation fi)
        {
            Debug.Assert(!string.IsNullOrEmpty(fi.FileName));
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
                _logger.Debug("SetEndOfFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetEndOfFile(rawFileName, rawByteOffset, rawFileInfo);

                _logger.Debug("SetEndOfFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetEndOfFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus SetAllocationSizeProxy(string rawFileName, long rawLength,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("SetAllocationSizeProxy : {0}", rawFileName);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetAllocationSize(rawFileName, rawLength, rawFileInfo);

                _logger.Debug("SetAllocationSizeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetAllocationSizeProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                _logger.Debug("SetFileAttributesProxy : {0}", rawFileName);
                _logger.Debug("\tAttributes\t{0}", (FileAttributes) rawAttributes);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileAttributes(rawFileName, (FileAttributes) rawAttributes, rawFileInfo);

                _logger.Debug("SetFileAttributesProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileAttributesProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus SetFileTimeProxy(string rawFileName,
            ref FILETIME rawCreationTime, ref FILETIME rawLastAccessTime, ref FILETIME rawLastWriteTime,
            DokanFileInfo rawFileInfo)
        {
            var ctime = (rawCreationTime.dwLowDateTime != 0 || rawCreationTime.dwHighDateTime != 0) &&
                        (rawCreationTime.dwLowDateTime != -1 || rawCreationTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long) rawCreationTime.dwHighDateTime << 32) |
                                        (uint) rawCreationTime.dwLowDateTime)
                : (DateTime?) null;
            var atime = (rawLastAccessTime.dwLowDateTime != 0 || rawLastAccessTime.dwHighDateTime != 0) &&
                        (rawLastAccessTime.dwLowDateTime != -1 || rawLastAccessTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long) rawLastAccessTime.dwHighDateTime << 32) |
                                        (uint) rawLastAccessTime.dwLowDateTime)
                : (DateTime?) null;
            var mtime = (rawLastWriteTime.dwLowDateTime != 0 || rawLastWriteTime.dwHighDateTime != 0) &&
                        (rawLastWriteTime.dwLowDateTime != -1 || rawLastWriteTime.dwHighDateTime != -1)
                ? DateTime.FromFileTime(((long) rawLastWriteTime.dwHighDateTime << 32) |
                                        (uint) rawLastWriteTime.dwLowDateTime)
                : (DateTime?) null;

            try
            {
                _logger.Debug("SetFileTimeProxy : {0}", rawFileName);
                _logger.Debug("\tCreateTime\t{0}", ctime);
                _logger.Debug("\tAccessTime\t{0}", atime);
                _logger.Debug("\tWriteTime\t{0}", mtime);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileTime(rawFileName, ctime, atime, mtime, rawFileInfo);

                _logger.Debug("SetFileTimeProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileTimeProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteFileProxy(string rawFileName,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("DeleteFileProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.DeleteFile(rawFileName, rawFileInfo);

                _logger.Debug("DeleteFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus DeleteDirectoryProxy(string rawFileName,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("DeleteDirectoryProxy : {0}", rawFileName);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.DeleteDirectory(rawFileName, rawFileInfo);

                _logger.Debug("DeleteDirectoryProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("DeleteDirectoryProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                _logger.Debug("MoveFileProxy : {0}", rawFileName);
                _logger.Debug("\tNewFileName\t{0}", rawNewFileName);
                _logger.Debug("\tReplaceIfExisting\t{0}", rawReplaceIfExisting);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.MoveFile(rawFileName, rawNewFileName, rawReplaceIfExisting, rawFileInfo);

                _logger.Debug("MoveFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("MoveFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                _logger.Debug("LockFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.LockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                _logger.Debug("LockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("LockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
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
                _logger.Debug("UnlockFileProxy : {0}", rawFileName);
                _logger.Debug("\tByteOffset\t{0}", rawByteOffset);
                _logger.Debug("\tLength\t{0}", rawLength);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.UnlockFile(rawFileName, rawByteOffset, rawLength, rawFileInfo);

                _logger.Debug("UnlockFileProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("UnlockFileProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        ////

        public NtStatus GetDiskFreeSpaceProxy(ref long rawFreeBytesAvailable, ref long rawTotalNumberOfBytes,
            ref long rawTotalNumberOfFreeBytes,
            DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("GetDiskFreeSpaceProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.GetDiskFreeSpace(out rawFreeBytesAvailable, out rawTotalNumberOfBytes,
                    out rawTotalNumberOfFreeBytes, rawFileInfo);

                _logger.Debug("\tFreeBytesAvailable\t{0}", rawFreeBytesAvailable);
                _logger.Debug("\tTotalNumberOfBytes\t{0}", rawTotalNumberOfBytes);
                _logger.Debug("\tTotalNumberOfFreeBytes\t{0}", rawTotalNumberOfFreeBytes);
                _logger.Debug("GetDiskFreeSpaceProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetDiskFreeSpaceProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus GetVolumeInformationProxy(StringBuilder rawVolumeNameBuffer, uint rawVolumeNameSize,
            ref uint rawVolumeSerialNumber, ref uint rawMaximumComponentLength,
            ref FileSystemFeatures rawFileSystemFlags,
            StringBuilder rawFileSystemNameBuffer, uint rawFileSystemNameSize,
            DokanFileInfo rawFileInfo)
        {
            rawMaximumComponentLength = 256;
            rawVolumeSerialNumber = _serialNumber;
            try
            {
                _logger.Debug("GetVolumeInformationProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);
                string label;
                string name;
                var result = _operations.GetVolumeInformation(out label, out rawFileSystemFlags, out name, rawFileInfo);

                if (result == DokanResult.Success)
                {
                    Debug.Assert(!string.IsNullOrEmpty(name));
                    Debug.Assert(!string.IsNullOrEmpty(label));
                    rawVolumeNameBuffer.Append(label);
                    rawFileSystemNameBuffer.Append(name);

                    _logger.Debug("\tVolumeNameBuffer\t{0}", rawVolumeNameBuffer);
                    _logger.Debug("\tFileSystemNameBuffer\t{0}", rawFileSystemNameBuffer);
                    _logger.Debug("\tVolumeSerialNumber\t{0}", rawVolumeSerialNumber);
                    _logger.Debug("\tFileSystemFlags\t{0}", rawFileSystemFlags);
                }

                _logger.Debug("GetVolumeInformationProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetVolumeInformationProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus MountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("MountedProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.Mounted(rawFileInfo);

                _logger.Debug("MountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("MountedProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus UnmountedProxy(DokanFileInfo rawFileInfo)
        {
            try
            {
                _logger.Debug("UnmountedProxy:");
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.Unmounted(rawFileInfo);

                _logger.Debug("UnmountedProxy Return : {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("UnmountedProxy Throw : {0}", ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus GetFileSecurityProxy(string rawFileName,
            ref SECURITY_INFORMATION rawRequestedInformation,
            IntPtr rawSecurityDescriptor, uint rawSecurityDescriptorLength,
            ref uint rawSecurityDescriptorLengthNeeded,
            DokanFileInfo rawFileInfo)
        {
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
                _logger.Debug("GetFileSecurityProxy : {0}", rawFileName);
                _logger.Debug("\tFileSystemSecurity\t{0}", sect);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                FileSystemSecurity sec;
                var result = _operations.GetFileSecurity(rawFileName, out sec, sect, rawFileInfo);
                if (result == DokanResult.Success /*&& sec != null*/)
                {
                    Debug.Assert(sec != null);
                    _logger.Debug("\tFileSystemSecurity Result : {0}", sec);
                    var buffer = sec.GetSecurityDescriptorBinaryForm();
                    rawSecurityDescriptorLengthNeeded = (uint) buffer.Length;
                    if (buffer.Length > rawSecurityDescriptorLength)
                        return DokanResult.BufferOverflow;

                    Marshal.Copy(buffer, 0, rawSecurityDescriptor, buffer.Length);
                }

                _logger.Debug("GetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("GetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        public NtStatus SetFileSecurityProxy(string rawFileName,
            ref SECURITY_INFORMATION rawSecurityInformation, IntPtr rawSecurityDescriptor,
            uint rawSecurityDescriptorLength,
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
                Marshal.Copy(rawSecurityDescriptor, buffer, 0, (int) rawSecurityDescriptorLength);
                var sec = rawFileInfo.IsDirectory ? (FileSystemSecurity) new DirectorySecurity() : new FileSecurity();
                sec.SetSecurityDescriptorBinaryForm(buffer);

                _logger.Debug("SetFileSecurityProxy : {0}", rawFileName);
                _logger.Debug("\tAccessControlSections\t{0}", sect);
                _logger.Debug("\tFileSystemSecurity\t{0}", sec);
                _logger.Debug("\tContext\t{0}", rawFileInfo);

                var result = _operations.SetFileSecurity(rawFileName, sec, sect, rawFileInfo);

                _logger.Debug("SetFileSecurityProxy : {0} Return : {1}", rawFileName, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("SetFileSecurityProxy : {0} Throw : {1}", rawFileName, ex.Message);
                return DokanResult.InvalidParameter;
            }
        }

        /// <summary>
        /// Converts the value of <paramref name="dateTime"/> to a Windows file time.
        /// If <paramref name="dateTime"/> is null or before 12:00 midnight January 1, 1601 C.E. UTC, it returns 0
        /// </summary>
        /// <returns>The value of <paramref name="dateTime"/> expressed as a Windows file time
        ///  -or- it returns 0 if <paramref name="dateTime"/> is before 12:00 midnight January 1, 1601 C.E. UTC or null.</returns>
        [Pure]
        private static long ToFileTime(DateTime? dateTime)
        {
            //See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365739(v=vs.85).aspx
            return dateTime.HasValue && (dateTime.Value >= DateTime.FromFileTime(0))
                ? dateTime.Value.ToFileTime()
                : 0;
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
