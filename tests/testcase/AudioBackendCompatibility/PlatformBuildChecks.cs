using System.Reflection;
using System.Runtime.CompilerServices;
using THSSS.Audio;

// Guards the *artifact*, not the runtime: a ModuleInitializer runs before Main, so this check
// fails the test binary itself if the compiled assembly contains the wrong platform surface.
//
// Why it exists: the backends are selected with #if by target RID, and a #if mistake does not
// break compilation — it silently deletes a whole backend (or leaves a Windows-only P/Invoke in a
// Linux/macOS build) and the game still starts, only failing at the first sound or first window
// creation. Asserting "present iff Windows" pins the intended shape: XAudio2 and the Win32 HWND
// bindings must exist only in Windows-targeted artifacts and must never be compiled away on
// Windows, so the same source cannot quietly ship a build with no audio backend at all.
internal static class PlatformBuildChecks
{
    [ModuleInitializer]
    internal static void Verify()
    {
        Assembly game = typeof(AudioBackendSelection).Assembly;
        bool windows = OperatingSystem.IsWindows();
        bool hasXAudio = game.GetType("THSSS.Audio.XAudio2Backend") != null;
        Type glfw = game.GetType("THSSS.Platform.GlfwNative", throwOnError: true)!;
        bool hasModuleHandle = glfw.GetMethod("GetModuleHandle", BindingFlags.Static | BindingFlags.NonPublic) != null;
        bool hasWin32Window = glfw.GetMethod("GetWin32Window", BindingFlags.Static | BindingFlags.NonPublic) != null;
        // Both directions are checked (== not just "missing on non-Windows"): presence on the
        // wrong OS means an #if leaked, absence on Windows means a backend was dropped.
        if (hasXAudio != windows || hasModuleHandle != windows || hasWin32Window != windows)
            throw new InvalidOperationException("Platform build mismatch: Windows audio/Win32 bindings must exist only in Windows-targeted artifacts.");
        Console.WriteLine("PASS platform build: Windows audio and Win32 imports " + (windows ? "present" : "excluded"));
    }
}
