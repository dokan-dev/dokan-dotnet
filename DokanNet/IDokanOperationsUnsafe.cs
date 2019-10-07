using System;

namespace DokanNet
{
    /// <summary>
    /// This is a sub-interface of <see cref="IDokanOperations"/> that can optionally be implemented
    /// to get access to the raw, unmanaged buffers for ReadFile() and WriteFile() for performance optimization.
    /// Marshalling the unmanaged buffers to and from byte[] arrays for every call of these APIs incurs an extra copy
    /// that can be avoided by reading from or writing directly to the unmanaged buffers.
    /// 
    /// Implementation of this interface is optional. If it is implemented, the overloads of
    /// Read/WriteFile(IntPtr, length) will be called instead of Read/WriteFile(byte[]). The caller can fill or read
    /// from the unmanaged API with Marshal.Copy, Buffer.MemoryCopy or similar.
    /// </summary>
    public interface IDokanOperationsUnsafe : IDokanOperations
    {
        /// <summary>
        /// ReadFile callback on the file previously opened in <see cref="CreateFile"/>.
        /// It can be called by different thread at the same time,
        /// therefore the read has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="buffer">Read buffer that has to be fill with the read result.</param>
        /// <param name="bufferLength">The size of 'buffer' in bytes.
        /// The buffer size depends of the read size requested by the kernel.</param>
        /// <param name="bytesRead">Total number of bytes that has been read.</param>
        /// <param name="offset">Offset from where the read has to be proceed.</param>
        /// <param name="info">An <see cref="IDokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="WriteFile"/>
        NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info);

        /// <summary>
        /// WriteFile callback on the file previously opened in <see cref="CreateFile"/>
        /// It can be called by different thread at the same time,
        /// therefore the write/context has to be thread safe.
        /// </summary>
        /// <param name="fileName">File path requested by the Kernel on the FileSystem.</param>
        /// <param name="buffer">Data that has to be written.</param>
        /// <param name="bufferLength">The size of 'buffer' in bytes.</param>
        /// <param name="bytesWritten">Total number of bytes that has been write.</param>
        /// <param name="offset">Offset from where the write has to be proceed.</param>
        /// <param name="info">An <see cref="IDokanFileInfo"/> with information about the file or directory.</param>
        /// <returns><see cref="NtStatus"/> or <see cref="DokanResult"/> appropriate to the request result.</returns>
        /// <seealso cref="ReadFile"/>
        NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info);
    }
}
