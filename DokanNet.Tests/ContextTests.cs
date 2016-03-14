using System;
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

        private class TracingContext
        {
            private ContextTests test;

            private string name;

            public TracingContext(ContextTests test)
            {
                this.test = test;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "name")]
            public void InitTrace(string name)
            {
                this.name = name;
                test.context = null;
            }

            public override string ToString() => name != null ? $"{nameof(TestContext)} '{name}' #{++test.contextAccessCount}" : base.ToString();
        }

        private TracingContext context;

        private int contextAccessCount;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            smallData = new byte[4096];
            for (int i = 0; i < smallData.Length; ++i)
                smallData[i] = (byte)(i % 256);

            largeData = new byte[5 * FILE_BUFFER_SIZE + 65536];
            for (int i = 0; i < largeData.Length; ++i)
                largeData[i] = (byte)(i % 251);
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

            context = new TracingContext(this);
        }

        [TestCleanup]
        public void Cleanup()
        {
            bool hasUnmatchedInvocations = false;
            DokanOperationsFixture.ClearInstance(out hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void Create_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(Create_PassesContextCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadWriteAccess, WriteShare, FileMode.Create, FileOptions.None, context: context);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);
            context.InitTrace(nameof(Create_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.Create())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            Assert.AreEqual(5, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenRead_PassesContextCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.SetupReadFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);
            context.InitTrace(nameof(OpenRead_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[value.Length];
                int readBytes = stream.Read(target, 0, target.Length);
            }

#if !LOGONLY
            Assert.AreEqual(4, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
            context.InitTrace(nameof(OpenRead_WithLargeFile_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;
                do
                {
                    totalReadBytes += stream.Read(target, totalReadBytes, target.Length - totalReadBytes);
                } while (totalReadBytes < largeData.Length);

            }

#if !LOGONLY
            Assert.AreEqual(9, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFile_InParallel_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: context);
            fixture.SetupReadFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
            context.InitTrace(nameof(OpenRead_WithLargeFile_InParallel_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenRead())
            {
                var target = new byte[largeData.Length];
                int totalReadBytes = 0;

                Parallel.For(0, DokanOperationsFixture.NumberOfChunks(FILE_BUFFER_SIZE, largeData.Length), i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
                    var count = Math.Min(FILE_BUFFER_SIZE, target.Length - origin);
                    lock (stream)
                    {
                        stream.Seek(origin, SeekOrigin.Begin);
                        totalReadBytes += stream.Read(target, origin, count);
                    }
                });
            }

#if !LOGONLY
            Assert.AreEqual(9, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
            string value = $"TestValue for test {nameof(OpenWrite_PassesContextCorrectly)}";
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.SetupWriteFile(path, Encoding.UTF8.GetBytes(value), value.Length, context: context);
            context.InitTrace(nameof(OpenWrite_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                stream.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);
            }

#if !LOGONLY
            Assert.AreEqual(5, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
            context.InitTrace(nameof(OpenWrite_WithLargeFile_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                int totalWrittenBytes = 0;

                do
                {
                    int writtenBytes = Math.Min(FILE_BUFFER_SIZE, largeData.Length - totalWrittenBytes);
                    stream.Write(largeData, totalWrittenBytes, writtenBytes);
                    totalWrittenBytes += writtenBytes;
                } while (totalWrittenBytes < largeData.Length);
            }

#if !LOGONLY
            Assert.AreEqual(10, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFile_InParallel_PassesContextCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: context);
            fixture.SetupWriteFileInChunks(path, largeData, FILE_BUFFER_SIZE, context: context);
            context.InitTrace(nameof(OpenWrite_WithLargeFile_InParallel_PassesContextCorrectly));
#endif

            var sut = new FileInfo(fixture.FileName.AsDriveBasedPath());

            using (var stream = sut.OpenWrite())
            {
                int totalWrittenBytes = 0;

                Parallel.For(0, DokanOperationsFixture.NumberOfChunks(FILE_BUFFER_SIZE, largeData.Length), i =>
                {
                    var origin = i * FILE_BUFFER_SIZE;
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
            Assert.AreEqual(10, contextAccessCount, "Unexpected number of context accesses");

            fixture.VerifyAll();
#endif
        }
    }
}