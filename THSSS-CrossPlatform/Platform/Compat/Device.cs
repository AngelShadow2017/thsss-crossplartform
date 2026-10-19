// Direct3D9 device emulated with OpenGL for the cross-platform build.
using System;
using Silk.NET.OpenGL;
using SlimDX.DirectInput;

namespace SlimDX.Direct3D9;

public partial class Device : IDisposable
{
    private BlendingFactor _sourceBlend = BlendingFactor.SrcAlpha;
    private BlendingFactor _destinationBlend = BlendingFactor.OneMinusSrcAlpha;
    private Device3D _pipeline3D;

    private Device3D Pipeline3D => _pipeline3D ??= new Device3D(GL);

    public Device() { }

    public Device(GL gl, int width, int height)
    {
        GL = gl;
        CurrentContext = gl;
        Width = width;
        Height = height;
        Viewport = new Viewport(0, 0, width, height, 0f, 1f);
    }

    /// <summary>
    /// Adopts the GL context owned by the window layer. The game reaches this
    /// constructor through Direct3D.CreateDevice after Program set CurrentContext.
    /// </summary>
    public Device(Direct3D direct3D, int adapter, DeviceType deviceType, IntPtr handle,
                  CreateFlags createFlags, PresentParameters parameters)
    {
        GL = CurrentContext ?? throw new InvalidOperationException(
            "Direct3D9.Device requires the OpenGL context published by the window layer.");
        Width = parameters.BackBufferWidth > 0 ? parameters.BackBufferWidth : 640;
        Height = parameters.BackBufferHeight > 0 ? parameters.BackBufferHeight : 480;
        GameWidth = Width;
        GameHeight = Height;
        Viewport = new Viewport(0, 0, Width, Height, 0f, 1f);
    }

    public static GL CurrentContext { get; set; }

