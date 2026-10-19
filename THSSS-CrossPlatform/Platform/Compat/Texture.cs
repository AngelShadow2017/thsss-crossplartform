// Cross-platform Texture (replaces SlimDX.Direct3D9.Texture)
using System;
using System.Drawing;
using System.IO;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SlimDX.Direct3D9;

/// <summary>Cross-platform texture using OpenGL</summary>
public unsafe class Texture : IDisposable
{
    private GL _gl;
    private uint _handle;
    private int _width;
    private int _height;
    private int _levels;
    private bool _disposed;

    public uint Handle => _handle;
    public bool Disposed => _disposed;
    public int Width => _width;
    public int Height => _height;
    public int LevelCount => _levels;
    public Format Format { get; private set; }
    public Pool Pool { get; private set; }
    public Usage Usage { get; private set; }

    public Texture(Device device, int width, int height, int levels, Usage usage, Format format, Pool pool)
        : this(device?.GL, width, height, levels, usage, format, pool) { }

    public Texture(GL gl, int width, int height, int levels, Usage usage, Format format, Pool pool)
    {
        _gl = gl;
        _width = width;
        _height = height;
        _levels = Math.Max(1, levels);
        Format = format;
        Pool = pool;
        Usage = usage;

        _handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _handle);

        // Set default filtering
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        // Allocate storage
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0,
            (GLEnum)PixelFormat.Rgba, (GLEnum)PixelType.UnsignedByte, null);
    }

    public static Texture FromFile(Device device, string fileName, Usage usage, Pool pool, Filter filter, Filter mipFilter, int colorKey)
        => FromFile(device, fileName);

    public static Texture FromFile(Device device, string fileName, int width, int height, int mipLevels, Usage usage, Format format, Pool pool)
        => FromFile(device, fileName);

    public SurfaceDescription GetLevelDescription(int level) => new SurfaceDescription();

    public static Texture FromFile(Device device, string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"Texture file not found: {fileName}");

        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(fileName);
        return FromImage(device, image);
    }

    public static Texture FromStream(Device device, Stream stream, int width, int height)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
        return FromImage(device, image);
    }

    public static Texture FromImage(Device device, Image<Rgba32> image)
    {
        var tex = new Texture(device.GL, image.Width, image.Height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

        unsafe
        {
            // Atlas UVs address source row zero at v=0. Preserve upload order;
            // screen-space Y inversion is handled by the sprite projection.
            // Make sure the target texture is bound for the upload below.
            tex._gl.BindTexture(Silk.NET.OpenGL.TextureTarget.Texture2D, tex._handle);
            image.ProcessPixelRows(accessor =>
            {
                int h = accessor.Height;
                for (int y = 0; y < h; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    fixed (Rgba32* ptr = row)
                    {
                        tex._gl.TexSubImage2D((GLEnum)TextureTarget.Texture2D, 0, 0, y,
                            (uint)accessor.Width, 1,
                            (GLEnum)PixelFormat.Rgba, (GLEnum)PixelType.UnsignedByte, ptr);
                    }
                }
            });
        }

        return tex;
    }

    public static Texture FromFile(Device device, string fileName, int width, int height, int mipLevels, Usage usage, Format format, Pool pool, Filter filter, Filter mipFilter, int colorKey)
        => FromFile(device, fileName);

    public static Texture FromFile(Device device, string fileName, int width, int height, int mipLevels, Usage usage, Format format, Pool pool, Filter filter, Filter mipFilter, int colorKey, ImageInformation? imageInfo)
    {
        if (File.Exists(fileName))
        {
            return FromFile(device, fileName);
        }
        // Return a placeholder texture
        return new Texture(device.GL, Math.Max(1, width), Math.Max(1, height), 1, usage, format, pool);
    }

    public Surface GetSurfaceLevel(int level)
    {
        return new Surface(_gl, _width, _height, (Usage & Usage.RenderTarget) != 0, _handle);
    }

    public DataRectangle LockRectangle(int level, System.Drawing.Rectangle rect, LockFlags flags)
    {
        _lockedRectangle = rect;
        _lockedData = new DataStream(rect.Width * rect.Height * 4, true, true);
        return new DataRectangle { Pitch = rect.Width * 4, Data = _lockedData };
    }

    public DataRectangle LockRectangle(int level, LockFlags flags)
    {
        return LockRectangle(level, new System.Drawing.Rectangle(0, 0, _width, _height), flags);
    }

    private DataStream _lockedData;
    private System.Drawing.Rectangle _lockedRectangle;

    public unsafe void UnlockRectangle(int level)
    {
        var bytes = _lockedData.ToArray();
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        fixed (byte* ptr = bytes)
        {
            _gl.TexSubImage2D(TextureTarget.Texture2D, 0,
                _lockedRectangle.X, _lockedRectangle.Y,
                (uint)_lockedRectangle.Width, (uint)_lockedRectangle.Height,
                PixelFormat.Bgra, PixelType.UnsignedByte, ptr);
        }
        _lockedData = null;
    }

    public void GenerateMipMaps()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void SetFilter(TextureFilterType minFilter, TextureFilterType magFilter, TextureFilterType mipFilter)
    {
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)ConvertFilter(minFilter, mipFilter));
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)ConvertFilter(magFilter, TextureFilterType.None));
    }

    private TextureMinFilter ConvertFilter(TextureFilterType filter, TextureFilterType mipFilter)
    {
        return (filter, mipFilter) switch
        {
            (TextureFilterType.Point, TextureFilterType.Point) => TextureMinFilter.NearestMipmapNearest,
            (TextureFilterType.Point, TextureFilterType.Linear) => TextureMinFilter.NearestMipmapLinear,
            (TextureFilterType.Linear, TextureFilterType.Point) => TextureMinFilter.LinearMipmapNearest,
            (TextureFilterType.Linear, TextureFilterType.Linear) => TextureMinFilter.LinearMipmapLinear,
            (TextureFilterType.Point, _) => TextureMinFilter.Nearest,
            (TextureFilterType.Linear, _) => TextureMinFilter.Linear,
            _ => TextureMinFilter.Linear,
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                _gl.DeleteTexture(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
    }
}

public enum Filter
{
    None = 0,
    Point = 1,
    Linear = 2,
    Triangle = 3,
    Box = 4,
    MirrorDithered = 5,
}

public struct ImageInformation
{
    public int Width;
    public int Height;
    public int Depth;
    public int MipMapCount;
    public Format Format;
    public int Resource_type;
}
