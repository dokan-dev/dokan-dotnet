using System.Runtime.InteropServices;

namespace DokanNet.Native
{
    /// <summary>
    /// Contains information about the stream found by the FindFirstStreamW or FindNextStreamW function.
    /// 
    /// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365741(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal struct WIN32_FIND_STREAM_DATA
    {
        /// <summary>
        /// A LARGE_INTEGER value that specifies the size of the stream, in bytes.
        /// </summary>
        public long StreamSize;
        /// <summary>
        /// The name of the stream. The string name format is ":streamname:$streamtype".
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cStreamName;
    }
}
