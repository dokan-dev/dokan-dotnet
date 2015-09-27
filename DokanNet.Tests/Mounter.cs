using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DokanNet.Tests
{
    [TestClass]
    public static class Mounter
    {
        private static Thread mounterThread;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            (mounterThread = new Thread(new ThreadStart(() => DokanOperationsFixture.Operations.Mount(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture), DokanOptions.DebugMode | DokanOptions.NetworkDrive, 1)))).Start();
            var drive = new DriveInfo(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));
            while (!drive.IsReady)
                Thread.Sleep(50);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            mounterThread.Abort();
            Dokan.Unmount(DokanOperationsFixture.MOUNT_POINT);
            Dokan.RemoveMountPoint(DokanOperationsFixture.MOUNT_POINT.ToString(CultureInfo.InvariantCulture));
        }
    }
}
