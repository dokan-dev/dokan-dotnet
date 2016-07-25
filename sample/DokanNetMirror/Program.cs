using DokanNet;
using System;

namespace DokanNetMirror
{
    internal class Programm
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