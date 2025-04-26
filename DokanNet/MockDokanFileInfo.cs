using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DokanNet.Native;
using Microsoft.Win32.SafeHandles;
using static DokanNet.FormatProviders;

#pragma warning disable 649,169

namespace DokanNet
{
    /// <summary>
    /// Mockable Dokan file information on the current operation.
    /// </summary>
    /// <remarks>
    /// Because <see cref="DokanFileInfo"/> cannot be instantiated in C#, it's very difficult to write
    /// test harnesses for unit testing.  This class implements the same public interface so it's
    /// possible to mock it, such that the implementor of IDokanOperations can essentially act like
    /// the dokany kernel driver and call into that implementation to verify correct behavior.  It
    /// also has support methods available to cause it (and the Dokan.Net library) to behave in certain
    /// ways useful for testing all potential returns, both success and failure.
    /// </remarks>
    public sealed class MockDokanFileInfo : IDokanFileInfo
    {
        /// <summary>
        /// A <see cref="DOKAN_OPTIONS"/> structure used to help the kernel notification functions work.
        /// </summary>
        private DOKAN_OPTIONS _dokanOptions = new DOKAN_OPTIONS();

        /// <summary>
        /// This must be set to a potentially valid path. Examples might be @"M:\\" or @"C:\\JunctionPoint".
        /// </summary>
        /// <remarks>The trailing backslash is not optional for drive letters, and must be omitted for paths.</remarks>
        public string MountPoint
        {
            get => _dokanOptions.MountPoint;
            set => _dokanOptions.MountPoint = value;
        }

        /// <summary>
        /// Gets or sets context that can be used to carry information between operation.
        /// The Context can carry an arbitrary type, like <c><see cref="System.IO.FileStream"/></c>, <c>struct</c>, <c>int</c>,
        /// or internal reference that will help the implementation understand the request context of the event.
        /// </summary>
        public object Context
        {
            get; set;
        }

        /// <summary>
        /// Process id for the thread that originally requested a given I/O
        /// operation.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a filename references a directory.
        /// Must be set in <see cref="IDokanOperations.CreateFile"/> if the file is to
        /// appear to be a folder.
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has to be deleted
        /// during the <see cref="IDokanOperations.Cleanup"/> event.
        /// </summary>
        public bool DeletePending { get; set; }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        public bool PagingIo { get; set; }

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        public bool SynchronousIo { get; set; }

        /// <summary>
        /// Read or write directly from data source without cache.
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// If <c>true</c>, write to the current end of file instead 
        /// of using the <c>Offset</c> parameter.
        /// </summary>
        public bool WriteToEndOfFile { get; set; }

        /// <summary>
        /// Set this to null if you want to test against token unavailability.
        /// </summary>
        /// <remarks>Initialized to the current process token, which is the only
        /// token a standard user account can get.</remarks>
        public WindowsIdentity WhatGetRequestorShouldReturn = WindowsIdentity.GetCurrent();

        /// <summary>
        /// This method needs to be called in <see cref="IDokanOperations.CreateFile"/> to
        /// determine what account and what privileges are available to the filesystem client.
        /// </summary>
        /// <returns>An <c><see cref="WindowsIdentity"/></c> with the access token, 
        /// -or- <c>null</c> if the operation was not successful.</returns>
        /// <remarks>This Mock implementation returns <see cref="WhatGetRequestorShouldReturn"/>.
        /// </remarks>
        public WindowsIdentity GetRequestor() => WhatGetRequestorShouldReturn;

        /// <summary>
        /// Set this to false if you want to test against TryResetTimeout failure
        /// </summary>
        public bool WhatTryResetTimeoutShouldReturn { get; set; }

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to extend with.</param>
        /// <returns>true if the operation was successful.</returns>
        /// <remarks>This Mock implementation returns <see cref="WhatTryResetTimeoutShouldReturn"/>.
        /// </remarks>
        public bool TryResetTimeout(int milliseconds) => WhatTryResetTimeoutShouldReturn;


        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return
                DokanFormat(
                    $"mock: {{{Context}, {DeletePending}, {IsDirectory}, {NoCache}, {PagingIo}, #{ProcessId}, {SynchronousIo}, {WriteToEndOfFile}}}");
        }
    }
}

#pragma warning restore 649, 169