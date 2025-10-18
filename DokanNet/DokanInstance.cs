using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using DokanNet.Logging;
using DokanNet.Native;
using LTRData.Extensions.Native.Memory;

namespace DokanNet;

/// <summary>
/// Created by <see cref="DokanInstanceBuilder.Build(IDokanOperations2)"/>.
/// It holds all the resources required to be alive for the time of the mount.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class DokanInstance : IDisposable
{
    internal NativeStructWrapper<DOKAN_OPTIONS> DokanOptions { get; private set; }
    internal NativeStructWrapper<DOKAN_OPERATIONS> DokanOperations { get; private set; }
    internal DokanHandle DokanHandle { get; private set; }
    internal Dokan Dokan { get; private set; }

#if NET9_0_OR_GREATER
    private readonly Lock _disposeLock = new();
#else
    private readonly object _disposeLock = new();
#endif

    private bool _disposed = false;

    /// <summary>
    /// Event when this object is about to be disposed.
    /// </summary>
    public event EventHandler? Disposing;

    /// <summary>
    /// Event when this object has been disposed.
    /// </summary>
    public event EventHandler? Disposed;

    /// <summary>
    /// True when this object is about to be disposed.
    /// </summary>
    public bool IsDisposing { get; private set; }

    /// <summary>
    /// True when this object has been disposed and is no longer valid.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_disposeLock)
            {
                return _disposed;
            }
        }
    }

    /// <summary>
    /// Event when this object is about to be disposed.
    /// </summary>
    protected void OnDisposing(EventArgs e) => Disposing?.Invoke(this, e);

    /// <summary>
    /// Event when this object has been disposed.
    /// </summary>
    protected void OnDisposed(EventArgs e) => Disposed?.Invoke(this, e);

    /// <summary>
    /// Mount the filesystem described by <see cref="DOKAN_OPTIONS"/>.
    /// <see cref="IDokanOperations2"/> will start to received operations from the system and applications for this device.
    /// See <see cref="DokanInstanceBuilder"/>
    /// </summary>
    internal DokanInstance(ILogger logger, DOKAN_OPTIONS options, Dokan dokan, IDokanOperations2 operations)
    {
        DokanOptions = NativeStructWrapper.Wrap(options);
        var preparedOperations = PrepareOperations(logger, operations);
        DokanOperations = NativeStructWrapper.Wrap(preparedOperations);
        var status = NativeMethods.DokanCreateFileSystem(DokanOptions, DokanOperations, out var handle);
        if (status != DokanStatus.Success)
        {
            throw new DokanException(status);
        }

        DokanHandle = handle;
        Dokan = dokan;

        // This is just a way to keep a reference alive to this object and associated
        // operations as long as the file system is mounted
        KeepReferenceAlive();

        async void KeepReferenceAlive()
        {
            var gchandle = GCHandle.Alloc(this);

            try
            {
                await WaitForFileSystemClosedAsync(uint.MaxValue).ConfigureAwait(false);
            }
            catch
            {
            }
            finally
            {
                if (gchandle.IsAllocated)
                {
                    gchandle.Free();
                }
            }
        }
    }

    /// <summary>
    /// Check if the FileSystem is still running or not.
    /// </summary>
    /// <returns>Whether the FileSystem is still running or not.</returns>
    public bool IsFileSystemRunning()
    {
        return DokanHandle is not null && !DokanHandle.IsInvalid && !DokanHandle.IsClosed
            && NativeMethods.DokanIsFileSystemRunning(DokanHandle);
    }

    /// <summary>
    /// Wait until the FileSystem is unmount.
    /// </summary>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If set to zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If set to INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>See <a href="https://docs.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">WaitForSingleObject</a> for a description of return values.</returns>
    public uint WaitForFileSystemClosed(uint milliSeconds)
    {
        return DokanHandle is null || DokanHandle.IsClosed || DokanHandle.IsInvalid ?
            0 : NativeMethods.DokanWaitForFileSystemClosed(DokanHandle, milliSeconds);
    }

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP

    /// <summary>
    /// Wait asynchronously until the FileSystem is unmounted.
    /// </summary>
    /// <param name="milliSeconds">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the object is signaled or the interval elapses. If <paramref name="milliSeconds" /> is zero,
    /// the function does not enter a wait state if the object is not signaled; it always returns immediately. If <paramref name="milliSeconds"/> is INFINITE, the function will return only when the object is signaled.</param>
    /// <returns>True if instance was dismounted or false if time out occurred.</returns>
    public async Task<bool> WaitForFileSystemClosedAsync(uint milliSeconds)
        => DokanHandle is null || DokanHandle.IsClosed || DokanHandle.IsInvalid
        || await new DokanInstanceNotifyCompletion(this, milliSeconds);
