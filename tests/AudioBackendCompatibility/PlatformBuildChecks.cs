using System.Reflection;
using System.Runtime.CompilerServices;
using THSSS.Audio;

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
        if (hasXAudio != windows || hasModuleHandle != windows || hasWin32Window != windows)
            throw new InvalidOperationException("Platform build mismatch: Windows audio/Win32 bindings must exist only in Windows-targeted artifacts.");
        Console.WriteLine("PASS platform build: Windows audio and Win32 imports " + (windows ? "present" : "excluded"));
    }
}
