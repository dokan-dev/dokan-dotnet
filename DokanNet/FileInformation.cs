using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet
{
    /// <summary>
    /// Information about a file or directory
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
        /// When the file or directory was created.
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// When the file or directory was last accessed.
        /// </summary>
        public DateTime LastAccessTime { get; set; }
        /// <summary>
        /// When the file or directory was last updated.
        /// </summary>
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }
    }
}