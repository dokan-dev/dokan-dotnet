using System.IO;

namespace DokanNet.Tests
{
    internal static class FileSettings
    {
        public const FileAccess ReadAttributesAccess = FileAccess.ReadAttributes;

        public const FileAccess ReadPermissionsAccess = FileAccess.ReadPermissions;

        public const FileAccess ReadAttributesPermissionsAccess = ReadAttributesAccess | ReadPermissionsAccess;

        public const FileAccess ChangePermissionsAccess = FileAccess.ReadAttributes | FileAccess.ReadPermissions | FileAccess.ChangePermissions;

        public const FileAccess ReadAccess = FileAccess.ReadData | FileAccess.ReadExtendedAttributes | FileAccess.ReadAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public const FileAccess WriteAccess = FileAccess.WriteData | FileAccess.AppendData | FileAccess.WriteExtendedAttributes | FileAccess.ReadAttributes | FileAccess.WriteAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public const FileAccess ReadWriteAccess = ReadAccess | WriteAccess;

        public const FileAccess SetOwnershipAccess = ReadAccess | WriteAccess | FileAccess.Delete | FileAccess.ChangePermissions | FileAccess.SetOwnership;

        public const FileAccess DeleteAccess = FileAccess.ReadAttributes | FileAccess.Delete;

        public const FileAccess CopyToAccess = ReadAccess | WriteAccess | FileAccess.Delete | FileAccess.ChangePermissions;

        public const FileAccess MoveFromAccess = FileAccess.ReadAttributes | FileAccess.Delete | FileAccess.Synchronize;

        public const FileAccess ReplaceAccess = ReadAccess | FileAccess.WriteData | FileAccess.Delete;

        public const FileAccess OpenDirectoryAccess = FileAccess.Synchronize;

        public const FileAccess ReadDirectoryAccess = FileAccess.ReadData | FileAccess.Synchronize;

        public const FileAccess WriteDirectoryAccess = FileAccess.WriteData | FileAccess.Synchronize;

        public const FileAccess AppendToDirectoryAccess = FileAccess.AppendData | FileAccess.Synchronize;

        public const FileAccess DeleteFromDirectoryAccess = FileAccess.Delete | FileAccess.ReadAttributes | FileAccess.Synchronize;

        public const FileShare ReadOnlyShare = FileShare.Read;

        public const FileShare ReadShare = FileShare.Read | FileShare.Delete;

        public const FileShare ReadWriteShare = FileShare.ReadWrite | FileShare.Delete;

        public const FileShare WriteShare = FileShare.None;

        public const FileShare OpenDirectoryShare = FileShare.None;

        public const FileOptions ReadFileOptions = FileOptions.None;

        public const FileOptions WriteFileOptions = FileOptions.None;

        public const FileOptions OpenReparsePointOptions = (FileOptions)0x00200000;

        public const FileOptions OpenNoBufferingOptions = (FileOptions)0x20000000;
    }
}