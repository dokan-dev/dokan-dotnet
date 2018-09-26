using System;
using DokanNet;

namespace DokanNetMirror
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                bool unsafeReadWrite = args.Length > 0 && args[0].Equals("-unsafe", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"Using unsafe methods: {unsafeReadWrite}");
                var mirror = unsafeReadWrite ? new UnsafeMirror("C:") : new Mirror("C:");
                mirror.Mount("n:\\", DokanOptions.DebugMode, 5);

                Console.WriteLine(@"Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}