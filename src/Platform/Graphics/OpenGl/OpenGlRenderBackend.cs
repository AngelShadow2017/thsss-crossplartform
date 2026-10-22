using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using THSSS.Graphics;

namespace THSSS.Graphics.OpenGl;

/// <summary>
/// OpenGL 3.3 core implementation. This is the existing renderer moved behind the
/// backend interface without changing its behaviour: the same shader versions,
/// the same D3D9 blend/sampler value tables and the same framebuffer handling.
/// </summary>
internal sealed class OpenGlRenderBackend : IRenderBackend
{
    private readonly GL _gl;
    private uint _vao;
    private bool _vaoCreated;
    private int _framebufferWidth = 640;
    private int _framebufferHeight = 480;
    private bool _scissorEnabled;

    internal OpenGlRenderBackend(GL gl) => _gl = gl;

    internal GL GL => _gl;

    public BackendKind Kind => BackendKind.OpenGl;

    public BackendFeatures Features => new(
        SupportsSamplerObjects: true,
        SupportsRgb8Textures: true,
        OriginBottomLeft: true,
        MaxColourAttachments: 8,
        ShaderLanguage: "glsl-330");

    public void Resize(int framebufferWidth, int framebufferHeight)
    {
        _framebufferWidth = Math.Max(1, framebufferWidth);
        _framebufferHeight = Math.Max(1, framebufferHeight);
    }

    public int FramebufferWidth => _framebufferWidth;
    public int FramebufferHeight => _framebufferHeight;

    // ---- resources -------------------------------------------------------

    public IRenderTexture CreateTexture(int width, int height, TextureFormat format, ReadOnlySpan<byte> pixels)
    {
        var texture = new OpenGlTexture(_gl, width, height, format, levels: 1);
        if (!pixels.IsEmpty) texture.Upload(pixels, width * (format == TextureFormat.Rgb8 ? 3 : 4));
        return texture;
    }

    public IRenderTexture CreateRenderTexture(int width, int height, TextureFormat format)
        => new OpenGlTexture(_gl, width, height, format, levels: 1);

    public IRenderBuffer CreateBuffer(BufferKind kind, int sizeInBytes)
        => new OpenGlBuffer(_gl, kind, sizeInBytes);

    public IRenderShader CreateShader(ShaderSource source)
        => new OpenGlShader(_gl, source);

    public void SetCapability(BackendCapability capability, bool enabled)
    {
        var cap = capability switch
        {
            BackendCapability.DepthTest => Silk.NET.OpenGL.EnableCap.DepthTest,
            BackendCapability.ScissorTest => Silk.NET.OpenGL.EnableCap.ScissorTest,
            BackendCapability.Multisample => Silk.NET.OpenGL.EnableCap.Multisample,
            _ => Silk.NET.OpenGL.EnableCap.CullFace,
        };
        if (enabled) _gl.Enable(cap); else _gl.Disable(cap);
    }

    public void SetBlendState(BlendState state)
    {
        if (state.Enabled) _gl.Enable(Silk.NET.OpenGL.EnableCap.Blend);
        else _gl.Disable(Silk.NET.OpenGL.EnableCap.Blend);
        _gl.BlendFunc(ToGl(state.Source), ToGl(state.Destination));
        _gl.BlendEquation(state.Equation switch
        {
            BlendEquation.Subtract => BlendEquationModeEXT.FuncSubtract,
            BlendEquation.ReverseSubtract => BlendEquationModeEXT.FuncReverseSubtract,
            BlendEquation.Min => BlendEquationModeEXT.Min,
            BlendEquation.Max => BlendEquationModeEXT.Max,
            _ => BlendEquationModeEXT.FuncAdd,
        });
    }

