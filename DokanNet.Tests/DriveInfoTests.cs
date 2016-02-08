using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class DriveInfoTests
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
            bool hasUnmatchedInvocations = false;
            DokanOperationsFixture.ClearInstance(out hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetAvailableFreeSpace_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            var availableFreeSpace = 1 << 10;
            fixture.SetupDiskFreeSpace(freeBytesAvailable: availableFreeSpace);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.AreEqual(0, sut.AvailableFreeSpace, nameof(sut.AvailableFreeSpace));
#else
            Assert.AreEqual(availableFreeSpace, sut.AvailableFreeSpace, nameof(sut.AvailableFreeSpace));

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetDriveFormat_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.RootName, FileAccess.Synchronize, FileShare.None);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.IsNotNull(sut.DriveFormat, nameof(sut.DriveFormat));
            Console.WriteLine(sut.DriveFormat);
#else
            Assert.AreEqual(DokanOperationsFixture.FILESYSTEM_NAME, sut.DriveFormat, nameof(sut.DriveFormat));

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetDriveType_CallsApiCorrectly()
        {
            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if NETWORK_DRIVE
            Assert.AreEqual(DriveType.Network, sut.DriveType, nameof(sut.DriveType));
#else
            Assert.AreEqual(DriveType.Removable, sut.DriveType, nameof(sut.DriveType));
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetIsReady_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.RootName;
#if LOGONLY
            fixture.SetupAny();
#else
            var anyDateTime = new DateTime(2000, 1, 1, 12, 0, 0);
            fixture.SetupCreateFile(path, ReadAttributesAccess, ReadWriteShare, FileMode.Open);
            fixture.SetupGetFileInformation(path, FileAttributes.Directory, anyDateTime, anyDateTime, anyDateTime);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Console.WriteLine($"sut.IsReady {sut.IsReady}");
#else
            Assert.IsTrue(sut.IsReady, nameof(sut.IsReady));
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetName_CallsApiCorrectly()
        {
            string path = DokanOperationsFixture.RootName.AsDriveBasedPath();

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual(path, sut.Name, nameof(sut.Name));
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetRootDirectory_CallsApiCorrectly()
        {
            string path = DokanOperationsFixture.RootName.AsDriveBasedPath();

#if LOGONLY
            fixture.SetupAny();
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.IsNotNull(sut.RootDirectory, nameof(sut.RootDirectory));
            Console.WriteLine(sut.RootDirectory);
#else
            Assert.AreEqual(path, sut.RootDirectory?.Name, nameof(sut.RootDirectory));
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetTotalFreeSpace_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            var totalFreeSpace = 1 << 14;
            fixture.SetupDiskFreeSpace(totalNumberOfFreeBytes: totalFreeSpace);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.AreEqual(0, sut.TotalFreeSpace, nameof(sut.TotalFreeSpace));
#else
            Assert.AreEqual(totalFreeSpace, sut.TotalFreeSpace, nameof(sut.TotalFreeSpace));

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetTotalSize_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            var totalSize = 1 << 20;
            fixture.SetupDiskFreeSpace(totalNumberOfBytes: totalSize);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.AreEqual(0, sut.TotalSize, nameof(sut.TotalSize));
#else
            Assert.AreEqual(totalSize, sut.TotalSize, nameof(sut.TotalSize));

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void GetVolumeLabel_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupOpenDirectory(DokanOperationsFixture.RootName, FileAccess.Synchronize, FileShare.None);
            fixture.SetupGetVolumeInformation(DokanOperationsFixture.VOLUME_LABEL, DokanOperationsFixture.FILESYSTEM_NAME);
#endif

            var sut = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));

#if LOGONLY
            Assert.IsNotNull(sut.VolumeLabel, nameof(sut.VolumeLabel));
            Console.WriteLine(sut.VolumeLabel);
#else
            Assert.AreEqual(DokanOperationsFixture.VOLUME_LABEL, sut.VolumeLabel, nameof(sut.VolumeLabel));

            fixture.VerifyAll();
#endif
        }
    }
}
