using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using DokanNet.Native;
using System.Runtime.Versioning;

namespace DokanNet;

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP

/// <summary>
/// Support for async/await operation on DokanInstance objects
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
internal sealed class DokanInstanceNotifyCompletion : ICriticalNotifyCompletion
{
    public DokanInstanceNotifyCompletion(DokanInstance dokanInstance, uint milliSeconds)
    {
        DokanInstance = dokanInstance;
        MilliSeconds = milliSeconds;
    }

    public DokanInstance DokanInstance { get; }
    public uint MilliSeconds { get; }
    public bool IsCompleted => !DokanInstance.IsFileSystemRunning();
    private nint waitHandle;
    private bool timedOut;
    private Action? continuation;

    public DokanInstanceNotifyCompletion GetAwaiter() => this;

    public void OnCompleted(Action continuation) => throw new NotSupportedException();

    public void UnsafeOnCompleted(Action continuation)
    {
        this.continuation = continuation;

        if (!NativeMethods.DokanRegisterWaitForFileSystemClosed(DokanInstance.DokanHandle,
                                                                out waitHandle,
                                                                Callback,
                                                                (nint)GCHandle.Alloc(this),
                                                                MilliSeconds))
        {
            throw new Win32Exception();
        }
    }

    private static void Callback(nint state, bool timedOut)
    {
        var handle = GCHandle.FromIntPtr(state);
        var target = (DokanInstanceNotifyCompletion)handle.Target!;

        handle.Free();

        while (target.waitHandle == 0)
        {
            Thread.Sleep(20);
        }

        NativeMethods.DokanUnregisterWaitForFileSystemClosed(target.waitHandle,
            waitForCallbacks: false);

        target.waitHandle = 0;

        target.timedOut = timedOut;

        target.continuation!();
    }

    /// <summary>
    /// Gets a value indicating whether DokanInstance was closed or if await timed out
    /// </summary>
    /// <returns>True if DokanInstance was closed or false if await timed out</returns>
    public bool GetResult()
    {
        if (timedOut)
        {
            return false;
        }

        if (!IsCompleted)
        {
            throw new InvalidOperationException($"Invalid state for {nameof(GetResult)}");
        }

        return true;
    }
}

#endif
