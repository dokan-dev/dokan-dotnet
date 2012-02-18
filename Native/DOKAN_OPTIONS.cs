using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal struct DOKAN_OPTIONS
    {
        public ushort Version;
        public ushort ThreadCount; // number of threads to be used
        public uint Options;
        public ulong GlobalContext;
        [MarshalAs(UnmanagedType.LPWStr)] public string MountPoint;
    }
}