    private static BlendingFactor ToGl(BlendFactor f) => f switch
    {
        BlendFactor.Zero => BlendingFactor.Zero,
        BlendFactor.One => BlendingFactor.One,
        BlendFactor.SourceColor => BlendingFactor.SrcColor,
        BlendFactor.InverseSourceColor => BlendingFactor.OneMinusSrcColor,
        BlendFactor.SourceAlpha => BlendingFactor.SrcAlpha,
        BlendFactor.InverseSourceAlpha => BlendingFactor.OneMinusSrcAlpha,
        BlendFactor.DestinationColor => BlendingFactor.DstColor,
        BlendFactor.InverseDestinationColor => BlendingFactor.OneMinusDstColor,
        BlendFactor.DestinationAlpha => BlendingFactor.DstAlpha,
        BlendFactor.InverseDestinationAlpha => BlendingFactor.OneMinusDstAlpha,
        _ => BlendingFactor.SrcAlphaSaturate,
    };

    public void AttachDepth(IRenderDepthSurface? depth)
    {
        if (_gl.GetInteger(GLEnum.DrawFramebufferBinding) == 0) return;
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer,
            depth is OpenGlDepthSurface glDepth ? glDepth.Id : 0);
    }

    public (bool DepthTest, bool CullFace) QueryRasterState()
        => (_gl.IsEnabled(Silk.NET.OpenGL.EnableCap.DepthTest),
            _gl.IsEnabled(Silk.NET.OpenGL.EnableCap.CullFace));

    public void SetRasterState(bool depthTest, bool cullFace)
    {
        if (depthTest) _gl.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
        else _gl.Disable(Silk.NET.OpenGL.EnableCap.DepthTest);
        if (cullFace) _gl.Enable(Silk.NET.OpenGL.EnableCap.CullFace);
        else _gl.Disable(Silk.NET.OpenGL.EnableCap.CullFace);
    }

    public IRenderDepthSurface CreateDepthSurface(int width, int height, DepthFormat format)
    {
        uint previous = (uint)_gl.GetInteger(GLEnum.RenderbufferBinding);
        uint id = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, id);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
            format == THSSS.Graphics.DepthFormat.Depth16 ? InternalFormat.DepthComponent16 : InternalFormat.DepthComponent24,
            (uint)width, (uint)height);
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, previous);
        return new OpenGlDepthSurface(_gl, id, width, height);
    }

    public IRenderFramebuffer CreateFramebuffer(IRenderTexture? color, DepthFormat depth, int width, int height)
        => new OpenGlFramebuffer(_gl, color, depth, width, height);

    // ---- frame state -----------------------------------------------------

    public void RestoreFramebufferBindings(IRenderFramebuffer? draw, IRenderFramebuffer? read)
    {
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, draw is OpenGlFramebuffer d ? d.Fbo : 0);
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, read is OpenGlFramebuffer r ? r.Fbo : 0);
    }

    /// <param name="colourAttachment">
    /// 0..n selects colour attachment n; -1 selects the default framebuffer's back buffer.
    /// </param>
    public void SetReadBuffer(int colourAttachment)
        => _gl.ReadBuffer(colourAttachment < 0
            ? ReadBufferMode.Back
            : (ReadBufferMode)((int)GLEnum.ColorAttachment0 + colourAttachment));

    public IRenderFramebuffer? CurrentDrawFramebuffer
        => _tracked.TryGetValue((uint)_gl.GetInteger(GLEnum.DrawFramebufferBinding), out var d) ? d : null;

    public IRenderFramebuffer? CurrentReadFramebuffer
        => _tracked.TryGetValue((uint)_gl.GetInteger(GLEnum.ReadFramebufferBinding), out var r) ? r : null;


    private static readonly Dictionary<uint, IRenderFramebuffer> _tracked = new();
    internal static void TrackFramebuffer(OpenGlFramebuffer framebuffer) => _tracked[framebuffer.Fbo] = framebuffer;
    internal static void UntrackFramebuffer(uint fbo) => _tracked.Remove(fbo);

    public void SetViewport(int x, int y, int width, int height, float minDepth, float maxDepth)
    {
        _gl.Viewport(x, y, (uint)Math.Max(1, width), (uint)Math.Max(1, height));
        _gl.DepthRange(minDepth, maxDepth);
    }

    public void Clear(ClearMask mask, float red, float green, float blue, float alpha, float depth, int stencil)
    {
        _gl.ClearColor(red, green, blue, alpha);
        _gl.ClearDepth(depth);
        _gl.ClearStencil(stencil);
        bool writeDepth = _gl.IsEnabled(EnableCap.DepthTest);
        _gl.DepthMask(true);
        _gl.Clear(ToClearMask(mask));
        _gl.DepthMask(writeDepth);
    }

    public void SetScissor(bool enabled, int x, int y, int width, int height)
    {
        if (enabled)
        {
            if (!_scissorEnabled) _gl.Enable(EnableCap.ScissorTest);
            _gl.Scissor(x, y, (uint)Math.Max(0, width), (uint)Math.Max(0, height));
        }
        else if (_scissorEnabled)
        {
            _gl.Disable(EnableCap.ScissorTest);
        }
        _scissorEnabled = enabled;
    }

    private static ClearBufferMask ToClearMask(ClearMask mask)
    {
        ClearBufferMask result = 0;
        if ((mask & ClearMask.Color) != 0) result |= ClearBufferMask.ColorBufferBit;
        if ((mask & ClearMask.Depth) != 0) result |= ClearBufferMask.DepthBufferBit;
        if ((mask & ClearMask.Stencil) != 0) result |= ClearBufferMask.StencilBufferBit;
        return result;
    }

    // ---- fixed function --------------------------------------------------

    public void SetDepthTest(bool enabled)
    {
        if (enabled) _gl.Enable(EnableCap.DepthTest); else _gl.Disable(EnableCap.DepthTest);
    }

    public void SetDepthWrite(bool enabled) => _gl.DepthMask(enabled);

    public void SetCullMode(CullMode mode)
    {
        if (mode == CullMode.None)
        {
            _gl.Disable(EnableCap.CullFace);
            return;
        }
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(GLEnum.Back);
        _gl.FrontFace(mode == CullMode.Clockwise ? FrontFaceDirection.CW : FrontFaceDirection.Ccw);
    }

    public void SetBlend(BlendState blend)
    {
        if (!blend.Enabled)
        {
            _gl.Disable(EnableCap.Blend);
            return;
        }
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(ToBlendFactor(blend.Source), ToBlendFactor(blend.Destination));
        _gl.BlendEquation(ToBlendEquation(blend.Equation));
    }

    /// <summary>D3D9 D3DBLEND* to GL factor; the table lives here only.</summary>
    internal static BlendingFactor ToBlendFactor(BlendFactor factor) => factor switch
    {
        BlendFactor.Zero => BlendingFactor.Zero,
        BlendFactor.One => BlendingFactor.One,
        BlendFactor.SourceColor => BlendingFactor.SrcColor,
        BlendFactor.InverseSourceColor => BlendingFactor.OneMinusSrcColor,
        BlendFactor.SourceAlpha => BlendingFactor.SrcAlpha,
        BlendFactor.InverseSourceAlpha => BlendingFactor.OneMinusSrcAlpha,
        BlendFactor.DestinationAlpha => BlendingFactor.DstAlpha,
        BlendFactor.InverseDestinationAlpha => BlendingFactor.OneMinusDstAlpha,
        BlendFactor.DestinationColor => BlendingFactor.DstColor,
        BlendFactor.InverseDestinationColor => BlendingFactor.OneMinusDstColor,
        BlendFactor.SourceAlphaSaturate => BlendingFactor.SrcAlphaSaturate,
        _ => BlendingFactor.One,
    };

    private static BlendEquationModeEXT ToBlendEquation(BlendEquation equation) => equation switch
    {
        BlendEquation.Subtract => BlendEquationModeEXT.FuncSubtract,
        BlendEquation.ReverseSubtract => BlendEquationModeEXT.FuncReverseSubtract,
        BlendEquation.Min => BlendEquationModeEXT.Min,
        BlendEquation.Max => BlendEquationModeEXT.Max,
        _ => BlendEquationModeEXT.FuncAdd,
    };

    public void SetSamplerState(SamplerStateBlock state) => OpenGlTexture.ApplySamplerState(_gl, 0, state);

    public void SetUniformBlock(int binding, IRenderBuffer? buffer, ReadOnlySpan<byte> data)
    {
        if (buffer is OpenGlBuffer glBuffer)
        {
            glBuffer.Update(data, 0);
            _gl.BindBufferBase(BufferTargetARB.UniformBuffer, (uint)binding, glBuffer.Id);
        }
    }

    public void BindFramebuffer(IRenderFramebuffer? framebuffer)
    {
        uint target = framebuffer is OpenGlFramebuffer glFbo ? glFbo.Fbo : 0;
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target);
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, target);
    }

    public void BindTexture(IRenderTexture? texture)
        => _gl.BindTexture(TextureTarget.Texture2D, texture is OpenGlTexture glTexture ? glTexture.Id : 0);

    // ---- drawing ---------------------------------------------------------

    public void Draw(IRenderShader shader, IRenderBuffer vertices, VertexLayout layout,
                     PrimitiveKind kind, int startVertex, int vertexCount)
    {
        if (shader is not OpenGlShader glShader || vertices is not OpenGlBuffer glBuffer) return;
        if (!_vaoCreated)
        {
            _vao = _gl.GenVertexArray();
            _vaoCreated = true;
        }
        // One VAO owned by the backend: attribute state is re-specified per draw, so a
        // stale VAO from another caller can never make DrawArrays invalid.
        _gl.BindVertexArray(_vao);
        glShader.Use();
        OpenGlBuffer.BindAttributes(_gl, glBuffer, layout);
        _gl.DrawArrays(ToPrimitive(kind), startVertex, (uint)vertexCount);
        _gl.BindVertexArray(0);
    }

    private static PrimitiveType ToPrimitive(PrimitiveKind kind) => kind switch
    {
        PrimitiveKind.TriangleStrip => PrimitiveType.TriangleStrip,
        PrimitiveKind.LineList => PrimitiveType.Lines,
        _ => PrimitiveType.Triangles,
    };

    public byte[] ReadPixels(int x, int y, int width, int height)
    {
        var pixels = new byte[Math.Max(1, width) * Math.Max(1, height) * 4];
        // Pixel-pack state is global; a bound PBO or non-default pack layout from the
        // caller would silently redirect the read. Save, normalise, restore.
        uint packBuffer = (uint)_gl.GetInteger(GLEnum.PixelPackBufferBinding);
        Span<int> names = [ (int)GLEnum.PackAlignment, (int)GLEnum.PackRowLength,
                            (int)GLEnum.PackSkipRows, (int)GLEnum.PackSkipPixels ];
        Span<int> saved = stackalloc int[4];
        for (int i = 0; i < 4; i++) saved[i] = _gl.GetInteger((GLEnum)names[i]);
        try
        {
            _gl.BindBuffer(BufferTargetARB.PixelPackBuffer, 0);
            for (int i = 0; i < 4; i++) _gl.PixelStore((GLEnum)names[i], i == 0 ? 1 : 0);
            unsafe
            {
                fixed (byte* data = pixels)
                    _gl.ReadPixels(x, y, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            }
        }
        finally
        {
            for (int i = 0; i < 4; i++) _gl.PixelStore((GLEnum)names[i], saved[i]);
            _gl.BindBuffer(BufferTargetARB.PixelPackBuffer, packBuffer);
        }
        return pixels;
    }

    public bool TryGetError(out string message)
    {
        var error = _gl.GetError();
        message = error == GLEnum.NoError ? string.Empty : error.ToString();
        return error != GLEnum.NoError;
    }

    public void Dispose() { }
}
