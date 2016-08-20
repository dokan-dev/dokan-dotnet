using System;
using System.Diagnostics.CodeAnalysis;
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
        public TestContext TestContext { get; set; }

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

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            var attributes = FileAttributes.Directory;
            var creationTime = new DateTime(2015, 1, 1, 12, 0, 0);
            var lastWriteTime = new DateTime(2015, 3, 31, 12, 0, 0);
            var lastAccessTime = new DateTime(2015, 4, 1, 6, 0, 0);
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
            Assert.AreEqual(fixture.DirectoryName, sut.Name, nameof(sut.Name));
            Assert.AreEqual(fixture.DirectoryName.AsDriveBasedPath(), sut.FullName, nameof(sut.FullName));
            Assert.AreEqual(attributes, sut.Attributes, nameof(sut.Attributes));
            Assert.AreEqual(creationTime, sut.CreationTime, nameof(sut.CreationTime));
            Assert.AreEqual(lastWriteTime, sut.LastWriteTime, nameof(sut.LastWriteTime));
            Assert.AreEqual(lastAccessTime, sut.LastAccessTime, nameof(sut.LastAccessTime));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExists_WhereDirectoryExists_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            Assert.IsTrue(sut.Exists, "Directory should exist");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        public void GetExists_WhereDirectoryDoesNotExist_ReturnsCorrectResult()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path.AsRootedPath(), DokanResult.PathNotFound);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            Assert.IsFalse(sut.Exists, "Directory should not exist");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetExtension_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            Assert.AreEqual(Path.GetExtension(path), sut.Extension, "Unexpected extension");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetParent_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Parent.FullName, "Unexpected parent directory");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

            Assert.AreEqual(DokanOperationsFixture.RootName.AsDriveBasedPath(), sut.Root.FullName, "Unexpected parent directory");

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path.AsRootedPath(), DokanResult.PathNotFound);
            fixture.ExpectCreateDirectory(path.AsRootedPath());
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.Create();

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_WhereTargetExists_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.Create();

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException))]
        public void Create_WhereTargetIsFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Normal);
            fixture.ExpectOpenDirectory(DokanOperationsFixture.RootName, FileAccess.Synchronize, FileShare.None);
            fixture.ExpectFindFiles(DokanOperationsFixture.RootName, new[]
            {
                new FileInformation()
                {
                    FileName = path, Attributes = FileAttributes.Normal,
                    Length = 0,
                    CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today
                }
            });
            fixture.ExpectCreateDirectoryToFail(path.AsRootedPath(), DokanResult.FileExists);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.Create();
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CreateSubdirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string basePath = fixture.DirectoryName,
                path = Path.Combine(basePath, fixture.SubDirectoryName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path.AsRootedPath(), DokanResult.FileNotFound);
            fixture.ExpectCreateFile(basePath.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(basePath.AsRootedPath(), FileAttributes.Directory);
            fixture.ExpectCreateDirectory(path.AsRootedPath());
#endif

            var sut = new DirectoryInfo(basePath.AsDriveBasedPath());
            var directory = sut.CreateSubdirectory(fixture.SubDirectoryName);

#if LOGONLY
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Assert.AreEqual(path.AsDriveBasedPath(), directory.FullName);

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void CreateSubdirectory_WhereTargetExists_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string basePath = fixture.DirectoryName,
                path = Path.Combine(basePath, fixture.SubDirectoryName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
#endif

            var sut = new DirectoryInfo(basePath.AsDriveBasedPath());
            var directory = sut.CreateSubdirectory(fixture.SubDirectoryName);

#if LOGONLY
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            Assert.IsNotNull(directory, nameof(sut.CreateSubdirectory));
            Assert.AreEqual(path.AsDriveBasedPath(), directory.FullName);

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException))]
        public void CreateSubdirectory_WhereTargetIsFile_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string basePath = fixture.DirectoryName,
                path = Path.Combine(basePath, fixture.SubDirectoryName);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Normal);
            fixture.ExpectCreateFile(basePath.AsRootedPath(), ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(basePath.AsRootedPath(), FileAttributes.Directory);
            fixture.ExpectCreateDirectoryToFail(path.AsRootedPath(), DokanResult.FileExists);
#endif

            var sut = new DirectoryInfo(basePath.AsDriveBasedPath());
            sut.CreateSubdirectory(fixture.SubDirectoryName);
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Delete_WhereRecurseIsFalse_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectOpenDirectory(path, DeleteFromDirectoryAccess);
            fixture.ExpectDeleteDirectory(path);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(false);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsNonempty_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.DirectoryName.AsRootedPath(),
                subFileName = "SubFile.ext",
                subFilePath = Path.DirectorySeparatorChar + subFileName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", DokanOperationsFixture.GetEmptyDirectoryDefaultFiles().Concat(new[]
                    {
                        new FileInformation()
                        {
                            FileName = subFileName, Attributes = FileAttributes.Normal,
                            Length = 100,
                            CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today
                        }
                    }).ToArray());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, DokanOperationsFixture.GetEmptyDirectoryDefaultFiles().Concat(new[]
                {
                    new FileInformation()
                    {
                        FileName = subFileName, Attributes = FileAttributes.Normal,
                        Length = 100,
                        CreationTime = DateTime.Today, LastWriteTime = DateTime.Today, LastAccessTime = DateTime.Today
                    }
                }).ToArray());
            }
            fixture.ExpectCreateFile(path + subFilePath, DeleteAccess, ReadWriteShare, FileMode.Open, deleteOnClose: true);
            fixture.ExpectGetFileInformation(path + subFilePath, FileAttributes.Normal);
            fixture.ExpectDeleteFile(path + subFilePath);
            fixture.ExpectOpenDirectory(path, DeleteFromDirectoryAccess);
            fixture.ExpectDeleteDirectory(path);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(true);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void Delete_WhereRecurseIsTrueAndDirectoryIsEmpty_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", DokanOperationsFixture.GetEmptyDirectoryDefaultFiles());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, DokanOperationsFixture.GetEmptyDirectoryDefaultFiles());
            }
            fixture.ExpectOpenDirectory(path, DeleteFromDirectoryAccess);
            fixture.ExpectDeleteDirectory(path);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.Delete(true);

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAccessControl_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ReadAttributesPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultDirectorySecurity);
            fixture.ExpectCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(DokanOperationsFixture.RootName,
                DokanOperationsFixture.DefaultDirectorySecurity);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            var security = sut.GetAccessControl();

