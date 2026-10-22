using System;
using Silk.NET.OpenGL;
using THSSS.Graphics;

namespace THSSS.Graphics.OpenGl;

internal sealed class OpenGlHandle : IRenderHandle
{
    internal OpenGlHandle(uint id) => Id = id;
    internal uint Id { get; }
    public override string ToString() => "gl:" + Id;
}

internal sealed unsafe class OpenGlTexture : IRenderTexture
{
    private readonly GL _gl;
    private uint _id;
    private readonly int _levels;
    private bool _disposed;

    internal OpenGlTexture(GL gl, int width, int height, TextureFormat format, int levels)
    {
        _gl = gl;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Format = format;
        var internalFormat = format == TextureFormat.Rgb8 ? InternalFormat.Rgb8 : InternalFormat.Rgba8;
        var pixelFormat = format == TextureFormat.Rgb8 ? PixelFormat.Rgb : PixelFormat.Rgba;

        int available = 1 + (int)Math.Floor(Math.Log2(Math.Max(Width, Height)));
        _levels = levels <= 0 ? available : Math.Min(Math.Max(1, levels), available);

        _id = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _id);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, _levels - 1);
        for (int level = 0; level < _levels; level++)
            _gl.TexImage2D(TextureTarget.Texture2D, level, internalFormat,
                (uint)Math.Max(1, Width >> level), (uint)Math.Max(1, Height >> level), 0,
                pixelFormat, PixelType.UnsignedByte, null);
        Handle = new OpenGlHandle(_id);
    }

    internal uint Id => _id;

    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public TextureFormat Format { get; }

    public void Upload(ReadOnlySpan<byte> pixels, int rowPitch)
    {
        if (pixels.IsEmpty) return;
        _gl.BindTexture(TextureTarget.Texture2D, _id);
        _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        var pixelFormat = Format == TextureFormat.Rgb8 ? PixelFormat.Rgb : PixelFormat.Rgba;
        unsafe
        {
            fixed (byte* data = pixels)
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)Width, (uint)Height,
                    pixelFormat, PixelType.UnsignedByte, data);
        }
    }

    public void SetSamplerState(SamplerStateBlock state) => ApplySamplerState(_gl, _id, state);

    /// <summary>
    /// D3DTEXTUREFILTERTYPE/D3DTEXTUREADDRESS mapped onto the texture object. Kept as
    /// texture parameters instead of sampler objects so the same call works on
    /// backends (and GL versions) without sampler support.
    /// </summary>
    internal static void ApplySamplerState(GL gl, uint textureId, SamplerStateBlock state)
    {
        if (textureId == 0) return;
        gl.BindTexture(TextureTarget.Texture2D, textureId);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ToAddress(state.AddressU));
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ToAddress(state.AddressV));

        var min = ToMinFilter(state.MinFilter, state.MipFilter);
        var mag = state.MagFilter switch
        {
            1 => TextureMagFilter.Nearest,
            2 => TextureMagFilter.Linear,
            _ => TextureMagFilter.Linear,
        };
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)min);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)mag);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, state.MipLodBias);
        if (state.BorderEnabled)
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor,
                new[] { state.BorderRed, state.BorderGreen, state.BorderBlue, state.BorderAlpha });
    }

    private static int ToAddress(int address) => address switch
    {
        1 => (int)TextureWrapMode.ClampToEdge,
        3 => (int)TextureWrapMode.MirroredRepeat,
        4 => (int)TextureWrapMode.ClampToBorder,
        _ => (int)TextureWrapMode.Repeat,
    };

    private static TextureMinFilter ToMinFilter(int minFilter, int mipFilter) => (minFilter, mipFilter) switch
    {
        (1, 0) => TextureMinFilter.Nearest,
        (1, 1) => TextureMinFilter.NearestMipmapNearest,
        (1, 2) => TextureMinFilter.NearestMipmapLinear,
        (2, 0) => TextureMinFilter.Linear,
        (2, 1) => TextureMinFilter.LinearMipmapNearest,
        (2, 2) => TextureMinFilter.LinearMipmapLinear,
        _ => TextureMinFilter.Linear,
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_id != 0) _gl.DeleteTexture(_id);
        _id = 0;
    }
}

/// <summary>Backend-owned depth renderbuffer.</summary>
internal sealed class OpenGlDepthSurface : IRenderDepthSurface
{
    private readonly GL _gl;
    private readonly uint _id;

    internal OpenGlDepthSurface(GL gl, uint id, int width, int height)
    {
        _gl = gl; _id = id; Width = width; Height = height;
        Handle = new OpenGlHandle(id);
    }

    internal uint Id => _id;
    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public uint NativeAttachmentId => _id;

    public void Dispose() => _gl.DeleteRenderbuffer(_id);
}
