using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class ContextTests
    {
        private const int FILE_BUFFER_SIZE = 262144;

        private static byte[] smallData;

        private static byte[] largeData;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            smallData = new byte[4096];
            for (var i = 0; i < smallData.Length; ++i)
                smallData[i] = (byte) (i%256);

            largeData = new byte[5*FILE_BUFFER_SIZE + 65536];
            for (var i = 0; i < largeData.Length; ++i)
                largeData[i] = (byte) (i%251);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            largeData = null;
            smallData = null;
        }

        [TestInitialize]
        public void Initialize()
        {
            DokanOperationsFixture.InitInstance(TestContext.TestName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DokanOperationsFixture.ClearInstance(out bool hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var value = $"TestValue for test {nameof(Create_PassesContextCorrectly)}";
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadWriteAccess, WriteShare, FileMode.Create, FileOptions.None, context: context);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.Create())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            fixture.VerifyContextWriteInvocations(path, 1);
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var value = $"TestValue for test {nameof(OpenRead_PassesContextCorrectly)}";
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.ExpectReadFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[value.Length];
                var readBytes = stream.Read(target, 0, target.Length);
            }

#if !LOGONLY
            fixture.VerifyContextReadInvocations(path, 1);
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;
                do
                {
                    totalReadBytes += stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                } while (totalReadBytes < largeData.Length);
            }

#if !LOGONLY
            fixture.VerifyContextReadInvocations(path, 6);
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_InParallel_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.ExpectReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[largeData.Length];
                var totalReadBytes = 0;

                Parallel.For(0, DokanOperationsFixture.NumberOfChunks(FILE_BUFFER_SIZE, largeData.Length), i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        totalReadBytes += stream.Read(target, origin, count);
                    }
                });
            }

#if !LOGONLY
            fixture.VerifyContextReadInvocations(path, 6);
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var value = $"TestValue for test {nameof(OpenWrite_PassesContextCorrectly)}";
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.ExpectWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);

            fixture.PermitProbeFile(path, Encoding.UTF8.GetBytes(value));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            fixture.VerifyContextWriteInvocations(path, 1);
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                var totalWrittenBytes = 0;

                do
                {
                    var writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);
            }

#if !LOGONLY
            fixture.VerifyContextWriteInvocations(path, 6);
#endif
        }

        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_InParallel_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var context = new object();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.ExpectWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);

            fixture.PermitProbeFile(path, largeData);
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                var totalWrittenBytes = 0;

                Parallel.For(0, DokanOperationsFixture.NumberOfChunks(FILE_BUFFER_SIZE, largeData.Length), i =>
                {
                    var origin = i*FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, largeData.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        stream.Write(largeData, origin, count);
                        totalWrittenBytes += count;
                    }
                });
            }

#if !LOGONLY
            fixture.VerifyContextWriteInvocations(path, 6);
#endif
        }
    }
}