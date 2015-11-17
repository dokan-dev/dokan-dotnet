using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal struct WIN32_FIND_STREAM_DATA
    {
        public long StreamSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cStreamName;
    }
}