#if !LOGONLY
            Assert.IsNotNull(security, "Security descriptor should be set");
            Assert.AreEqual(DokanOperationsFixture.DefaultDirectorySecurity.AsString(), security.AsString(), "Security descriptors should match");
            fixture.Verify();
#endif
        }

        private static void GetDirectories_OnRootDirectory_CallsApiCorrectly(bool supportsPatternSearch)
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.RootDirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }

#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Directory))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetDirectories().Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetDirectories_OnRootDirectory_WithPatternSearch_CallsApiCorrectly()
        {
            GetDirectories_OnRootDirectory_CallsApiCorrectly(true);
        }

        [TestMethod, TestCategory(TestCategories.Success), TestCategory(TestCategories.NoPatternSearch)]
        public void GetDirectories_OnRootDirectory_WithoutPatternSearch_CallsApiCorrectly()
        {
            GetDirectories_OnRootDirectory_CallsApiCorrectly(false);
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubDirectory")]
        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetDirectories_OnSubDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.DirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.DirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories().Length);
#else
            CollectionAssert.AreEqual(
                fixture.DirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Directory))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetDirectories().Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetDirectoriesWithFilter_OnRootDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
            var filter = "*r2";
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, filter, fixture.RootDirectoryItems.Where(i => regex.IsMatch(i.FileName)).ToList());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, filter, DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetDirectories(filter), nameof(sut.GetDirectories));
            Console.WriteLine(sut.GetDirectories(filter).Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Where(
                    i => i.Attributes.HasFlag(FileAttributes.Directory) && regex.IsMatch(i.FileName))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetDirectories(filter).Select(d => d.Name).ToArray(),
                nameof(sut.GetDirectories));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFiles_OnRootDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.RootDirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles().Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetFiles().Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubDirectory")]
        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFiles_OnSubDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.DirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.DirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles().Length);
#else
            CollectionAssert.AreEqual(
                fixture.DirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetFiles().Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFilesWithFilter_OnRootDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
            var filter = "*bD*";
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, filter, fixture.RootDirectoryItems.Where(i => regex.IsMatch(i.FileName)).ToList());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, filter, DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(filter), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles(filter).Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal) && regex.IsMatch(i.FileName))
                    .Select(i => i.FileName)
                    .ToArray(),
                sut.GetFiles(filter).Select(f => f.Name).ToArray(),
                nameof(sut.GetFiles));

            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubDirectory")]
        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFiles_UnknownDates_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            //Remove all dates
            var files = DokanOperationsFixture.RemoveDatesFromFileInformations(fixture.DirectoryItems);

            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", files);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, files);
            }
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFiles(), nameof(sut.GetFiles));
            Console.WriteLine(sut.GetFiles().Length);
