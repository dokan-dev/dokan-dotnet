using System;
using System.Collections.Concurrent;
using System.Threading;
using DokanNet.Logging;

namespace DokanNet
{
    /// <summary>
    /// Simple buffer pool for buffers used by <see cref="IDokanOperations.ReadFile"/> and
    /// <see cref="IDokanOperations.WriteFile"/> to avoid excessive Gen2 garbage collections due to large buffer
    /// allocation on the large object heap (LOH).
    /// 
    /// This pool is a bit different than say System.Buffers.ArrayPool(T) in that <see cref="RentBuffer" /> only returns
    /// exact size buffers. This is because the Read/WriteFile APIs only take a byte[] array as a parameter, not
    /// buffer and length. As such, it would be back compat breaking to return buffers that are bigger than the
    /// data length. To limit the amount of memory consumption, we only buffer sizes that are powers of 2 because
    /// common buffer sizes are typically that. There isn't anything preventing pooling buffers of any size if
    /// we find that there's another common buffer size in use. Only pool buffers 1MB or smaller and only
    /// up to 10 buffers of each size for further memory capping.
    /// </summary>
    internal class BufferPool
    {
        // An empty array does not contain data and can be statically cached.
        private static readonly byte[] _emptyArray = new byte[0];

        private readonly uint _maxBuffersPerPool; // Max buffers to cache per buffer size.

        // The pools for each buffer size. Index is log2(size).
        private readonly ConcurrentBag<byte[]>[] _pools;

        // Number of bytes served out over the pool's lifetime.
        private long _servedBytes;

        /// <summary>
        /// Constructs a new buffer pool.
        /// </summary>
        /// <param name="maxBufferSize">The max size (bytes) buffer that will be cached. </param>
        /// <param name="maxBuffersPerBufferSize">Maximum number of buffers cached per buffer size.</param>
        public BufferPool(uint maxBufferSize = 1024 * 1024, uint maxBuffersPerBufferSize = 10)
        {
            _maxBuffersPerPool = maxBuffersPerBufferSize;
            int log2 = GetPoolIndex(maxBufferSize);
            if (log2 == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBufferSize), maxBufferSize, "Must be a power of 2.");
            }

            // Create empty pools for each size.
            _pools = new ConcurrentBag<byte[]>[log2 + 1];
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new ConcurrentBag<byte[]>();
            }
        }

        /// <summary>
        /// Default, process-wide buffer pool instance.
        /// </summary>
        public static BufferPool Default { get; } = new BufferPool();

        /// <summary>
        /// Clears the buffer pool by releasing all buffers.
        /// </summary>
        public void Clear()
        {
            _servedBytes = 0;
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new ConcurrentBag<byte[]>(); // There's no clear method on ConcurrentBag...
            }
        }

        /// <summary>
        /// Number of bytes served over the pool's lifetime.
        /// </summary>
        public long ServedBytes => Interlocked.Read(ref _servedBytes);

        /// <summary>
        /// Gets a buffer from the buffer pool of the exact specified size.
        /// If the size if not a power of 2, a buffer is still returned, but it is not poolable.
        /// </summary>
        /// <param name="bufferSize">The size of buffer requested.</param>
        /// <param name="logger">Logger for debug spew about what the buffer pool did.</param>
        /// <returns>The byte[] buffer.</returns>
        public byte[] RentBuffer(uint bufferSize, ILogger logger)
        {
            if (bufferSize == 0)
            {
                return _emptyArray; // byte[0] is statically cached.
            }

            Interlocked.Add(ref _servedBytes, bufferSize);

            // If the number is not a power of 2, we have nothing to offer.
            int poolIndex = GetPoolIndex(bufferSize);
            if (poolIndex == -1 || poolIndex >= _pools.Length)
            {
                logger.Debug($"Buffer size {bufferSize} not power of 2 or too large, returning unpooled buffer.");
                return new byte[bufferSize];
            }

            // Try getting a buffer from the pool. If it's empty, make a new buffer.
            ConcurrentBag<byte[]> pool = _pools[poolIndex];
            if (pool.TryTake(out byte[] buffer))
            {
                logger.Debug($"Using pooled buffer from pool {poolIndex}.");
            }
            else
            {
                logger.Debug($"Pool {poolIndex} empty, creating new buffer.");
                buffer = new byte[bufferSize];
            }

            return buffer;
        }

        /// <summary>
        /// Returns a previously rented buffer to the buffer pool.
        /// If the buffer size is not an exact power of 2, the buffer is ignored.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="logger">Logger for debug spew about what the buffer pool did.</param>
        public void ReturnBuffer(byte[] buffer, ILogger logger)
        {
            if (buffer.Length == 0)
            {
                return; // Do nothing - _emptyArray caches this statically.
            }

            // If the buffer is a power of 2 and below max pooled size, return it to the appropriate pool.
            int poolIndex = GetPoolIndex((uint)buffer.Length);
            if (poolIndex >= 0 && poolIndex < _pools.Length)
            {
                // Check if the pool is full. This is racy if multiple threads return buffers concurrently,
                // but it's close enough - we'd just get a couple extra buffers in the pool at worst.
                ConcurrentBag<byte[]> pool = _pools[poolIndex];
                if (pool.Count < _maxBuffersPerPool)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    pool.Add(buffer);
                    logger.Debug($"Returned buffer to pool {poolIndex}.");
                }
                else
                {
                    logger.Debug($"Pool {poolIndex} is full, discarding buffer.");
                }
            }
            else
            {
                logger.Debug($"{poolIndex} (size {buffer.Length}) outside pool range, discarding buffer.");
            }
        }

        /// <summary>
        /// Computes the pool index given a buffer size. The pool index is log2(size),
        /// if size is a power of 2. If size is not a power of 2, -1 is returned (invalid pool index).
        /// </summary>
        /// <param name="bufferSize">Buffer size in bytes.</param>
        /// <returns>The pool index, log2(number), or -1 if bufferSize is not a power of 2.</returns>
        private static int GetPoolIndex(uint bufferSize)
        {
            double log2 = Math.Log(bufferSize, 2);
            int log2AsInt = (int)log2;

            // If they are not equal, the number is not a power of 2.
            //
            if (log2 != log2AsInt)
            {
                return -1;
            }

            return log2AsInt;
        }
    }
}
