using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using static DokanNet.Tests.FileSettings;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class OverlappedTest
    {
        private const long FILE_BUFFER_SIZE = 1 << 19;

        private static class Native
        {
            private const string KERNEL_32_DLL = "kernel32.dll";

            [Flags]
            public enum DesiredAccess : uint
            {
                GENERIC_ALL = 0x10000000,
                GENERIC_EXECUTE = 0x20000000,
                GENERIC_WRITE = 0x40000000,
                GENERIC_READ = 0x80000000
            }

            [Flags]
            public enum ShareMode : uint
            {
                FILE_SHARE_NONE = 0x0,
                FILE_SHARE_READ = 0x1,
                FILE_SHARE_WRITE = 0x2,
                FILE_SHARE_DELETE = 0x4
            }

            public enum CreationDisposition : uint
            {
                CREATE_NEW = 1,
                CREATE_ALWAYS = 2,
                OPEN_EXISTING = 3,
                OPEN_ALWAYS = 4,
                TRUNCATE_EXSTING = 5
            }

            public enum MoveMethod : uint
            {
                FILE_BEGIN = 0,
                FILE_CURRENT = 1,
                FILE_END = 2
            }

            [Flags]
            public enum FlagsAndAttributes : uint
            {
                FILE_ATTRIBUTE_READONLY = 0x0001,
                FILE_ATTRIBUTE_HIDDEN = 0x0002,
                FILE_ATTRIBUTE_SYSTEM = 0x0004,
                FILE_ATTRIBUTE_ARCHIVE = 0x0020,
                FILE_ATTRIBUTE_NORMAL = 0x0080,
                FILE_ATTRIBUTE_TEMPORARY = 0x100,
                FILE_ATTRIBUTE_OFFLINE = 0x1000,
                FILE_ATTRIBUTE_ENCRYPTED = 0x4000,
                FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
                FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
                FILE_FLAG_SESSION_AWARE = 0x00800000,
                FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
                FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
                FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
                FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
                FILE_FLAG_RANDOM_ACCESS = 0x10000000,
                FILE_FLAG_NO_BUFFERING = 0x20000000,
                FILE_FLAG_OVERLAPPED = 0x40000000,
                FILE_FLAG_WRITE_THROUGH = 0x80000000
            }

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern SafeFileHandle CreateFile(string lpFileName, DesiredAccess dwDesiredAccess, ShareMode dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition, FlagsAndAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool ReadFileEx(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool WriteFileEx(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            private delegate void FileIOCompletionRoutine(int dwErrorCode, int dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);

            [DebuggerDisplay("{DebuggerDisplay(),nq}")]
            internal class OverlappedChunk
            {
                public byte[] Buffer { get; }

                public int BytesTransferred { get; set; }

                public int Win32Error { get; set; }

                public OverlappedChunk(int count) : this(new byte[count])
                {
                }

                public OverlappedChunk(byte[] buffer)
                {
                    Buffer = buffer;
                    BytesTransferred = 0;
                    Win32Error = 0;
                }

                private string DebuggerDisplay() => $"{nameof(OverlappedChunk)} Buffer={Buffer?.Length ?? -1} BytesTransferred={BytesTransferred}";
            }

            internal static SafeFileHandle CreateFile(string fileName) => CreateFile(fileName, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_NONE, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            internal static int BufferSize(long bufferSize, long fileSize, int segment) => (int)Math.Min(bufferSize, fileSize - segment * bufferSize);

            internal static OverlappedChunk[] ReadEx(string fileName, long bufferSize, long fileSize)
            {
                var chunks = Enumerable.Range(0, (int)(fileSize / bufferSize + 1))
                    .Select(i => new OverlappedChunk(BufferSize(bufferSize, fileSize, i)))
                    .ToArray();
                var waitHandles = Enumerable.Repeat<Func<EventWaitHandle>>(() => new ManualResetEvent(false), chunks.Length).Select(e => e()).ToArray();

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(waitHandles)));
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_READ, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    for (int i = 0; i < chunks.Length; ++i)
                    {
                        var offset = i * FILE_BUFFER_SIZE;
                        var overlapped = new NativeOverlapped() { OffsetHigh = (int)(offset >> 32), OffsetLow = (int)(offset & 0xffffffff), EventHandle = IntPtr.Zero };

                        var chunk = chunks[i];
                        var waitHandle = waitHandles[i];
                        FileIOCompletionRoutine completion = (int dwErrorCode, int dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) =>
                        {
                            chunk.Win32Error = dwErrorCode;
                            chunk.BytesTransferred = dwNumberOfBytesTransferred;
                            waitHandle.Set();
                        };

                        if (!ReadFileEx(handle, chunk.Buffer, BufferSize(bufferSize, fileSize, i), ref overlapped, completion))
                            chunk.Win32Error = Marshal.GetLastWin32Error();
                    }
                }

                awaiterThread.Join();

                return chunks;
            }

            internal static void WriteEx(string fileName, long bufferSize, long fileSize, OverlappedChunk[] chunks)
            {
                var waitHandles = Enumerable.Repeat<Func<EventWaitHandle>>(() => new ManualResetEvent(false), chunks.Length).Select(e => e()).ToArray();

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(waitHandles)));
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_WRITE, ShareMode.FILE_SHARE_NONE, IntPtr.Zero, CreationDisposition.OPEN_ALWAYS, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    for (int i = 0; i < chunks.Length; ++i)
                    {
                        var offset = i * FILE_BUFFER_SIZE;
                        var overlapped = new NativeOverlapped() { OffsetHigh = (int)(offset >> 32), OffsetLow = (int)(offset & 0xffffffff), EventHandle = IntPtr.Zero };

                        var chunk = chunks[i];
                        var waitHandle = waitHandles[i];
                        FileIOCompletionRoutine completion = (int dwErrorCode, int dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) =>
                        {
                            chunk.Win32Error = dwErrorCode;
                            chunk.BytesTransferred = dwNumberOfBytesTransferred;
                            waitHandle.Set();
                        };

                        if (!WriteFileEx(handle, chunk.Buffer, BufferSize(bufferSize, fileSize, i), ref overlapped, completion))
                            chunk.Win32Error = Marshal.GetLastWin32Error();
                    }
                }

                awaiterThread.Join();
            }
        }

        private static byte[] testData;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            testData = DokanOperationsFixture.InitBlockTestData(FILE_BUFFER_SIZE, 5 * FILE_BUFFER_SIZE + 65536);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            testData = null;
        }

        [TestInitialize]
        public void Initialize()
        {
            DokanOperationsFixture.InitInstance();
        }

        [TestCleanup]
        public void Cleanup()
        {
            bool hasUnmatchedInvocations = false;
            DokanOperationsFixture.ClearInstance(out hasUnmatchedInvocations);
            Assert.IsFalse(hasUnmatchedInvocations, "Found Mock invocations without corresponding setups");
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenRead_WithLargeFileUsingContext_Overlapped_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: testData);
            fixture.SetupReadFileInChunks(path, testData, (int)FILE_BUFFER_SIZE, context: testData, synchronousIo: false);
