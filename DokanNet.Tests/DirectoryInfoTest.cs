using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    [TestClass]
    public partial class DirectoryInfoTest
    {
        [TestInitialize]
        public void Initialize()
        {
            DokanOperationsFixture.InitInstance();
        }

        [TestCleanup]
        public void Cleanup()
        {
            DokanOperationsFixture.ClearInstance();
        }

        [TestMethod]
        public void ReadAttributes_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Directory;
            var creationTime = new DateTime(2015, 1, 1, 12, 0, 0);
            var lastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0);
            var lastAccessTime = new DateTime(2015, 4, 1, 6, 0, 0);
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT, attributes, creationTime, lastWriteTime, lastAccessTime);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.Name, nameof(sut.Name));
            Assert.AreNotEqual(default(FileAttributes), sut.Attributes, nameof(sut.Attributes));
            Assert.AreNotEqual(DateTime.MinValue, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreNotEqual(DateTime.MinValue, sut.LastAccessTime, nameof(sut.LastAccessTime));
#else
            Assert.AreEqual(DokanOperationsFixture.RootPath, sut.Name, nameof(sut.Name));
            Assert.AreEqual(attributes, sut.Attributes, nameof(sut.Attributes));
            Assert.AreEqual(creationTime, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreEqual(lastWriteTime, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreEqual(lastAccessTime, sut.LastAccessTime, nameof(sut.LastAccessTime));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetDirectories_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => (i.Attributes & FileAttributes.Directory) != 0).Select(i => i.FileName).ToArray(),
                sut.GetDirectories().Select(d => d.Name).ToArray(), 
                nameof(sut.GetDirectories));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetFiles_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles().Length);
#else
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => (i.Attributes & FileAttributes.Normal) != 0).Select(i => i.FileName).ToArray(),
                sut.GetFiles().Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetFileSystemInfos_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos().Length);
#else
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos().Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetDirectoriesWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string filter = "*r2";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(filter), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories(filter).Length);
#else
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => (i.Attributes & FileAttributes.Directory) != 0 && regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetDirectories(filter).Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetFilesWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string filter = "*bD*";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(filter), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles(filter).Length);
#else
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => (i.Attributes & FileAttributes.Normal) != 0 && regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetFiles(filter).Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void GetFileSystemInfosWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string filter = "*bD*";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT);
            fixture.SetupFindFiles(DokanOperationsFixture.ROOT, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(filter), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos(filter).Length);
#else
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos(filter).Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void CreateDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, FileAttributes.Directory, DateTime.Now, DateTime.Now, DateTime.Now);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootPath);

#if LOGONLY
            Assert.IsNotNull(sut.CreateSubdirectory(DokanOperationsFixture.DirectoryName), nameof(sut.CreateSubdirectory));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            var directory = sut.CreateSubdirectory(DokanOperationsFixture.DirectoryName);
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Assert.AreEqual(DokanOperationsFixture.DirectoryPath, directory.FullName);

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Delete_WhereRecurseIsFalse_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, FileAttributes.Directory, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupOpenDirectory(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName);
            fixture.SetupDeleteDirectory(DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, false);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryPath);

            sut.Delete(false);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsNonEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            string directoryPath = DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName, subFileName = "SubFile.ext", subFilePath = Path.DirectorySeparatorChar + subFileName;
            fixture.SetupCreateFile(directoryPath, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(directoryPath, FileAttributes.Directory, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupOpenDirectory(directoryPath);
            fixture.SetupFindFiles(directoryPath, new Collection<FileInformation>(new[] {
                new FileInformation() { FileName = subFileName, Attributes = FileAttributes.Normal, Length = 100, CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today }
            }));
            fixture.SetupCreateFile(directoryPath + subFilePath,
                DokanOperationsFixture.DELETE_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(directoryPath + subFilePath,
                FileAttributes.Normal, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupDeleteFile(directoryPath + subFilePath);
            fixture.SetupDeleteDirectory(directoryPath, true);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryPath);

            sut.Delete(true);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            string directoryPath = DokanOperationsFixture.ROOT + DokanOperationsFixture.DirectoryName;
            fixture.SetupCreateFile(directoryPath, DokanOperationsFixture.READATTRIBUTES_FILEACCESS, DokanOperationsFixture.READWRITE_FILESHARE, FileMode.Open);
            fixture.SetupGetFileInformation(directoryPath, FileAttributes.Directory, DateTime.Now, DateTime.Now, DateTime.Now);
            fixture.SetupOpenDirectory(directoryPath);
            fixture.SetupFindFiles(directoryPath, new Collection<FileInformation>());
            fixture.SetupDeleteDirectory(directoryPath, true);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryPath);

            sut.Delete(true);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }
    }
}
