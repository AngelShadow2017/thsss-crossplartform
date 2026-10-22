using System;

namespace THSSS.Graphics;

/// <summary>
/// Chooses the render backend at startup. `THSSS_RENDERER=gl|vk` overrides the default,
/// which stays OpenGL. An unavailable or misconfigured backend fails loudly instead of
/// silently falling back, so a run never claims one backend while using another.
/// </summary>
public static class RenderBackendSelection
{
    public const string Variable = "THSSS_RENDERER";

    /// <summary>
    /// Native presentation handles when the window exposes them. Set before the first
    /// Device is created; presenting backends read it while constructing their context.
    /// </summary>
    public static Vulkan.VulkanSurfaceSource? SurfaceSource { get; set; }

    public static BackendKind Requested { get; } = Parse(Environment.GetEnvironmentVariable(Variable));

    private static BackendKind Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return BackendKind.OpenGl;
        return value.Trim().ToLowerInvariant() switch
        {
            "gl" or "opengl" or "gl33" => BackendKind.OpenGl,
            "vk" or "vulkan" => BackendKind.Vulkan,
            _ => throw new ArgumentException(
                $"Unknown {Variable} value '{value}'. Supported: gl, vk.", nameof(value)),
        };
    }
}
