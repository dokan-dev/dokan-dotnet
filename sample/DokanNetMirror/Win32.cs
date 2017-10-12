using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DokanNetMirror
{
    public class Win32
    {
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);
    }
}