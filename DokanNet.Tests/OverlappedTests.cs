using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using static DokanNet.Tests.FileSettings;

namespace DokanNet.Tests
{
    [TestClass]
    public sealed class OverlappedTests
    {
        private const long FILE_BUFFER_SIZE = 1 << 19;

        private static class NativeMethods
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

            [DllImport(KERNEL_32_DLL, SetLastError = true, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
            private static extern SafeFileHandle CreateFile(string lpFileName, DesiredAccess dwDesiredAccess,
                ShareMode dwShareMode, IntPtr lpSecurityAttributes, CreationDisposition dwCreationDisposition,
                FlagsAndAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool ReadFileEx(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead,
                ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool SetEndOfFile(SafeFileHandle hFile);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern int SetFilePointer(SafeFileHandle hFile, int lDistanceToMove,
                out int lpDistanceToMoveHigh, MoveMethod dwMoveMethod);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool WriteFileEx(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite,
                ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            private delegate void FileIOCompletionRoutine(
                int dwErrorCode, int dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);

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

                [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                    Justification = "Used for debugging only")]
                private string DebuggerDisplay()
                    => $"{nameof(OverlappedChunk)} Buffer={Buffer?.Length ?? -1} BytesTransferred={BytesTransferred}";
            }

            internal static int BufferSize(long bufferSize, long fileSize, int chunks)
                => (int) Math.Min(bufferSize, fileSize - chunks*bufferSize);

            internal static int NumberOfChunks(long bufferSize, long fileSize)
            {
                var quotient = Math.DivRem(fileSize, bufferSize, out long remainder);
                return (int) quotient + (remainder > 0 ? 1 : 0);
            }

            internal static OverlappedChunk[] ReadEx(string fileName, long bufferSize, long fileSize)
            {
                var chunks = Enumerable.Range(0, NumberOfChunks(bufferSize, fileSize))
                    .Select(i => new OverlappedChunk(BufferSize(bufferSize, fileSize, i)))
                    .ToArray();
                var waitHandles = Enumerable.Repeat<Func<EventWaitHandle>>(() => new ManualResetEvent(false), chunks.Length)
                    .Select(e => e())
                    .ToArray();
                var completions = Enumerable.Range(0, chunks.Length)
                    .Select<int, FileIOCompletionRoutine>(
                        i => (int dwErrorCode, int dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) =>
                        {
                            chunks[i].Win32Error = dwErrorCode;
                            chunks[i].BytesTransferred = dwNumberOfBytesTransferred;
                            waitHandles[i].Set();
                        }).ToArray();

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(waitHandles)));
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_READ | ShareMode.FILE_SHARE_DELETE, IntPtr.Zero,
                        CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_NO_BUFFERING | FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    for (var i = 0; i < chunks.Length; ++i)
                    {
                        var offset = i*bufferSize;
                        var overlapped = new NativeOverlapped()
                        {
                            OffsetHigh = (int) (offset >> 32),
                            OffsetLow = (int) (offset & 0xffffffff),
                            EventHandle = IntPtr.Zero
                        };
                        var buffer = chunks[i].Buffer;

                        if (!ReadFileEx(handle, buffer, buffer.Length, ref overlapped, completions[i]))
                        {
                            chunks[i].Win32Error = Marshal.GetLastWin32Error();
                            waitHandles[i].Set();
                        }
                    }
                }

                awaiterThread.Join();

                Array.ForEach(completions, c => GC.KeepAlive(c));

                return chunks;
            }

