using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// Options used by <see cref="NativeMethods.DokanMain"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal struct DOKAN_OPTIONS
    {
        /// <summary>
        /// Supported Dokan Version, ex. "530" (Dokan ver 0.5.3).
        /// </summary>
        public ushort Version;

        /// <summary>
        /// Number of threads to be used internally by Dokan library
        /// </summary>
        public ushort ThreadCount;

        /// <summary>
        /// Combination of DOKAN_OPTIONS_*
        /// </summary>
        public uint Options;

        /// <summary>
        /// FileSystem can store anything here.
        /// </summary>
        public ulong GlobalContext;

        /// <summary>
        /// Mount point.
        /// Can be "M:\" (drive letter) or "C:\mount\dokan" (path in NTFS)
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string MountPoint;

        /// <summary>
        /// UNC provider name
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string UNCName;

        /// <summary>
        /// Timeout in milliseconds.
        /// </summary>
        public uint Timeout;

        /// <summary>
        /// Device allocation size.
        /// </summary>
        public uint AllocationUnitSize;

        /// <summary>
        /// Device sector size
        /// </summary>
        public uint SectorSize;
    }
}