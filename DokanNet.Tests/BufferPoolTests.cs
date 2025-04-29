using System;
using DokanNet.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DokanNet.Tests
{
    /// <summary>
    /// Tests for <see cref="BufferPool"/>.
    /// </summary>
    [TestClass]
    public sealed class BufferPoolTests
    {
        /// <summary>
        /// Rudimentary test for <see cref="BufferPool"/>.
        /// </summary>
        [TestMethod, TestCategory(TestCategories.Success)]
        public void BufferPoolBasicTest()
        {
            BufferPool pool = new BufferPool();
            ILogger logger = new TraceLogger();

            // Verify buffer is pooled.
            const int MB = 1024 * 1024;
            byte[] buffer = pool.RentBuffer(MB, logger);
            pool.ReturnBuffer(buffer, logger);

            byte[] buffer2 = pool.RentBuffer(MB, logger);
            Assert.AreSame(buffer, buffer2, "Expected recycling of 1 MB buffer.");

            // Verify buffer that buffer not power of 2 is not pooled.
            buffer = pool.RentBuffer(MB - 1, logger);
            pool.ReturnBuffer(buffer, logger);

            buffer2 = pool.RentBuffer(MB - 1, logger);
            Assert.AreNotSame(buffer, buffer2, "Did not expect recycling of 1 MB - 1 byte buffer.");

            // Run through a bunch of random buffer sizes and make sure we always get a buffer of the right size.
            int seed = Environment.TickCount;
            Console.WriteLine($"Random seed: {seed}");
            Random random = new Random(seed);

            for (int i = 0; i < 1000; i++)
            {
                int size = random.Next(0, 2 * MB);
                buffer = pool.RentBuffer(size, logger);
                Assert.AreEqual(size, buffer.Length, "Wrong buffer size.");
                pool.ReturnBuffer(buffer, logger);
            }
        }
    }
}