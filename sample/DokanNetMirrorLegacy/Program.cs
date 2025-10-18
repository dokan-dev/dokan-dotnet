using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using DokanNet;
using DokanNet.Logging;

#if NET5_0_OR_GREATER
[assembly: SupportedOSPlatform("windows")]
#endif

namespace DokanNetMirror
{
    internal class Program
    {
        private const string MirrorKey = "-what";
        private const string MountKey = "-where";
        private const string UseUnsafeKey = "-unsafe";

        private static async Task Main(string[] args)
        {
            try
            {
                var arguments = args
                   .Select(x => x.Split(['='], 2, StringSplitOptions.RemoveEmptyEntries))
                   .ToDictionary(x => x[0], x => x.Length > 1 ? x[1] as object : true, StringComparer.OrdinalIgnoreCase);

                var mirrorPath = arguments.TryGetValue(MirrorKey, out var mirrorPathObj)
                   ? (string)mirrorPathObj
                   : @"C:\";

                var mountPath = arguments.TryGetValue(MountKey, out var mountPathObj)
                   ? (string)mountPathObj
                   : @"N:\";

                var unsafeReadWrite = arguments.ContainsKey(UseUnsafeKey);

                using (var mirrorLogger = new ConsoleLogger("[Mirror] "))
                using (var dokanLogger = new ConsoleLogger("[Dokan] "))
                using (var dokan = new Dokan(dokanLogger))
                {
                    Console.WriteLine($"Using unsafe methods: {unsafeReadWrite}");
                    var mirror = unsafeReadWrite
                        ? new UnsafeMirror(mirrorLogger, mirrorPath)
                        : new Mirror(mirrorLogger, mirrorPath);

                    var dokanBuilder = new DokanInstanceBuilder(dokan)
                        .ConfigureLogger(() => dokanLogger)
                        .ConfigureOptions(options =>
                        {
                            options.Options = DokanOptions.DebugMode;
                            options.MountPoint = mountPath;
                        });
                    using var dokanInstance = dokanBuilder.Build(mirror);
                    using var notify = new Notify(mirrorPath, mountPath, dokanInstance);
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        dokan.RemoveMountPoint(mountPath);
                    };

                    await dokanInstance.WaitForFileSystemClosedAsync(uint.MaxValue);
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