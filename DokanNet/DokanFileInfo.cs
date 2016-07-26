using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DokanNet.Native;
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

        [MarshalAs(UnmanagedType.U1)] private bool _isDirectory;

        [MarshalAs(UnmanagedType.U1)] private bool _deleteOnClose;

        [MarshalAs(UnmanagedType.U1)] private bool _pagingIo;

        [MarshalAs(UnmanagedType.U1)] private bool _synchronousIo;

        [MarshalAs(UnmanagedType.U1)] private bool _nocache;

        [MarshalAs(UnmanagedType.U1)] private bool _writeToEndOfFile;

        /// <summary>
        /// Context that can be used to carry information between operation.
        /// The Context can carry whatever type like <see cref="FileStream"/>, struct, <see cref="int"/>,
        /// internal reference that will help the implementation understand the request context of the event.
        /// </summary>
        public object Context
        {
            get { return _context != 0 ? ((GCHandle) ((IntPtr) _context)).Target : null; }
            set
            {
                if (_context != 0)
                {
                    ((GCHandle) ((IntPtr) _context)).Free();
                    _context = 0;
                }
                if (value != null)
                {
                    _context = (ulong) (IntPtr) GCHandle.Alloc(value);
                }
            }
        }

        /// <summary>
        /// Process id for the thread that originally requested a 
        /// given I/O operation
        /// </summary>
        public int ProcessId => (int) _processId;

        /// <summary>
        /// Requesting a directory file
        /// IsDirectory has to be set in CreateFile is the file appear to be a folder.
        /// </summary>
        public bool IsDirectory
        {
            get { return _isDirectory; }
            set { _isDirectory = value; }
        }

        /// <summary>
        /// Flag if the file has to be delete during <see cref="IDokanOperations.Cleanup"/> event.
        /// </summary>
        public bool DeleteOnClose
        {
            get { return _deleteOnClose; }
            set { _deleteOnClose = value; }
        }

        /// <summary>
        /// Read or write is paging IO.
        /// </summary>
        public bool PagingIo => _pagingIo;

        /// <summary>
        /// Read or write is synchronous IO.
        /// </summary>
        public bool SynchronousIo => _synchronousIo;

        /// <summary>
        /// Read or write directly from data source without cache
        /// </summary>
        public bool NoCache => _nocache;

        /// <summary>
        /// If true, write to the current end of file instead 
        /// of Offset parameter.
        /// </summary>
        public bool WriteToEndOfFile => _writeToEndOfFile;

        /// <summary>
        /// This method needs be called in <see cref="IDokanOperations.CreateFile"/>, OpenDirectory or CreateDirectly
        /// callback.
        /// </summary>
        /// <returns>An <see cref="WindowsIdentity"/> with the access token, -or- null if the operation was not successful</returns>
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
            return NativeMethods.DokanResetTimeout((uint) milliseconds, this);
        }

        /// <summary>
        /// Dokan informations on the current operation
        /// </summary>
        private DokanFileInfo()
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return
                DokanFormat(
                    $"{{{Context}, {DeleteOnClose}, {IsDirectory}, {NoCache}, {PagingIo}, #{ProcessId}, {SynchronousIo}, {WriteToEndOfFile}}}");
        }
    }
}

#pragma warning restore 649, 169