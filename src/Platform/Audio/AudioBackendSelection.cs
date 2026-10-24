using System;

namespace THSSS.Audio;

public static class AudioBackendSelection
{
    public static string Resolve(string value)
    {
        string name = (value ?? "auto").Trim().ToLowerInvariant();
        return name switch
        {
            "" or "auto" => DefaultBackend(),
            "openal" => "openal",
            "xaudio2" when OperatingSystem.IsWindows() => "xaudio2",
            "xaudio2" => throw new PlatformNotSupportedException("The xaudio2 audio backend requires Windows 10/11 (XAudio2 2.9)."),
            "coreaudio" when OperatingSystem.IsMacOS() => "coreaudio",
            "coreaudio" => throw new PlatformNotSupportedException("The coreaudio backend requires macOS."),
            _ => throw new ArgumentException("Unknown THSSS_AUDIO_BACKEND '" + value + "'. Supported: auto, openal, xaudio2, coreaudio.", nameof(value)),
        };
    }

    private static string DefaultBackend() =>
        OperatingSystem.IsWindows() ? "xaudio2" : OperatingSystem.IsMacOS() ? "coreaudio" : "openal";

    internal static IAudioBackend Create()
    {
        string name = Resolve(Environment.GetEnvironmentVariable("THSSS_AUDIO_BACKEND"));
        if (name == "openal") return new OpenAlBackend();
#if THSSS_MACOS
        if (name == "coreaudio") return new CoreAudioBackend();
#endif
#if THSSS_WINDOWS
        return new XAudio2Backend();
#else
        throw new PlatformNotSupportedException("XAudio2 is not compiled into this build. Use a Windows-targeted build (-r win-x64) or select OpenAL.");
#endif
    }
}
