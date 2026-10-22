using System;

namespace THSSS.Graphics.Vulkan;

/// <summary>
/// The native handles a platform must supply to create a Vulkan presentation surface.
/// Exactly one of the platform variants is meaningful; the rest stay zero.
/// </summary>
public sealed class VulkanSurfaceSource
{
    public required VulkanPlatform Platform { get; init; }
    public IntPtr Display { get; init; }
    public IntPtr Window { get; init; }
    public IntPtr Instance { get; init; }
}

public enum VulkanPlatform { X11, Wayland, Win32, Cocoa }

public static class VulkanSurfaceExtensions
{
    /// <summary>Instance extensions required for presenting on the given platform.</summary>
    public static string[] RequiredExtensions(VulkanPlatform platform) => platform switch
    {
        VulkanPlatform.X11 => new[] { "VK_KHR_surface", "VK_KHR_xlib_surface" },
        VulkanPlatform.Wayland => new[] { "VK_KHR_surface", "VK_KHR_wayland_surface" },
        VulkanPlatform.Win32 => new[] { "VK_KHR_surface", "VK_KHR_win32_surface" },
        VulkanPlatform.Cocoa => new[] { "VK_KHR_surface", "VK_EXT_metal_surface" },
        _ => new[] { "VK_KHR_surface" },
    };
}
