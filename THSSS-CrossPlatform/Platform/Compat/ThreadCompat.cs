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
    /// A bounded join preserves the original intent: the thread is finished, or
    /// about to be, when control returns to the caller.
    /// </summary>
    public static void Stop(Thread thread, int timeoutMilliseconds = 5000)
    {
        if (thread == null || !thread.IsAlive) return;
        if (thread == Thread.CurrentThread) return;
        if (!thread.Join(timeoutMilliseconds))
            Console.Error.WriteLine($"[thread] worker did not stop within {timeoutMilliseconds} ms");
    }
}