#endif

            var outputs = Native.ReadEx(DokanOperationsFixture.FileName.AsDriveBasedPath(), FILE_BUFFER_SIZE, testData.Length);

#if !LOGONLY
            for (int i = 0; i < outputs.Length; ++i)
            {
                Assert.AreEqual(0, outputs[i].Win32Error, $"Unexpected Win32 error in output {i}");
                //Assert.AreEqual(outputs[i].BytesTransferred, Native.BufferSize(i), $"Unexpected number of bytes read in output {i}");
                Assert.IsTrue(Enumerable.All(outputs[i].Buffer, b => b == (byte)i + 1), $"Unexpected data in output {i}");
            }

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Success)]
        public void OpenWrite_WithLargeFileUsingContext_Overlapped_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: testData);
            fixture.SetupWriteFileInChunks(path, testData, (int)FILE_BUFFER_SIZE, context: testData, synchronousIo: false);
#endif

            var inputs = Enumerable.Range(0, (int)(testData.Length / FILE_BUFFER_SIZE + 1))
                .Select(i => new Native.OverlappedChunk(Enumerable.Repeat((byte)(i + 1), Native.BufferSize(FILE_BUFFER_SIZE, testData.Length, i)).ToArray()))
                .ToArray();

            Native.WriteEx(DokanOperationsFixture.FileName.AsDriveBasedPath(), FILE_BUFFER_SIZE, testData.Length, inputs);

