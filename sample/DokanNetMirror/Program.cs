using System;
using DokanNet;

namespace DokanNetMirror
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                var mirror = new Mirror("C:");
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