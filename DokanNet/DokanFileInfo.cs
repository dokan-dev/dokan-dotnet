using DokanNet.Native;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static DokanNet.FormatProviders;

#pragma warning disable 649,169

namespace DokanNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public sealed class DokanFileInfo
    {
        private ulong _context;
        private readonly ulong _dokanContext;
        private readonly IntPtr _dokanOptions;
        private readonly uint _processId;

        [MarshalAs(UnmanagedType.U1)]
        private bool _isDirectory;

        [MarshalAs(UnmanagedType.U1)]
        private bool _deleteOnClose;

        [MarshalAs(UnmanagedType.U1)]
        private bool _pagingIo;

        [MarshalAs(UnmanagedType.U1)]
        private bool _synchronousIo;

        [MarshalAs(UnmanagedType.U1)]
        private bool _nocache;

        [MarshalAs(UnmanagedType.U1)]
        private bool _writeToEndOfFile;

        /// <summary>
        /// FileSystem can store anything here
        /// </summary>
        public object Context
        {
            get { return _context != 0 ? ((GCHandle)((IntPtr)_context)).Target : null; }
            set
            {
                if (_context != 0)
                {
                    ((GCHandle)((IntPtr)_context)).Free();
                    _context = 0;
                }
                if (value != null)
                {
                    _context = (ulong)(IntPtr)GCHandle.Alloc(value);
                }
            }
        }

        /// <summary>
        /// Process id for the thread that originally requested a 
        /// given I/O operation
        /// </summary>
        public int ProcessId
        {
            get { return (int)_processId; }
        }

        /// <summary>
        /// Requesting a directory file
        /// </summary>
        public bool IsDirectory
        {
            get { return _isDirectory; }
            set { _isDirectory = value; }
        }

        /// <summary>
        /// Delete on when "cleanup" is called
        /// </summary>
        public bool DeleteOnClose
        {
            get { return _deleteOnClose; }
            set { _deleteOnClose = value; }
        }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        public bool PagingIo
        {
            get { return _pagingIo; }
        }

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        public bool SynchronousIo
        {
            get { return _synchronousIo; }
        }

        public bool NoCache
        {
            get { return _nocache; }
        }

        /// <summary>
        /// If true, write to the current end of file instead 
        /// of Offset parameter.
        /// </summary>
        public bool WriteToEndOfFile
        {
            get { return _writeToEndOfFile; }
        }

        /// <summary>
        /// Get the handle to <see cref="WindowsIdentity"/>.
        /// This method needs be called in <see cref="IDokanOperations.CreateFile"/>, OpenDirectory or CreateDirectly
        /// callback.
        /// </summary>
        /// <returns>An <see cref="WindowsIdentity"/> with the access token, 
        /// -or- null if the operation was not successful</returns>
        public WindowsIdentity GetRequestor()
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

        /// <summary>
        /// Extends the time out of the current IO operation in driver.
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to extend with</param>
        /// <returns>If the operation was successful</returns>
        public bool TryResetTimeout(int milliseconds)
        {
            return NativeMethods.DokanResetTimeout((uint)milliseconds, this);
        }

        private DokanFileInfo()
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return DokanFormat($"{{{Context}, {DeleteOnClose}, {IsDirectory}, {NoCache}, {PagingIo}, #{ProcessId}, {SynchronousIo}, {WriteToEndOfFile}}}");
        }
    }
}

#pragma warning restore 649, 169