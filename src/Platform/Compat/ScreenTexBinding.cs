// Offscreen scene texture binding for SlimDX.Direct3D9.Device.
//
// The stage composites itself through ScreenTexManager: Begin() renders into a
// texture, End() restores the back buffer and the result is drawn as a sprite so
// the Warp effect can post-process it. The original build used
// Device.SetRenderTarget + Surface.GetSurfaceLevel; this file keeps that binding
// separate from Device.cs so the render-target plumbing stays in one place.
namespace SlimDX.Direct3D9;

public partial class Device
{
    /// <summary>True while drawing into an offscreen texture instead of the window.</summary>
    public bool IsRenderingToTexture => RenderTarget != null;
}
