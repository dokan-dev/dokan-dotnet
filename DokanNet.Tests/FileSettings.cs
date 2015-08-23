using System;
using System.IO;

namespace DokanNet.Tests
{
    internal static class FileSettings
    {
        public static FileAccess ReadAttributesAccess => FileAccess.ReadAttributes;

        public static FileAccess ReadPermissionsAccess => FileAccess.ReadPermissions;

        public static FileAccess ReadAttributesPermissionsAccess => ReadAttributesAccess | ReadPermissionsAccess;

        public static FileAccess ChangePermissionsAccess => FileAccess.ReadAttributes | FileAccess.ReadPermissions | FileAccess.ChangePermissions;

        public static FileAccess ReadAccess => FileAccess.ReadData | FileAccess.ReadExtendedAttributes | FileAccess.ReadAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public static FileAccess WriteAccess => FileAccess.WriteData | FileAccess.AppendData | FileAccess.WriteExtendedAttributes | FileAccess.ReadAttributes | FileAccess.WriteAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public static FileAccess ReadWriteAccess => ReadAccess | WriteAccess;

        public static FileAccess SetOwnershipAccess => ReadAccess | WriteAccess | FileAccess.Delete | FileAccess.ChangePermissions | FileAccess.SetOwnership;

        public static FileAccess DeleteAccess => FileAccess.ReadAttributes | FileAccess.Delete;

        public static FileAccess CopyToAccess => ReadAccess | WriteAccess | FileAccess.Delete | FileAccess.ChangePermissions;

        public static FileAccess MoveFromAccess => FileAccess.ReadAttributes | FileAccess.Delete | FileAccess.Synchronize;

        public static FileAccess ReplaceAccess => ReadAccess | FileAccess.WriteData | FileAccess.Delete;

        public static FileShare ReadOnlyShare => FileShare.Read;

        public static FileShare ReadShare => FileShare.Read | FileShare.Delete;

        public static FileShare ReadWriteShare => FileShare.ReadWrite | FileShare.Delete;

        public static FileShare WriteShare => FileShare.None;
    }
}
