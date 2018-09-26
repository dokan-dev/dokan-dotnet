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
        private static Thread mounterThread2;

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

            (mounterThread = new Thread(() => DokanOperationsFixture.Operations.Mount(DokanOperationsFixture.NormalMountPoint, dokanOptions, 5))).Start();
            (mounterThread2 = new Thread(() => DokanOperationsFixture.UnsafeOperations.Mount(DokanOperationsFixture.UnsafeMountPoint, dokanOptions, 5))).Start();
            var drive = new DriveInfo(DokanOperationsFixture.NormalMountPoint);
            var drive2 = new DriveInfo(DokanOperationsFixture.UnsafeMountPoint);
            while (!drive.IsReady || !drive2.IsReady)
                Thread.Sleep(50);
            while (DokanOperationsFixture.HasPendingFiles)
                Thread.Sleep(50);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            mounterThread.Abort();
            mounterThread2.Abort();
            Dokan.Unmount(DokanOperationsFixture.NormalMountPoint[0]);
            Dokan.Unmount(DokanOperationsFixture.UnsafeMountPoint[0]);
            Dokan.RemoveMountPoint(DokanOperationsFixture.NormalMountPoint);
            Dokan.RemoveMountPoint(DokanOperationsFixture.UnsafeMountPoint);
        }
    }
}