            internal static void WriteEx(string fileName, long bufferSize, long fileSize, OverlappedChunk[] chunks)
            {
                var waitHandles = Enumerable.Repeat<Func<EventWaitHandle>>(() => new ManualResetEvent(false), chunks.Length)
                    .Select(e => e())
                    .ToArray();
                var completions = Enumerable.Range(0, chunks.Length)
                    .Select<int, FileIOCompletionRoutine>(
                        i => (int dwErrorCode, int dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) =>
                        {
                            chunks[i].Win32Error = dwErrorCode;
                            chunks[i].BytesTransferred = dwNumberOfBytesTransferred;
                            waitHandles[i].Set();
                        }).ToArray();

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(waitHandles)));
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_WRITE, ShareMode.FILE_SHARE_NONE,
                        IntPtr.Zero, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_NO_BUFFERING | FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    var offsetHigh = (int) (fileSize >> 32);
                    if (SetFilePointer(handle, (int) (fileSize & 0xffffffff), out offsetHigh, MoveMethod.FILE_BEGIN) !=
                        fileSize || offsetHigh != (int) (fileSize >> 32) || !SetEndOfFile(handle))
                    {
                        chunks[0].Win32Error = Marshal.GetLastWin32Error();
                        throw new InvalidOperationException();
                    }

                    for (var i = 0; i < chunks.Length; ++i)
                    {
                        var offset = i*bufferSize;
                        var overlapped = new NativeOverlapped()
                        {
                            OffsetHigh = (int) (offset >> 32),
                            OffsetLow = (int) (offset & 0xffffffff),
                            EventHandle = IntPtr.Zero
                        };
                        var buffer = chunks[i].Buffer;

                        if (!WriteFileEx(handle, buffer, buffer.Length, ref overlapped, completions[i]))
                            chunks[i].Win32Error = Marshal.GetLastWin32Error();
                    }
                }

                awaiterThread.Join();

                Array.ForEach(completions, c => GC.KeepAlive(c));
            }
        }

        private static byte[] testData;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            testData = DokanOperationsFixture.InitBlockTestData(FILE_BUFFER_SIZE, 5*FILE_BUFFER_SIZE + 65536);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            testData = null;
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

        [TestMethod, TestCategory(TestCategories.Manual)]
        public void OpenRead_WithLargeFileUsingContext_Overlapped_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, context: testData);
            fixture.ExpectReadFileInChunks(path, testData, (int) FILE_BUFFER_SIZE, context: testData, synchronousIo: false);
#endif

            var outputs = NativeMethods.ReadEx(fixture.FileName.AsDriveBasedPath(), FILE_BUFFER_SIZE, testData.Length);

#if !LOGONLY
            for (var i = 0; i < outputs.Length; ++i)
            {
                Assert.AreEqual(0, outputs[i].Win32Error, $"Unexpected Win32 error in output {i}");
                Assert.AreEqual(NativeMethods.BufferSize(FILE_BUFFER_SIZE, testData.Length, i), outputs[i].BytesTransferred, $"Unexpected number of bytes read in output {i}");
                Assert.IsTrue(Enumerable.All(outputs[i].Buffer, b => b == (byte) i + 1), $"Unexpected data in output {i}");
            }

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Manual)]
        public void OpenWrite_WithLargeFileUsingContext_Overlapped_CallsApiCorrectly()
        {
            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.Open, context: testData);
            fixture.ExpectSetAllocationSize(path, testData.Length);
            fixture.ExpectSetEndOfFile(path, testData.Length);
            fixture.ExpectWriteFileInChunks(path, testData, (int) FILE_BUFFER_SIZE, context: testData, synchronousIo: false);
#endif

            var inputs = Enumerable.Range(0, NativeMethods.NumberOfChunks(FILE_BUFFER_SIZE, testData.Length))
                .Select(i => new NativeMethods.OverlappedChunk(Enumerable.Repeat((byte) (i + 1), NativeMethods.BufferSize(FILE_BUFFER_SIZE, testData.Length, i)).ToArray()))
                .ToArray();

            NativeMethods.WriteEx(fixture.FileName.AsDriveBasedPath(), FILE_BUFFER_SIZE, testData.Length, inputs);

