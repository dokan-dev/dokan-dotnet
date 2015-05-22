using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet
{
    [StructLayout(LayoutKind.Auto)]
    public struct FileInformation
    {
        public string FileName { get; set; }
        public FileAttributes Attributes{ get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Length { get; set; }
    }
}