using System;
using DokanNet.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    /// <summary>
    /// Tests for <see cref="IDokanOperationsUnsafe"/>. This is leveraging the same set of tests as
    /// <see cref="FileInfoTests"/> by deriving from that class, but by calling
    /// DokanOperationsFixture.InitInstance(unsafeOperations: true) from setup to send all
    /// Read/WriteFile calls through the Read/WriteFile(IntPtr buffer, uint bufferLength) overloads instead
    /// of the Read/WriteFile(byte[] buffer) overloads.
    /// </summary>
    [TestClass]
    public sealed class FileInfoTestsUnsafe : FileInfoTests
    {
        [ClassInitialize]
        public static new void ClassInitialize(TestContext context)
        {
            // Just invoke the base class init.
            FileInfoTests.ClassInitialize(context);
        }

        [ClassCleanup]
        public static new void ClassCleanup()
        {
            // Just invoke the base class cleanup.
            FileInfoTests.ClassCleanup();
        }

        [TestInitialize]
        public override void Initialize()
        {
            // Clear the buffer pool (so we can validate in Cleanup()) and init test fixture.
            BufferPool.Default.Clear();
            DokanOperationsFixture.InitInstance(TestContext.TestName, unsafeOperations: true);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            // Verify no buffers were pooled and then call base class Cleanup().
            Assert.AreEqual(0, BufferPool.Default.ServedBytes, "Expected zero buffer pooling activity when using IDokanOperationsUnsafe.");
            BufferPool.Default.Clear();
            base.Cleanup();
        }
    }
}
