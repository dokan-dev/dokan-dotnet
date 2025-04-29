using System.Runtime.InteropServices;

namespace DokanNet.Native;

/// <summary>
/// Dokan mount options used to describe dokan device behaviour
/// </summary>
/// <see cref="NativeMethods.DokanMain"/>
/// <remarks>This is the same structure as <c>PDOKAN_OPTIONS</c> (dokan.h) in the C version of Dokan.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
public sealed class DOKAN_OPTIONS
{
    /// <summary>
    /// Version of the dokan features requested (version "123" is equal to Dokan version 1.2.3).
    /// </summary>
    public ushort Version { get; set; }

    /// <summary>
    /// Only use a single thread to process events. This is highly not recommended as can easily create a bottleneck.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)]
    public bool SingleThread { get; set; }

    /// <summary>
    /// Features enable for the mount. See <see cref="DokanOptions"/>.
    /// </summary>
    public DokanOptions Options { get; set; }

    /// <summary>
    /// FileSystem can store anything here.
    /// </summary>
    public ulong GlobalContext { get; set; }

    /// <summary>
    /// Mount point.
    /// Can be <c>M:\\</c>(drive letter) or <c>C:\\mount\\dokan</c> (path in NTFS).
    /// </summary>
    [field: MarshalAs(UnmanagedType.LPWStr)]
    public string? MountPoint { get; set; }

    /// <summary>
    /// UNC name used for network volume.
    /// </summary>
    [field: MarshalAs(UnmanagedType.LPWStr)]
    public string? UNCName { get; set; }

    private uint Timeout;
    /// <summary>
    /// Max timeout in milliseconds of each request before Dokan give up.
    /// </summary>
    public System.TimeSpan TimeOut
    {
        get => System.TimeSpan.FromMilliseconds(Timeout);
        set => Timeout = (uint)value.TotalMilliseconds;
    }

    /// <summary>
    /// Allocation Unit Size of the volume. This will behave on the file size.
    /// </summary>
    public int AllocationUnitSize { get; set; }

    /// <summary>
    /// Sector Size of the volume. This will behave on the file size.
    /// </summary>
    public int SectorSize { get; set; }
    /// <summary>
    /// Length of the optional VolumeSecurityDescriptor provided. Set 0 will disable the option.
    /// </summary>
    public int VolumeSecurityDescriptorLength { get; set; }

    /// <summary>
    /// Optional Volume Security descriptor. See <a href="https://docs.microsoft.com/en-us/windows/win32/api/securitybaseapi/nf-securitybaseapi-initializesecuritydescriptor">InitializeSecurityDescriptor</a>
    /// </summary>
    [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 16384, ArraySubType = UnmanagedType.U1)]
    public byte[]? VolumeSecurityDescriptor { get; set; }
}
