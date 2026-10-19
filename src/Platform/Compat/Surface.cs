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
    // Default framebuffer may be physically larger than the original logical backbuffer.
    internal Rectangle? ReadbackRegion { get; set; }
    internal uint DepthBuffer { get; private set; }
    private bool _ownsTexture;

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

        _ownsTexture = true;
        _textureHandle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _textureHandle);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0,
            Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        uint previousDraw = (uint)_gl.GetInteger(GLEnum.DrawFramebufferBinding);
        uint previousRead = (uint)_gl.GetInteger(GLEnum.ReadFramebufferBinding);
        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, _textureHandle, 0);
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, previousDraw);
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, previousRead);
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

    public static void ToFile(Surface surface, string fileName, ImageFileFormat format)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ObjectDisposedException.ThrowIf(surface.IsDisposed, surface);
        SixLabors.ImageSharp.Formats.IImageEncoder encoder = format switch
        {
            ImageFileFormat.Jpg => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
            ImageFileFormat.Png => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
            ImageFileFormat.Bmp => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder(),
            ImageFileFormat.Tga => new SixLabors.ImageSharp.Formats.Tga.TgaEncoder(),
            ImageFileFormat.Ppm => new SixLabors.ImageSharp.Formats.Pbm.PbmEncoder
            { ColorType = SixLabors.ImageSharp.Formats.Pbm.PbmColorType.Rgb,
              ComponentType = SixLabors.ImageSharp.Formats.Pbm.PbmComponentType.Byte },
            _ => throw new NotSupportedException($"Screenshot encoding {format} is not supported.")
        };
        var gl = surface.GL;
        var region = surface.ReadbackRegion ?? new Rectangle(0,0,surface.Width,surface.Height);
        var pixels = new byte[checked(region.Width * region.Height * 4)];
        uint previousRead = (uint)gl.GetInteger(GLEnum.ReadFramebufferBinding);
        uint previousPackBuffer = (uint)gl.GetInteger(GLEnum.PixelPackBufferBinding);
        var packNames = new[] { GLEnum.PackAlignment, GLEnum.PackRowLength, GLEnum.PackSkipRows, GLEnum.PackSkipPixels };
        var packValues = Array.ConvertAll(packNames, name => gl.GetInteger(name));
        uint temporaryFbo = 0;
        int sourceReadBuffer = 0;
        bool sourceBound = false;
        try
        {
            uint sourceFbo = surface.FBO;
            if (sourceFbo == 0 && surface._textureHandle != 0)
            {
                temporaryFbo = gl.GenFramebuffer();
                sourceFbo = temporaryFbo;
                gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFbo);
                gl.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D, surface._textureHandle, 0);
            }
            gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFbo);
            sourceReadBuffer = gl.GetInteger(GLEnum.ReadBuffer);
            sourceBound = true;
            gl.ReadBuffer(sourceFbo != 0 ? ReadBufferMode.ColorAttachment0 : (ReadBufferMode)sourceReadBuffer);
            if (gl.CheckFramebufferStatus(FramebufferTarget.ReadFramebuffer) != GLEnum.FramebufferComplete)
                throw new InvalidOperationException("Cannot capture an incomplete surface framebuffer.");
            gl.BindBuffer(BufferTargetARB.PixelPackBuffer, 0);
            for (int i = 0; i < packNames.Length; i++) gl.PixelStore(packNames[i], i == 0 ? 1 : 0);
            fixed (byte* data = pixels)
                gl.ReadPixels(region.X, region.Y, (uint)region.Width, (uint)region.Height,
                    PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }
        finally
        {
            if (sourceBound) gl.ReadBuffer((ReadBufferMode)sourceReadBuffer);
            gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, previousRead);
            gl.BindBuffer(BufferTargetARB.PixelPackBuffer, previousPackBuffer);
            for (int i = 0; i < packNames.Length; i++) gl.PixelStore(packNames[i], packValues[i]);
            if (temporaryFbo != 0) gl.DeleteFramebuffer(temporaryFbo);
        }
        int stride = region.Width * 4;
        var row = new byte[stride];
        int flippedRows = surface._isRenderTarget || surface._textureHandle == 0 ? region.Height / 2 : 0;
        for (int y = 0; y < flippedRows; y++)
        {
            int top = y * stride, bottom = (region.Height - 1 - y) * stride;
            Array.Copy(pixels, top, row, 0, stride);
            Array.Copy(pixels, bottom, pixels, top, stride);
            Array.Copy(row, 0, pixels, bottom, stride);
        }
        using var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
            pixels, region.Width, region.Height);
        if (region.Width != surface.Width || region.Height != surface.Height)
            SixLabors.ImageSharp.Processing.ProcessingExtensions.Mutate(image, ctx =>
                SixLabors.ImageSharp.Processing.ResizeExtensions.Resize(ctx,surface.Width,surface.Height));
        using var stream = System.IO.File.Create(fileName);
        image.Save(stream, encoder);
    }

    /// <summary>D3D9 depth-stencil surface; the GL render target carries its own depth.</summary>
    public static Surface CreateDepthStencil(Device device, int width, int height, Format format,
        MultiSampleType multiSample, int quality, bool discard)
    {
        if (multiSample != MultiSampleType.None)
            throw new NotSupportedException("Multisampled depth requires a matching multisampled render target.");
        var surface = new Surface(device.GL, width, height, false);
        var gl = device.GL;
        uint previous = (uint)gl.GetInteger(GLEnum.RenderbufferBinding);
        surface.DepthBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, surface.DepthBuffer);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
            format == Format.D16 ? InternalFormat.DepthComponent16 : InternalFormat.DepthComponent24,
            (uint)width, (uint)height);
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, previous);
        return surface;
    }

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
        if (DepthBuffer != 0) { _gl.DeleteRenderbuffer(DepthBuffer); DepthBuffer = 0; }
        if (_ownsTexture && _textureHandle != 0) { _gl.DeleteTexture(_textureHandle); _textureHandle = 0; }
        _disposed = true;
    }
}