#if !LOGONLY
            for (var i = 0; i < inputs.Length; ++i)
            {
                Assert.AreEqual(0, inputs[i].Win32Error, $"Unexpected Win32 error in input {i}");
                Assert.AreEqual(NativeMethods.BufferSize(FILE_BUFFER_SIZE, testData.Length, i), inputs[i].BytesTransferred, $"Unexpected number of bytes written in input {i}");
            }

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Manual)]
        [DeploymentItem("OverlappedTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\OverlappedTests.Configuration.xml", "ConfigRead", DataAccessMethod.Sequential)]
        public void OpenRead_WithVariableSizes_Overlapped_CallsApiCorrectly()
        {
            var bufferSize = int.Parse((string) TestContext.DataRow["BufferSize"]);
            var fileSize = int.Parse((string) TestContext.DataRow["FileSize"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var testData = DokanOperationsFixture.InitBlockTestData(bufferSize, fileSize);
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, ReadAccess, ReadShare, FileMode.Open, context: testData);
            fixture.ExpectReadFileInChunks(path, testData, bufferSize, context: testData, synchronousIo: false);
#endif

            var outputs = NativeMethods.ReadEx(fixture.FileName.AsDriveBasedPath(), bufferSize, testData.Length);

#if !LOGONLY
            for (var i = 0; i < outputs.Length; ++i)
            {
                Assert.AreEqual(0, outputs[i].Win32Error, $"Unexpected Win32 error in output {i} for BufferSize={bufferSize}, FileSize={fileSize}");
                Assert.AreEqual(NativeMethods.BufferSize(bufferSize, fileSize, i), outputs[i].BytesTransferred, $"Unexpected number of bytes read in output {i} for BufferSize={bufferSize}, FileSize={fileSize}");
                Assert.IsTrue(Enumerable.All(outputs[i].Buffer, b => b == (byte) i + 1), $"Unexpected data in output {i} for BufferSize={bufferSize}, FileSize={fileSize}");
            }

            fixture.Verify();
#endif
        }

        [TestMethod, TestCategory(TestCategories.Manual)]
        [DeploymentItem("OverlappedTests.Configuration.xml")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\OverlappedTests.Configuration.xml", "ConfigWrite", DataAccessMethod.Sequential)]
        public void OpenWrite_WithVariableSizes_Overlapped_CallsApiCorrectly()
        {
            var bufferSize = int.Parse((string) TestContext.DataRow["BufferSize"]);
            var fileSize = int.Parse((string) TestContext.DataRow["FileSize"]);

            var fixture = DokanOperationsFixture.Instance;

            var path = fixture.FileName.AsRootedPath();
            var testData = DokanOperationsFixture.InitBlockTestData(bufferSize, fileSize);
#if LOGONLY
            fixture.PermitAny();
#else
            fixture.ExpectCreateFile(path, WriteAccess, WriteShare, FileMode.Open, context: testData);
            fixture.ExpectSetAllocationSize(path, testData.Length);
            fixture.ExpectSetEndOfFile(path, testData.Length);
#if NETWORK_DRIVE
            fixture.ExpectWriteFileInChunks(path, testData, bufferSize, context: testData, synchronousIo: false);
#else
            fixture.ExpectWriteFileInChunks(path, testData, bufferSize, context: testData);
#endif
#endif

            var inputs = Enumerable.Range(0, NativeMethods.NumberOfChunks(bufferSize, fileSize))
                .Select(i => new NativeMethods.OverlappedChunk(Enumerable.Repeat((byte) (i + 1), NativeMethods.BufferSize(bufferSize, testData.Length, i)).ToArray()))
                .ToArray();

            NativeMethods.WriteEx(fixture.FileName.AsDriveBasedPath(), bufferSize, testData.Length, inputs);

#if !LOGONLY
            for (var i = 0; i < inputs.Length; ++i)
            {
                Assert.AreEqual(0, inputs[i].Win32Error, $"Unexpected Win32 error in input {i} for BufferSize={bufferSize}, FileSize={fileSize}");
                Assert.AreEqual(NativeMethods.BufferSize(bufferSize, fileSize, i), inputs[i].BytesTransferred, $"Unexpected number of bytes written in input {i} for BufferSize={bufferSize}, FileSize={fileSize}");
            }

            fixture.Verify();
#endif
        }
    }
}