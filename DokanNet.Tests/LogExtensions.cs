using System;

namespace DokanNet.Tests
{
    internal static class LogExtensions
    {
        public static string Log(this DokanFileInfo info)
        {
            return $"{nameof(DokanFileInfo)} {{{info.Context?.GetType().Name ?? "<null>"}, {info.DeleteOnClose}, {info.IsDirectory}, {info.NoCache}, {info.PagingIo}, {info.ProcessId}, {info.SynchronousIo}, {info.WriteToEndOfFile}}}";
        }
    }
}
