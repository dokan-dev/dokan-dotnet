using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet
{
    /// <summary>
    /// Used to provide file information to %Dokan during operations by
    ///  - <see cref="IDokanOperations.GetFileInformation"/>
    ///  - <see cref="IDokanOperations.FindFiles"/>
    ///  - <see cref="IDokanOperations.FindStreams"/> 
    ///  - <see cref="IDokanOperations.FindFilesWithPattern"/>.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{FileName}, {Length}, {CreationTime}, {LastWriteTime}, {LastAccessTime}, {Attributes}")]
    public struct FileInformation
    {
        /// <summary>
        /// Gets or sets the name of the file or directory.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the <c><see cref="FileAttributes"/></c> for the file or directory.
        /// </summary>
        public FileAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no creation time.
        /// </summary>
        public DateTime? CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the last access time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no last access time.
        /// </summary>
        public DateTime? LastAccessTime { get; set; }

        /// <summary>
        /// Gets or sets the last write time of the file or directory.
        /// If equal to <c>null</c>, the value will not be set or the file has no last write time.
        /// </summary>
        public DateTime? LastWriteTime { get; set; }

        /// <summary>
        /// Gets or sets the length of the file.
        /// </summary>
        public long Length { get; set; }
    }
}