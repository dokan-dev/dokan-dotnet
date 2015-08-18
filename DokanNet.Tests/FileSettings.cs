using System;
using System.IO;

namespace DokanNet.Tests
{
    internal static class FileSettings
    {
        public static FileAccess ReadAttributesAccess => FileAccess.ReadAttributes;

        public static FileAccess ReadAccess => FileAccess.ReadData | FileAccess.ReadExtendedAttributes | FileAccess.ReadAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public static FileAccess WriteAccess => FileAccess.WriteData | FileAccess.AppendData | FileAccess.WriteExtendedAttributes | FileAccess.ReadAttributes | FileAccess.WriteAttributes | FileAccess.ReadPermissions | FileAccess.Synchronize;

        public static FileAccess ReadWriteAccess => ReadAccess | WriteAccess;

        public static FileAccess DeleteAccess => FileAccess.ReadAttributes | FileAccess.Delete;

        public static FileAccess CopyToAccess => ReadAccess | WriteAccess | FileAccess.Delete | FileAccess.ChangePermissions;

        public static FileAccess MoveFromAccess => FileAccess.ReadAttributes | FileAccess.Delete | FileAccess.Synchronize;

        public static FileAccess MoveToAccess => FileAccess.WriteData | FileAccess.Synchronize;

        public static FileAccess MoveDirectoryToAccess => FileAccess.AppendData | FileAccess.Synchronize;

        public static FileShare AppendShare => FileShare.Read;

        public static FileShare ReadShare => FileShare.Read | FileShare.Delete;

        public static FileShare ReadWriteShare => FileShare.ReadWrite | FileShare.Delete;

        public static FileShare WriteShare => FileShare.None;

        public static FileShare MoveShare => FileShare.ReadWrite;
    }
}
