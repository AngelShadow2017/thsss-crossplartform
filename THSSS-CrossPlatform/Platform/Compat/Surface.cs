// Surface compatibility layer (render targets)
using System;
using Silk.NET.OpenGL;
using System.Drawing;

namespace SlimDX.Direct3D9;

public unsafe class Surface : IDisposable
{
    private GL _gl;
    private uint _fbo;
    private uint _textureHandle;
    private int _width;
    private int _height;
    private bool _isRenderTarget;
    private bool _disposed;

    public uint FBO => _fbo;

    internal GL GL => _gl;

    public int Width => _width;

    public int Height => _height;

    public bool IsDisposed => _disposed;

    public Surface(GL gl, int width, int height, bool isRenderTarget)
    {
        _gl = gl;
        _width = width;
        _height = height;
        _isRenderTarget = isRenderTarget;

        if (!isRenderTarget) return;

        _textureHandle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _textureHandle);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0,
            Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        uint previousFbo = (uint)_gl.GetInteger(GLEnum.FramebufferBinding);
        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _textureHandle, 0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, previousFbo);
    }

    public Surface(GL gl, int width, int height, bool isRenderTarget, uint sharedTextureHandle)
    {
        _gl = gl;
        _width = width;
        _height = height;
        _isRenderTarget = isRenderTarget;
        _textureHandle = sharedTextureHandle;

        if (!isRenderTarget) return;

        uint previousDraw = (uint)_gl.GetInteger(GLEnum.DrawFramebufferBinding);
        uint previousRead = (uint)_gl.GetInteger(GLEnum.ReadFramebufferBinding);
        _fbo = _gl.GenFramebuffer();
        try
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _textureHandle, 0);
            var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != GLEnum.FramebufferComplete)
                throw new InvalidOperationException($"Incomplete texture framebuffer: {status}");
        }
        finally
        {
            _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, previousDraw);
            _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, previousRead);
        }
    }

    public DataRectangle LockRectangle(LockFlags flags)
    {
        return new DataRectangle { Pitch = _width * 4, Data = new DataStream(_width * _height * 4, true, true) };
    }

    public DataRectangle LockRectangle(Rectangle rect, LockFlags flags)
    {
        return new DataRectangle { Pitch = _width * 4, Data = new DataStream(rect.Width * rect.Height * 4, true, true) };
    }

    public void UnlockRectangle() { }

    /// <summary>
    /// D3DXSaveSurfaceToFile equivalent. The screenshot feature writes a PNG at the
    /// extension the game asked for; pixel data comes from the current framebuffer.
    /// </summary>
    public static void ToFile(Surface surface, string fileName, ImageFileFormat format)
    {
        if (surface?.GL == null) return;
        var gl = surface.GL;
        var pixels = new byte[Math.Max(1, surface.Width) * Math.Max(1, surface.Height) * 4];
        unsafe
        {
            fixed (byte* data = pixels)
                gl.ReadPixels(0, 0, (uint)Math.Max(1, surface.Width), (uint)Math.Max(1, surface.Height),
                    PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }
        using var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
            pixels, Math.Max(1, surface.Width), Math.Max(1, surface.Height));
        using var stream = System.IO.File.Create(fileName);
        image.Save(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
    }

    /// <summary>D3D9 depth-stencil surface; the GL render target carries its own depth.</summary>
    public static Surface CreateDepthStencil(Device device, int width, int height, Format format,
        MultiSampleType multiSample, int quality, bool discard)
        => new(device.GL, width, height, false);

    public void Bind()
    {
        if (_fbo != 0) _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
    }

    public void Unbind() => _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void Dispose()
    {
        if (_disposed) return;
        if (_fbo != 0)
        {
            _gl.DeleteFramebuffer(_fbo);
            _fbo = 0;
        }
        _disposed = true;
    }
}
