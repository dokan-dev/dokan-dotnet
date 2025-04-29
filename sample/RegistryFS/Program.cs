using System;
using System.Runtime.Versioning;
using DokanNet;
using DokanNet.Logging;

#if NET5_0_OR_GREATER
[assembly: SupportedOSPlatform("windows")]
#endif

namespace RegistryFS
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                using var mre = new System.Threading.ManualResetEvent(false);
                using var dokanLogger = new ConsoleLogger("[Dokan] ");
                using var dokan = new Dokan(dokanLogger);
                Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    mre.Set();
                };

                var rfs = new RFS();
                var dokanBuilder = new DokanInstanceBuilder(dokan)
                    .ConfigureOptions(options =>
                    {
                        options.Options = DokanOptions.DebugMode | DokanOptions.StderrOutput;
                        options.MountPoint = "r:\\";
                    });
                using (var dokanInstance = dokanBuilder.Build(rfs))
                {
                    mre.WaitOne();
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
