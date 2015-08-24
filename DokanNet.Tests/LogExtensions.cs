namespace DokanNet.Tests
{
    internal static class LogExtensions
    {
        public static string Log(this DokanFileInfo info)
            => $"{nameof(DokanFileInfo)} {{{info.Context?.GetType().Name ?? "<null>"}, {info.DeleteOnClose}, {info.IsDirectory}, {info.NoCache}, {info.PagingIo}, {info.ProcessId}, {info.SynchronousIo}, {info.WriteToEndOfFile}}}";

        public static string Log(this FileInformation fileInfo)
            => $"{nameof(FileInformation)} {{{fileInfo.FileName}, [{fileInfo.Attributes}], {fileInfo.CreationTime}, {fileInfo.LastWriteTime}, {fileInfo.LastAccessTime}, {fileInfo.Length}}}";
    }
}