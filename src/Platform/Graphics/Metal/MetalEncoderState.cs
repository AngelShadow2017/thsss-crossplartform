namespace THSSS.Graphics.Metal;

public sealed partial class MetalRenderBackend
{
    private static void SetViewport(nint encoder, MetalNative.Viewport viewport)
        => MetalNative.SetViewport(encoder, viewport);
    private static void SetScissor(nint encoder, MetalNative.Scissor scissor)
        => MetalNative.SetScissor(encoder, scissor);
}
