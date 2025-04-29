using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    [TestClass]
    public static class Mounter
    {
        private static Logging.NullLogger NullLogger = new Logging.NullLogger();
        private static Dokan Dokan;
        private static DokanInstance safeMount;
        private static DokanInstance unsafeMount;

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

            Dokan = new Dokan(NullLogger);
            var safeDokanBuilder = new DokanInstanceBuilder(Dokan)
                .ConfigureOptions(options =>
                {
                    options.Options = dokanOptions;
                    options.MountPoint = DokanOperationsFixture.NormalMountPoint;
                });

            safeMount = safeDokanBuilder.Build(DokanOperationsFixture.Operations);

            var unsafeDokanBuilder = new DokanInstanceBuilder(Dokan)
               .ConfigureOptions(options =>
               {
                   options.Options = dokanOptions;
                   options.MountPoint = DokanOperationsFixture.UnsafeMountPoint;
               });
            unsafeMount = unsafeDokanBuilder.Build(DokanOperationsFixture.UnsafeOperations);
            var drive = new DriveInfo(DokanOperationsFixture.NormalMountPoint);
            var drive2 = new DriveInfo(DokanOperationsFixture.UnsafeMountPoint);
            while (!drive.IsReady || !drive2.IsReady)
            {
                Thread.Sleep(50);
            }

            while (DokanOperationsFixture.HasPendingFiles)
            {
                Thread.Sleep(50);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            safeMount.Dispose();
            unsafeMount.Dispose();
        }
    }
}