    public GL GL { get; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    /// <summary>Logical resolution the game was authored for.</summary>
    public int GameWidth { get; set; } = 640;

    public int GameHeight { get; set; } = 480;

    /// <summary>Physical window framebuffer size, published by the window layer.</summary>
    public int FramebufferWidth { get; set; }

    public int FramebufferHeight { get; set; }

    public Viewport Viewport { get; set; }

    public Surface RenderTarget { get; private set; }

    public Surface GetRenderTarget(int index) => RenderTarget;

    public Surface DepthStencilSurface { get; set; }

    // Fixed-function shader state. The 3D scenes set these before issuing draw
    // calls; the OpenGL path renders sprites only, so they are retained, not applied.
    public VertexDeclaration VertexDeclaration { get; set; }

    public VertexFormat VertexFormat { get; set; }

    public Material Material { get; set; }

    public VertexShader VertexShader { get; set; }

    public PixelShader PixelShader { get; set; }

    public Direct3D Direct3D { get; } = new();

    public bool IsDisposed => false;

    public Capabilities Capabilities => new();

    // ---- render state -----------------------------------------------------
    public void SetRenderState(RenderState state, int value)
    {
        if (GL == null) return;
        switch (state)
        {
            case RenderState.AlphaBlendEnable:
                SetCapability(Silk.NET.OpenGL.EnableCap.Blend, value != 0);
                break;
            case RenderState.ZEnable:
                SetCapability(Silk.NET.OpenGL.EnableCap.DepthTest, value != 0);
                break;
            case RenderState.ScissorTestEnable:
                SetCapability(Silk.NET.OpenGL.EnableCap.ScissorTest, value != 0);
                break;
            case RenderState.SrcBlend: // same slot as SourceBlend (19)
                _sourceBlend = ToBlendingFactor(value);
                GL.BlendFunc(_sourceBlend, _destinationBlend);
                break;
            case RenderState.DestBlend: // same slot as DestinationBlend (20)
                _destinationBlend = ToBlendingFactor(value);
                GL.BlendFunc(_sourceBlend, _destinationBlend);
                break;
        }
    }

    public void SetRenderState(RenderState state, bool value) => SetRenderState(state, value ? 1 : 0);
    public void SetRenderState(RenderState state, float value) => SetRenderState(state, (int)value);
    public void SetRenderState(RenderState state, Blend value) => SetRenderState(state, (int)value);
    public void SetRenderState(RenderState state, BlendOperation value) { }
    public void SetRenderState(RenderState state, Compare value) { }
    public void SetRenderState(RenderState state, Cull value) { }
    public void SetRenderState(RenderState state, FillMode value) { }
    public void SetRenderState(RenderState state, ShadeMode value) { }
    public void SetRenderState(RenderState state, FogMode value) { }
    public void SetRenderState(RenderState state, TransformType value) { }

    public int GetRenderState(RenderState state) => 0;

    private void SetCapability(Silk.NET.OpenGL.EnableCap capability, bool enabled)
    {
        if (enabled) GL.Enable(capability);
        else GL.Disable(capability);
    }

    /// <summary>D3DBLEND_* constants to GL factors. 1 is ZERO and 2 is ONE.</summary>
    private static BlendingFactor ToBlendingFactor(int value) => value switch
    {
        1 => BlendingFactor.Zero,
        2 => BlendingFactor.One,
        3 => BlendingFactor.SrcColor,
        4 => BlendingFactor.OneMinusSrcColor,
        5 => BlendingFactor.SrcAlpha,
        6 => BlendingFactor.OneMinusSrcAlpha,
        7 => BlendingFactor.DstAlpha,
        8 => BlendingFactor.OneMinusDstAlpha,
        9 => BlendingFactor.DstColor,
        10 => BlendingFactor.OneMinusDstColor,
        _ => BlendingFactor.One,
    };

    public void SetSamplerState(int sampler, SamplerState state, int value) { }
    public void SetSamplerState(int sampler, SamplerState state, TextureFilter value) { }
    public void SetSamplerState(int sampler, SamplerState state, TextureAddress value) { }
    public void SetTextureStageState(int stage, TextureStage state, int value) { }
    public void SetTextureStageState(int stage, TextureStage state, TextureOperation value) { }
    public void SetTextureStageState(int stage, TextureStage state, TextureArgument value) { }
    public void SetTextureStageState(int stage, TextureStage state, TextureFilterType value) { }
    public void SetTexture(int stage, Texture texture)
    {
        if (stage == 0) Pipeline3D.SetTexture(texture);
    }
    public void SetTransform(TransformType type, Matrix matrix)
        => SetTransform((TransformState)(int)type, matrix);

    public void SetTransform(TransformState type, Matrix matrix)
    {
        if (type is TransformState.World or TransformState.View or TransformState.Projection)
            Pipeline3D.SetTransform(type, matrix);
    }

    /// <summary>D3D's default projection is identity; 3D backgrounds rely on the
    /// game setting View/World only, so a perspective matrix is installed for
    /// the first 3D draw of a frame when the game has not set one.</summary>
    internal void EnsureDefaultProjection()
    {
        if (Pipeline3D.Projection == System.Numerics.Matrix4x4.Identity)
        {
            float aspect = (float)Math.Max(1, GameWidth) / Math.Max(1, GameHeight);
            Pipeline3D.SetTransform(TransformState.Projection,
                Matrix.PerspectiveFovLH(0.7853982f, aspect, 0.1f, 10000f));
        }
    }
    public Matrix GetTransform(TransformType type)
    {
        var m = Pipeline3D.Projection;
        // Only the projection has a synthetic default; the rest round-trip as identity.
        if (type != TransformType.Projection) return Matrix.Identity;
        return new Matrix
        {
            M11 = m.M11, M12 = m.M12, M13 = m.M13, M14 = m.M14,
            M21 = m.M21, M22 = m.M22, M23 = m.M23, M24 = m.M24,
            M31 = m.M31, M32 = m.M32, M33 = m.M33, M34 = m.M34,
            M41 = m.M41, M42 = m.M42, M43 = m.M43, M44 = m.M44,
        };
    }
    public void SetMaterial(ref Material material) => Material = material;
    public void SetLight(int index, ref Light light) { }
    public void SetLight(int index, Light light) { }
    public void EnableLight(int index, bool enable) { }
    public void SetViewport(Viewport viewport) => Viewport = viewport;
    public Viewport GetViewport() => Viewport;

    public void BeginScene() { }

    public void EndScene() { }

    /// <summary>GLFW presents the default framebuffer after the render callback.</summary>
    public void Present() { }

    public void Present(Present flags) { }

    // ---- immediate geometry ----------------------------------------------
    // The game only draws 2D sprites through Sprite; 3D primitives are unused,
    // so these stay no-ops instead of pretending to render.
    public void DrawUserPrimitives<T>(PrimitiveType type, int primitiveCount, T[] data) where T : struct { }
    public void DrawUserPrimitives<T>(PrimitiveType type, int primitiveCount, T[] data, VertexFormat format) where T : struct { }
    public void DrawPrimitives(PrimitiveType type, int startVertex, int primitiveCount)
    {
        EnsureDefaultProjection();
        ApplyBlendState();
        Pipeline3D.Draw(type, startVertex, primitiveCount);
    }
    public void DrawIndexedPrimitives(PrimitiveType type, int baseVertex, int startVertex, int vertexCount, int startIndex, int primitiveCount) { }
    public void SetStreamSource(int streamNumber, VertexBuffer streamData, int offsetInBytes, int stride)
    {
        _streamOffset = offsetInBytes;
        Pipeline3D.SetStreamSource(streamData);
    }

    private int _streamOffset;

    /// <summary>Blend factor currently requested through SetRenderState.</summary>
    internal BlendingFactor CurrentSourceBlend => _sourceBlend;
    internal BlendingFactor CurrentDestinationBlend => _destinationBlend;

    /// <summary>Applies the blend mode the game configured for the next draw.</summary>
    internal void ApplyBlendState()
    {
        GL.Enable(Silk.NET.OpenGL.EnableCap.Blend);
        GL.BlendFunc(_sourceBlend, _destinationBlend);
    }

    /// <summary>Resets blend to the sprite batcher's default (SrcAlpha/1-SrcAlpha).</summary>
    internal void ResetBlendState()
    {
        _sourceBlend = BlendingFactor.SrcAlpha;
        _destinationBlend = BlendingFactor.OneMinusSrcAlpha;
        GL.BlendFunc(_sourceBlend, _destinationBlend);
    }
    public void SetIndices(IndexBuffer indexData) => Pipeline3D.SetIndices(indexData);
    public void SetVertexDeclaration(VertexDeclaration declaration)
    {
        VertexDeclaration = declaration;
        Pipeline3D.SetVertexDeclaration(declaration);
    }
    public void SetVertexShader(VertexShader shader) { }
    public void SetPixelShader(PixelShader shader) { }
    public void SetFVF(VertexFormat format) { }

    public Surface GetBackBuffer(int swapChain, int index) => new(GL, GameWidth, GameHeight, false);
    public Surface GetDepthStencilSurface() => DepthStencilSurface;

    public void SetRenderTarget(int index, Surface surface)
    {
        if (GL == null) return;
        RenderTarget = surface;
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, surface?.FBO ?? 0);
    }

