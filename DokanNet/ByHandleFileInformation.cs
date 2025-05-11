using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DokanNet;

/// <summary>
/// Used to provide file information to %Dokan during operations by
///  - <see cref="IDokanOperations2.GetFileInformation"/>
///  - <see cref="IDokanOperations2.FindFiles"/>
///  - <see cref="IDokanOperations2.FindStreams"/> 
///  - <see cref="IDokanOperations2.FindFilesWithPattern"/>.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{Length}, {CreationTime}, {LastWriteTime}, {LastAccessTime}, {Attributes}")]
public struct ByHandleFileInformation
{
    /// <summary>
    /// Initializes with <see cref="NumberOfLinks"/> set to 1.
    /// </summary>
    public ByHandleFileInformation()
    {
        this = default;
        NumberOfLinks = 1;
    }

    /// <summary>
    /// Gets or sets the <c><see cref="FileAttributes"/></c> for the file or directory.
    /// </summary>
    public FileAttributes Attributes { get; set; }

    /// <summary>
    /// Gets or sets the creation time of the file or directory.
    /// If equal to <c>null</c>, the value will not be set or the file has no creation time.
    /// </summary>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the last access time of the file or directory.
    /// If equal to <c>null</c>, the value will not be set or the file has no last access time.
    /// </summary>
    public DateTime? LastAccessTime { get; set; }

    /// <summary>
    /// Gets or sets the last write time of the file or directory.
    /// If equal to <c>null</c>, the value will not be set or the file has no last write time.
    /// </summary>
    public DateTime? LastWriteTime { get; set; }

    /// <summary>
    /// Gets or sets the length of the file.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Number of links to the same file.
    /// </summary>
    public int NumberOfLinks { get; set; }

    /// <summary>
    /// Index number of file in the file system.
    /// </summary>
    public long FileIndex { get; set; }
}
