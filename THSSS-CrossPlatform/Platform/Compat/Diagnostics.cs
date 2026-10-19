using System;

namespace SlimDX.Direct3D9;

/// <summary>
/// Opt-in tracing used while verifying the OpenGL compatibility layer.
/// Everything is behind an environment switch so normal runs stay silent.
/// </summary>
internal static class Diagnostics
{
    private static readonly bool _enabled = Environment.GetEnvironmentVariable("THSSS_DIAG") == "1";
    private static int _budget;

    internal static bool IsEnabled => _enabled;

    internal static bool ShouldLog()
    {
        if (!_enabled) return false;
        lock (typeof(Diagnostics)) return _budget++ < 200;
    }

    internal static void Log(string message)
    {
        if (ShouldLog()) Console.WriteLine(message);
    }
}