    public void SetDepthStencilSurface(Surface surface) => DepthStencilSurface = surface;

    public void Clear(ClearFlags flags, System.Drawing.Color color, float z, int stencil)
    {
        if (GL == null) return;
        GL.Disable(Silk.NET.OpenGL.EnableCap.ScissorTest);
        int width = RenderTarget?.Width ?? Math.Max(1, FramebufferWidth > 0 ? FramebufferWidth : Viewport.Width);
        int height = RenderTarget?.Height ?? Math.Max(1, FramebufferHeight > 0 ? FramebufferHeight : Viewport.Height);
        GL.Viewport(0, 0, (uint)width, (uint)height);
        GL.ClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        GL.Clear(ToClearMask(flags));
    }

    public void Clear(ClearFlags flags, int color, float z, int stencil)
        => Clear(flags, System.Drawing.Color.FromArgb((color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF), z, stencil);

    public void Clear(ClearFlags flags, Color4 color, float z, int stencil)
    {
        if (GL == null) return;
        GL.Disable(Silk.NET.OpenGL.EnableCap.ScissorTest);
        GL.ClearColor(color.Red, color.Green, color.Blue, color.Alpha);
        GL.Clear(ToClearMask(flags));
    }

    private static ClearBufferMask ToClearMask(ClearFlags flags)
    {
        var mask = ClearBufferMask.None;
        if ((flags & ClearFlags.Target) != 0) mask |= ClearBufferMask.ColorBufferBit;
        if ((flags & ClearFlags.ZBuffer) != 0) mask |= ClearBufferMask.DepthBufferBit;
        if ((flags & ClearFlags.Stencil) != 0) mask |= ClearBufferMask.StencilBufferBit;
        return mask;
    }

    public Surface CreateOffscreenPlainSurface(int width, int height, Format format, Pool pool)
        => new(GL, width, height, false);

    public Surface CreateRenderTarget(int width, int height, Format format, MultiSampleType multiSample, int quality, bool lockable)
        => new(GL, width, height, true);

    public Texture CreateTexture(int width, int height, int levels, Usage usage, Format format, Pool pool)
        => new(this, width, height, levels, usage, format, pool);

    public VertexBuffer CreateVertexBuffer(int size, Usage usage, VertexFormat format, Pool pool)
        => new(this, size, usage, format, pool);

    public IndexBuffer CreateIndexBuffer(int size, Usage usage, Format format, Pool pool)
        => new(this, size, usage, pool);

    public VertexDeclaration CreateVertexDeclaration(VertexElement[] elements) => new(this, elements);

    public void DrawText(Sprite sprite, string text, System.Drawing.Rectangle rect, DrawTextFormat flags, System.Drawing.Color color) { }
    public void DrawText(Font font, string text, System.Drawing.Rectangle rect, DrawTextFormat flags, System.Drawing.Color color) { }

    public StateBlock CreateStateBlock(StateBlockType type) => new(this, type);

    /// <summary>The emulated device never loses its context.</summary>
    public Result TestCooperativeLevel() => Result.Ok;

    public void Reset(PresentParameters parameters) { }

    public void Dispose() { }
}
