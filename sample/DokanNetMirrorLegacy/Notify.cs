using System;
using System.IO;
using DokanNet;

namespace DokanNetMirror
{
    internal class Notify : IDisposable
    {
        private readonly string _sourcePath;
        private readonly string _targetPath;
        private readonly DokanInstance _dokanInstance;
        private readonly FileSystemWatcher _commonFsWatcher;
        private readonly FileSystemWatcher _fileFsWatcher;
        private readonly FileSystemWatcher _dirFsWatcher;
        private bool _disposed;

        public Notify(string mirrorPath, string mountPath, DokanInstance dokanInstance)
        {
            _sourcePath = mirrorPath;
            _targetPath = mountPath;
            _dokanInstance = dokanInstance;

            _commonFsWatcher = new FileSystemWatcher(mirrorPath)
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

            _commonFsWatcher.Changed += OnCommonFileSystemWatcherChanged;
            _commonFsWatcher.Created += OnCommonFileSystemWatcherCreated;
            _commonFsWatcher.Renamed += OnCommonFileSystemWatcherRenamed;

            _commonFsWatcher.EnableRaisingEvents = true;

            _fileFsWatcher = new FileSystemWatcher(mirrorPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
            };

            _fileFsWatcher.Deleted += OnCommonFileSystemWatcherFileDeleted;

            _fileFsWatcher.EnableRaisingEvents = true;

            _dirFsWatcher = new FileSystemWatcher(mirrorPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };

            _dirFsWatcher.Deleted += OnCommonFileSystemWatcherDirectoryDeleted;

            _dirFsWatcher.EnableRaisingEvents = true;
        }

        private string AlterPathToMountPath(string path)
        {
            var relativeMirrorPath = path.Substring(_sourcePath.Length).TrimStart('\\');

            return Path.Combine(_targetPath, relativeMirrorPath);
        }

        private void OnCommonFileSystemWatcherFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (_dokanInstance.IsDisposed)
            {
                return;
            }

            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.Notify.Delete(_dokanInstance, fullPath, false);
        }

        private void OnCommonFileSystemWatcherDirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            if (_dokanInstance.IsDisposed)
            {
                return;
            }

            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.Notify.Delete(_dokanInstance, fullPath, true);
        }

        private void OnCommonFileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (_dokanInstance.IsDisposed)
            {
                return;
            }

            var fullPath = AlterPathToMountPath(e.FullPath);

            Dokan.Notify.Update(_dokanInstance, fullPath);
        }

        private void OnCommonFileSystemWatcherCreated(object sender, FileSystemEventArgs e)
        {
            if (_dokanInstance.IsDisposed)
            {
                return;
            }

            var fullPath = AlterPathToMountPath(e.FullPath);
            var isDirectory = Directory.Exists(fullPath);

            Dokan.Notify.Create(_dokanInstance, fullPath, isDirectory);
        }

        private void OnCommonFileSystemWatcherRenamed(object sender, RenamedEventArgs e)
        {
            if (_dokanInstance.IsDisposed)
            {
                return;
            }

            var oldFullPath = AlterPathToMountPath(e.OldFullPath);
            var oldDirectoryName = Path.GetDirectoryName(e.OldFullPath);

            var fullPath = AlterPathToMountPath(e.FullPath);
            var directoryName = Path.GetDirectoryName(e.FullPath);

            var isDirectory = Directory.Exists(e.FullPath);
            var isInSameDirectory = string.Equals(oldDirectoryName, directoryName);

            Dokan.Notify.Rename(_dokanInstance, oldFullPath, fullPath, isDirectory, isInSameDirectory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    _commonFsWatcher.Changed -= OnCommonFileSystemWatcherChanged;
                    _commonFsWatcher.Created -= OnCommonFileSystemWatcherCreated;
                    _commonFsWatcher.Renamed -= OnCommonFileSystemWatcherRenamed;

                    _commonFsWatcher.Dispose();
                    _fileFsWatcher.Dispose();
                    _dirFsWatcher.Dispose();

                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
