using System;
using System.Runtime.InteropServices;

namespace DokanNet.Native;

/// <summary>
/// Contains information about the stream found by the <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364424(v=vs.85).aspx">FindFirstStreamW (MSDN)</a> 
/// or <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa364430(v=vs.85).aspx">FindNextStreamW (MSDN)</a> function.
/// </summary>
/// \see <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa365741(v=vs.85).aspx">WIN32_FIND_STREAM_DATA structure (MSDN)</a>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
internal struct WIN32_FIND_STREAM_DATA
{
    /// <summary>
    /// A <c>long</c> value that specifies the size of the stream, in bytes.
    /// </summary>
    public long StreamSize { get; set; }

    /// <summary>
    /// The name of the stream. The string name format is "<c>:streamname:$streamtype</c>".
    /// </summary>
    /// <summary>
    /// Size of <c>cStreamName</c> in the Windows definition: <c>WCHAR cStreamName[MAX_PATH + 36]</c>.
    /// </summary>
    internal const int StreamNameCapacity = 260 + 36;

    public unsafe fixed char cStreamName[StreamNameCapacity];

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    public unsafe ReadOnlySpan<char> StreamName
    {
        set => value.CopyTo(MemoryMarshal.CreateSpan(ref cStreamName[0], StreamNameCapacity - 1));
    }
#else
    public unsafe ReadOnlySpan<char> StreamName
    {
        set
        {
            fixed (char* ptr = cStreamName)
            {
                value.CopyTo(new(ptr, StreamNameCapacity - 1));
            }
        }
    }
#endif
}
