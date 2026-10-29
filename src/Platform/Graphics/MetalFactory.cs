using System;

namespace THSSS.Graphics;

internal static class MetalFactory
{
    internal static IRenderBackend Create(nint cocoaWindow)
    {
#if THSSS_MACOS
        return new Metal.MetalRenderBackend(cocoaWindow);
#else
        throw new PlatformNotSupportedException("Metal is only compiled in macOS-targeted builds.");
#endif
    }
}
