using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class FileInfoTests
    {
        private const int FILE_BUFFER_SIZE = 262144;

        private const int SMALL_DATA_SIZE = 4096;

        private const int LARGE_DATA_SIZE = 5*FILE_BUFFER_SIZE + FILE_BUFFER_SIZE/4;

        private static byte[] smallData;

        private static byte[] largeData;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            smallData = DokanOperationsFixture.InitPeriodicTestData(SMALL_DATA_SIZE);

            largeData = DokanOperationsFixture.InitPeriodicTestData(LARGE_DATA_SIZE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            largeData = null;
            smallData = null;
        }

        [TestInitialize]
        public void Initialize()
        {
            DokanOperationsFixture.InitInstance(TestContext.TestName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            var hasUnmatchedInvocations = false;
            DokanOperationsFixture.ClearInstance(out hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAttributes_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Normal;
            var creationTime = new DateTime(2015, 6, 1, 12, 0, 0);
            var lastWriteTime = new DateTime(2015, 7, 31, 12, 0, 0);
            var lastAccessTime = new DateTime(2015, 8, 1, 6, 0, 0);
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, attributes, creationTime: creationTime, lastWriteTime: lastWriteTime, lastAccessTime: lastAccessTime);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.Name, nameof(sut.Name));
            Assert.AreNotEqual(default(FileAttributes), sut.Attributes, nameof(sut.Attributes));
            Assert.AreNotEqual(DateTime.MinValue, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastAccessTime, nameof(sut.LastAccessTime));
#else
            Assert.AreEqual(fixture.FileName, sut.Name, nameof(sut.Name));
            Assert.AreEqual(fixture.FileName.AsDriveBasedPath(), sut.FullName, nameof(sut.FullName));
            Assert.AreEqual(attributes, sut.Attributes, nameof(sut.Attributes));
            Assert.AreEqual(creationTime, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreEqual(lastWriteTime, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreEqual(lastAccessTime, sut.LastAccessTime, nameof(sut.LastAccessTime));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new FileInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Directory.FullName,
                "Unexpected parent directory");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExists_WhereFileExists_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            Assert.IsTrue(sut.Exists, "File should exist");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        public void GetExists_WhereFileDoesNotExist_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            Assert.IsFalse(sut.Exists, "File should not exist");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExtension_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            Assert.AreEqual(Path.GetExtension(path), sut.Extension, "Unexpected extension");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetIsReadOnly_WhereFileIsReadOnly_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.ReadOnly);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            Assert.IsTrue(sut.IsReadOnly, "File should be read/write");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetIsReadOnly_WhereFileIsReadWrite_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            Assert.IsFalse(sut.IsReadOnly, "File should be readonly");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void AppendText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(AppendText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.OpenOrCreate, FileOptions.SequentialScan);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var writer = sut.AppendText())
            {
                writer.Write(value);
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectFindStreams(path, new FileInformation[0]);
            fixture.ExpectCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.PermitCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.ExpectGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.ExpectSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.ExpectSetFileTime(destinationPath);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.CopyTo(fixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NonEmpty")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsNonEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
            string value = $"TestValue for test {nameof(CopyTo_WhereSourceIsNonEmpty_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal, length: value.Length);
            fixture.ExpectFindStreams(path, new FileInformation[0]);
            fixture.ExpectCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.ExpectGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.ExpectSetEndOfFile(destinationPath, value.Length);
#if NETWORK_DRIVE
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length, synchronousIo: false);
            fixture.ExpectWriteFile(destinationPath, Encoding.UTF8.GetBytes(value), value.Length, synchronousIo: false);
#else
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.ExpectWriteFile(destinationPath, Encoding.UTF8.GetBytes(value), value.Length);
#endif
            fixture.ExpectSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.ExpectSetFileTime(destinationPath);

            fixture.PermitCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.PermitProbeFile(destinationPath, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.CopyTo(fixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal, length: largeData.Length);
            fixture.ExpectCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.ExpectGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.ExpectFindStreams(path, new FileInformation[0]);
            fixture.ExpectSetEndOfFile(destinationPath, largeData.Length);
#if NETWORK_DRIVE
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, synchronousIo: false);
            fixture.ExpectWriteFileInChunks(destinationPath, largeData, FILE_BUFFER_SIZE, synchronousIo: false);
#else
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
            fixture.ExpectWriteFileInChunks(destinationPath, largeData, FILE_BUFFER_SIZE);
#endif
            fixture.ExpectSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.ExpectSetFileTime(destinationPath);

            fixture.PermitProbeFile(path, largeData);
            fixture.PermitProbeFile(destinationPath, largeData);
            fixture.PermitCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.CopyTo(fixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void CopyTo_WhereSourceDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.CopyTo(fixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void CopyTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectFindStreams(path, new FileInformation[0]);
            fixture.ExpectCreateFileToFail(destinationPath, DokanResult.FileExists, true);
            fixture.ExpectCloseFile(path);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.CopyTo(fixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(Create_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadWriteAccess, WriteShare, FileMode.Create, FileOptions.None);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.Create())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CreateText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(CreateText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.Create, FileOptions.SequentialScan);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var writer = sut.CreateText())
            {
                writer.Write(value);
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Delete_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, DeleteAccess, ReadWriteShare, FileMode.Open, deleteOnClose: true);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectDeleteFile(path);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.Delete();

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        public void Delete_WhereFileDoesNotExists_IgnoresResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.Delete();

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectGetFileSecurity(path, DokanOperationsFixture.DefaultFileSecurity);
            fixture.ExpectCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());
            var security = sut.GetAccessControl();

#if !LOGONLY
            Assert.IsNotNull(security, "Security descriptor should be set");
            Assert.AreEqual(DokanOperationsFixture.DefaultFileSecurity.AsString(), security.AsString(),
                "Security descriptors should match");
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectCreateFile(destinationPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectOpenDirectoryWithoutCleanup(DokanOperationsFixture.RootName, WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.PermitGetFileInformationToFail(destinationPath, DokanResult.FileNotFound, true);
            fixture.PermitOpenDirectory(DokanOperationsFixture.RootName, attributes: FileAttributes.Normal);
            fixture.ExpectMoveFile(path, destinationPath, false);
            fixture.PermitGetFileInformation(destinationPath, FileAttributes.Normal, false);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string origin = Path.Combine(fixture.DirectoryName, fixture.FileName),
                destination = Path.Combine(fixture.DestinationDirectoryName, fixture.DestinationFileName),
                path = origin.AsRootedPath(),
                destinationPath = destination.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectOpenDirectoryWithoutCleanup(fixture.DestinationDirectoryName.AsRootedPath(), WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectCreateFile(destinationPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.PermitGetFileInformationToFail(destinationPath, DokanResult.FileNotFound);
            fixture.PermitOpenDirectory(fixture.DestinationDirectoryName.AsRootedPath(), attributes: FileAttributes.Normal);
            fixture.ExpectMoveFile(path, destinationPath, false);
            fixture.PermitGetFileInformation(destinationPath, FileAttributes.Normal, false);
#endif

            var sut = new FileInfo(origin.AsDriveBasedPath());

            sut.MoveTo(destination.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void MoveTo_WhereSourceDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectCreateFile(destinationPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectOpenDirectoryWithoutCleanup(fixture.DestinationDirectoryName.AsRootedPath(), WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectGetFileInformationToFail(destinationPath, DokanResult.FileNotFound);
            fixture.ExpectOpenDirectory(DokanOperationsFixture.RootName, attributes: FileAttributes.Normal);
            fixture.ExpectMoveFileToFail(path, destinationPath, false, DokanResult.FileExists);
            fixture.ExpectOpenDirectoryWithoutCleanup(DokanOperationsFixture.RootName, WriteDirectoryAccess, FileShare.ReadWrite);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationFileName.AsDriveBasedPath());
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void OpenFile_InSpecifiedMode(FileInfo info, FileMode mode, System.IO.FileAccess[] accessModes)
        {
            foreach (var access in accessModes)
            {
                Console.WriteLine($"{nameof(info.Open)} {mode}/{access}");
                using (var stream = info.Open(mode, access))
                {
#if !LOGONLY
                    Assert.IsNotNull(stream, $"{nameof(info.Open)} {mode}/{access}");
#endif
                    if (access.HasFlag(System.IO.FileAccess.Write))
                    {
                        Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                        stream.Write(smallData, 0, smallData.Length);
#if !LOGONLY
                        Assert.AreEqual(smallData.Length, stream.Position, "Unexpected write count");
#endif
                    }

                    if (access.HasFlag(System.IO.FileAccess.ReadWrite))
                        stream.Seek(0, SeekOrigin.Begin);

                    if (access.HasFlag(System.IO.FileAccess.Read))
                    {
                        Assert.IsTrue(stream.CanRead, "Stream should be readable");
                        var target = new byte[4096];
                        var readBytes = stream.Read(target, 0, target.Length);
#if !LOGONLY
                        Assert.AreEqual(target.Length, readBytes, "Unexpected read count");
                        CollectionAssert.AreEquivalent(smallData, target, "Unexpected result content");
#endif
                    }
                }
            }
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsAppend_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);

            fixture.PermitProbeFile(path, smallData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new {Mode = FileMode.Append, AccessModes = new[] {System.IO.FileAccess.Write}};
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] {WriteAccess, ReadWriteAccess})
                fixture.ExpectCreateFile(path, access, WriteShare, FileMode.Create, FileOptions.None);
            fixture.ExpectReadFile(path, smallData, smallData.Length);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Create, AccessModes = new[] {System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreateNew_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] {WriteAccess, ReadWriteAccess})
                fixture.ExpectCreateFile(path, access, WriteShare, FileMode.CreateNew, FileOptions.None);
            fixture.ExpectReadFile(path, smallData, smallData.Length);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] {System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void Open_WhereFileModeIsCreateNew_AndFileExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileExists);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] {System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpen_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] {ReadAccess, WriteAccess, ReadWriteAccess})
                fixture.ExpectCreateFile(path, access, WriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFile(path, smallData, smallData.Length);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] {System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void Open_WhereFileModeIsOpen_AndFileDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] {System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpenOrCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            foreach (var access in new[] {WriteAccess, ReadWriteAccess})
                fixture.ExpectCreateFile(path, access, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);

            fixture.PermitProbeFile(path, smallData, smallData.Length);
            fixture.PermitProbeFile(path, smallData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.OpenOrCreate, AccessModes = new[] {System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite} };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsTruncate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectSetAllocationSize(path, 0);
            fixture.ExpectWriteFile(path, smallData, smallData.Length);

            fixture.PermitProbeFile(path, smallData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Truncate, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                var readBytes = stream.Read(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Timing)]
        public void OpenRead_WithDelay_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_WithDelay_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFileWithDelay(path, Encoding.UTF8.GetBytes(value), value.Length, DokanOperationsFixture.IODelay);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                var readBytes = stream.Read(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;
                do
                {
                    var readBytes = stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                    Assert.AreEqual(Math.Min(FILE_BUFFER_SIZE, target.Length - totalReadBytes), readBytes, $"Unexpected empty read at origin {totalReadBytes}");
                    totalReadBytes += readBytes;
                } while (totalReadBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFileUsingContext_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: largeData);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;

                do
                {
                    var readBytes = stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                    Assert.AreEqual(Math.Min(FILE_BUFFER_SIZE, target.Length - totalReadBytes), readBytes, $"Unexpected empty read at origin {totalReadBytes}");
                    totalReadBytes += readBytes;
                } while (totalReadBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;

                Parallel.For(0, largeData.Length/FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        var readBytes = stream.Read(target, origin, count);
                        Assert.AreEqual(count, readBytes, $"Unexpected empty read at origin {origin}");
                        totalReadBytes += readBytes;
                    }
                });

#if !LOGONLY
                Assert.AreEqual(largeData.Length, totalReadBytes, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFileUsingContext_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: largeData);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;

                Parallel.For(0, largeData.Length/FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        var readBytes = stream.Read(target, origin, count);
                        Assert.AreEqual(count, readBytes, $"Unexpected empty read at origin {origin}");
                        totalReadBytes += readBytes;
                    }
                });

#if !LOGONLY
                Assert.AreEqual(largeData.Length, totalReadBytes, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

#if USER_MODE_LOCK
        [TestMethod, TestCategory(TestCategories.Success)]
#endif
        public void OpenRead_WithLockingAndUnlocking_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_WithLockingAndUnlocking_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.ExpectLockUnlockFile(path, 0, value.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                stream.Lock(0, target.Length);
                var readBytes = stream.Read(target, 0, target.Length);
                stream.Unlock(0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var reader = sut.OpenText())
            {
                var target = new char[value.Length];
                var readBytes = reader.ReadBlock(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, new string(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Timing)]
        public void OpenWrite_WithDelay_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithDelay_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFileWithDelay(path, Encoding.UTF8.GetBytes(value), value.Length, DokanOperationsFixture.IODelay);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                var totalWrittenBytes = 0;

                do
                {
                    var writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFileUsingContext_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: largeData);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                var totalWrittenBytes = 0;

                do
                {
                    var writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                var totalWrittenBytes = 0;

                Parallel.For(0, largeData.Length/FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, largeData.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        stream.Write(largeData, origin, count);
                        totalWrittenBytes += count;
                    }
                });

#if !LOGONLY
                Assert.AreEqual(largeData.Length, totalWrittenBytes, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFileUsingContext_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: largeData);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                var totalWrittenBytes = 0;

                Parallel.For(0, largeData.Length/FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, largeData.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        stream.Write(largeData, origin, count);
                        totalWrittenBytes += count;
                    }
                });

#if !LOGONLY
                Assert.AreEqual(largeData.Length, totalWrittenBytes, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithFlush_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithFlush_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.ExpectFlushFileBuffers(path);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
                stream.Flush(true);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

#if USER_MODE_LOCK
        [TestMethod, TestCategory(TestCategories.Success)]
#endif
        public void OpenWrite_WithLockingAndUnlocking_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithLockingAndUnlocking_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.ExpectLockUnlockFile(path, 0, value.Length);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Lock(0, value.Length);
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
                stream.Unlock(0, value.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void Replace_WhereParentIsRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath(),
                destinationPath = fixture.DestinationFileName.AsRootedPath(),
                destinationBackupPath = fixture.DestinationBackupFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(destinationPath, ReplaceAccess | FileAccess.Reserved, ReadWriteShare, FileMode.Open);
            fixture.ExpectCreateFileWithoutCleanup(destinationPath, ReplaceAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectCreateFileWithoutCleanup(path, SetOwnershipAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectSetFileAttributes(path, FileAttributes.Normal);
            fixture.ExpectSetFileTime(path);
            fixture.ExpectGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.ExpectFindStreams(destinationPath, new FileInformation[0]);
            fixture.PermitGetFileInformationToFail(destinationBackupPath, NtStatus.ObjectPathNotFound, true);
            fixture.PermitOpenDirectory(DokanOperationsFixture.RootName, ReadDirectoryAccess, ReadWriteShare, attributes: FileAttributes.Normal);
            fixture.ExpectCreateFile(destinationBackupPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectOpenDirectoryWithoutCleanup(DokanOperationsFixture.RootName, WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectMoveFile(destinationPath, destinationBackupPath, true);
            fixture.PermitGetFileInformation(destinationBackupPath, FileAttributes.Normal, false);
            fixture.ExpectCreateFile(destinationPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectMoveFile(path, destinationPath, true);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            sut.Replace(fixture.DestinationFileName.AsDriveBasedPath(), fixture.DestinationBackupFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void Replace_WhereParentIsDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string origin = Path.Combine(fixture.DirectoryName, fixture.FileName),
                destination = Path.Combine(fixture.DestinationDirectoryName, fixture.DestinationFileName),
                destinationBackup = Path.Combine(fixture.DestinationDirectoryName, fixture.DestinationBackupFileName),
                path = origin.AsRootedPath(),
                destinationPath = destination.AsRootedPath(),
                destinationBackupPath = destinationBackup.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(destinationPath, ReplaceAccess | FileAccess.Reserved, ReadWriteShare, FileMode.Open);
            fixture.ExpectCreateFileWithoutCleanup(destinationPath, ReplaceAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectCreateFileWithoutCleanup(path, SetOwnershipAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.ExpectGetFileInformation(path, FileAttributes.Normal);
            fixture.ExpectSetFileAttributes(path, FileAttributes.Normal);
            fixture.ExpectSetFileTime(path);
            fixture.ExpectGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.ExpectFindStreams(destinationPath, new FileInformation[0]);
            fixture.PermitGetFileInformationToFail(destinationBackupPath, NtStatus.ObjectPathNotFound, true);
            fixture.PermitOpenDirectory(fixture.DestinationDirectoryName.AsRootedPath(), ReadDirectoryAccess, ReadWriteShare, attributes: FileAttributes.Normal);
            fixture.ExpectCreateFile(destinationBackupPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectOpenDirectoryWithoutCleanup(fixture.DestinationDirectoryName.AsRootedPath(), WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectMoveFile(destinationPath, destinationBackupPath, true);
            fixture.PermitGetFileInformation(destinationBackupPath, FileAttributes.Normal, false);
            fixture.ExpectCreateFile(destinationPath, WriteDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectMoveFile(path, destinationPath, true);
#endif

            var sut = new FileInfo(origin.AsDriveBasedPath());

            sut.Replace(destination.AsDriveBasedPath(), destinationBackup.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void SetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName;

            var security = new FileSecurity();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ChangePermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Normal);
            fixture.ExpectGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultFileSecurity);
            fixture.ExpectSetFileSecurity(path.AsRootedPath(), security);
            fixture.ExpectCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity, AccessControlSections.Access);
#endif

            var sut = new FileInfo(path.AsDriveBasedPath());
            sut.SetAccessControl(security);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetFileInformation_WhereDatesIsUnknown_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Normal;
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, attributes, creationTime: DateTime.MinValue, lastWriteTime: DateTime.MinValue, lastAccessTime: DateTime.MinValue);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if !LOGONLY
            //As DirectoryInfo do not have any handling om uninitialized date
            //do we have to use this date insted of DateTime.MinValue
            var defaultDate = DateTime.FromFileTime(0);
            Assert.AreEqual(defaultDate, sut.CreationTime, $"File should have unknown {nameof(sut.CreationTime)}.");
            Assert.AreEqual(defaultDate, sut.LastWriteTime, $"File should have unknown {nameof(sut.LastWriteTime)}.");
            Assert.AreEqual(defaultDate, sut.LastAccessTime, $"File should have unknown {nameof(sut.LastAccessTime)}.");
            Assert.AreEqual(attributes, sut.Attributes, $"{nameof(sut.Attributes)} is not initialized.");

            fixture.Verify();
#endif
        }
    }
}