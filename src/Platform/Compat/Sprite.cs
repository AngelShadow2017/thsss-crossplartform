// Cross-platform sprite batching (replaces SlimDX.Direct3D9.Sprite, which used D3DXSprite).
using System;
using System.Drawing;
using Silk.NET.OpenGL;
using THSSS.Graphics;
using SlimDX;

namespace SlimDX.Direct3D9;

/// <summary>
/// Batches 2D quads and draws them with one shader. Coordinates are the game's
/// logical 640x480 space with the origin at the top-left, matching D3D9's
/// pre-transformed vertex layout.
/// </summary>
public class Sprite : IDisposable
{
    private const int FloatsPerVertex = 9;   // x, y, z, u, v, r, g, b, a
    private const int VerticesPerQuad = 6;   // two triangles
    private const int MaxBatchSize = 2048;

    private readonly GL _gl;
    private readonly Device _device;
    private readonly float[] _vertices = new float[MaxBatchSize * VerticesPerQuad * FloatsPerVertex];

    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _shader;
    private readonly THSSS.Graphics.IRenderShader _backendShader;
    private readonly THSSS.Graphics.IRenderBuffer _backendBuffer;
    private static readonly THSSS.Graphics.VertexLayout SpriteLayout = new(
        FloatsPerVertex * sizeof(float),
        new[]
        {
            new THSSS.Graphics.VertexAttribute(0, 3, THSSS.Graphics.VertexAttributeType.Float, false, 0),
            new THSSS.Graphics.VertexAttribute(1, 2, THSSS.Graphics.VertexAttributeType.Float, false, 3 * sizeof(float)),
            new THSSS.Graphics.VertexAttribute(2, 4, THSSS.Graphics.VertexAttributeType.Float, false, 5 * sizeof(float)),
        });
    private readonly int _textureLocation;
    private readonly int _glTextureLocation;
    private THSSS.Graphics.IRenderBuffer _uniformBlock;

    private int _batchCount;
    private IRenderHandle _currentTexture;
    private Texture _currentTextureObject;
    private Matrix _transform = Matrix.Identity;
    private bool _disposed;
    private SpriteFlags _flags;
    private (BlendingFactor Source, BlendingFactor Destination, BlendOperation Operation, bool Enabled) _savedBlend;
    private bool _savedDepth, _savedCull;

    public Sprite(Device device)
    {
        _device = device;
        _gl = device.GL;
        if (device.GL != null) _shader = CreateShader(out _glTextureLocation);
        _backendShader = device.RenderBackend.CreateShader(new THSSS.Graphics.ShaderSource
        {
            Vertex = SpriteVertexSource, Fragment = SpriteFragmentSource,
            VertexInputs = new[] { "aPosition", "aTexCoord", "aColor" },
        });
        _textureLocation = _backendShader.GetUniformLocation("uTexture");
        _backendBuffer = device.RenderBackend.CreateBuffer(THSSS.Graphics.BufferKind.Vertex,
            MaxBatchSize * VerticesPerQuad * FloatsPerVertex * sizeof(float));
        // std140 block: two mat4 = 128 bytes; created once and refilled per draw.
        _uniformBlock = device.RenderBackend.CreateBuffer(
            THSSS.Graphics.BufferKind.Uniform, 128);
        if (device.GL != null) (_vao, _vbo) = CreateBuffers();
    }

    public bool Disposed => _disposed;

    /// <summary>
    /// D3DXSprite applies this matrix to the stored vertices; MySprite sets it,
    /// draws, then restores Identity. Flush first so pending quads keep the old one.
    /// </summary>
    public Matrix Transform
    {
        get => _transform;
        set
        {
            if (_batchCount > 0) Flush();
            _transform = value;
        }
    }

    public void Begin() => Begin(SpriteFlags.None);

