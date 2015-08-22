using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class FileInfoTest
    {
        private byte[] source;

        [TestInitialize]
        public void Initialize()
        {
            DokanOperationsFixture.InitInstance();

            source = new byte[4096];
            for (int i = 0; i < source.Length; ++i)
                source[i] = (byte)(i % 256);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DokanOperationsFixture.ClearInstance();
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void AppendText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(AppendText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.OpenOrCreate);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var writer = sut.AppendText()) {
                writer.Write(value);
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void CopyTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, attributes: FileAttributes.Normal);
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

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void CopyTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.CopyTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(Create_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadWriteAccess, WriteShare, FileMode.Create);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.Create()) {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void CreateText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(CreateText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, ReadOnlyShare, FileMode.Create);
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ReadAttributesPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Normal);
            fixture.SetupGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultFileSecurity);
            fixture.SetupCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.SetupGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity);
#endif

            var sut = new FileInfo(path.AsDriveBasedPath());
            var security = sut.GetAccessControl();

#if !LOGONLY
            Assert.IsNotNull(security, "Security descriptor should be set");
            Assert.AreEqual(DokanOperationsFixture.DefaultFileSecurity.AsString(), security.AsString(), "Security descriptors should match");
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void MoveTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupMoveFile(path, destinationPath, false);
            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupCleanupFile(destinationPath, true);
            fixture.SetupCleanupFile(destinationPath);

            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenBlankDirectory();
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath());
        }

        private void OpenFile_InSpecifiedMode(FileInfo info, FileMode mode, System.IO.FileAccess[] accessModes)
        {
            foreach (var access in accessModes) {
                using (var stream = info.Open(mode, access)) {
#if !LOGONLY
                    Assert.IsNotNull(stream, $"{nameof(info.Open)} {mode}/{access}");
#endif
                    if (access.HasFlag(System.IO.FileAccess.Write)) {
                        Assert.IsTrue(stream.CanWrite, "Stream should be writable");
                        stream.Write(source, 0, source.Length);
#if !LOGONLY
                        Assert.AreEqual(source.Length, stream.Position, "Unexpected write count");
#endif
                    }

                    if (access.HasFlag(System.IO.FileAccess.ReadWrite))
                        stream.Seek(0, SeekOrigin.Begin);

                    if (access.HasFlag(System.IO.FileAccess.Read)) {
                        Assert.IsTrue(stream.CanRead, "Stream should be readable");
                        var target = new byte[4096];
                        int readBytes = stream.Read(target, 0, target.Length);
#if !LOGONLY
                        Assert.AreEqual(target.Length, readBytes, "Unexpected read count");
                        CollectionAssert.AreEquivalent(source, target, "Unexpected result content");
#endif
                    }
                }
                Console.WriteLine($"{nameof(info.Open)} {mode}/{access}");
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsAppend_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Append, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Create);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Create, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsCreateNew_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.CreateNew);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpen_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { ReadAccess, WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Open);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpenOrCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, WriteShare, FileMode.OpenOrCreate);
            foreach (var access in new[] { WriteAccess, ReadWriteAccess})
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.OpenOrCreate);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.OpenOrCreate, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsTruncate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.Open);
            fixture.SetupSetAllocationSize(path, 0);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            var parameters = new { Mode = FileMode.Truncate, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void OpenRead_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead()) {
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void OpenText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var reader = sut.OpenText()) {
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void OpenWrite_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite()) {
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

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Replace_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationFileName.AsRootedPath(),
                destinationBackupPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationBackupFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(destinationPath, (FileAccess)18022539, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(destinationPath, ReplaceAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupCreateFileWithoutCleanup(path, SetOwnershipAccess, WriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal);
            fixture.SetupSetFileAttributes(path, FileAttributes.Normal);
            fixture.SetupSetFileTime(path);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupMoveFile(destinationPath, destinationBackupPath, true);
            fixture.SetupCleanupFile(destinationBackupPath);
            fixture.SetupCleanupFile(destinationBackupPath, /* ERROR: This should be false! */true);
            fixture.SetupMoveFile(path, destinationPath, true);
            fixture.SetupCleanupFile(destinationPath);
            fixture.SetupCleanupFile(destinationPath, /* ERROR: This should be false! */true);

            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenBlankDirectory();
#endif

            var sut = new FileInfo(DokanOperationsFixture.FileName.AsDriveBasedPath());

            sut.Replace(DokanOperationsFixture.DestinationFileName.AsDriveBasedPath(), DokanOperationsFixture.DestinationBackupFileName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
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
