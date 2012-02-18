using System;

namespace DokanNet
{
    [Flags]
    public enum FileAccess : uint
    {
        GenericRead = 2147483648,
        GenericWrite = 1073741824,
        GenericExecute = 536870912,
        ReadData = 1,
        WriteData = 2,
        AppendData = 4,
        ReadExtendedAttributes = 8,
        WriteExtendedAttributes = 16,
        Execute = 32,
        ReadAttributes = 128,
        WriteAttributes = 256,
        Delete = 65536,
        ReadPermissions = 131072,
        ChangePermissions = 262144,
        SetOwnership = 524288,
        Synchronize = 1048576,
    }
}