    public void Begin(SpriteFlags flags)
    {
        _batchCount = 0;
        _currentTexture = null;
        _flags = flags;
        _savedBlend = _device.CaptureBlendState();
        (_savedDepth, _savedCull) = _device.RenderBackend.QueryRasterState();
        if ((flags & SpriteFlags.DoNotModifyRenderState) != 0) return;
        // Blend factors were just set by the 3D pass (D3D keeps them as device
        // state); honour them here instead of forcing alpha blending, then reset
        // so later sprites use the classic SrcAlpha/1-SrcAlpha.
        _device.SetRenderState(RenderState.AlphaBlendEnable, (flags & SpriteFlags.AlphaBlend) != 0);
        if ((flags & SpriteFlags.AlphaBlend) != 0)
        {
            _device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            _device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        }
        _device.ApplyBlendState();
        _device.RenderBackend.SetRasterState(false, false);
    }

    public void End()
    {
        Flush();
        if ((_flags & SpriteFlags.DoNotSaveState) != 0) return;
        _device.RestoreBlendState(_savedBlend);
        _device.RenderBackend.SetRasterState(_savedDepth, _savedCull);
    }

    public void SetTransform(Matrix transform) => Transform = transform;

    public void Draw(Texture texture, Vector3 center, Vector3 position, Color4 color)
        => Draw(texture, null, center, position, color);

    public void Draw(Texture texture, Vector3 center, Vector3 position, Color color)
        => Draw(texture, null, center, position, new Color4(color));

    public void Draw(Texture texture, Rectangle? sourceRect, Vector3 position, Color4 color)
        => Draw(texture, sourceRect, null, position, color);

    public void Draw(Texture texture, Rectangle sourceRect, Vector3 position, Color4 color)
        => Draw(texture, (Rectangle?)sourceRect, null, position, color);

    public void Draw(Texture texture, Rectangle sourceRect, Vector3 rotationCenter, Vector3 position, Color4 color)
        => Draw(texture, (Rectangle?)sourceRect, (Vector3?)rotationCenter, position, color);

    public void Draw(Texture texture, Rectangle? sourceRect, Vector3? rotationCenter, Vector3 position, Color4 color)
    {
        if (texture == null) return;
        if (_batchCount > 0 && !ReferenceEquals(_currentTexture, texture.Handle)) Flush();
        if (_batchCount >= MaxBatchSize) Flush();
        _currentTexture = texture.Handle;
        _currentTextureObject = texture;

        var (u0, v0, u1, v1) = AtlasCoordinates(texture, sourceRect);
        // Atlas rows upload in source order, so v=0 is the image's top row. A GL
        // render target keeps its top row at v=1, so only that source inverts.
        if ((texture.Usage & Usage.RenderTarget) != 0 && _device.RenderBackend.Features.OriginBottomLeft)
        {
            (v0, v1) = (1f - v0, 1f - v1);
        }

        float width = sourceRect?.Width ?? texture.Width;
        float height = sourceRect?.Height ?? texture.Height;
        float centerX = rotationCenter?.X ?? 0f;
        float centerY = rotationCenter?.Y ?? 0f;
        float left = position.X - centerX;
        float top = position.Y - centerY;
        float right = left + width;
        float bottom = top + height;

        int o = _batchCount * VerticesPerQuad * FloatsPerVertex;
        // D3D9 sprite vertices are already screen-space and use clockwise winding.
        WriteVertex(ref o, left, top, position.Z, u0, v0, color);
        WriteVertex(ref o, right, top, position.Z, u1, v0, color);
        WriteVertex(ref o, left, bottom, position.Z, u0, v1, color);
        WriteVertex(ref o, right, top, position.Z, u1, v0, color);
        WriteVertex(ref o, right, bottom, position.Z, u1, v1, color);
        WriteVertex(ref o, left, bottom, position.Z, u0, v1, color);
        _batchCount++;
    }

    private void WriteVertex(ref int offset, float x, float y, float z, float u, float v, Color4 color)
    {
        _vertices[offset++] = x;
        _vertices[offset++] = y;
        _vertices[offset++] = z;
        _vertices[offset++] = u;
        _vertices[offset++] = v;
        _vertices[offset++] = color.Red;
        _vertices[offset++] = color.Green;
        _vertices[offset++] = color.Blue;
        _vertices[offset++] = color.Alpha;
    }

