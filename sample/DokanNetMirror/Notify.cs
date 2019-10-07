using System.IO;
using DokanNet;

namespace DokanNetMirror
{
    internal static class Notify
    {
        private static string sourcePath;
        private static string targetPath;
        private static FileSystemWatcher commonFsWatcher;
        private static FileSystemWatcher fileFsWatcher;
        private static FileSystemWatcher dirFsWatcher;

        public static void Start(string mirrorPath, string mountPath)
        {
            sourcePath = mirrorPath;
            targetPath = mountPath;

            commonFsWatcher = new FileSystemWatcher(mirrorPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Security |
                    NotifyFilters.Size
            };

            commonFsWatcher.Changed += OnCommonFileSystemWatcherChanged;
            commonFsWatcher.Created += OnCommonFileSystemWatcherCreated;
            commonFsWatcher.Renamed += OnCommonFileSystemWatcherRenamed;

            commonFsWatcher.EnableRaisingEvents = true;

            fileFsWatcher = new FileSystemWatcher(mirrorPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            fileFsWatcher.Deleted += OnCommonFileSystemWatcherFileDeleted;

            fileFsWatcher.EnableRaisingEvents = true;

            dirFsWatcher = new FileSystemWatcher(mirrorPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };

            dirFsWatcher.Deleted += OnCommonFileSystemWatcherDirectoryDeleted;

            dirFsWatcher.EnableRaisingEvents = true;
        }

        private static string AlterPathToMountPath(string path)
        {
            var relativeMirrorPath = path.Substring(sourcePath.Length).TrimStart('\\');

            return Path.Combine(targetPath, relativeMirrorPath);
        }

        private static void OnCommonFileSystemWatcherFileDeleted(object sender, FileSystemEventArgs e)
        {
            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.DokanNotifyDelete(fullPath, false);
        }

        private static void OnCommonFileSystemWatcherDirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.DokanNotifyDelete(fullPath, true);
        }

        private static void OnCommonFileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.DokanNotifyUpdate(fullPath);
        }

        private static void OnCommonFileSystemWatcherCreated(object sender, FileSystemEventArgs e)
        {
            var fullPath = AlterPathToMountPath(e.FullPath);
            var isDirectory = Directory.Exists(fullPath);

            Dokan.DokanNotifyCreate(fullPath, isDirectory);
        }

        private static void OnCommonFileSystemWatcherRenamed(object sender, RenamedEventArgs e)
        {
            var oldFullPath = AlterPathToMountPath(e.OldFullPath);
            var oldDirectoryName = Path.GetDirectoryName(e.OldFullPath);

            var fullPath = AlterPathToMountPath(e.FullPath);
            var directoryName = Path.GetDirectoryName(e.FullPath);

            var isDirectory = Directory.Exists(e.FullPath);
            var isInSameDirectory = oldDirectoryName.Equals(directoryName);

            Dokan.DokanNotifyRename(oldFullPath, fullPath, isDirectory, isInSameDirectory);
        }
    }
}
