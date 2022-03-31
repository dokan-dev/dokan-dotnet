using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using DokanNet;
using Microsoft.Win32;
using FileAccess = DokanNet.FileAccess;

namespace RegistryFS
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                using (var mre = new System.Threading.ManualResetEvent(false))
                {
                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        e.Cancel = true;
                        mre.Set();
                    };

                    var rfs = new RFS();
                    var dokanBuilder = new DokanBuilder()
                        .ConfigureOptions(options =>
                        {
                            options.Options = DokanOptions.DebugMode | DokanOptions.StderrOutput;
                            options.MountPoint = "r:\\";
                        });
                    using (var dokan = dokanBuilder.Build(rfs))
                    {
                        mre.WaitOne();
                    }
                    Console.WriteLine(@"Success");
                }
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}
