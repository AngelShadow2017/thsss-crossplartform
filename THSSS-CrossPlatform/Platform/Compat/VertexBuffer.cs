// Cross-platform VertexBuffer and VertexDeclaration
using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace SlimDX.Direct3D9;

/// <summary>Cross-platform vertex buffer</summary>
public unsafe class VertexBuffer : IDisposable
{
    private GL _gl;
    private uint _handle;
    private int _sizeInBytes;
    private int _stride;
    private bool _disposed;

    public uint Handle => _handle;
    public bool Disposed => _disposed;
    public int SizeInBytes => _sizeInBytes;
    public int Stride => _stride;

    public VertexBuffer(Device device, int sizeInBytes, Usage usage, VertexFormat format, Pool pool)
    {
        _gl = device.GL;
        _sizeInBytes = sizeInBytes;
        _stride = GetStride(format);

        _handle = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)sizeInBytes, (void*)0, BufferUsageARB.DynamicDraw);
    }

    public VertexBuffer(Device device, int sizeInBytes, Usage usage, VertexDeclaration declaration, Pool pool)
    {
        _gl = device.GL;
        _sizeInBytes = sizeInBytes;
        _stride = declaration?.Stride ?? 32;

        _handle = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)sizeInBytes, (void*)0, BufferUsageARB.DynamicDraw);
    }

    private int GetStride(VertexFormat format)
    {
        int stride = 0;
        if ((format & VertexFormat.Position) != 0 || (format & VertexFormat.Xyz) != 0)
            stride += 12; // 3 floats
        if ((format & VertexFormat.Normal) != 0)
            stride += 12;
        if ((format & VertexFormat.Diffuse) != 0)
            stride += 4; // 1 uint (D3DCOLOR)
        if ((format & VertexFormat.Specular) != 0)
            stride += 4;
        if ((format & VertexFormat.Texture1) != 0)
            stride += 8; // 2 floats
        return Math.Max(stride, 16);
    }

    public DataStream Lock(int offsetToLock, int sizeToLock, LockFlags flags)
    {
        return new DataStream(sizeToLock, true, true);
    }

    public DataStream Lock(LockFlags flags)
    {
        return Lock(0, _sizeInBytes, flags);
    }

    public void Unlock()
    {
        // In OpenGL, unmap the buffer
    }

    public void Bind()
    {
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
    }

    public void SetData<T>(T[] data) where T : unmanaged
    {
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
        int byteCount = data.Length * Marshal.SizeOf<T>();
        _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)byteCount, (ReadOnlySpan<T>)data);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                _gl.DeleteBuffer(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
    }
}

/// <summary>Cross-platform vertex declaration</summary>
public unsafe class VertexDeclaration : IDisposable
{
    private GL _gl;
    private VertexElement[] _elements;
    private bool _disposed;

    public int Stride { get; private set; }
    public bool Disposed => _disposed;

    public VertexDeclaration(Device device, VertexElement[] elements)
    {
        _gl = device.GL;
        _elements = elements;

        // Calculate stride
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

    public void Apply()
    {
        uint index = 0;
        foreach (var elem in _elements)
        {
            if (elem.Usage == DeclarationUsage.Position || elem.Usage == DeclarationUsage.PositionT)
            {
                _gl.VertexAttribPointer(index, GetElementComponents(elem.Type), GetElementType(elem.Type), false, (uint)Stride, (void*)elem.Offset);
                _gl.EnableVertexAttribArray(index);
                index++;
            }
            else if (elem.Usage == DeclarationUsage.TextureCoordinate)
            {
                _gl.VertexAttribPointer(index, GetElementComponents(elem.Type), GetElementType(elem.Type), false, (uint)Stride, (void*)elem.Offset);
                _gl.EnableVertexAttribArray(index);
                index++;
            }
            else if (elem.Usage == DeclarationUsage.Color)
            {
                _gl.VertexAttribPointer(index, 4, VertexAttribPointerType.UnsignedByte, true, (uint)Stride, (void*)elem.Offset);
                _gl.EnableVertexAttribArray(index);
                index++;
            }
        }
    }

    private int GetElementComponents(DeclarationType type) => type switch
    {
        DeclarationType.Float1 => 1,
        DeclarationType.Float2 => 2,
        DeclarationType.Float16_2 => 2,
        DeclarationType.Float3 => 3,
        DeclarationType.Float4 => 4,
        DeclarationType.Float16_4 => 4,
        DeclarationType.Color => 4,
        DeclarationType.UByte4 => 4,
        DeclarationType.Short2 => 2,
        DeclarationType.Short4 => 4,
        _ => 1,
    };

    private VertexAttribPointerType GetElementType(DeclarationType type) => type switch
    {
        DeclarationType.Float1 => VertexAttribPointerType.Float,
        DeclarationType.Float2 => VertexAttribPointerType.Float,
        DeclarationType.Float3 => VertexAttribPointerType.Float,
        DeclarationType.Float4 => VertexAttribPointerType.Float,
        DeclarationType.Color => VertexAttribPointerType.UnsignedByte,
        DeclarationType.UByte4 => VertexAttribPointerType.UnsignedByte,
        DeclarationType.Short2 => VertexAttribPointerType.Short,
        DeclarationType.Short4 => VertexAttribPointerType.Short,
        DeclarationType.Float16_2 => VertexAttribPointerType.HalfFloat,
        DeclarationType.Float16_4 => VertexAttribPointerType.HalfFloat,
        _ => VertexAttribPointerType.Float,
    };

    public void Dispose()
    {
        _disposed = true;
    }
}

// VertexElement and DeclarationMethod are in Direct3D9AdditionalTypes.cs
