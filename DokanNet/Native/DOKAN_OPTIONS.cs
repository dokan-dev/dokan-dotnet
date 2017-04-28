using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// Dokan mount options used to describe dokan device behaviour
    /// </summary>
    /// <see cref="NativeMethods.DokanMain"/>
    /// <remarks>This is the same structure as <c>PDOKAN_OPTIONS</c> (dokan.h) in the C++ version of Dokan.</remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    internal struct DOKAN_OPTIONS
    {
        /// <summary>
        /// Version of the dokan features requested (version "123" is equal to Dokan version 1.2.3).
        /// </summary>
        public ushort Version;

        /// <summary>
        /// Number of threads to be used internally by Dokan library. More thread will handle more event at the same time.
        /// </summary>
        public ushort ThreadCount;

        /// <summary>
        /// Features enable for the mount. See <see cref="DokanOptions"/>.
        /// </summary>
        public uint Options;

        /// <summary>
        /// FileSystem can store anything here.
        /// </summary>
        public ulong GlobalContext;

        /// <summary>
        /// Mount point.
        /// Can be <c>M:\\</c>(drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string MountPoint;

        /// <summary>
        /// UNC name used for network volume.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string UNCName;

        /// <summary>
        /// Max timeout in milliseconds of each request before Dokan give up.
        /// </summary>
        public uint Timeout;

        /// <summary>
        /// Allocation Unit Size of the volume. This will behave on the file size.
        /// </summary>
        public uint AllocationUnitSize;

        /// <summary>
        /// Sector Size of the volume. This will behave on the file size.
        /// </summary>
        public uint SectorSize;
    }
}