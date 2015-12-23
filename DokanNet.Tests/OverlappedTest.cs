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
            private static extern bool ReadFileEx(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            [DllImport(KERNEL_32_DLL, SetLastError = true)]
            private static extern bool WriteFileEx(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, ref NativeOverlapped lpOverlapped, FileIOCompletionRoutine lpCompletionRoutine);

            private delegate void FileIOCompletionRoutine(uint dwErrorCode, uint dwNumberOfBytesTransfered, ref NativeOverlapped lpOverlapped);

            [DebuggerDisplay("{DebuggerDisplay(),nq}")]
            private struct OverlappedRecord
            {
                public byte[] Buffer { get; }

                public uint BytesTransferred { get; set; }

                public NativeOverlapped? Overlapped { get; set; }

                public ManualResetEvent ResetEvent { get; }

                public FileIOCompletionRoutine Completion { get; set; }

                public int Win32Error { get; set; }

                public OverlappedRecord(uint count) : this(new byte[count])
                {
                }

                public OverlappedRecord(byte[] buffer)
                {
                    Buffer = buffer;
                    BytesTransferred = 0;
                    Overlapped = null;
                    ResetEvent = new ManualResetEvent(false);
                    Completion = null;
                    Win32Error = 0;
                }

                private string DebuggerDisplay() => $"{nameof(OverlappedRecord)} Buffer={Buffer?.Length ?? -1} BytesTransferred={BytesTransferred}";
            }

            internal static SafeFileHandle CreateFile(string fileName) => CreateFile(fileName, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_NONE, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            internal static uint BufferSize(int segment) => (uint)Math.Min(FILE_BUFFER_SIZE, testData.Length - segment * FILE_BUFFER_SIZE);

            private static void ReadEx(SafeFileHandle handle, long offset, uint count, OverlappedRecord output)
            {
                var overlapped = output.Overlapped.Value;
                output.Completion = (uint dwErrorCode, uint dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) => {
                    output.BytesTransferred = dwNumberOfBytesTransferred;
                    output.ResetEvent.Set();
                };

                if (!ReadFileEx(handle, output.Buffer, count, ref overlapped, output.Completion))
                    output.Win32Error = Marshal.GetLastWin32Error();
            }

            internal static byte[][] Read(string fileName, int segments)
            {
                var slots = new OverlappedRecord[segments];
                for (int i = 0; i < segments; ++i)
                {
                    var offset = i * FILE_BUFFER_SIZE;
                    slots[i] = new OverlappedRecord(BufferSize(i)) { Overlapped = new NativeOverlapped() { OffsetHigh = (int)(offset >> 32), OffsetLow = (int)(offset & 0xffffffff), EventHandle = IntPtr.Zero } };
                }

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(slots.Select(s => s.ResetEvent).ToArray())));
                //awaiterThread.SetApartmentState(ApartmentState.MTA);
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_READ, ShareMode.FILE_SHARE_READ, IntPtr.Zero, CreationDisposition.OPEN_EXISTING, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    for (int i = 0; i < segments; ++i)
                        ReadEx(handle, i * FILE_BUFFER_SIZE, BufferSize(i), slots[i]);
                }

                awaiterThread.Join();

                return slots.Select(s => s.Buffer).ToArray();
            }

            private static void WriteEx(SafeFileHandle handle, long offset, uint count, OverlappedRecord input)
            {
                var overlapped = input.Overlapped.Value;
                input.Completion = (uint dwErrorCode, uint dwNumberOfBytesTransferred, ref NativeOverlapped lpOverlapped) => {
                    input.BytesTransferred = dwNumberOfBytesTransferred;
                    input.ResetEvent.Set();
                };

                if (!WriteFileEx(handle, input.Buffer, count, ref overlapped, input.Completion))
                    input.Win32Error = Marshal.GetLastWin32Error();
            }

            internal static void Write(string fileName, int segments, byte[][] inputs)
            {
                var slots = new OverlappedRecord[segments];
                for (int i = 0; i < segments; ++i)
                {
                    var offset = i * FILE_BUFFER_SIZE;
                    slots[i] = new OverlappedRecord(inputs[i]) { Overlapped = new NativeOverlapped() { OffsetHigh = (int)(offset >> 32), OffsetLow = (int)(offset & 0xffffffff), EventHandle = IntPtr.Zero } };
                }

                var awaiterThread = new Thread(new ThreadStart(() => WaitHandle.WaitAll(slots.Select(s => s.ResetEvent).ToArray())));
                awaiterThread.Start();

                using (var handle = CreateFile(fileName, DesiredAccess.GENERIC_WRITE, ShareMode.FILE_SHARE_NONE, IntPtr.Zero, CreationDisposition.OPEN_ALWAYS, FlagsAndAttributes.FILE_FLAG_OVERLAPPED, IntPtr.Zero))
                {
                    for (int i = 0; i < segments; ++i)
                        WriteEx(handle, i * FILE_BUFFER_SIZE, BufferSize(i), slots[i]);
                }

                awaiterThread.Join();
            }
        }

        private static byte[] testData;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            testData = new byte[5 * FILE_BUFFER_SIZE + 65536];
            for (int i = 0; i < testData.Length; ++i)
                testData[i] = (byte)(i / FILE_BUFFER_SIZE + 1);
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

            var segments = testData.Length / (int)FILE_BUFFER_SIZE + 1;

            var results = Native.Read(DokanOperationsFixture.FileName.AsDriveBasedPath(), segments);

#if !LOGONLY
            for (int i = 0; i < segments; ++i)
                Assert.IsTrue(Enumerable.All(results[i], b => b == (byte)i + 1), $"Unexpected data in segment {i}");

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

            var segments = testData.Length / (int)FILE_BUFFER_SIZE + 1;
            var inputs = (byte[][])Array.CreateInstance(typeof(byte[]), segments);
            for (int i = 0; i < segments; ++i)
                inputs[i] = Enumerable.Repeat((byte)(i + 1), (int)Native.BufferSize(i)).ToArray();

            Native.Write(DokanOperationsFixture.FileName.AsDriveBasedPath(), segments, inputs);

#if !LOGONLY
            fixture.VerifyAll();
#endif
        }
    }
}