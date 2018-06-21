using System;
using System.Linq;
using DokanNet;

namespace DokanNetMirror
{
    internal class Program
    {
        private const string MirrorKey = "/what";
        private const string MountKey = "/where";
        private static void Main(string[] args)
        {

            try
            {
                var arguments = args
                   .Select(x => x.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                   .ToDictionary(x => x[0], x => x[1]);

                string mirrorPath;
                if (arguments.ContainsKey(MirrorKey))
                {
                    mirrorPath = arguments[MirrorKey];
                }
                else
                {
                    Console.WriteLine("input what you want to mirror");
                    mirrorPath = Console.ReadLine();
                }


                string mountPath;
                if (arguments.ContainsKey(MountKey))
                {
                    mountPath = arguments[MountKey];
                }
                else
                {
                    Console.WriteLine("input where you want to mirror");
                    mountPath = Console.ReadLine();
                }

                var mirror = new Mirror(mirrorPath);
                mirror.Mount(mountPath, DokanOptions.DebugMode, 5);

                Console.WriteLine(@"Success");
            }
            catch (DokanException ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
        }
    }
}