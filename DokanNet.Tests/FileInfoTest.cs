using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    [TestClass]
    public partial class FileInfoTest
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

        private void OpenFile_InSpecifiedMode(FileInfo info, FileMode mode, System.IO.FileAccess[] accessModes)
        {
            foreach (var access in accessModes) {
                using (var stream = info.Open(mode, access)) {
#if !LOGONLY
                    Assert.IsNotNull(stream, $"{nameof(info.Open)} {mode}/{access}");
#endif
                    if ((access & System.IO.FileAccess.Write) != 0)
                        stream.Write(source, 0, source.Length);

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
        public void Open_WhereFileModeIsAppend_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.WRITE_FILESHARE, FileMode.OpenOrCreate);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.Append, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Open_WhereFileModeIsCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.READ_FILEACCESS | DokanOperationsFixture.WRITE_FILEACCESS })
                fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, access, DokanOperationsFixture.WRITE_FILESHARE, FileMode.Create);
            fixture.SetupReadFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.Create, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Open_WhereFileModeIsCreateNew_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var access in new[] { DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.READ_FILEACCESS | DokanOperationsFixture.WRITE_FILEACCESS })
                fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, access, DokanOperationsFixture.WRITE_FILESHARE, FileMode.CreateNew);
            fixture.SetupReadFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void Open_WhereFileModeIsCreateNew_AndFileExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.CreateNew, AccessModes = new[] { System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod]
        public void Open_WhereFileModeIsOpen_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.READ_FILEACCESS, DokanOperationsFixture.WRITE_FILESHARE, FileMode.Open);
            foreach (var access in new[] { DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.READ_FILEACCESS | DokanOperationsFixture.WRITE_FILEACCESS })
                fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, access, DokanOperationsFixture.WRITE_FILESHARE, FileMode.Open);
            fixture.SetupReadFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void Open_WhereFileModeIsOpen_AndFileDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.Open, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);
        }

        [TestMethod]
        public void Open_WhereFileModeIsOpenOrCreate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.READ_FILEACCESS, DokanOperationsFixture.WRITE_FILESHARE, FileMode.OpenOrCreate);
            foreach (var access in new[] { DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.READ_FILEACCESS | DokanOperationsFixture.WRITE_FILEACCESS })
                fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, access, DokanOperationsFixture.WRITE_FILESHARE, FileMode.OpenOrCreate);
            fixture.SetupReadFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.OpenOrCreate, AccessModes = new[] { System.IO.FileAccess.Read, System.IO.FileAccess.Write, System.IO.FileAccess.ReadWrite } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Open_WhereFileModeIsTruncate_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.WRITE_FILEACCESS, DokanOperationsFixture.WRITE_FILESHARE, FileMode.Open);
            fixture.SetupSetAllocationSize(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, 0);
            fixture.SetupWriteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, source, source.Length);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            var parameters = new { Mode = FileMode.Truncate, AccessModes = new[] { System.IO.FileAccess.Write } };
            OpenFile_InSpecifiedMode(sut, parameters.Mode, parameters.AccessModes);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Delete_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.DELETE_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open, true);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupDeleteFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.Delete();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Delete_WhereFileDoesNotExists_IgnoresResult()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.Delete();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Exists_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            Assert.IsTrue(sut.Exists, "File should exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Exists_WhereFileDoesNotExist_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            Assert.IsFalse(sut.Exists, "File should not exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void CopyTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupGetVolumeInformationByCreateFile(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.READ_FILEACCESS, DokanOperationsFixture.READ_FILESHARE, FileMode.Open);
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.READ_FILEACCESS, FileShare.Read, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.CopyTo(@DokanOperationsFixture.ROOT + DokanOperationsFixture.DestinationFileName);

#if LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void MoveTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.MOVEFROM_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.DestinationFileName, DokanOperationsFixture.MOVETO_FILEACCESS, DokanOperationsFixture.MOVE_FILESHARE, FileMode.Open);
            fixture.SetupMoveFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.ROOT + DokanOperationsFixture.DestinationFileName, false);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.MoveTo(DokanOperationsFixture.DestinationFilePath);

#if LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException), "Expected FileNotFoundException not thrown")]
        public void MoveTo_WhereSourceDoesNotExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanResult.FileNotFound);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.MoveTo(DokanOperationsFixture.DestinationFilePath);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, DokanOperationsFixture.MOVEFROM_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.FileName, FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupCreateFileWithError(DokanOperationsFixture.ROOT + DokanOperationsFixture.DestinationFileName, DokanResult.FileExists);
#endif

            var sut = new FileInfo(DokanOperationsFixture.FilePath);

            sut.MoveTo(DokanOperationsFixture.DestinationFilePath);
        }
    }
}
