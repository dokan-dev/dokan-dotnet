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
    /// %Dokan file information on the current operation.
    /// </summary>
    /// <remarks>
    /// This class cannot be instantiated in C#, it is created by the kernel %Dokan driver.
    /// This is the same structure as <c>_DOKAN_FILE_INFO</c> (dokan.h) in the C++ version of Dokan.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public sealed class DokanFileInfo
    {
        private ulong _context;

        /// <summary>
        /// Used internally, never modify.
        /// </summary>
        private readonly ulong _dokanContext;

        /// <summary>
        /// A pointer to the <see cref="DOKAN_OPTIONS"/> which was passed to <see cref="DokanNet.Native.NativeMethods.DokanMain"/>.
        /// </summary>
        private readonly IntPtr _dokanOptions;

        private readonly uint _processId;

        [MarshalAs(UnmanagedType.U1)] private bool _isDirectory;

        [MarshalAs(UnmanagedType.U1)] private bool _deleteOnClose;

        [MarshalAs(UnmanagedType.U1)] private bool _pagingIo;

        [MarshalAs(UnmanagedType.U1)] private bool _synchronousIo;

        [MarshalAs(UnmanagedType.U1)] private bool _noCache;

        [MarshalAs(UnmanagedType.U1)] private bool _writeToEndOfFile;

        /// <summary>
        /// Determine if this instance was constructed for testing, or by the kernel.
        /// </summary>
        public bool Testing { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="DokanFileInfo"/> class from being created. 
        /// The class is created by the %Dokan kernel driver.
        /// </summary>
        private DokanFileInfo()
        {
        }

        /// <summary>
        /// Construct an instance of this class for testing purposes. (You do not need to construct instances unless you are testing.)
        /// </summary>
        /// <param name="testing"></param>
        public DokanFileInfo(bool testing = true)
        {
            Testing = true;
        }

        /// <summary>
        /// Gets or sets context that can be used to carry information between operation.
        /// The Context can carry whatever type like <c><see cref="System.IO.FileStream"/></c>, <c>struct</c>, <c>int</c>,
        /// or internal reference that will help the implementation understand the request context of the event.
        /// </summary>
        public object Context
        {
            get => _context != 0
                ? ((GCHandle)(IntPtr)_context).Target
                : null;
            set
            {
                if (_context != 0)
                {
                    ((GCHandle)(IntPtr)_context).Free();
                    _context = 0;
                }

                if (value != null)
                {
                    _context = (ulong)(IntPtr)GCHandle.Alloc(value);
                }
            }
        }

        /// <summary>
        /// Process id for the thread that originally requested a given I/O
        /// operation.
        /// </summary>
        public int ProcessId => (int)_processId;

        /// <summary>
        /// Gets or sets a value indicating whether it requesting a directory
        /// file. Must be set in <see cref="IDokanOperations.CreateFile"/> if
        /// the file appear to be a folder.
        /// </summary>
        public bool IsDirectory
        {
            get => _isDirectory;
            set => _isDirectory = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the file has to be delete
        /// during the <see cref="IDokanOperations.Cleanup"/> event.
        /// </summary>
        public bool DeleteOnClose
        {
            get => _deleteOnClose;
            set => _deleteOnClose = value;
        }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        public bool PagingIo
        {
            get => _pagingIo;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _pagingIo = value;
            }
        }

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        public bool SynchronousIo
        {
            get => _synchronousIo;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _synchronousIo = value;
            }
        }

        /// <summary>
        /// Read or write directly from data source without cache.
        /// </summary>
        public bool NoCache
        {
            get => _noCache;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _noCache = value;
            }
        }

        /// <summary>
        /// If <c>true</c>, write to the current end of file instead 
        /// of using the <c>Offset</c> parameter.
        /// </summary>
        public bool WriteToEndOfFile
        {
            get => _writeToEndOfFile;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _writeToEndOfFile = value;
            }
        }

        /// <summary>
        /// This method needs to be called in <see cref="IDokanOperations.CreateFile"/>.
        /// </summary>
        /// <returns>An <c><see cref="WindowsIdentity"/></c> with the access token, 
        /// -or- <c>null</c> if the operation was not successful.</returns>
        public WindowsIdentity GetRequestor()
        {
            if (!Testing)
            {

                SafeFileHandle sfh = null;
                try
                {
                    using (sfh = new SafeFileHandle(NativeMethods.DokanOpenRequestorToken(this), true))
                    {
                        return new WindowsIdentity(sfh.DangerousGetHandle());
                    }
                }
                catch
                {
                    try
                    {
                        return new WindowsIdentity(NativeMethods.DokanOpenRequestorToken(this));
                    }
                    catch
                    {
                        return null;
                    }
                }
                finally
                {
                    if (sfh != null)
                    {
                        sfh.Dispose();
                    }
                }
            }
            else
            {
                return IdentityOfRequestor;
            }
        }

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to extend with.</param>
        /// <returns>If the operation was successful.</returns>
        public bool TryResetTimeout(int milliseconds)
        {
            if (!Testing)
                return NativeMethods.DokanResetTimeout((uint)milliseconds, this);
            return ResetTimeoutReturn;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return
                DokanFormat(
                    $"{{{Context}, {DeleteOnClose}, {IsDirectory}, {NoCache}, {PagingIo}, #{ProcessId}, {SynchronousIo}, {WriteToEndOfFile}}}");
        }

        #region Test setup members
        private WindowsIdentity _requestor;

        /// <summary>
        /// For testing, set the WindowsIdentity returned by <see cref="GetRequestor"/>.
        /// </summary>
        /// <remarks>GetRequestor() can return null if the <see cref="WindowsIdentity"/> constructor fails.
        /// As such, this can be left or explicitly set null.</remarks>
        public WindowsIdentity IdentityOfRequestor
        {
            private get => _requestor;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _requestor = value;
            }
        }

        private bool _resetTimeoutReturn;

        /// <summary>
        /// For testing, set the return value of <see cref="TryResetTimeout"/>.
        /// </summary>
        public bool ResetTimeoutReturn
        {
            private get => _resetTimeoutReturn;
            set
            {
                if (!Testing)
                    throw new InvalidOperationException();
                _resetTimeoutReturn = true;
            }
        }
        #endregion
    }
}

#pragma warning restore 649, 169