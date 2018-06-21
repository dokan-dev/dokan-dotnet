using System;
using System.Linq;

namespace DokanNet.Tests
{
    static class FileAccessUtils
    {
        private const FileAccess FILE_GENERIC_READ =
            FileAccess.ReadAttributes |
            FileAccess.ReadData |
            FileAccess.ReadExtendedAttributes |
            FileAccess.ReadPermissions |
            FileAccess.Synchronize;

        private const FileAccess FILE_GENERIC_WRITE =
            FileAccess.AppendData |
            FileAccess.WriteAttributes |
            FileAccess.WriteData |
            FileAccess.WriteExtendedAttributes |
            FileAccess.ReadPermissions |
            FileAccess.Synchronize;

        private const FileAccess FILE_GENERIC_EXECUTE =
            FileAccess.Execute |
            FileAccess.ReadAttributes |
            FileAccess.ReadPermissions |
            FileAccess.Synchronize;

        private static readonly FileAccess FILE_ALL_ACCESS = (FileAccess)Enum.GetValues(typeof(FileAccess)).Cast<long>().Sum();

        public static FileAccess MapSpecificToGenericAccess(FileAccess desiredAccess)
        {
            var outDesiredAccess = desiredAccess;

            var genericRead = false;
            var genericWrite = false;
            var genericExecute = false;
            var genericAll = false;
            if ((outDesiredAccess & FILE_GENERIC_READ) == FILE_GENERIC_READ)
            {
                outDesiredAccess |= FileAccess.GenericRead;
                genericRead = true;
            }
            if ((outDesiredAccess & FILE_GENERIC_WRITE) == FILE_GENERIC_WRITE)
            {
                outDesiredAccess |= FileAccess.GenericWrite;
                genericWrite = true;
            }
            if ((outDesiredAccess & FILE_GENERIC_EXECUTE) == FILE_GENERIC_EXECUTE)
            {
                outDesiredAccess |= FileAccess.GenericExecute;
                genericExecute = true;
            }
            if ((outDesiredAccess & FILE_ALL_ACCESS) == FILE_ALL_ACCESS)
            {
                outDesiredAccess |= FileAccess.GenericAll;
                genericAll = true;
            }

            if (genericRead)
                outDesiredAccess &= ~FILE_GENERIC_READ;
            if (genericWrite)
                outDesiredAccess &= ~FILE_GENERIC_WRITE;
            if (genericExecute)
                outDesiredAccess &= ~FILE_GENERIC_EXECUTE;
            if (genericAll)
                outDesiredAccess &= ~FILE_ALL_ACCESS;

            return outDesiredAccess;
        }
    }
}
