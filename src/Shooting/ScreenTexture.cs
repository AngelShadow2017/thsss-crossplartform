using System;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class ScreenTexture : IDisposable
{
    public Texture renderTexture { get; private set; }
    private Surface oldRenderSurface, oldDepthStencilSurface;
    private Surface renderSurface, renderDepthStencilSurface;
    private readonly Device DeviceMain;
    private readonly int WindowWidth, WindowHeight;

    public ScreenTexture(Device device, int width, int height)
    {
        DeviceMain = device;
        WindowWidth = width;
        WindowHeight = height;
        SetupRenderTexture();
    }

    private void SetupRenderTexture()
    {
        Texture texture = null;
        Surface surface = null, depth = null;
        try
        {
            texture = new Texture(DeviceMain,WindowWidth,WindowHeight,1,Usage.RenderTarget,Format.X8R8G8B8,Pool.Default);
            surface = texture.GetSurfaceLevel(0);
            depth = Surface.CreateDepthStencil(DeviceMain,WindowWidth,WindowHeight,Format.D16,MultiSampleType.None,0,true);
        }
        catch
        {
            depth?.Dispose(); surface?.Dispose(); texture?.Dispose();
            throw;
        }
        renderSurface?.Dispose(); renderDepthStencilSurface?.Dispose(); renderTexture?.Dispose();
        renderTexture = texture;
        renderSurface = surface;
        renderDepthStencilSurface = depth;
    }

    public void Begin()
    {
        oldRenderSurface = DeviceMain.GetRenderTarget(0);
        oldDepthStencilSurface = DeviceMain.DepthStencilSurface;
        DeviceMain.SetRenderTarget(0,renderSurface);
        DeviceMain.DepthStencilSurface = renderDepthStencilSurface;
        DeviceMain.Clear(ClearFlags.ZBuffer | ClearFlags.Target,new Color4(0f,0f,0f,0f),1f,0);
    }

    public void End()
    {
        DeviceMain.SetRenderTarget(0,oldRenderSurface);
        DeviceMain.DepthStencilSurface = oldDepthStencilSurface;
    }
    public void Reset() => SetupRenderTexture();
    public void Dispose()
    {
        renderSurface?.Dispose(); renderDepthStencilSurface?.Dispose(); renderTexture?.Dispose();
    }
}
