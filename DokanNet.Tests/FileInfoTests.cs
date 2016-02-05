using System;
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

        private static byte[] smallData;

        private static byte[] largeData;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            smallData = DokanOperationsFixture.InitPeriodicTestData(4096);

            largeData = DokanOperationsFixture.InitPeriodicTestData(5 * FILE_BUFFER_SIZE + 65536);
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
            DokanOperationsFixture.InitInstance();
        }

        [TestCleanup]
        public void Cleanup()
        {
            bool hasUnmatchedInvocations = false;
            DokanOperationsFixture.ClearInstance(out hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAttributes_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Normal;
            var creationTime = new DateTime(2015, 6, 1, 12, 0, 0);
            var lastWriteTime = new DateTime(2015, 7, 31, 12, 0, 0);
            var lastAccessTime = new DateTime(2015, 8, 1, 6, 0, 0);
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, attributes, creationTime, lastWriteTime, lastAccessTime);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.Name, nameof(sut.Name));
            Assert.AreNotEqual(default(FileAttributes), sut.Attributes, nameof(sut.Attributes));
            Assert.AreNotEqual(DateTime.MinValue, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastAccessTime, nameof(sut.LastAccessTime));
#else
            Assert.AreEqual(DokanOperationsFixture.FileName, sut.Name, nameof(sut.Name));
            Assert.AreEqual(DokanOperationsFixture.FileName.AsDriveBasedPath(), sut.FullName, nameof(sut.FullName));
            Assert.AreEqual(attributes, sut.Attributes, nameof(sut.Attributes));
            Assert.AreEqual(creationTime, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreEqual(lastWriteTime, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreEqual(lastAccessTime, sut.LastAccessTime, nameof(sut.LastAccessTime));

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new FileInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Directory.FullName, "Unexpected parent directory");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExists_WhereFileExists_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            Assert.IsTrue(sut.Exists, "File should exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        public void GetExists_WhereFileDoesNotExist_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            Assert.IsFalse(sut.Exists, "File should not exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExtension_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            Assert.AreEqual(Path.GetExtension(path), sut.Extension, "Unexpected extension");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetIsReadOnly_WhereFileIsReadOnly_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.ReadOnly);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            Assert.IsTrue(sut.IsReadOnly, "File should be read/write");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetIsReadOnly_WhereFileIsReadWrite_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            Assert.IsFalse(sut.IsReadOnly, "File should be readonly");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void AppendText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(AppendText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.OpenOrCreate, FileOptions.SequentialScan);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var writer = sut.AppendText())
            {
                writer.Write(value);
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupFindStreams(path, new FileInformation[0]);
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
#if !NETWORK_DRIVE
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
#endif
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.SetupSetFileTime(destinationPath);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "NonEmpty")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsNonEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
            string value = $"TestValue for test {nameof(CopyTo_WhereSourceIsNonEmpty_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, length: value.Length);
            fixture.SetupFindStreams(path, new FileInformation[0]);
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupSetEndOfFile(destinationPath, value.Length);
#if NETWORK_DRIVE
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length, synchronousIo: false);
            fixture.SetupWriteFile(destinationPath, Encoding.UTF8.GetBytes(value), value.Length, synchronousIo: false);
#else
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.SetupWriteFile(destinationPath, Encoding.UTF8.GetBytes(value), value.Length);
#endif
            fixture.SetupSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.SetupSetFileTime(destinationPath);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CopyTo_WhereSourceIsLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, length: largeData.Length);
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileOptions.SequentialScan, attributes: FileAttributes.Normal);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupFindStreams(path, new FileInformation[0]);
            fixture.SetupSetEndOfFile(destinationPath, largeData.Length);
#if NETWORK_DRIVE
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, synchronousIo: false);
            fixture.SetupWriteFileInChunks(destinationPath, largeData, FILE_BUFFER_SIZE, synchronousIo: false);
#else
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
            fixture.SetupWriteFileInChunks(destinationPath, largeData, FILE_BUFFER_SIZE);
