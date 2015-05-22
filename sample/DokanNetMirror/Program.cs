using System;
using System.Collections;
using DokanNet;
using System.Collections.Generic;
using System.Security.AccessControl;

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
