using System;
using Silk.NET.OpenGL;
using THSSS.Graphics;

namespace THSSS.Graphics.OpenGl;

internal sealed class OpenGlFramebuffer : IRenderFramebuffer
{
    private readonly GL _gl;
    private uint _fbo;
    private uint _depthRenderbuffer;
    private bool _disposed;

    internal OpenGlFramebuffer(GL gl, IRenderTexture? color, DepthFormat depth, int width, int height)
    {
        _gl = gl;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Color = color;

        uint previousDraw = (uint)_gl.GetInteger(GLEnum.DrawFramebufferBinding);
        uint previousRead = (uint)_gl.GetInteger(GLEnum.ReadFramebufferBinding);
        _fbo = _gl.GenFramebuffer();
        try
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            if (color is OpenGlTexture glTexture)
            {
                _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D, glTexture.Id, 0);
                Width = glTexture.Width;
                Height = glTexture.Height;
            }
            if (depth != DepthFormat.None)
            {
                _depthRenderbuffer = _gl.GenRenderbuffer();
                _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRenderbuffer);
                _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                    depth == DepthFormat.Depth16 ? InternalFormat.DepthComponent16 : InternalFormat.DepthComponent24,
                    (uint)Width, (uint)Height);
                _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                    RenderbufferTarget.Renderbuffer, _depthRenderbuffer);
            }
            IsComplete = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == GLEnum.FramebufferComplete;
        }
        finally
        {
            _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, previousDraw);
            _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, previousRead);
        }
        Handle = new OpenGlHandle(_fbo);
        OpenGlRenderBackend.TrackFramebuffer(this);
    }

    internal uint Fbo => _fbo;

    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsComplete { get; }
    public IRenderTexture? Color { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_depthRenderbuffer != 0) { _gl.DeleteRenderbuffer(_depthRenderbuffer); _depthRenderbuffer = 0; }
        if (_fbo != 0) { OpenGlRenderBackend.UntrackFramebuffer(_fbo); _gl.DeleteFramebuffer(_fbo); _fbo = 0; }
    }
}
