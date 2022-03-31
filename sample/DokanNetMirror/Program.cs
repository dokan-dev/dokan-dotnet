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
                using (var mirrorLogger = new ConsoleLogger("[Mirror] "))
                using (var dokanLogger = new ConsoleLogger("[Dokan] "))
                {
                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        e.Cancel = true;
                        mre.Set();
                    };

                    Console.WriteLine($"Using unsafe methods: {unsafeReadWrite}");
                    var mirror = unsafeReadWrite
                        ? new UnsafeMirror(mirrorLogger, mirrorPath)
                        : new Mirror(mirrorLogger, mirrorPath);

                    var dokanBuilder = new DokanBuilder()
                        .ConfigureLogger(() => dokanLogger)
                        .ConfigureOptions(options => {
                            options.Options = DokanOptions.DebugMode | DokanOptions.EnableNotificationAPI;
                            options.MountPoint = mountPath;
                        });
                    using (var dokan = dokanBuilder.Build(mirror))
                    using (var notify = new Notify(mirrorPath, mountPath, dokan))
                    {
                        mre.WaitOne();
                    }
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