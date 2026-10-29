using Silk.NET.OpenGL;
using THSSS.Graphics;
using THSSS.Graphics.OpenGl;

// Runs only where the OS can create the real native device. A missing device is reported
// as UNSUPPORTED and fails unless THSSS_REQUIRE_NATIVE_API=0, so CI cannot mistake a
// silent skip for a pass.
int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
static float[] Stereo(int frames) => new float[frames * 2];

string label;
try
{
    label = Run();
}
catch (PlatformNotSupportedException error)
{
    Console.WriteLine("UNSUPPORTED: " + error.Message);
    return Environment.GetEnvironmentVariable("THSSS_REQUIRE_NATIVE_API") == "0" ? 2 : 3;
}
Console.WriteLine($"PASS NativeApiCompatibility: {checks} checks {label}");
return 0;

string Run()
{
    if (OperatingSystem.IsMacOS()) return MetalAndCoreAudio();
    if (OperatingSystem.IsWindows())
    {
        Check(THSSS.Audio.AudioBackendSelection.Resolve(null) == "xaudio2", "Windows defaults to XAudio2");
        using var xaudio = THSSS.Audio.AudioBackendSelection.Create().CreateSource(22050);
        xaudio.Submit(Stereo(200), 22050);
        xaudio.Play();
        xaudio.Pause();
        xaudio.Clear();
        return "windows/xaudio2";
    }
    if (OperatingSystem.IsLinux())
    {
        Check(RenderBackendSelection.Requested == BackendKind.OpenGl, "Linux defaults to OpenGL");
        Check(THSSS.Audio.AudioBackendSelection.Resolve(null) == "openal", "Linux defaults to OpenAL");
        if (Environment.GetEnvironmentVariable("DISPLAY") is null && Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is null)
            throw new PlatformNotSupportedException("No display server for a real OpenGL context.");
        using var window = Silk.NET.Windowing.Window.Create(Silk.NET.Windowing.WindowOptions.Default with
        {
            IsVisible = false,
            API = Silk.NET.Windowing.GraphicsAPI.Default,
        });
        window.Initialize();
        using (var gl = GL.GetApi(window))
        using (var backend = new OpenGlRenderBackend(gl))
        {
            using var texture = backend.CreateRenderTexture(4, 4, TextureFormat.Rgba8);
            using var framebuffer = backend.CreateFramebuffer(texture, DepthFormat.Depth24, 4, 4);
            backend.BindFramebuffer(framebuffer);
            backend.Clear(ClearMask.Color, 0, 1, 0, 1, 1, 0);
            var pixel = backend.ReadPixels(1, 1, 1, 1);
            Check(pixel[1] > 200 && pixel[0] < 40, "real OpenGL clear/readback is green");
        }
        using var audio = THSSS.Audio.AudioBackendSelection.Create().CreateSource(22050);
        audio.Submit(Stereo(200), 22050);
        audio.Play();
        Check(audio.IsPlaying, "real OpenAL source plays");
        audio.Pause();
        audio.Clear();
        return "linux/opengl+openal";
    }
    throw new PlatformNotSupportedException("Unsupported platform.");
}

string MetalAndCoreAudio()
{
    Check(RenderBackendSelection.Requested == BackendKind.Metal, "macOS defaults to Metal");
    var metal = typeof(RenderBackendSelection).Assembly.GetType("THSSS.Graphics.Metal.MetalRenderBackend", true)!;
    using var backend = (IRenderBackend)Activator.CreateInstance(metal, new object?[] { (nint)0 })!;
    Check(backend.Kind == BackendKind.Metal, "real Metal device created");
    using var texture = backend.CreateRenderTexture(4, 4, TextureFormat.Rgba8);
    using var framebuffer = backend.CreateFramebuffer(texture, DepthFormat.Depth24, 4, 4);
    backend.BindFramebuffer(framebuffer);
    backend.Clear(ClearMask.Color, 0, 1, 0, 1, 1, 0);
    var pixel = backend.ReadPixels(1, 1, 1, 1);
    Check(pixel[1] > 200 && pixel[0] < 40, "real Metal clear/readback is green");
    Check(THSSS.Audio.AudioBackendSelection.Resolve(null) == "coreaudio", "macOS defaults to CoreAudio");
    using var audio = THSSS.Audio.AudioBackendSelection.Create().CreateSource(22050);
    audio.Submit(Stereo(200), 22050);
    audio.Play();
    audio.Pause();
    audio.Clear();
    return "macos/metal+coreaudio";
}