    private static (float U0, float V0, float U1, float V1) AtlasCoordinates(Texture texture, Rectangle? source)
    {
        if (!source.HasValue) return (0f, 0f, 1f, 1f);
        var r = source.Value;
        return ((float)r.X / texture.Width, (float)r.Y / texture.Height,
                (float)(r.X + r.Width) / texture.Width, (float)(r.Y + r.Height) / texture.Height);
    }

    private void Flush()
    {
        if (_batchCount == 0) return;

        // The projection always covers the fixed logical surface. A back-buffer
        // draw is letterboxed inside the window; an offscreen pass fills its target.
        var target = _device.RenderTarget;
        int width = target?.Width ?? Math.Max(1, _device.FramebufferWidth > 0
            ? _device.FramebufferWidth : _device.Viewport.Width);
        int height = target?.Height ?? Math.Max(1, _device.FramebufferHeight > 0
            ? _device.FramebufferHeight : _device.Viewport.Height);
        var (viewportX, viewportY, viewportWidth, viewportHeight) =
            FitLogicalSurface(width, height);
        _device.RenderBackend.SetViewport(viewportX, viewportY, viewportWidth, viewportHeight, 0f, 1f);

        var projection = Matrix.OrthoOffCenterLH(0, _device.GameWidth, _device.GameHeight, 0, -1, 1);
        var effect = _device.ActiveEffect;
        if (_gl != null) _gl.ActiveTexture(TextureUnit.Texture0);
        _device.RenderBackend.BindTexture(_currentTextureObject?.RenderTexture);
        if (effect != null)
        {
            effect.SourceTexture = _currentTextureObject;
            effect.Bind(_transform, projection);
        }
        else
        {
            if (_gl != null) _gl.UseProgram(_shader);
            _device.RenderBackend.SetUniformBlock(0, _uniformBlock,
                BuildUniformBlock(projection, _transform));
        }

        _device.ApplyBlendState();
        int floats = _batchCount * VerticesPerQuad * FloatsPerVertex;
        var vertexSpan = _vertices.AsSpan(0, floats);
        var vertexBytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(vertexSpan);
        if (effect != null && _gl == null)
        {
            _backendBuffer.Update(vertexBytes, 0);
            _device.RenderBackend.Draw(effect.BackendShader, _backendBuffer, SpriteLayout,
                THSSS.Graphics.PrimitiveKind.TriangleList, 0, _batchCount * VerticesPerQuad);
        }
        else if (effect != null)
        {
            // Effect passes render with their own program; keep the classic sprite batch.
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)(floats * sizeof(float)), (ReadOnlySpan<float>)vertexSpan);
            _gl.DrawArrays(Silk.NET.OpenGL.PrimitiveType.Triangles, 0, (uint)(_batchCount * VerticesPerQuad));
            _gl.BindVertexArray(0);
        }
        else
        {
            _backendBuffer.Update(vertexBytes, 0);
            _backendShader.SetUniform(_textureLocation, 0);
            _device.RenderBackend.Draw(_backendShader, _backendBuffer, SpriteLayout,
                THSSS.Graphics.PrimitiveKind.TriangleList, 0, _batchCount * VerticesPerQuad);
        }

        if (_gl != null) _gl.BindVertexArray(0);
        effect?.Unbind();
        _batchCount = 0;
    }

    private (int X, int Y, int Width, int Height) FitLogicalSurface(int width, int height)
    {
        var content = THSSS.Graphics.PresentationGeometry.Fit(width, height, _device.GameWidth, _device.GameHeight);
        return (content.X, content.Y, content.Width, content.Height);
    }

    /// <summary>
    /// std140 layout for SpriteUniforms: uTransform then uProjection, each a 4x4 matrix
    /// stored column-major, so the block is a flat 128-byte buffer.
    /// </summary>
    private static byte[] BuildUniformBlock(Matrix projection, Matrix transform)
    {
        var bytes = new byte[128];
        WriteMatrix(bytes, 0, transform.ToOpenGLMatrix());
        WriteMatrix(bytes, 64, projection.ToOpenGLMatrix());
        return bytes;
    }

    private static void WriteMatrix(byte[] destination, int offset, ReadOnlySpan<float> matrix)
    {
        for (int i = 0; i < 16; i++)
            BitConverter.TryWriteBytes(destination.AsSpan(offset + i * 4, 4), matrix[i]);
    }

    private const string SpriteVertexSource = @"#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;
