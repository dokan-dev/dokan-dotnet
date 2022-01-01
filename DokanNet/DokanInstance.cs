using System;
using System.Collections.Generic;
using System.Text;
using DokanNet.Native;

namespace DokanNet
{
    /// <summary>
    /// Created by <see cref="Dokan.CreateFileSystem"/>.
    /// It holds all the resources required to be alive for the time of the mount.
    /// </summary>
    public class DokanInstance
    {
        internal DOKAN_OPTIONS DokanOptions;
        internal DOKAN_OPERATIONS DokanOperations;
        internal DokanOperationProxy DokanOperationProxy;
        internal IntPtr DokanHandle;
    }
}