#endif
            fixture.SetupSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.SetupSetFileTime(destinationPath);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void CopyTo_WhereSourceDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void CopyTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupFindStreams(path, new FileInformation[0]);
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(Create_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadWriteAccess, WriteShare, FileMode.Create, FileOptions.None);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.Create())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CreateText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(CreateText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.Create, FileOptions.SequentialScan);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var writer = sut.CreateText())
            {
                writer.Write(value);
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Delete_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, DeleteAccess, ReadWriteShare, FileMode.Open, deleteOnClose: true);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupDeleteFile(path);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.Delete();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        public void Delete_WhereFileDoesNotExists_IgnoresResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.Delete();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupGetFileSecurity(path, DokanOperationsFixture.DefaultFileSecurity);
            fixture.SetupCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.SetupGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());
            var security = sut.GetAccessControl();

#if !LOGONLY
            Assert.IsNotNull(security, "Security descriptor should be set");
            Assert.AreEqual(DokanOperationsFixture.DefaultFileSecurity.AsString(), security.AsString(), "Security descriptors should match");
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenDirectoryWithoutCleanup(string.Empty, WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.SetupMoveFile(path, destinationPath, false);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string origin = Path.Combine(DokanOperationsFixture.DirectoryName, DokanOperationsFixture.FileName),
                destination = Path.Combine(DokanOperationsFixture.DestinationDirectoryName, DokanOperationsFixture.DestinationFileName),
                path = origin.AsRootedPath(),
                destinationPath = destination.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupOpenDirectoryWithoutCleanup(DokanOperationsFixture.DestinationDirectoryName.AsRootedPath(), WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.SetupMoveFile(path, destinationPath, false);
#endif

            var sut = new FileInfo(origin.AsDriveBasedPath());

            sut.MoveTo(destination.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void MoveTo_WhereSourceDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupMoveFileWithError(path, destinationPath, false, DokanResult.FileExists);
            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenDirectoryWithoutCleanup(string.Empty, WriteDirectoryAccess, FileShare.ReadWrite);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
                        int readBytes = stream.Read(target, 0, target.Length);
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

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Append, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Create, FileOptions.None);
            fixture.SetupReadFile(path, smallData, smallData.Length);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Create, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreateNew_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.CreateNew, FileOptions.None);
            fixture.SetupReadFile(path, smallData, smallData.Length);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void Open_WhereFileModeIsCreateNew_AndFileExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpen_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { ReadAccess, WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFile(path, smallData, smallData.Length);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void Open_WhereFileModeIsOpen_AndFileDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpenOrCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupReadFile(path, smallData, smallData.Length);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.OpenOrCreate, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsTruncate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupSetAllocationSize(path, 0);
            fixture.SetupWriteFile(path, smallData, smallData.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Truncate, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                int readBytes = stream.Read(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Timing)]
        public void OpenRead_WithDelay_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_WithDelay_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFileWithDelay(path, Encoding.UTF8.GetBytes(value), value.Length, DokanOperationsFixture.IODelay);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                int readBytes = stream.Read(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;
                do
                {
                    int readBytes = stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                    Assert.AreEqual(Math.Min(FILE_BUFFER_SIZE, target.Length - totalReadBytes), readBytes, $"Unexpected empty read at origin {totalReadBytes}");
                    totalReadBytes += readBytes;
                } while (totalReadBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFileUsingContext_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: largeData);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;

                do
                {
                    int readBytes = stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                    Assert.AreEqual(Math.Min(FILE_BUFFER_SIZE, target.Length - totalReadBytes), readBytes, $"Unexpected empty read at origin {totalReadBytes}");
                    totalReadBytes += readBytes;
                } while (totalReadBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected read count");
                CollectionAssert.AreEqual(largeData, target, "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;

                Parallel.For(0, largeData.Length / FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        int readBytes = stream.Read(target, origin, count);
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
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFileUsingContext_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: largeData);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;

                Parallel.For(0, largeData.Length / FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        int readBytes = stream.Read(target, origin, count);
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
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLockingAndUnlocking_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_WithLockingAndUnlocking_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.SetupLockUnlockFile(path, 0, value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                Assert.IsTrue(stream.CanRead, "Stream should be readable");
                var target = new byte[value.Length];
                stream.Lock(0, target.Length);
                int readBytes = stream.Read(target, 0, target.Length);
                stream.Unlock(0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, Encoding.UTF8.GetString(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.SequentialScan);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var reader = sut.OpenText())
            {
                var target = new char[value.Length];
                int readBytes = reader.ReadBlock(target, 0, target.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, readBytes, "Unexpected read count");
                Assert.AreEqual(value, new string(target), "Unexpected result content");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Timing)]
        public void OpenWrite_WithDelay_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithDelay_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFileWithDelay(path, Encoding.UTF8.GetBytes(value), value.Length, DokanOperationsFixture.IODelay);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);

#if !LOGONLY
                Assert.AreEqual(value.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                int totalWrittenBytes = 0;

                do
                {
                    int writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFileUsingContext_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: largeData);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                int totalWrittenBytes = 0;

                do
                {
                    int writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);

#if !LOGONLY
                Assert.AreEqual(largeData.Length, stream.Position, "Unexpected write count");
#endif
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                int totalWrittenBytes = 0;

                Parallel.For(0, largeData.Length / FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
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
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFileUsingContext_InRandomOrder_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: largeData);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: largeData);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                int totalWrittenBytes = 0;

                Parallel.For(0, largeData.Length / FILE_BUFFER_SIZE + 1, i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
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
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithFlush_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithFlush_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.SetupFlushFileBuffers(path);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

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
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLockingAndUnlocking_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_WithLockingAndUnlocking_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
            fixture.SetupLockUnlockFile(path, 0, value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

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
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void Replace_WhereParentIsRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath(),
                destinationBackupPath = DokanOperationsFixture.DestinationBackupFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(destinationPath, ReplaceAccess | FileAccess.Reserved, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(destinationPath, ReplaceAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(path, SetOwnershipAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupSetFileAttributes(path, FileAttributes.Normal);
            fixture.SetupSetFileTime(path);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupFindStreams(destinationPath, new FileInformation[0]);
            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenDirectoryWithoutCleanup(string.Empty, WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.SetupMoveFile(destinationPath, destinationBackupPath, true);
            fixture.SetupMoveFile(path, destinationPath, true);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.Replace(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath(), DokanOperationsFixture.DestinationBackupFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void Replace_WhereParentIsDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string origin = Path.Combine(DokanOperationsFixture.DirectoryName, DokanOperationsFixture.FileName),
                destination = Path.Combine(DokanOperationsFixture.DestinationDirectoryName, DokanOperationsFixture.DestinationFileName),
                destinationBackup = Path.Combine(DokanOperationsFixture.DestinationDirectoryName, DokanOperationsFixture.DestinationBackupFileName),
                path = origin.AsRootedPath(),
                destinationPath = destination.AsRootedPath(),
                destinationBackupPath = destinationBackup.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(destinationPath, ReplaceAccess | FileAccess.Reserved, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(destinationPath, ReplaceAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(path, SetOwnershipAccess, WriteShare, FileMode.Open, FileOptions.None);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupSetFileAttributes(path, FileAttributes.Normal);
            fixture.SetupSetFileTime(path);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupFindStreams(destinationPath, new FileInformation[0]);
            fixture.SetupOpenDirectoryWithoutCleanup(DokanOperationsFixture.DestinationDirectoryName.AsRootedPath(), WriteDirectoryAccess, FileShare.ReadWrite);
            fixture.SetupMoveFile(destinationPath, destinationBackupPath, true);
            fixture.SetupMoveFile(path, destinationPath, true);
#endif

            var sut = new FileInfo(origin.AsDriveBasedPath());

            sut.Replace(destination.AsDriveBasedPath(), destinationBackup.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void SetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName;

            var security = new FileSecurity();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ChangePermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Normal);
            fixture.SetupGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultFileSecurity);
            fixture.SetupSetFileSecurity(path.AsRootedPath(), security);
            fixture.SetupCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.SetupGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity, AccessControlSections.Access);
#endif

            var sut = new FileInfo(path.AsDriveBasedPath());
            sut.SetAccessControl(security);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }
    }
}