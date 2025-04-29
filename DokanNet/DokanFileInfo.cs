using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using DokanNet.Native;
using Microsoft.Win32.SafeHandles;
using static DokanNet.FormatProviders;

namespace DokanNet;

/// <summary>
/// %Dokan file information on the current operation.
/// </summary>
/// <remarks>
/// This class cannot be instantiated in C#, it is created by the kernel %Dokan driver.
/// This is the same structure as <c>_DOKAN_FILE_INFO</c> (dokan.h) in the C version of Dokan.
/// </remarks>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct DokanFileInfo
{
    private long context;

    /// <summary>
    /// Used internally, never modify.
    /// </summary>
    private readonly ulong dokanContext;

    /// <summary>
    /// A pointer to the <see cref="DOKAN_OPTIONS"/> which was passed to <see cref="Native.NativeMethods.DokanMain(DOKAN_OPTIONS, DOKAN_OPERATIONS)"/>.
    /// </summary>
    private readonly nint dokanOptions;

    /// <summary>
    /// Reserved. Used internally by Dokan library. Never modify.
    /// If the processing for the event requires extra data to be associated with it
    /// then a pointer to that data can be placed here
    /// </summary>
    private readonly nint processingContext;

    /// <summary>
    /// Process id for the thread that originally requested a given I/O
    /// operation.
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Gets or sets a value indicating whether it requesting a directory
    /// file. Must be set in <see cref="IDokanOperations2.CreateFile"/> if
    /// the file appear to be a folder.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file has to be deleted
    /// during the <see cref="IDokanOperations2.Cleanup"/> event.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool DeletePending { get; set; }

    /// <summary>
    /// Read or write is paging IO.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool PagingIo { get; }

    /// <summary>
    /// Read or write is synchronous IO.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool SynchronousIo { get; }

    /// <summary>
    /// Read or write directly from data source without cache.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool NoCache { get; }

    /// <summary>
    /// If <c>true</c>, write to the current end of file instead 
    /// of using the <c>Offset</c> parameter.
    /// </summary>
    [field: MarshalAs(UnmanagedType.U1)] public bool WriteToEndOfFile { get; }

    /// <summary>
    /// Gets or sets context that can be used to carry information between operation.
    /// The Context can carry whatever type like <c><see cref="System.IO.FileStream"/></c>, <c>struct</c>, <c>int</c>,
    /// or internal reference that will help the implementation understand the request context of the event.
    /// </summary>
    public object? Context
    {
        readonly get
        {
            if (context != 0)
            {
                return ((GCHandle)(nint)context).Target;
            }

            return null;
        }

        set
        {
            if (context != 0)
            {
                ((GCHandle)(nint)context).Free();
                context = 0;
            }

            if (value is not null)
            {
                context = (nint)GCHandle.Alloc(value);
            }
        }
    }

    /// <summary>
    /// This method needs to be called in <see cref="IDokanOperations2.CreateFile"/>.
    /// </summary>
    /// <returns>An <c><see cref="WindowsIdentity"/></c> with the access token.</returns>
    public readonly WindowsIdentity GetRequestor()
    {
        using var sfh = GetRequestorToken();

        return new(sfh.DangerousGetHandle());
    }

    /// <summary>
    /// This method needs to be called in <see cref="IDokanOperations2.CreateFile"/>.
    /// </summary>
    /// <returns>A <c><see cref="SafeAccessTokenHandle"/></c> with the access token.</returns>
    public readonly SafeAccessTokenHandle GetRequestorToken() => NativeMethods.DokanOpenRequestorToken(this);

    /// <summary>
    /// Extends the time out of the current IO operation in driver.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds to extend with.</param>
    /// <returns>If the operation was successful.</returns>
    public readonly bool TryResetTimeout(int milliseconds) => NativeMethods.DokanResetTimeout((uint)milliseconds, this);

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override readonly string ToString() => DokanFormat(
        $"{{{nameof(Context)}=0x{context:X}:'{Context}', {nameof(DeletePending)}={DeletePending}, {nameof(IsDirectory)}={IsDirectory}, {nameof(NoCache)}={NoCache}, {nameof(PagingIo)}={PagingIo}, {nameof(ProcessId)}={ProcessId}, {nameof(SynchronousIo)}={SynchronousIo}, {nameof(WriteToEndOfFile)}={WriteToEndOfFile}}}")!;
}
