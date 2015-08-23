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
                mirror.Mount("n:\\", DokanOptions.DebugMode, 5);

                Console.WriteLine("Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