#else
            var receivedFiles = sut.GetFiles();

            // As DirectoryInfo does not handle uninitialized DateTime values correctly
            // it has to be provided with DateTime.FromFileTime(0) as default minimum date
            var defaultDate = DateTime.FromFileTime(0);
            var expectedDateCollection = Enumerable.Repeat(defaultDate, receivedFiles.Length).ToArray();

            CollectionAssert.AreEqual(
                receivedFiles.Select(f => f.Name).ToArray(),
                fixture.DirectoryItems.Where(i => i.Attributes.HasFlag(FileAttributes.Normal)).Select(i => i.FileName).ToArray(),
                nameof(sut.GetFiles));

            CollectionAssert.AreEqual(
                expectedDateCollection,
                receivedFiles.Select(x => x.CreationTime).ToArray(),
                nameof(FileInformation.CreationTime));
            CollectionAssert.AreEqual(
                expectedDateCollection,
                receivedFiles.Select(x => x.LastWriteTime).ToArray(),
                nameof(FileInformation.LastWriteTime));
            CollectionAssert.AreEqual(
                expectedDateCollection,
                receivedFiles.Select(x => x.LastAccessTime).ToArray(),
                nameof(FileInformation.LastAccessTime));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFileSystemInfos_OnRootDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.RootDirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos().Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos().Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SubDirectory")]
        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFileSystemInfos_OnSubDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, "*", fixture.DirectoryItems);
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.DirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos().Length);
#else
            CollectionAssert.AreEqual(
                fixture.DirectoryItems.Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos().Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFileSystemInfosWithFilter_OnRootDirectory_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = DokanOperationsFixture.RootName;
            var filter = "*bD*";
            var regex = new Regex(filter.Replace('?', '.').Replace("*", ".*"));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectOpenDirectory(path);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path, filter, fixture.RootDirectoryItems.Where(i => regex.IsMatch(i.FileName)).ToList());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path, filter, DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path, fixture.RootDirectoryItems);
            }
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(filter), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos(filter).Length);
#else
            CollectionAssert.AreEqual(
                fixture.RootDirectoryItems.Where(i => regex.IsMatch(i.FileName)).Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos(filter).Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void GetFileSystemInfos_OnRootDirectory_WhereSearchOptionIsAllDirectories_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var pathsAndItems = new[]
            {
                new { Path = DokanOperationsFixture.RootName, Items = fixture.RootDirectoryItems },
                new { Path = fixture.DirectoryName.AsRootedPath(), Items = DokanOperationsFixture.GetEmptyDirectoryDefaultFiles().Concat(fixture.DirectoryItems).ToArray() },
                new { Path = Path.Combine(fixture.DirectoryName, fixture.SubDirectoryName).AsRootedPath(), Items = DokanOperationsFixture.GetEmptyDirectoryDefaultFiles() .Concat(fixture.SubDirectoryItems) .ToArray() },
                new { Path = fixture.Directory2Name.AsRootedPath(), Items = DokanOperationsFixture.GetEmptyDirectoryDefaultFiles().Concat(fixture.Directory2Items).ToArray() },
                new { Path = Path.Combine(fixture.Directory2Name, fixture.SubDirectory2Name).AsRootedPath(), Items = DokanOperationsFixture.GetEmptyDirectoryDefaultFiles().ToArray() }
            };
#if LOGONLY
            fixture.SetupAny();
#else
            foreach (var pathAndItem in pathsAndItems)
            {
                fixture.ExpectOpenDirectory(pathAndItem.Path);
                if (supportsPatternSearch)
                {
                    fixture.ExpectFindFilesWithPattern(pathAndItem.Path, "*", pathAndItem.Items);
                }
                else
                {
                    fixture.ExpectFindFilesWithPatternToFail(pathAndItem.Path, "*", DokanResult.NotImplemented);
                    fixture.ExpectFindFiles(pathAndItem.Path, pathAndItem.Items);
                }
            }
#endif

            var sut = new DirectoryInfo(DokanOperationsFixture.RootName.AsDriveBasedPath());

#if LOGONLY
            Assert.IsNotNull(sut.GetFileSystemInfos(), nameof(sut.GetFileSystemInfos));
            Console.WriteLine(sut.GetFileSystemInfos().Length);
#else
            CollectionAssert.AreEqual(
                pathsAndItems.Select(p => p.Items.Where(f => f.FileName.Any(c => c != '.'))).Aggregate((i1, i2) => i1.Union(i2).ToArray()).Select(i => i.FileName).ToArray(),
                sut.GetFileSystemInfos("*", SearchOption.AllDirectories).Select(f => f.Name).ToArray(),
                nameof(sut.GetFileSystemInfos));

            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsRoot_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.DirectoryName.AsRootedPath(),
                destinationPath = fixture.DestinationDirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectCreateFile(destinationPath, AppendToDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.ExpectOpenDirectoryWithoutCleanup(DokanOperationsFixture.RootName, AppendToDirectoryAccess, FileShare.ReadWrite);
            fixture.PermitGetFileInformationToFail(destinationPath, DokanResult.FileNotFound);
            fixture.PermitOpenDirectory(DokanOperationsFixture.RootName, attributes: FileAttributes.Normal);
            fixture.ExpectMoveFile(path, destinationPath, false);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationDirectoryName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ParentIs")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void MoveTo_WhereParentIsDirectory_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string origin = Path.Combine(fixture.DirectoryName, fixture.SubDirectoryName),
                destination = Path.Combine(fixture.DestinationDirectoryName, fixture.DestinationSubDirectoryName),
                path = origin.AsRootedPath(),
                destinationPath = destination.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileWithoutCleanup(path, MoveFromAccess, ReadWriteShare, FileMode.Open, FileOptions.None);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectOpenDirectoryWithoutCleanup(fixture.DestinationDirectoryName.AsRootedPath(), AppendToDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectCreateFile(destinationPath, AppendToDirectoryAccess, FileShare.ReadWrite, FileMode.Open);
            fixture.PermitGetFileInformationToFail(destinationPath, DokanResult.PathNotFound);
            fixture.PermitOpenDirectory(fixture.DestinationDirectoryName.AsRootedPath(), attributes: FileAttributes.Normal);
            fixture.ExpectMoveFile(path, destinationPath, false);
#endif

            var sut = new DirectoryInfo(origin.AsDriveBasedPath());

            sut.MoveTo(destination.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(DirectoryNotFoundException), "Expected DirectoryNotFoundException not thrown")]
        public void MoveTo_WhereSourceDoesNotExist_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFileToFail(path, DokanResult.PathNotFound);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationDirectoryName.AsDriveBasedPath());
        }

        [TestMethod, TestCategory(TestCategories.Failure)]
        [ExpectedException(typeof(IOException), "Expected IOException not thrown")]
        public void MoveTo_WhereTargetExists_Throws()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.DirectoryName.AsRootedPath(),
                destinationPath = fixture.DestinationDirectoryName.AsRootedPath();
