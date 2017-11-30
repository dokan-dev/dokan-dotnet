using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    [TestClass]
    public static class Mounter
    {
        private static Thread mounterThread;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var dokanOptions = DokanOptions.DebugMode | DokanOptions.MountManager | DokanOptions.CurrentSession;
#if NETWORK_DRIVE
            dokanOptions |= DokanOptions.NetworkDrive;
#else
            dokanOptions |= DokanOptions.RemovableDrive;
#endif
#if USER_MODE_LOCK
            dokanOptions |= DokanOptions.UserModeLock;
#endif

            (mounterThread = new Thread(() => DokanOperationsFixture.Operations.Mount(DokanOperationsFixture.MOUNT_POINT, dokanOptions, 5))).Start();
            var drive = new DriveInfo(DokanOperationsFixture.MOUNT_POINT);
            while (!drive.IsReady)
                Thread.Sleep(50);
            while (DokanOperationsFixture.HasPendingFiles)
                Thread.Sleep(50);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            mounterThread.Abort();
            Dokan.Unmount(DokanOperationsFixture.MOUNT_POINT[0]);
            Dokan.RemoveMountPoint(DokanOperationsFixture.MOUNT_POINT);
        }
    }
}