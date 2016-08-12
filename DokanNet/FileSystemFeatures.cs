using System;

namespace DokanNet
{
    /// <summary>
    /// Defines File System Attribute Extensions
    /// </summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/cc246323.aspx">File System Attribute Extensions</a>
    [Flags]
#pragma warning disable 3009
    public enum FileSystemFeatures : uint
#pragma warning restore 3009
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// File system supports case-sensitive file names.
        /// </summary>
        CaseSensitiveSearch = 1,

        /// <summary>
        /// File system preserves the case of file names when it stores the name on disk.
        /// </summary>
        CasePreservedNames = 2,

        /// <summary>
        /// File system supports Unicode in file names.
        /// </summary>
        UnicodeOnDisk = 4,

        /// <summary>
        /// File system preserves and enforces access control lists.
        /// </summary>
        PersistentAcls = 8,

        /// <summary>
        /// File system supports remote storage.
        /// </summary>
        SupportsRemoteStorage = 256,

        /// <summary>
        /// File system supports per-user quotas.
        /// </summary>
        VolumeQuotas = 32,

        /// <summary>
        /// File system supports sparse files.
        /// </summary>
        SupportsSparseFiles = 64,

        /// <summary>
        /// File system supports reparse points.
        /// </summary>
        SupportsReparsePoints = 128,

        /// <summary>
        /// Volume is a compressed volume. This flag is incompatible with FILE_FILE_COMPRESSION.
        /// This does not affect how data is transferred over the network.
        /// </summary>
        VolumeIsCompressed = 32768,

        /// <summary>
        /// File system supports object identifiers.
        /// </summary>
        SupportsObjectIDs = 65536,

        /// <summary>
        /// File system supports encryption.
        /// </summary>
        SupportsEncryption = 131072,

        /// <summary>
        /// File system supports multiple named data streams for a file.
        /// </summary>
        NamedStreams = 262144,

        /// <summary>
        /// Specified volume is read-only.
        /// </summary>
        ReadOnlyVolume = 524288,

        /// <summary>
        /// Specified volume can be written to one time only.  The write MUST be performed in sequential order.
        /// </summary>
        SequentialWriteOnce = 1048576,

        /// <summary>
        /// File system supports transaction processing.
        /// </summary>
        SupportsTransactions = 2097152
    }
}