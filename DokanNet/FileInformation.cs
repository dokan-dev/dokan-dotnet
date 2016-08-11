using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet
{
    /// <summary>
    /// Use to provide file informations to Dokan during operations
    /// <see cref="IDokanOperations.GetFileInformation"/>, <see cref="IDokanOperations.FindFiles"/>,
    /// <see cref="IDokanOperations.FindStreams"/> or <see cref="IDokanOperations.FindFilesWithPattern"/>
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct FileInformation
    {
        /// <summary>
        /// Name on the file or directory.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// <see cref="FileAttributes"/> for the file or directory.
        /// </summary>
        public FileAttributes Attributes { get; set; }
        /// <summary>
        /// File creation time.
        /// If equal to null, the value will not be set or the file has no creation time.
        /// </summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>
        /// File alst access time.
        /// If equal to null, the value will not be set or the file has no creation time.
        /// </summary>
        public DateTime? LastAccessTime { get; set; }
        /// <summary>
        /// File last write time.
        /// If equal to null, the value will not be set or the file has no last write time.
        /// </summary>
        public DateTime? LastWriteTime { get; set; }

        /// <summary>
        /// File length.
        /// </summary>
        public long Length { get; set; }
    }
}