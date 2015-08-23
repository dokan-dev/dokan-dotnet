using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class DirectoryInfoTest
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
        [TestCategory(TestCategories.Success)]
        public void GetAttributes_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Directory;
            var creationTime = new DateTime(2015, 1, 1, 12, 0, 0);
            var lastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0);
            var lastAccessTime = new DateTime(2015, 4, 1, 6, 0, 0);
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
            Assert.AreEqual(DokanOperationsFixture.DirectoryName, sut.Name, nameof(sut.Name));
            Assert.AreEqual(DokanOperationsFixture.DirectoryName.AsDriveBasedPath(), sut.FullName, nameof(sut.FullName));
            Assert.AreEqual(attributes, sut.Attributes, nameof(sut.Attributes));
            Assert.AreEqual(creationTime, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreEqual(lastWriteTime, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreEqual(lastAccessTime, sut.LastAccessTime, nameof(sut.LastAccessTime));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetExists_WhereDirectoryExists_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            Assert.IsTrue(sut.Exists, "Directory should exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
        public void GetExists_WhereDirectoryDoesNotExist_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path.AsRootedPath(), DokanResult.PathNotFound);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            Assert.IsFalse(sut.Exists, "Directory should not exist");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetExtension_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            Assert.AreEqual(Path.GetExtension(path), sut.Extension, "Unexpected extension");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetParent_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Parent.FullName, "Unexpected parent directory");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Root.FullName, "Unexpected parent directory");

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.Create();

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void CreateSubdirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.CreateSubdirectory(DokanOperationsFixture.DirectoryName), nameof(sut.CreateSubdirectory));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            var directory = sut.CreateSubdirectory(path);
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Assert.AreEqual(path.AsDriveBasedPath(), directory.FullName);

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Delete_WhereRecurseIsFalse_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory);
            fixture.SetupOpenDirectory(path);
            fixture.SetupDeleteDirectory(path, false);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(false);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsNonempty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath(),
                subFileName = "SubFile.ext",
                subFilePath = Path.DirectorySeparatorChar + subFileName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory);
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.GetEmptyDirectoryFiles().Concat(new[] {
                new FileInformation() { FileName = subFileName, Attributes = FileAttributes.Normal, Length = 100, CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today }
            }).ToArray());
            fixture.SetupCreateFile(path + subFilePath, DeleteAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path + subFilePath, FileAttributes.Normal);
            fixture.SetupDeleteFile(path + subFilePath);
            fixture.SetupDeleteDirectory(path, true);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(true);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsEmpty_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory);
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.GetEmptyDirectoryFiles());
            fixture.SetupDeleteDirectory(path, true);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(true);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ReadAttributesPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
            fixture.SetupGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultDirectorySecurity);
            fixture.SetupCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.SetupGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            var security = sut.GetAccessControl();

#if !LOGONLY
            Assert.IsNotNull(security, "Security descriptor should be set");
            Assert.AreEqual(DokanOperationsFixture.DefaultDirectorySecurity.AsString(), security.AsString(), "Security descriptors should match");
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetDirectories_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Directory)).Select(i => i.FileName).ToArray(),
                sut.GetDirectories().Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetDirectoriesWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
            string filter = "*r2";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(filter), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories(filter).Length);
#else
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Directory) && regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetDirectories(filter).Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetFiles_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles().Length);
#else
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal)).Select(i => i.FileName).ToArray(),
                sut.GetFiles().Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetFilesWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
            string filter = "*bD*";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(filter), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles(filter).Length);
#else
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
            CollectionAssert.AreEqual(
                DokanOperationsFixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal) && regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetFiles(filter).Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void GetFileSystemInfos_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

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
        [TestCategory(TestCategories.Success)]
        public void GetFileSystemInfosWithFilter_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
            string filter = "*bD*";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(path);
            fixture.SetupFindFiles(path, DokanOperationsFixture.RootDirectoryItems as IList<FileInformation>);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

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
        [TestCategory(TestCategories.Success)]
        public void MoveTo_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationDirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory);
            fixture.SetupMoveFile(path, destinationPath, false);
            fixture.SetupCleanupFile(destinationPath, true);
            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupCleanupFile(destinationPath/* This call is probably redundant. */);

            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenBlankDirectory();
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationDirectoryName.AsDriveBasedPath());

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(DirectoryNotFoundException), "Expected DirectoryNotFoundException not thrown")]
        public void MoveTo_WhereSourceDoesNotExist_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFileWithError(path, DokanResult.PathNotFound);
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationDirectoryName.AsDriveBasedPath());
        }

        [TestMethod]
        [TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName.AsRootedPath(),
                destinationPath = DokanOperationsFixture.DestinationDirectoryName.AsRootedPath();
    #if LOGONLY
                fixture.SetupAny();
    #else
            fixture.SetupCreateFile(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory);
            fixture.SetupCreateFileWithError(destinationPath, DokanResult.AlreadyExists);
    #endif

            var sut = new DirectoryInfo(DokanOperationsFixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(DokanOperationsFixture.DestinationDirectoryName.AsDriveBasedPath());

    #if !LOGONLY
            fixture.VerifyAll();
    #endif
        }

        [TestMethod]
        [TestCategory(TestCategories.Success)]
        public void SetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.DirectoryName;
            var security = new DirectorySecurity();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path.AsRootedPath(), ChangePermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
            fixture.SetupGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultDirectorySecurity);
            fixture.SetupSetFileSecurity(path.AsRootedPath(), security);
            fixture.SetupCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.SetupGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity, AccessControlSections.Access);

            // WARNING: This is probably an error in the Dokan driver!
            fixture.SetupOpenDirectoryWithError(path.AsRootedPath() + Path.DirectorySeparatorChar, DokanResult.PathNotFound);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.SetAccessControl(security);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }
    }
}
