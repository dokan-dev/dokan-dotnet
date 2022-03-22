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
                    Dokan.Init();
                    rfs.CreateFileSystem("r:\\", DokanOptions.DebugMode | DokanOptions.StderrOutput);
                    mre.WaitOne();
                    Dokan.Shutdown();
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
