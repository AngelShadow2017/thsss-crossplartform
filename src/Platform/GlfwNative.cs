using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Windowing;

namespace THSSS.Platform;

internal static unsafe class GlfwNative
{
    private const string Library="glfw";
#if THSSS_WINDOWS || THSSS_MACOS
    static GlfwNative()
    {
        NativeLibrary.SetDllImportResolver(typeof(GlfwNative).Assembly,(name,assembly,searchPath)=>
        {
            if(name!=Library)return 0;
#if THSSS_MACOS
            const string nativeName="libglfw.3.dylib";
#else
            const string nativeName="glfw3";
#endif
            return NativeLibrary.TryLoad(nativeName,assembly,searchPath,out var handle)?handle:0;
        });
    }
#endif
    internal enum Platform:int{Win32=0x00060001,Cocoa=0x00060002,Wayland=0x00060003,X11=0x00060004,Null=0x00060005}
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetPlatform")]private static extern int GetPlatform();
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetX11Display")]private static extern void* GetX11Display();
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetX11Window")]private static extern nint GetX11Window(nint window);
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetWaylandDisplay")]private static extern void* GetWaylandDisplay();
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetWaylandWindow")]private static extern void* GetWaylandWindow(nint window);
#if THSSS_MACOS
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetCocoaWindow")]private static extern nint GetCocoaWindow(nint window);
#endif
#if THSSS_WINDOWS
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwGetWin32Window")]private static extern nint GetWin32Window(nint window);
    [DllImport("kernel32",EntryPoint="GetModuleHandleW")]private static extern nint GetModuleHandle(string? name);
#endif
    [DllImport(Library,CallingConvention=CallingConvention.Cdecl,EntryPoint="glfwSetWindowAspectRatio")]private static extern void SetWindowAspectRatio(nint window,int numerator,int denominator);
    internal static void ConstrainGameAspect(IWindow window)
    {nint p=WindowPointer(window);if(p==0)throw new PlatformNotSupportedException("The window backend cannot apply the required 4:3 aspect constraint.");SetWindowAspectRatio(p,4,3);}
    internal static Platform ActivePlatform()=>(Platform)GetPlatform();
    internal static nint WindowPointer(IWindow window)
    {
        var f=window.GetType().GetField("_glfwWindow",BindingFlags.NonPublic|BindingFlags.Instance);
        return f?.GetValue(window) is Pointer p?(nint)Pointer.Unbox(p):0;
    }
    internal static nint CocoaWindow(nint glfwWindow)
    {
#if THSSS_MACOS
        if(glfwWindow==0)throw new InvalidOperationException("Metal requires a valid GLFW window.");
        nint window=GetCocoaWindow(glfwWindow);
        if(window==0)throw new InvalidOperationException("GLFW did not provide a Cocoa NSWindow for Metal.");
        return window;
#else
        throw new PlatformNotSupportedException("Cocoa NSWindow handles require a macOS build.");
#endif
    }
    internal static Graphics.Vulkan.VulkanSurfaceSource? Resolve(nint glfwWindow)
    {
        if(glfwWindow==0)return null;
        try
        {
            var p=ActivePlatform();
#if THSSS_WINDOWS
            if(p==Platform.Win32)return new Graphics.Vulkan.VulkanSurfaceSource{Platform=Graphics.Vulkan.VulkanPlatform.Win32,Window=GetWin32Window(glfwWindow),Instance=GetModuleHandle(null)};
#endif
            if(p==Platform.X11)return new Graphics.Vulkan.VulkanSurfaceSource{Platform=Graphics.Vulkan.VulkanPlatform.X11,Display=(nint)GetX11Display(),Window=GetX11Window(glfwWindow)};
            if(p==Platform.Wayland)return new Graphics.Vulkan.VulkanSurfaceSource{Platform=Graphics.Vulkan.VulkanPlatform.Wayland,Display=(nint)GetWaylandDisplay(),Window=(nint)GetWaylandWindow(glfwWindow)};
            return null;
        }
        catch(DllNotFoundException){return null;}
        catch(EntryPointNotFoundException){return null;}
    }
}
