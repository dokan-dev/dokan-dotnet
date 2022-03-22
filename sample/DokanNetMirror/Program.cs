using System;
using System.Linq;
using DokanNet;
using DokanNet.Logging;

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

                using (var mre = new System.Threading.ManualResetEvent(false))
                using (var dokanNetLogger = new ConsoleLogger("[Mirror] "))
                {
                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        e.Cancel = true;
                        mre.Set();
                    };

                    Console.WriteLine($"Using unsafe methods: {unsafeReadWrite}");
                    var mirror = unsafeReadWrite
                        ? new UnsafeMirror(dokanNetLogger, mirrorPath)
                        : new Mirror(dokanNetLogger, mirrorPath);

                    Dokan.Init();

                    using (var dokanInstance = mirror.CreateFileSystem(mountPath, DokanOptions.DebugMode | DokanOptions.EnableNotificationAPI))
                    using (var notify = new Notify(mirrorPath, mountPath, dokanInstance))
                    {
                        mre.WaitOne();
                    }

                    Dokan.Shutdown();
                }

                Console.WriteLine(@"Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}