#if LOGONLY
                fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path, MoveFromAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path, FileAttributes.Directory);
            fixture.ExpectCreateFileToFail(destinationPath, DokanResult.FileExists);
            fixture.ExpectOpenDirectoryWithoutCleanup(DokanOperationsFixture.RootName, AppendToDirectoryAccess, FileShare.ReadWrite);
            fixture.ExpectGetFileInformation(destinationPath, FileAttributes.Directory);
            fixture.ExpectOpenDirectory(DokanOperationsFixture.RootName, attributes: FileAttributes.Normal);
            fixture.ExpectMoveFileToFail(path, destinationPath, false, DokanResult.FileExists);
#endif

            var sut = new DirectoryInfo(fixture.DirectoryName.AsDriveBasedPath());

            sut.MoveTo(fixture.DestinationDirectoryName.AsDriveBasedPath());

#if !LOGONLY
            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        [DeploymentItem("DirectoryInfoTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DirectoryInfoTests.Configuration.xml", "ConfigFindFiles", DataAccessMethod.Sequential)]
        public void SetAccessControl_CallsApiCorrectly()
        {
            var supportsPatternSearch = bool.Parse((string) TestContext.DataRow["SupportsPatternSearch"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.DirectoryName;
            var security = new DirectorySecurity();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.ExpectCreateFile(path.AsRootedPath(), ChangePermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(path.AsRootedPath(), FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(path.AsRootedPath(), DokanOperationsFixture.DefaultDirectorySecurity);
            fixture.ExpectOpenDirectory(path.AsRootedPath(), share: FileShare.ReadWrite);
            if (supportsPatternSearch)
            {
                fixture.ExpectFindFilesWithPattern(path.AsRootedPath(), "*", Array.Empty<FileInformation>());
            }
            else
            {
                fixture.ExpectFindFilesWithPatternToFail(path.AsRootedPath(), "*", DokanResult.NotImplemented);
                fixture.ExpectFindFiles(path.AsRootedPath(), Array.Empty<FileInformation>());
            }
            fixture.ExpectSetFileSecurity(path.AsRootedPath(), security);
            fixture.ExpectCreateFile(DokanOperationsFixture.RootName, ReadPermissionsAccess, ReadWriteShare, FileMode.Open);
            fixture.ExpectGetFileInformation(DokanOperationsFixture.RootName, FileAttributes.Directory);
            fixture.ExpectGetFileSecurity(DokanOperationsFixture.RootName, DokanOperationsFixture.DefaultDirectorySecurity, AccessControlSections.Access);
#endif

            var sut = new DirectoryInfo(path.AsDriveBasedPath());
            sut.SetAccessControl(security);

#if !LOGONLY
            fixture.Verify();
#endif
        }
    }
}