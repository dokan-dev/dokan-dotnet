using System;
using System.Linq;
using DokanNet;

namespace DokanNetMirror
{
    internal class Program
    {
        private const string MirrorKey = "-what";
        private const string MountKey = "-where";
        private const string UseUnsafeKey = "-unsafe";

        private static void Main(string[] args)
        {
            try
            {
                var arguments = args
                   .Select(x => x.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                   .ToDictionary(x => x[0], x => x.Length > 1 ? x[1] as object : true, StringComparer.OrdinalIgnoreCase);

                var mirrorPath = arguments.ContainsKey(MirrorKey)
                   ? arguments[MirrorKey] as string
                   : @"C:\";

                var mountPath = arguments.ContainsKey(MountKey)
                   ? arguments[MountKey] as string
                   : @"N:\";

                var unsafeReadWrite = arguments.ContainsKey(UseUnsafeKey);

                Console.WriteLine($"Using unsafe methods: {unsafeReadWrite}");
                var mirror = unsafeReadWrite 
                    ? new UnsafeMirror(mirrorPath) 
                    : new Mirror(mirrorPath);

                Dokan.Init();

                using (DokanInstance dokanInstance = mirror.CreateFileSystem(mountPath, DokanOptions.DebugMode | DokanOptions.EnableNotificationAPI))
                {
                    var notify = new Notify();
                    notify.Start(mirrorPath, mountPath, dokanInstance);
                    dokanInstance.WaitForFileSystemClosed(uint.MaxValue);
                }

                Dokan.Shutdown();

                Console.WriteLine(@"Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}