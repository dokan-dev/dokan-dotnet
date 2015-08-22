using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokanNet.Tests
{
    internal static class StringExtensions
    {
        public static string AsRootedPath(this string path) => DokanOperationsFixture.RootedPath(path);

        public static string AsDriveBasedPath(this string path) => DokanOperationsFixture.DriveBasedPath(path);
    }
}
