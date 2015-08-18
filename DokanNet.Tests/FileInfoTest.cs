using System;
using System.IO;
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
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetDirectory_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetExists_WhereFileExists_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            Assert.IsFalse(sut.Exists, "File should not exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetExtension_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetIsReadOnly_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void AppendText_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
            string value = $"TestValue for test {nameof(AppendText_CallsApiCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, AppendShare, FileMode.OpenOrCreate);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFile(destinationPath, CopyToAccess, WriteShare, FileMode.CreateNew, FileAttributes.Normal);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupGetFileInformation(destinationPath, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupSetFileAttributes(destinationPath, default(FileAttributes));
            fixture.SetupSetFileTime(destinationPath);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.CopyTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.CopyTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void CopyTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.CopyTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void CreateText_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Delete_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, DeleteAccess, ReadWriteShare, FileMode.Open, deleteOnClose: true);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupDeleteFile(path);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.Delete();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void MoveTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFile(destinationPath, MoveToAccess, MoveShare, FileMode.Open);
            fixture.SetupMoveFile(path, destinationPath, false);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.MoveTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.MoveTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName),
                destinationPath = DokanOperationsFixture.RootedPath(DokanOperationsFixture.DestinationFileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            sut.MoveTo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.DestinationFileName));
        }

        private void OpenFile_InSpecifiedMode(FileInfo info, FileMode mode, System.IO.FileAccess[] accessModes)
        {
            foreach (var access in accessModes) {
                using (var stream = info.Open(mode, access)) {
#if !LOGONLY
                    Assert.IsNotNull(stream, $"{nameof(info.Open)} {mode}/{access}");
#endif
                    if ((access & System.IO.FileAccess.Write) != 0) {
                        stream.Write(source, 0, source.Length);
#if !LOGONLY
                        Assert.AreEqual(source.Length, stream.Position, "Unexpected write count");
#endif
                    }

                    if ((access & System.IO.FileAccess.ReadWrite) != 0)
                        stream.Seek(0, SeekOrigin.Begin);

                    if ((access & System.IO.FileAccess.Read) != 0) {
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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate);
            fixture.SetupGetFileInformation(path, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Create);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.CreateNew);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpen_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, WriteShare, FileMode.Open);
            foreach (var access in new[] { WriteAccess, ReadWriteAccess })
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.Open);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Open_WhereFileModeIsOpenOrCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, WriteShare, FileMode.OpenOrCreate);
            foreach (var access in new[] { WriteAccess, ReadWriteAccess})
                fixture.SetupCreateFile(path, access, WriteShare, FileMode.OpenOrCreate);
            fixture.SetupReadFile(path, source, source.Length);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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

            string path = DokanOperationsFixture.RootedPath(DokanOperationsFixture.FileName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.Open);
            fixture.SetupSetAllocationSize(path, 0);
            fixture.SetupWriteFile(path, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.DriveBasedPath(DokanOperationsFixture.FileName));

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
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void OpenText_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void OpenWrite_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Replace_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void SetAccessControl_CallsApiCorrectly()
        {
            Assert.Inconclusive("Not yet implemented");
        }
    }
}
