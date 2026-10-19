// .NET (Core) removed Thread.Abort: it is documented as unsupported and throws
// PlatformNotSupportedException. The game only aborts a worker that preloads a
// BGM track, and the work it does is bounded, so joining is both safe and
// deterministic - the caller waits for the preload to finish instead of killing it.
using System;
using System.Threading;

namespace SlimDX.Compat;

public static class ThreadCompat
{
    /// <summary>
    /// Replacement for Thread.Abort on runtimes where aborting is impossible.
    /// A successful return means the worker finished. Unlike Thread.Abort this
    /// waits for bounded work; timeout is an explicit failure, never false success.
    /// Callers must not dispose shared state after a timeout.
    /// </summary>
    public static void Stop(Thread thread, int timeoutMilliseconds = 5000)
    {
        if (thread == null || !thread.IsAlive) return;
        if (thread == Thread.CurrentThread) return;
        if (timeoutMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));
        if (!thread.Join(timeoutMilliseconds))
            throw new TimeoutException($"Background worker '{thread.Name ?? "unnamed"}' did not finish within {timeoutMilliseconds} ms. Shared resources must not be disposed while it is still running.");
    }
}
