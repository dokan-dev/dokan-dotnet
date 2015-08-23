using System;
using DokanNet;

namespace DokanNetMirror
{
    class Programm
    {
        static void Main(string[] args)
        {
            try
            {
                Mirror mirror = new Mirror("C:");
                mirror.Mount("n:\\", DokanOptions.DebugMode | DokanOptions.KeepAlive, 5);

                Console.WriteLine("Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
