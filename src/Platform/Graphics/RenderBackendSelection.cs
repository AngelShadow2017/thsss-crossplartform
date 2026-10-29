using System;

namespace THSSS.Graphics;

/// <summary>Selects a native backend without silently substituting another API.</summary>
public static class RenderBackendSelection
{
    public const string Variable = "THSSS_RENDERER";
    public static Vulkan.VulkanSurfaceSource? SurfaceSource { get; set; }
    public static nint CocoaWindow { get; set; }
    public static BackendKind Requested { get; } = Parse(Environment.GetEnvironmentVariable(Variable));

    private static BackendKind Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return OperatingSystem.IsMacOS() ? BackendKind.Metal : BackendKind.OpenGl;
        return value.Trim().ToLowerInvariant() switch
        {
            "gl" or "opengl" or "gl33" => BackendKind.OpenGl,
            "vk" or "vulkan" => BackendKind.Vulkan,
            "metal" => BackendKind.Metal,
            _ => throw new ArgumentException($"Unknown {Variable} value '{value}'. Supported: gl, vk, metal.", nameof(value)),
        };
    }
}