#if !LOGONLY
            for (int i = 0; i < inputs.Length; ++i)
            {
                Assert.AreEqual(0, inputs[i].Win32Error, "Unexpected Win32 error");
                //Assert.AreEqual(inputs[i].BytesTransferred, Native.BufferSize(i), "Unexpected number of bytes written");
            }

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Manual)]
        [DeploymentItem("OverlappedTest.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\OverlappedTest.Configuration.xml", "ConfigRead", DataAccessMethod.Sequential)]
        public void OpenRead_WithVariableSizes_Overlapped_CallsApiCorrectly()
        {
            var bufferSize = int.Parse((string)TestContext.DataRow["BufferSize"]);
            var fileSize = int.Parse((string)TestContext.DataRow["FileSize"]);

            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            var testData = DokanOperationsFixture.InitBlockTestData(bufferSize, fileSize);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, ReadAccess, ReadOnlyShare, FileMode.Open, FileOptions.None, context: testData);
            fixture.SetupReadFileInChunks(path, testData, bufferSize, context: testData, synchronousIo: false);
#endif

            var outputs = Native.ReadEx(DokanOperationsFixture.FileName.AsDriveBasedPath(), bufferSize, testData.Length);

#if !LOGONLY
            for (int i = 0; i < outputs.Length; ++i)
            {
                Assert.AreEqual(0, outputs[i].Win32Error, $"Unexpected Win32 error in output {i} for BufferSize={bufferSize}, FileSize={fileSize}");
                //Assert.AreEqual(outputs[i].BytesTransferred, Native.BufferSize(i), $"Unexpected number of bytes read in output {i}");
                Assert.IsTrue(Enumerable.All(outputs[i].Buffer, b => b == (byte)i + 1), $"Unexpected data in output {i} for BufferSize={bufferSize}, FileSize={fileSize}");
            }

            fixture.VerifyAll();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Manual)]
        [DeploymentItem("OverlappedTest.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\OverlappedTest.Configuration.xml", "ConfigWrite", DataAccessMethod.Sequential)]
        public void OpenWrite_WithVariableSizes_Overlapped_CallsApiCorrectly()
        {
            var bufferSize = int.Parse((string)TestContext.DataRow["BufferSize"]);
            var fileSize = int.Parse((string)TestContext.DataRow["FileSize"]);

            var fixture = DokanOperationsFixture.Instance;

            string path = DokanOperationsFixture.FileName.AsRootedPath();
            var testData = DokanOperationsFixture.InitBlockTestData(bufferSize, fileSize);
#if LOGONLY
            fixture.SetupAny();
#else
            fixture.SetupCreateFile(path, WriteAccess, WriteShare, FileMode.OpenOrCreate, FileOptions.None, context: testData);
            fixture.SetupWriteFileInChunks(path, testData, bufferSize, context: testData, synchronousIo: false);
#endif

            var numberOfChunks = testData.Length / bufferSize + 1;
            var inputs = Enumerable.Range(0, numberOfChunks).Select(i => new Native.OverlappedChunk(Enumerable.Repeat((byte)(i + 1), Native.BufferSize(FILE_BUFFER_SIZE, testData.Length, i)).ToArray())).ToArray();

            Native.WriteEx(DokanOperationsFixture.FileName.AsDriveBasedPath(), FILE_BUFFER_SIZE, testData.Length, inputs);

#if !LOGONLY
            for (int i = 0; i < inputs.Length; ++i)
            {
                Assert.AreEqual(0, inputs[i].Win32Error, $"Unexpected Win32 error for BufferSize={bufferSize}, FileSize={fileSize}");
                //Assert.AreEqual(inputs[i].BytesTransferred, Native.BufferSize(i), "Unexpected number of bytes written");
            }

            fixture.VerifyAll();
#endif
        }
    }
}