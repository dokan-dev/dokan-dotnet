using System.Security.Principal;

namespace DokanNet
{
    /// <summary>
    /// %Dokan file information interface.
    /// </summary>
    /// <remarks>
    /// This interface can be inherited in order to testunit user <see cref="IDokanOperations"/> implementation.
    /// </remarks>
    public interface IDokanFileInfo
    {
        /// <summary>
        /// Gets or sets context that can be used to carry information between operation.
        /// The Context can carry whatever type like <c><see cref="System.IO.FileStream"/></c>, <c>struct</c>, <c>int</c>,
        /// or internal reference that will help the implementation understand the request context of the event.
        /// </summary>
        object Context { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has to be delete
        /// during the <see cref="IDokanOperations.Cleanup"/> event.
        /// </summary>
        bool DeleteOnClose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it requesting a directory
        /// file. Must be set in <see cref="IDokanOperations.CreateFile"/> if
        /// the file appear to be a folder.
        /// </summary>
        bool IsDirectory { get; set; }

        /// <summary>
        /// Read or write directly from data source without cache.
        /// </summary>
        bool NoCache { get; }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        bool PagingIo { get; }

        /// <summary>
        /// Process id for the thread that originally requested a given I/O
        /// operation.
        /// </summary>
        int ProcessId { get; }

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        bool SynchronousIo { get; }

        /// <summary>
        /// If <c>true</c>, write to the current end of file instead 
        /// of using the <c>Offset</c> parameter.
        /// </summary>
        bool WriteToEndOfFile { get; }

        /// <summary>
        /// This method needs to be called in <see cref="IDokanOperations.CreateFile"/>.
        /// </summary>
        /// <returns>An <c><see cref="WindowsIdentity"/></c> with the access token, 
        /// -or- <c>null</c> if the operation was not successful.</returns>
        WindowsIdentity GetRequestor();

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to extend with.</param>
        /// <returns>If the operation was successful.</returns>
        bool TryResetTimeout(int milliseconds);
    }
}