layout (std140) uniform SpriteUniforms {
    mat4 uTransform;
    mat4 uProjection;
};
out vec2 TexCoord;
out vec4 Color;
void main()
{
    gl_Position = uProjection * uTransform * vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
    Color = aColor;
}";

    private const string SpriteFragmentSource = @"#version 330 core
in vec2 TexCoord;
in vec4 Color;
out vec4 FragColor;
uniform sampler2D uTexture;
void main()
{
    FragColor = texture(uTexture, TexCoord) * Color;
}";

    private uint CreateShader(out int textureLocation)
    {
        const string vertexSource = SpriteVertexSource;

        const string fragmentSource = SpriteFragmentSource;

        uint vertexShader = Compile(ShaderType.VertexShader, vertexSource);
        uint fragmentShader = Compile(ShaderType.FragmentShader, fragmentSource);
        uint program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linked);
        if (linked == 0) throw new InvalidOperationException($"Sprite shader link failed: {_gl.GetProgramInfoLog(program)}");
        _gl.DetachShader(program, vertexShader);
        _gl.DetachShader(program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        textureLocation = _gl.GetUniformLocation(program, "uTexture");
        return program;
    }

    private uint Compile(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compiled);
        if (compiled == 0) throw new InvalidOperationException($"Sprite {type} failed: {_gl.GetShaderInfoLog(shader)}");
        return shader;
    }

    private (uint Vao, uint Vbo) CreateBuffers()
    {
        uint vao = _gl.GenVertexArray();
        uint vbo = _gl.GenBuffer();
        _gl.BindVertexArray(vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)_vertices.Length * sizeof(float),
            (ReadOnlySpan<float>)_vertices, BufferUsageARB.DynamicDraw);

        const uint stride = FloatsPerVertex * sizeof(float);
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
        _gl.EnableVertexAttribArray(2);

        _gl.BindVertexArray(0);
        return (vao, vbo);
    }

    public void OnResetDevice() { }

    public void OnLostDevice() { }

    public void Dispose()
    {
        if (_disposed) return;
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_shader != 0) _gl.DeleteProgram(_shader);
        _backendShader.Dispose();
        _backendBuffer.Dispose();
        _uniformBlock.Dispose();
        _disposed = true;
    }
}

/// <summary>Color with float components (replaces SlimDX.Color4).</summary>
public struct Color4 : IEquatable<Color4>
{
    public float Red;
    public float Green;
    public float Blue;
    public float Alpha;

    public Color4(float r, float g, float b, float a) { Red = r; Green = g; Blue = b; Alpha = a; }
    public Color4(float r, float g, float b) { Red = r; Green = g; Blue = b; Alpha = 1.0f; }
    public Color4(Color color) { Red = color.R / 255f; Green = color.G / 255f; Blue = color.B / 255f; Alpha = color.A / 255f; }

    public static Color4 White => new(1, 1, 1, 1);
    public static Color4 Black => new(0, 0, 0, 1);
    public static Color4 Transparent => new(0, 0, 0, 0);

    public static implicit operator Color4(Color c) => new(c);

    public static Color4 operator *(Color4 a, Color4 b) =>
        new(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue, a.Alpha * b.Alpha);
    public static Color4 operator *(Color4 a, float s) =>
        new(a.Red * s, a.Green * s, a.Blue * s, a.Alpha * s);
    public static Color4 operator +(Color4 a, Color4 b) =>
        new(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue, a.Alpha + b.Alpha);

    public bool Equals(Color4 other) => Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;
    public override bool Equals(object other) => other is Color4 c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);
    public override string ToString() => $"{{Red:{Red} Green:{Green} Blue:{Blue} Alpha:{Alpha}}}";
}