#endif

    /// <summary>
    /// Notify Dokan that a file or directory has been created.
    /// </summary>
    /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <param name="isDirectory">Indicates if the path is a directory.</param>
    /// <returns>true if the notification succeeded.</returns>
    public bool NotifyCreate(string filePath, bool isDirectory)
    {
        return NativeMethods.DokanNotifyCreate(DokanHandle, filePath, isDirectory);
    }

    /// <summary>
    /// Notify Dokan that a file or directory has been deleted.
    /// </summary>
    /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <param name="isDirectory">Indicates if the path is a directory.</param>
    /// <returns>true if notification succeeded.</returns>
    public bool NotifyDelete(string filePath, bool isDirectory)
    {
        return NativeMethods.DokanNotifyDelete(DokanHandle, filePath, isDirectory);
    }

    /// <summary>
    /// Notify Dokan that file or directory attributes have changed.
    /// </summary>
    /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <returns>true if notification succeeded.</returns>
    public bool NotifyUpdate(string filePath)
    {
        return NativeMethods.DokanNotifyUpdate(DokanHandle, filePath);
    }

    /// <summary>
    /// Notify Dokan that file or directory extended attributes have changed.
    /// </summary>
    /// <param name="filePath">Absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <returns>true if notification succeeded.</returns>
    public bool NotifyXAttrUpdate(string filePath)
    {
        return NativeMethods.DokanNotifyXAttrUpdate(DokanHandle, filePath);
    }

    /// <summary>
    /// Notify Dokan that a file or directory has been renamed.
    /// This method supports in-place rename for file/directory within the same parent.
    /// </summary>
    /// <param name="oldPath">Old, absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <param name="newPath">New, absolute path to the file or directory, including the mount-point of the file system.</param>
    /// <param name="isDirectory">Indicates if the path is a directory.</param>
    /// <param name="isInSameDirectory">Indicates if the file or directory have the same parent directory.</param>
    /// <returns>true if notification succeeded.</returns>
    public bool NotifyRename(string oldPath, string newPath, bool isDirectory, bool isInSameDirectory)
    {
        return NativeMethods.DokanNotifyRename(DokanHandle, oldPath,
            newPath,
            isDirectory,
            isInSameDirectory);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        lock (_disposeLock)
        {
            if (!IsDisposed)
            {
                IsDisposing = true;

                if (disposing)
                {
                    OnDisposing(EventArgs.Empty);

                    // Dispose managed state (managed objects)
                    DokanHandle?.Dispose();     // This calls DokanCloseHandle and waits for dismount
                    DokanOptions?.Dispose();    // After that, it is safe to free unmanaged memory
                    DokanOperations?.Dispose();

                    OnDisposed(EventArgs.Empty);
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer

                // Set fields to null
                DokanOptions = null!;
                DokanOperations = null!;
                DokanHandle = null!;

                _disposed = true;
            }
        }
    }

    /// <inheritdoc/>
    ~DokanInstance()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static unsafe DOKAN_OPERATIONS PrepareOperations(ILogger logger, IDokanOperations2 operations)
    {
        var dokanOperationProxy = new DokanOperationProxy(logger, operations);

        return new DOKAN_OPERATIONS
        {
            ZwCreateFile = dokanOperationProxy.ZwCreateFileProxy,
            Cleanup = dokanOperationProxy.CleanupProxy,
            CloseFile = dokanOperationProxy.CloseFileProxy,
            ReadFile = dokanOperationProxy.ReadFileProxy,
            WriteFile = dokanOperationProxy.WriteFileProxy,
            FlushFileBuffers = dokanOperationProxy.FlushFileBuffersProxy,
            GetFileInformation = dokanOperationProxy.GetFileInformationProxy,
            FindFiles = dokanOperationProxy.FindFilesProxy,
            FindFilesWithPattern = dokanOperationProxy.FindFilesWithPatternProxy,
            SetFileAttributes = dokanOperationProxy.SetFileAttributesProxy,
            SetFileTime = dokanOperationProxy.SetFileTimeProxy,
            DeleteFile = dokanOperationProxy.DeleteFileProxy,
            DeleteDirectory = dokanOperationProxy.DeleteDirectoryProxy,
            MoveFile = dokanOperationProxy.MoveFileProxy,
            SetEndOfFile = dokanOperationProxy.SetEndOfFileProxy,
            SetAllocationSize = dokanOperationProxy.SetAllocationSizeProxy,
            LockFile = dokanOperationProxy.LockFileProxy,
            UnlockFile = dokanOperationProxy.UnlockFileProxy,
            GetDiskFreeSpace = dokanOperationProxy.GetDiskFreeSpaceProxy,
            GetVolumeInformation = dokanOperationProxy.GetVolumeInformationProxy,
            Mounted = dokanOperationProxy.MountedProxy,
            Unmounted = dokanOperationProxy.UnmountedProxy,
            GetFileSecurity = dokanOperationProxy.GetFileSecurityProxy,
            SetFileSecurity = dokanOperationProxy.SetFileSecurityProxy,
            FindStreams = dokanOperationProxy.FindStreamsProxy
        };
    }
}
