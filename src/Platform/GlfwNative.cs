using System;
using System.Runtime.InteropServices;
using Silk.NET.Windowing;

namespace THSSS.Platform;

/// <summary>
/// Native GLFW handle access. Silk.NET.GLFW 2.23 does not export the platform handle
/// getters, so they are bound here directly against the same libglfw the window uses.
/// Both X11 and Wayland are supported; the active platform comes from glfwGetPlatform
/// rather than from environment variables, because GLFW may be running on XWayland.
/// </summary>
internal static unsafe class GlfwNative
{
    private const string Library = "glfw";

    // Real GLFW values from glfw3.h (GLFW_PLATFORM_*): 0x00060001..0x00060005.
    internal enum Platform : int { Win32 = 0x00060001, Cocoa = 0x00060002, Wayland = 0x00060003, X11 = 0x00060004, Null = 0x00060005 }

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetPlatform")]
    private static extern int GetPlatform();

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetX11Display")]
    private static extern void* GetX11Display();

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetX11Window")]
    private static extern nint GetX11Window(nint window);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetWaylandDisplay")]
    private static extern void* GetWaylandDisplay();

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetWaylandWindow")]
    private static extern void* GetWaylandWindow(nint window);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetWin32Window")]
    private static extern nint GetWin32Window(nint window);

    [DllImport(Library, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowAspectRatio")]
    private static extern void SetWindowAspectRatio(nint window, int numerator, int denominator);

    internal static void ConstrainGameAspect(IWindow window)
    {
        nint pointer = WindowPointer(window);
        if (pointer == 0) throw new PlatformNotSupportedException("The window backend cannot apply the required 4:3 aspect constraint.");
        SetWindowAspectRatio(pointer, 4, 3);
    }

    internal static Platform ActivePlatform() => (Platform)GetPlatform();

    /// <summary>Raw <c>GLFWwindow*</c>, or zero when the window backend does not expose one.</summary>
    internal static nint WindowPointer(IWindow window)
    {
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        // GlfwWindow stores `internal unsafe WindowHandle* _glfwWindow`. Reflection boxes a
        // raw pointer as System.Reflection.Pointer, not IntPtr, so read it through that.
        var field = window.GetType().GetField("_glfwWindow", flags);
        if (field?.GetValue(window) is System.Reflection.Pointer pointer)
            return (nint)System.Reflection.Pointer.Unbox(pointer);
        return 0;
    }

    /// <summary>
    /// Fills the surface source for the platform GLFW is really using. Returns null when
    /// the platform is unsupported or the handles are unavailable, which leaves Vulkan
    /// unable to present; the caller must report unsupported window backends.
    /// </summary>
    internal static Graphics.Vulkan.VulkanSurfaceSource? Resolve(nint glfwWindow)
    {
        if (glfwWindow == 0) return null;
        try
        {
            return ActivePlatform() switch
            {
                Platform.X11 => new Graphics.Vulkan.VulkanSurfaceSource
                {
                    Platform = Graphics.Vulkan.VulkanPlatform.X11,
                    Display = (nint)GetX11Display(),
                    Window = GetX11Window(glfwWindow),
                },
                Platform.Wayland => new Graphics.Vulkan.VulkanSurfaceSource
                {
                    Platform = Graphics.Vulkan.VulkanPlatform.Wayland,
                    Display = (nint)GetWaylandDisplay(),
                    Window = (nint)GetWaylandWindow(glfwWindow),
                },
                Platform.Win32 => new Graphics.Vulkan.VulkanSurfaceSource
                {
                    Platform = Graphics.Vulkan.VulkanPlatform.Win32,
                    Window = GetWin32Window(glfwWindow),
                    Instance = GetModuleHandle(null),
                },
                _ => null,
            };
        }
        catch (DllNotFoundException) { return null; }
        catch (EntryPointNotFoundException) { return null; }
    }

    [DllImport("kernel32", EntryPoint = "GetModuleHandleW")]
    private static extern nint GetModuleHandle(string? name);
}
