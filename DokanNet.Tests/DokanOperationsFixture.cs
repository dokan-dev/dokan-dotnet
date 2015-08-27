using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    internal sealed class DokanOperationsFixture
    {
        private class Proxy : IDokanOperations
        {
            public IDokanOperations Target { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                Justification = "Explicit Exception handler")]
            private static DokanResult TryExecute(Func<DokanResult> func)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"**{ex.GetType().Name}**: {ex.Message}");
                    return DokanResult.ExceptionInService;
                }
            }

            public DokanResult Cleanup(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.Cleanup(fileName, info));

            public DokanResult CloseFile(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.CloseFile(fileName, info));

            public DokanResult CreateDirectory(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.CreateDirectory(fileName, info));

            public DokanResult CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
                => TryExecute(() => Target.CreateFile(fileName, access, share, mode, options, attributes, info));

            public DokanResult DeleteDirectory(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.DeleteDirectory(fileName, info));

            public DokanResult DeleteFile(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.DeleteFile(fileName, info));

            public DokanResult FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
                => Target.FindFiles(fileName, out files, info);

            public DokanResult FlushFileBuffers(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.FlushFileBuffers(fileName, info));

            public DokanResult GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, DokanFileInfo info)
                => Target.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, info);

            public DokanResult GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
                => Target.GetFileInformation(fileName, out fileInfo, info);

            public DokanResult GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
                => Target.GetFileSecurity(fileName, out security, sections, info);

            public DokanResult GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
                => Target.GetVolumeInformation(out volumeLabel, out features, out fileSystemName, info);

            public DokanResult LockFile(string fileName, long offset, long length, DokanFileInfo info)
                => TryExecute(() => Target.LockFile(fileName, offset, length, info));

            public DokanResult MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
                => TryExecute(() => Target.MoveFile(oldName, newName, replace, info));

            public DokanResult OpenDirectory(string fileName, DokanFileInfo info)
                => TryExecute(() => Target.OpenDirectory(fileName, info));

            public DokanResult ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
                => Target.ReadFile(fileName, buffer, out bytesRead, offset, info);

            public DokanResult SetAllocationSize(string fileName, long length, DokanFileInfo info)
                => TryExecute(() => Target.SetAllocationSize(fileName, length, info));

            public DokanResult SetEndOfFile(string fileName, long length, DokanFileInfo info)
                => TryExecute(() => Target.SetEndOfFile(fileName, length, info));

            public DokanResult SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
                => TryExecute(() => Target.SetFileAttributes(fileName, attributes, info));

            public DokanResult SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
                => TryExecute(() => Target.SetFileSecurity(fileName, security, sections, info));

            public DokanResult SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
                => TryExecute(() => Target.SetFileTime(fileName, creationTime, lastAccessTime, lastWriteTime, info));

            public DokanResult UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
                => TryExecute(() => Target.UnlockFile(fileName, offset, length, info));

            public DokanResult Unmount(DokanFileInfo info)
                => TryExecute(() => Target.Unmount(info));

            public DokanResult WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
                => Target.WriteFile(fileName, buffer, out bytesWritten, offset, info);
        }

        public const char MOUNT_POINT = 'Z';

        public const string VOLUME_LABEL = "Dokan Volume";

        public const string FILESYSTEM_NAME = "Dokan Test";

        private const FileSystemFeatures fileSystemFeatures = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch | FileSystemFeatures.SupportsRemoteStorage | FileSystemFeatures.UnicodeOnDisk;

        private const FileMode readFileMode = FileMode.Open;

        private const FileOptions readFileOptions = FileOptions.None;

        private const FileOptions writeFileOptions = FileOptions.None;

        private const FileAttributes readFileAttributes = default(FileAttributes);

        private const FileAttributes writeFileAttributes = default(FileAttributes);

        private static Proxy proxy = new Proxy();

        private Mock<IDokanOperations> operations = new Mock<IDokanOperations>(MockBehavior.Strict);

        private long pendingFiles;

        internal static IDokanOperations Operations => proxy;

        internal static DokanOperationsFixture Instance { get; private set; }

        internal static string DriveName = new string(new[] { MOUNT_POINT, Path.VolumeSeparatorChar });

        internal static string RootName => @"\";

        internal static string FileName => "File.ext";

        internal static string DestinationFileName => "DestinationFile.txe";

        internal static string DestinationBackupFileName => "BackupFile.txe";

        internal static string DirectoryName => "Dir";

        internal static string Directory2Name => "Dir2";

        internal static string DestinationDirectoryName => "DestinationDir";

        internal static string SubDirectoryName => "SubDir";

        internal static string SubDirectory2Name => "SubDir2";

        internal static FileInformation[] RootDirectoryItems { get; } = {
                new FileInformation() { FileName = DirectoryName, Attributes = FileAttributes.Directory, CreationTime = ToDateTime("2015-01-01 10:11:12"), LastWriteTime = ToDateTime("2015-01-01 20:21:22"), LastAccessTime = ToDateTime("2015-01-01 20:21:22") },
                new FileInformation() { FileName = Directory2Name, Attributes = FileAttributes.Directory, CreationTime = ToDateTime("2015-01-01 13:14:15"), LastWriteTime = ToDateTime("2015-01-01 23:24:25"), LastAccessTime = ToDateTime("2015-01-01 23:24:25") },
                new FileInformation() { FileName = FileName, Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-01-02 10:11:12"), LastWriteTime = ToDateTime("2015-01-02 20:21:22"), LastAccessTime = ToDateTime("2015-01-02 20:21:22") },
                new FileInformation() { FileName = "SecondFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-01-03 10:11:12"), LastWriteTime = ToDateTime("2015-01-03 20:21:22"), LastAccessTime = ToDateTime("2015-01-03 20:21:22") },
                new FileInformation() { FileName = "ThirdFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-01-04 10:11:12"), LastWriteTime = ToDateTime("2015-01-04 20:21:22"), LastAccessTime = ToDateTime("2015-01-04 20:21:22") }
            };

        internal static FileInformation[] DirectoryItems { get; } = {
                new FileInformation() { FileName = SubDirectoryName, Attributes = FileAttributes.Directory, CreationTime = ToDateTime("2015-02-01 10:11:12"), LastWriteTime = ToDateTime("2015-02-01 20:21:22"), LastAccessTime = ToDateTime("2015-02-01 20:21:22") },
                new FileInformation() { FileName = "SubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-02 10:11:12"), LastWriteTime = ToDateTime("2015-02-02 20:21:22"), LastAccessTime = ToDateTime("2015-02-02 20:21:22") },
                new FileInformation() { FileName = "SecondSubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-03 10:11:12"), LastWriteTime = ToDateTime("2015-02-03 20:21:22"), LastAccessTime = ToDateTime("2015-02-03 20:21:22") },
                new FileInformation() { FileName = "ThirdSubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-04 10:11:12"), LastWriteTime = ToDateTime("2015-02-04 20:21:22"), LastAccessTime = ToDateTime("2015-02-04 20:21:22") }
            };

        internal static FileInformation[] Directory2Items { get; } = {
                new FileInformation() { FileName = SubDirectory2Name, Attributes = FileAttributes.Directory, CreationTime = ToDateTime("2015-02-01 10:11:12"), LastWriteTime = ToDateTime("2015-02-01 20:21:22"), LastAccessTime = ToDateTime("2015-02-01 20:21:22") },
                new FileInformation() { FileName = "SubFile2.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-02 10:11:12"), LastWriteTime = ToDateTime("2015-02-02 20:21:22"), LastAccessTime = ToDateTime("2015-02-02 20:21:22") },
                new FileInformation() { FileName = "SecondSubFile2.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-03 10:11:12"), LastWriteTime = ToDateTime("2015-02-03 20:21:22"), LastAccessTime = ToDateTime("2015-02-03 20:21:22") },
                new FileInformation() { FileName = "ThirdSubFile2.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-02-04 10:11:12"), LastWriteTime = ToDateTime("2015-02-04 20:21:22"), LastAccessTime = ToDateTime("2015-02-04 20:21:22") }
            };

        internal static FileInformation[] SubDirectoryItems { get; } = {
                new FileInformation() { FileName = "SubSubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-03-01 10:11:12"), LastWriteTime = ToDateTime("2015-03-01 20:21:22"), LastAccessTime = ToDateTime("2015-03-01 20:21:22") },
                new FileInformation() { FileName = "SecondSubSubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-03-02 10:11:12"), LastWriteTime = ToDateTime("2015-03-02 20:21:22"), LastAccessTime = ToDateTime("2015-03-02 20:21:22") },
                new FileInformation() { FileName = "ThirdSubSubFile.ext", Attributes = FileAttributes.Normal, CreationTime = ToDateTime("2015-03-03 10:11:12"), LastWriteTime = ToDateTime("2015-03-03 20:21:22"), LastAccessTime = ToDateTime("2015-03-03 20:21:22") }
            };

        internal static DirectorySecurity DefaultDirectorySecurity { get; private set; }

        internal static FileSecurity DefaultFileSecurity { get; private set; }

        internal static TimeSpan IODelay = TimeSpan.FromSeconds(19);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DokanOperationsFixture()
        {
            InitInstance();
            Instance.SetupMount();

            InitSecurity();
        }

        private static DateTime ToDateTime(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture);

        internal static string DriveBasedPath(string fileName)
            => DriveName + RootedPath(fileName);

        internal static string RootedPath(string fileName)
            => Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + fileName.TrimStart(Path.DirectorySeparatorChar);

        internal static void InitInstance()
        {
            Instance = new DokanOperationsFixture();
            proxy.Target = Instance.operations.Object;
        }

        internal static void ClearInstance()
        {
            proxy.Target = null;
            Instance = null;
        }

        internal static void InitSecurity()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);

            DefaultDirectorySecurity = new DirectorySecurity();
            DefaultDirectorySecurity.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.Read | FileSystemRights.Traverse, AccessControlType.Allow));
            DefaultDirectorySecurity.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.Write | FileSystemRights.Delete, AccessControlType.Deny));

            DefaultFileSecurity = new FileSecurity();
            DefaultFileSecurity.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.Read | FileSystemRights.ExecuteFile, AccessControlType.Allow));
            DefaultFileSecurity.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.Write | FileSystemRights.Delete, AccessControlType.Deny));
        }

        internal static IList<FileInformation> GetEmptyDirectoryDefaultFiles()
            => new[] {
                new FileInformation() { FileName = ".", Attributes = FileAttributes.Directory, CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today },
                new FileInformation() { FileName = "..", Attributes = FileAttributes.Directory, CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today }
            };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal void SetupAny()
        {
            operations
                .Setup(d => d.Cleanup(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            operations
                .Setup(d => d.CloseFile(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            operations
                .Setup(d => d.CreateDirectory(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CreateDirectory)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            operations
                .Setup(d => d.CreateFile(It.IsAny<string>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>(), It.IsAny<FileMode>(), It.IsAny<FileOptions>(), It.IsAny<FileAttributes>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.CreateFile)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", [{access}], [{share}], {mode}, [{options}], [{attributes}], {info.Log()})"));

            operations
                .Setup(d => d.DeleteDirectory(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.DeleteDirectory)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            operations
                .Setup(d => d.DeleteFile(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.DeleteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            var files = GetEmptyDirectoryDefaultFiles();
            operations
                .Setup(d => d.FindFiles(It.IsAny<string>(), out files, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, IList<FileInformation> _files, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.FindFiles)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_files.Count}], {info.Log()})"));

            operations
                .Setup(d => d.FlushFileBuffers(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.FlushFileBuffers)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            long freeBytesAvailable = 0;
            long totalNumberOfBytes = 0;
            long totalNumberOfFreeBytes = 0;
            operations
                .Setup(d => d.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((long _freeBytesAvailable, long _totalNumberOfBytes, long _totalNumberOfFreeBytes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetDiskFreeSpace)}[{Interlocked.Read(ref pendingFiles)}] (out {_freeBytesAvailable}, out {_totalNumberOfBytes}, out {_totalNumberOfFreeBytes}, {info.Log()})"));

            var directoryInfo = new FileInformation()
            {
                FileName = "DummyDir",
                Attributes = FileAttributes.Directory,
                CreationTime = new DateTime(2015, 1, 1, 12, 0, 0),
                LastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0),
                LastAccessTime = new DateTime(2015, 5, 31, 12, 0, 0)
            };
            operations
                .Setup(d => d.GetFileInformation(It.IsAny<string>(), out directoryInfo, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileInformation _fileInfo, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetFileInformation)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_fileInfo.Log()}], {info.Log()})"));
            var fileInfo = new FileInformation()
            {
                FileName = "Dummy.ext",
                Attributes = FileAttributes.Normal,
                Length = 1024,
                CreationTime = new DateTime(2015, 1, 1, 12, 0, 0),
                LastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0),
                LastAccessTime = new DateTime(2015, 5, 31, 12, 0, 0)
            };
            operations
                .Setup(d => d.GetFileInformation(It.IsAny<string>(), out fileInfo, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileInformation _fileInfo, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetFileInformation)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_fileInfo.Log()}], {info.Log()})"));

            var fileSecurity = new FileSecurity() as FileSystemSecurity;
            operations
                .Setup(d => d.GetFileSecurity(It.IsAny<string>(), out fileSecurity, It.IsAny<AccessControlSections>(), It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileSystemSecurity _fileSecurity, AccessControlSections sections, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.GetFileSecurity)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out {_fileSecurity}, {sections}, {info.Log()})"));
            var directorySecurity = new DirectorySecurity() as FileSystemSecurity;
            operations
                .Setup(d => d.GetFileSecurity(It.IsAny<string>(), out directorySecurity, It.IsAny<AccessControlSections>(), It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileSystemSecurity _directorySecurity, AccessControlSections sections, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.GetFileSecurity)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out {_directorySecurity}, {sections}, {info.Log()})"));

            string volumeLabel = VOLUME_LABEL;
            var features = fileSystemFeatures;
            string fileSystemName = FILESYSTEM_NAME;
            operations
                .Setup(d => d.GetVolumeInformation(out volumeLabel, out features, out fileSystemName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string _volumeLabel, FileSystemFeatures _features, string _fileSystemName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.GetVolumeInformation)}[{Interlocked.Read(ref pendingFiles)}] (out \"{_volumeLabel}\", out [{_features}], out \"{_fileSystemName}\", {info.Log()})"));

            operations
                .Setup(d => d.LockFile(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, long offset, long length, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.LockFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {offset}, {length}, {info.Log()})"));

            operations
                .Setup(d => d.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string oldName, string newName, bool replace, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.MoveFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{oldName}\", \"{newName}\", {replace}, {info.Log()})"));

            operations
                .Setup(d => d.OpenDirectory(It.IsAny<string>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            int bytesRead = 0;
            operations
                .Setup(d => d.ReadFile(It.IsAny<string>(), It.IsAny<byte[]>(), out bytesRead, It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] buffer, int _bytesRead, long offset, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.ReadFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{buffer.Length}], {_bytesRead}, {offset}, {info.Log()})"));

            operations
                .Setup(d => d.SetAllocationSize(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, long length, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.SetAllocationSize)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {length}, {info.Log()})"));

            operations
                .Setup(d => d.SetEndOfFile(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, long length, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.SetEndOfFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {length}, {info.Log()})"));

            operations
                .Setup(d => d.SetFileAttributes(It.IsAny<string>(), It.IsAny<FileAttributes>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileAttributes attributes, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.SetFileAttributes)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{attributes}], {info.Log()})"));

            operations
                .Setup(d => d.SetFileSecurity(It.IsAny<string>(), It.IsAny<FileSystemSecurity>(), It.IsAny<AccessControlSections>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.SetFileSecurity)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{security}], {sections}, {info.Log()})"));

            operations
                .Setup(d => d.SetFileTime(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.SetFileTime)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {creationTime}, {lastAccessTime}, {lastWriteTime}, {info.Log()})"));

            operations
                .Setup(d => d.UnlockFile(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, long offset, long length, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.UnlockFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {offset}, {length}, {info.Log()})"));

            operations
                .Setup(d => d.Unmount(It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Unmount)}[{Interlocked.Read(ref pendingFiles)}] ({info.Log()})"));

            int bytesWritten = 0;
            operations
                .Setup(d => d.WriteFile(It.IsAny<string>(), It.IsAny<byte[]>(), out bytesWritten, It.IsAny<long>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] buffer, int _bytesWritten, long offset, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.WriteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{buffer.Length}], {_bytesWritten}, {offset}, {info.Log()})"));
        }

        private void SetupMount()
        {
            operations
                .Setup(d => d.CreateFile(RootName, FileAccess.ReadAttributes, ReadWriteShare, readFileMode, readFileOptions, readFileAttributes, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.CreateFile)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", [{access}], [{share}], {mode}, [{options}], [{attributes}], {info.Log()})"));
            operations
                .Setup(d => d.OpenDirectory(RootName, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            var fileInfo = new FileInformation()
            {
                FileName = RootName,
                Attributes = FileAttributes.Directory,
                CreationTime = new DateTime(2015, 1, 1, 12, 0, 0),
                LastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0),
                LastAccessTime = new DateTime(2015, 3, 31, 12, 0, 0)
            };
            operations
                .Setup(d => d.GetFileInformation(RootName, out fileInfo, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileInformation _fileInfo, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetFileInformation)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_fileInfo.Log()}], {info.Log()})"));
            operations
                .Setup(d => d.Cleanup(RootName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.CloseFile(RootName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));

            operations
                .Setup(d => d.CreateFile(It.Is<string>(s => s.Equals(@"\Desktop.ini", StringComparison.OrdinalIgnoreCase) || s.Equals(@"\Autorun.inf", StringComparison.OrdinalIgnoreCase)), ReadAccess, ReadWriteShare, readFileMode, readFileOptions, readFileAttributes, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.FileNotFound);
        }

        internal void SetupDiskFreeSpace(long freeBytesAvailable = 0, long totalNumberOfBytes = 0, long totalNumberOfFreeBytes = 0)
        {
            operations
                .Setup(d => d.OpenDirectory(RootName, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((long _freeBytesAvailable, long _totalNumberOfBytes, long _totalNumberOfFreeBytes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetDiskFreeSpace)}[{Interlocked.Read(ref pendingFiles)}] (out {_freeBytesAvailable}, out {_totalNumberOfBytes}, out {_totalNumberOfFreeBytes}, {info.Log()})"));
            operations
                .Setup(d => d.Cleanup(RootName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.CloseFile(RootName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupGetVolumeInformation(string volumeLabel, string fileSystemName)
        {
            var features = fileSystemFeatures;
            operations
                .Setup(d => d.GetVolumeInformation(out volumeLabel, out features, out fileSystemName, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string _volumeLabel, FileSystemFeatures _features, string _fileSystemName, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetVolumeInformation)}[{Interlocked.Read(ref pendingFiles)}] (out \"{_volumeLabel}\", out [{_features}], out \"{_fileSystemName}\", {info.Log()})"));
        }

        internal void SetupGetFileInformation(string path, FileAttributes attributes, DateTime? creationTime = null, DateTime? lastWriteTime = null, DateTime? lastAccessTime = null)
        {
            var defaultDateTime = DateTime.Now;
            var fileInfo = new FileInformation()
            {
                FileName = path,
                Attributes = attributes,
                CreationTime = creationTime ?? defaultDateTime,
                LastWriteTime = lastWriteTime ?? defaultDateTime,
                LastAccessTime = lastAccessTime ?? defaultDateTime
            };
            operations
                .Setup(d => d.GetFileInformation(path, out fileInfo, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileInformation _fileInfo, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetFileInformation)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_fileInfo.Log()}], {info.Log()})"));
        }

        internal void SetupFindFiles(string path, IList<FileInformation> fileInfos)
        {
            var anyDateTime = new DateTime(2000, 1, 1, 12, 0, 0);
            operations
                .Setup(d => d.FindFiles(path, out fileInfos, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, IList<FileInformation> _files, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.FindFiles)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", out [{_files.Count}], {info.Log()})"));
        }

        internal void SetupOpenDirectory(string path)
        {
            operations
                .Setup(d => d.OpenDirectory(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.Cleanup(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.CloseFile(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupOpenDirectoryWithError(string path, DokanResult result)
        {
            if (result == DokanResult.Success)
                throw new ArgumentException($"{DokanResult.Success} not supported", nameof(result));

            operations
                .Setup(d => d.OpenDirectory(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(result)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", **{result}**, {info.Log()})"));
        }

        internal void SetupCreateDirectory(string path)
        {
            operations
                .Setup(d => d.CreateDirectory(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CreateDirectory)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.Cleanup(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.CloseFile(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupDeleteDirectory(string path, bool recurse)
        {
            operations
                .Setup(d => d.DeleteDirectory(path, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.DeleteDirectory)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupCreateFile(string path, FileAccess access, FileShare share, FileMode mode, bool isDirectory = false, FileAttributes attributes = default(FileAttributes), bool deleteOnClose = false)
        {
            SetupCreateFileWithoutCleanup(path, access, share, mode, isDirectory, attributes, deleteOnClose);
            SetupCleanupFile(path);
        }

        internal void SetupCreateFileWithoutCleanup(string path, FileAccess access, FileShare share, FileMode mode, bool isDirectory = false, FileAttributes attributes = default(FileAttributes), bool deleteOnClose = false)
        {
            operations
                .Setup(d => d.CreateFile(path, access, share, mode, writeFileOptions, attributes, It.Is<DokanFileInfo>(i => i.IsDirectory == isDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileAccess _access, FileShare _share, FileMode _mode, FileOptions options, FileAttributes _attributes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.CreateFile)}[{Interlocked.Increment(ref pendingFiles)}] (\"{fileName}\", [{_access}], [{_share}], {_mode}, [{options}], [{_attributes}], {info.Log()})"));
        }

        internal void SetupCleanupFile(string path, bool isDirectory = false)
        {
            operations
                .Setup(d => d.Cleanup(path, It.Is<DokanFileInfo>(i => i.IsDirectory == isDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.Cleanup)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
            operations
                .Setup(d => d.CloseFile(path, It.Is<DokanFileInfo>(i => i.IsDirectory == isDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.CloseFile)}[{Interlocked.Decrement(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupCreateFileWithError(string path, DokanResult result)
        {
            if (result == DokanResult.Success)
                throw new ArgumentException($"{DokanResult.Success} not supported", nameof(result));

            operations
                .Setup(d => d.CreateFile(path, It.IsAny<FileAccess>(), It.IsAny<FileShare>(), It.IsAny<FileMode>(), It.IsAny<FileOptions>(), It.IsAny<FileAttributes>(), It.IsAny<DokanFileInfo>()))
                .Returns(result)
                .Callback((string fileName, FileAccess _access, FileShare _share, FileMode _mode, FileOptions options, FileAttributes _attributes, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.CreateFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", **{result}** [{_access}], [{_share}], {_mode}, [{options}], [{_attributes}], {info.Log()})"));
        }

        internal void SetupReadFile(string path, byte[] buffer, int bytesRead)
        {
            operations
                .Setup(d => d.ReadFile(path, It.IsAny<byte[]>(), out bytesRead, 0, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] _buffer, int _bytesRead, long _offset, DokanFileInfo info)
                    =>
                {
                    buffer.CopyTo(_buffer, 0);
                    Console.WriteLine($"{nameof(IDokanOperations.ReadFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_buffer.SequenceEqual(buffer)}, {_bytesRead}, {_offset}, {info.Log()})");
                });
        }

        internal void SetupReadFileWithDelay(string path, byte[] buffer, int bytesRead, TimeSpan delay)
        {
            operations
                .Setup(d => d.ReadFile(path, It.IsAny<byte[]>(), out bytesRead, 0, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                .Callback(() => Thread.Sleep(delay))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] _buffer, int _bytesRead, long _offset, DokanFileInfo info)
                    =>
                {
                    buffer.CopyTo(_buffer, 0);
                    Console.WriteLine($"{nameof(IDokanOperations.ReadFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_buffer.SequenceEqual(buffer)}, {_bytesRead}, {_offset}, {info.Log()})");
                });
        }

        internal void SetupReadFileInChunks(string path, byte[] buffer, int chunkSize)
        {
            for (int offset = 0; offset < buffer.Length; offset += chunkSize)
            {
                int bytesRead = Math.Min(chunkSize, buffer.Length - offset);
                operations
                    .Setup(d => d.ReadFile(path, It.IsAny<byte[]>(), out bytesRead, offset, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                    .Returns(DokanResult.Success)
                    .Callback((string fileName, byte[] _buffer, int _bytesRead, long _offset, DokanFileInfo info)
                        =>
                    {
                        Array.ConstrainedCopy(buffer, (int)_offset, _buffer, 0, _bytesRead);
                        Console.WriteLine($"{nameof(IDokanOperations.ReadFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_buffer.SequenceEqual(buffer.Skip((int)_offset).Take(_bytesRead))}, {_bytesRead}, {_offset}, {info.Log()})");
                    });
            }
        }

        internal void SetupWriteFile(string path, byte[] buffer, int bytesWritten)
        {
            operations
                .Setup(d => d.WriteFile(path, It.Is<byte[]>(b => b.SequenceEqual(buffer)), out bytesWritten, 0, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] _buffer, int _bytesWritten, long offset, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.WriteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_bytesWritten}, {offset}, {info.Log()})"));
        }

        internal void SetupWriteFileWithDelay(string path, byte[] buffer, int bytesWritten, TimeSpan delay)
        {
            operations
                .Setup(d => d.WriteFile(path, It.Is<byte[]>(b => b.SequenceEqual(buffer)), out bytesWritten, 0, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                .Callback(() => Thread.Sleep(delay))
                .Returns(DokanResult.Success)
                .Callback((string fileName, byte[] _buffer, int _bytesWritten, long offset, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.WriteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_bytesWritten}, {offset}, {info.Log()})"));
        }

        internal void SetupWriteFileInChunks(string path, byte[] buffer, int chunkSize)
        {
            for (int offset = 0; offset < buffer.Length; offset += chunkSize)
            {
                int bytesWritten = Math.Min(chunkSize, buffer.Length - offset);
                var chunk = buffer.Skip(offset).Take(bytesWritten);
                operations
                    .Setup(d => d.WriteFile(path, It.Is<byte[]>(b => b.SequenceEqual(chunk)), out bytesWritten, offset, It.Is<DokanFileInfo>(i => !i.IsDirectory && i.SynchronousIo)))
                    .Returns(DokanResult.Success)
                    .Callback((string fileName, byte[] _buffer, int _bytesWritten, long _offset, DokanFileInfo info)
                        => Console.WriteLine($"{nameof(IDokanOperations.WriteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_buffer.Length}], {_bytesWritten}, {_offset}, {info.Log()})"));
            }
        }

        internal void SetupDeleteFile(string path)
        {
            operations
                .Setup(d => d.DeleteFile(path, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.DeleteFile)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})"));
        }

        internal void SetupMoveFile(string path, string destinationPath, bool replace)
        {
            operations
                .Setup(d => d.MoveFile(path, destinationPath, replace, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string oldName, string newName, bool _replace, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.MoveFile)}[{Interlocked.Increment(ref pendingFiles)}] (\"{oldName}\", \"{newName}\", {_replace}, {info.Log()})"));
        }

        internal void SetupSetAllocationSize(string path, long length)
        {
            operations
                .Setup(d => d.SetAllocationSize(path, length, It.Is<DokanFileInfo>(i => !i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, long _length, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.SetAllocationSize)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {_length}, {info.Log()})"));
        }

        internal void SetupSetFileAttributes(string path, FileAttributes attributes)
        {
            operations
                .Setup(d => d.SetFileAttributes(path, attributes, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileAttributes _attributes, DokanFileInfo info) => Console.WriteLine($"{nameof(IDokanOperations.SetFileAttributes)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", [{_attributes}], {info.Log()})"));
        }

        internal void SetupSetFileTime(string path)
        {
            operations
                .Setup(d => d.SetFileTime(path, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.SetFileTime)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {creationTime}, {lastAccessTime}, {lastWriteTime}, {info.Log()})"));
        }

        internal void SetupGetFileSecurity(string path, FileSystemSecurity security, AccessControlSections access = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group)
        {
            operations
                .Setup(d => d.GetFileSecurity(path, out security, access, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileSystemSecurity _security, AccessControlSections _access, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.GetFileSecurity)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {_security.AsString()}, {_access}, {info.Log()})"));
        }

        internal void SetupSetFileSecurity(string path, FileSystemSecurity security)
        {
            operations
                //.Setup(d => d.SetFileSecurity(path, security, AccessControlSections.Access, It.IsAny<DokanFileInfo>()))
                .Setup(d => d.SetFileSecurity(path, It.IsAny<FileSystemSecurity>(), AccessControlSections.Access, It.IsAny<DokanFileInfo>()))
                .Returns(DokanResult.Success)
                .Callback((string fileName, FileSystemSecurity _security, AccessControlSections access, DokanFileInfo info)
                    => Console.WriteLine($"{nameof(IDokanOperations.SetFileSecurity)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {_security.AsString()}, {access}, {info.Log()})"));
        }

        internal void SetupOpenBlankDirectory()
        {
            operations
                .Setup(d => d.OpenDirectory(string.Empty, It.Is<DokanFileInfo>(i => i.IsDirectory)))
                .Returns(DokanResult.Success)
                .Callback((string fileName, DokanFileInfo info) =>
                {
                    Console.WriteLine("  *** WARNING: This is probably an error in the Dokan driver!");
                    Console.WriteLine($"  {nameof(IDokanOperations.OpenDirectory)}[{Interlocked.Read(ref pendingFiles)}] (\"{fileName}\", {info.Log()})");
                    Console.WriteLine("  ***");
                });
        }

        internal void VerifyAll()
        {
            for (int i = 1; Interlocked.Read(ref pendingFiles) > 0; ++i)
            {
                if (i > 5)
                    throw new TimeoutException("Cleanup wait cycles exceeded");

                Console.WriteLine($"Waiting for closure (#{i})");
                Thread.Sleep(5);
            }
            operations.VerifyAll();
        }
    }
}
