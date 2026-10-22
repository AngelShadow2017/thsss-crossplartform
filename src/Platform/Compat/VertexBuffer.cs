// Cross-platform VertexBuffer and VertexDeclaration, backed by IRenderBuffer.
using System;
using System.Runtime.InteropServices;
using THSSS.Graphics;

namespace SlimDX.Direct3D9;

/// <summary>Cross-platform vertex buffer</summary>
public unsafe class VertexBuffer : IDisposable
{
    private readonly IRenderBackend _backend;
    private IRenderBuffer _buffer;
    private int _sizeInBytes;
    private int _stride;
    private bool _disposed;
    private readonly VertexElement[] _elements;

    public IRenderHandle Handle => _buffer?.Handle;
    internal IRenderBuffer Buffer => _buffer;
    public bool Disposed => _disposed;
    public int SizeInBytes => _sizeInBytes;
    public int Stride => _stride;
    internal ReadOnlySpan<VertexElement> Elements => _elements;

    public VertexBuffer(Device device, int sizeInBytes, Usage usage, VertexFormat format, Pool pool)
    {
        _backend = device.RenderBackend;
        _sizeInBytes = sizeInBytes;
        _stride = GetStride(format);
        _buffer = _backend.CreateBuffer(BufferKind.Vertex, sizeInBytes);
        _elements = BuildElements(format);
    }

    public VertexBuffer(Device device, int sizeInBytes, Usage usage, VertexDeclaration declaration, Pool pool)
    {
        _backend = device.RenderBackend;
        _sizeInBytes = sizeInBytes;
        _stride = declaration?.Stride ?? 32;
        _buffer = _backend.CreateBuffer(BufferKind.Vertex, sizeInBytes);
        _elements = declaration?.Elements.ToArray() ?? Array.Empty<VertexElement>();
    }

    /// <summary>
    /// The game passes VertexFormat.None yet fills Lock() with TexturedVertex
    /// (pos + uv = 20 bytes) and then queries SetStreamSource with the same 20.
    /// Falling back to 20 keeps GetBufferSubData slices aligned with the writes.
    /// </summary>
    private int GetStride(VertexFormat format)
    {
        if (format == VertexFormat.None) return 20;

        int stride = 0;
        if ((format & VertexFormat.Position) != 0 || (format & VertexFormat.Xyz) != 0)
            stride += 12;
        if ((format & VertexFormat.Normal) != 0) stride += 12;
        if ((format & VertexFormat.Diffuse) != 0) stride += 4;
        if ((format & VertexFormat.Specular) != 0) stride += 4;
        if ((format & VertexFormat.Texture1) != 0) stride += 8;
        return stride == 0 ? 20 : stride;
    }

    private static VertexElement[] BuildElements(VertexFormat format)
    {
        if (format == VertexFormat.None)
        {
            return new[]
            {
                new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
                new VertexElement(0, 12, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TexCoord, 0),
            };
        }
        var elements = new System.Collections.Generic.List<VertexElement>();
        short offset = 0;
        if ((format & VertexFormat.Position) != 0 || (format & VertexFormat.Xyz) != 0)
        {
            elements.Add(new VertexElement(0, offset, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0));
            offset += 12;
        }
        if ((format & VertexFormat.Normal) != 0)
        {
            elements.Add(new VertexElement(0, offset, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0));
            offset += 12;
        }
        if ((format & VertexFormat.Diffuse) != 0)
        {
            elements.Add(new VertexElement(0, offset, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0));
            offset += 4;
        }
        if ((format & VertexFormat.Specular) != 0) offset += 4;
        if ((format & VertexFormat.Texture1) != 0)
            elements.Add(new VertexElement(0, offset, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TexCoord, 0));
        return elements.ToArray();
    }

    private DataStream _pendingData;
    private int _pendingDataOffset;

    /// <summary>
    /// Direct3D style lock: the caller writes vertices/indices into the returned
    /// stream and Unlock() uploads them. Sizing follows the D3D convention where
    /// zero means "everything from offset to the end".
    /// </summary>
    public DataStream Lock(int offsetToLock, int sizeToLock, LockFlags flags)
    {
        int size = sizeToLock > 0 ? sizeToLock : _sizeInBytes - offsetToLock;
        _pendingData = new DataStream(Math.Max(0, size), true, true);
        _pendingDataOffset = offsetToLock;
        return _pendingData;
    }

    public DataStream Lock(LockFlags flags) => Lock(0, 0, flags);

    public void Unlock()
    {
        if (_pendingData == null) return;
        var bytes = _pendingData.ToArray();
        if (bytes.Length > 0) _buffer.Update(bytes, _pendingDataOffset);
        _pendingData = null;
    }

    public void Bind() { }

    public void SetData<T>(T[] data) where T : unmanaged
    {
        int byteCount = data.Length * Marshal.SizeOf<T>();
        var bytes = new byte[byteCount];
        System.Buffer.BlockCopy(data, 0, bytes, 0, byteCount);
        _buffer.Update(bytes, 0);
    }

    public void WriteRange<T>(T[] data) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        var bytes = new byte[size * data.Length];
        System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        _buffer.Update(bytes, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _buffer?.Dispose();
        _buffer = null;
        _disposed = true;
    }
}

/// <summary>Cross-platform vertex declaration (GL layout description).</summary>
public unsafe class VertexDeclaration : IDisposable
{
    private VertexElement[] _elements;
    private bool _disposed;

    public int Stride { get; private set; }
    internal ReadOnlySpan<VertexElement> Elements => _elements;
    public bool Disposed => _disposed;

    public VertexDeclaration(Device device, VertexElement[] elements)
    {
        _elements = elements;
        foreach (var elem in elements)
        {
            int offset = elem.Offset + GetElementSize(elem.Type);
            if (offset > Stride) Stride = offset;
        }
    }

    private int GetElementSize(DeclarationType type) => type switch
    {
        DeclarationType.Float1 => 4,
        DeclarationType.Float2 => 8,
        DeclarationType.Float3 => 12,
        DeclarationType.Float4 => 16,
        DeclarationType.Color => 4,
        DeclarationType.UByte4 => 4,
        DeclarationType.Short2 => 4,
        DeclarationType.Short4 => 8,
        DeclarationType.Float16_2 => 4,
        DeclarationType.Float16_4 => 8,
        _ => 4,
    };

    /// <summary>Attributes are bound by the backend at draw time; nothing to do here.</summary>
    public void Apply() { }

    public void Dispose() => _disposed = true;
}

// VertexElement and DeclarationMethod live in Direct3D9AdditionalTypes.cs
