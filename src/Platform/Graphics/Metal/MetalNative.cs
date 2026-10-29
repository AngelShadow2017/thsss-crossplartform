#nullable enable
using System;
using System.Runtime.InteropServices;

namespace THSSS.Graphics.Metal;

internal static unsafe class MetalNative
{
    private const string ObjC = "/usr/lib/libobjc.A.dylib";
    [DllImport("/System/Library/Frameworks/Metal.framework/Metal")] internal static extern nint MTLCreateSystemDefaultDevice();
    [DllImport(ObjC)] internal static extern nint objc_getClass(string name);
    [DllImport(ObjC)] private static extern nint sel_registerName(string name);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern nint Send0(nint obj, nint sel);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern nint Send1(nint obj, nint sel, nint a);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern nint Send2(nint obj, nint sel, nint a, nint b);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern nint Send3(nint obj, nint sel, nint a, nint b, nint c);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern nint Send4(nint obj, nint sel, nint a, nint b, nint c, nint d);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendDouble(nint obj, nint sel, double value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendFloat(nint obj, nint sel, float value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendColor(nint obj, nint sel, ClearColor value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendViewport(nint obj, nint sel, Viewport value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendScissor(nint obj, nint sel, Scissor value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendSize(nint obj, nint sel, Size value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void Replace(nint obj, nint sel, Region region, nuint level, void* bytes, nuint pitch);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void Get(nint obj, nint sel, void* bytes, nuint pitch, Region region, nuint level);
    internal static nint Call(nint obj, string sel) => Send0(obj, sel_registerName(sel));
    internal static nint Call(nint obj, string sel, nint a) => Send1(obj, sel_registerName(sel), a);
    internal static nint Call(nint obj, string sel, nint a, nint b) => Send2(obj, sel_registerName(sel), a, b);
    internal static nint Call(nint obj, string sel, nint a, nint b, nint c) => Send3(obj, sel_registerName(sel), a, b, c);
    internal static nint Call(nint obj, string sel, nint a, nint b, nint c, nint d) => Send4(obj, sel_registerName(sel), a, b, c, d);
    internal static nint New(string type) => Require(Call(Call(objc_getClass(type), "alloc"), "init"), type);
    internal static nint Require(nint value, string operation) => value != 0 ? value : throw new InvalidOperationException("Metal failed: " + operation);
    internal static nint Text(string text)
    {
        nint bytes = Marshal.StringToCoTaskMemUTF8(text);
        try { return Require(Call(objc_getClass("NSString"), "stringWithUTF8String:", bytes), "NSString"); }
        finally { Marshal.FreeCoTaskMem(bytes); }
    }
    internal static string Error(nint error) => error == 0 ? "no NSError supplied" : Marshal.PtrToStringUTF8(Call(Call(error, "localizedDescription"), "UTF8String")) ?? "unknown NSError";
    internal static void Release(nint obj) { if (obj != 0) Call(obj, "release"); }
    internal static void Double(nint obj, string sel, double value) => SendDouble(obj, sel_registerName(sel), value);
    internal static void Float(nint obj, string sel, float value) => SendFloat(obj, sel_registerName(sel), value);
    internal static void Color(nint obj, ClearColor value) => SendColor(obj, sel_registerName("setClearColor:"), value);
    internal static void SetViewport(nint obj, Viewport value) => SendViewport(obj, sel_registerName("setViewport:"), value);
    internal static void SetScissor(nint obj, Scissor value) => SendScissor(obj, sel_registerName("setScissorRect:"), value);
    internal static void DrawableSize(nint obj, int w, int h) => SendSize(obj, sel_registerName("setDrawableSize:"), new(w, h));
    internal static void Upload(nint texture, int w, int h, void* bytes, int pitch) => Replace(texture, sel_registerName("replaceRegion:mipmapLevel:withBytes:bytesPerRow:"), new(0, 0, 0, (nuint)w, (nuint)h, 1), 0, bytes, (nuint)pitch);
    internal static void Read(nint texture, int x, int y, int w, int h, void* bytes) => Get(texture, sel_registerName("getBytes:bytesPerRow:fromRegion:mipmapLevel:"), bytes, (nuint)(w * 4), new((nuint)x, (nuint)y, 0, (nuint)w, (nuint)h, 1), 0);
    [StructLayout(LayoutKind.Sequential)] internal readonly record struct ClearColor(double R, double G, double B, double A);
    [StructLayout(LayoutKind.Sequential)] internal readonly record struct Viewport(double X, double Y, double Width, double Height, double Near, double Far);
    [StructLayout(LayoutKind.Sequential)] internal readonly record struct Scissor(nuint X, nuint Y, nuint Width, nuint Height);
    [StructLayout(LayoutKind.Sequential)] private readonly record struct Size(double Width, double Height);
    [StructLayout(LayoutKind.Sequential)] private readonly record struct Region(nuint X, nuint Y, nuint Z, nuint Width, nuint Height, nuint Depth);
}
