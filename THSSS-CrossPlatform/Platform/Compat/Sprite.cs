// Cross-platform sprite batching (replaces SlimDX.Direct3D9.Sprite, which used D3DXSprite).
using System;
using System.Drawing;
using Silk.NET.OpenGL;
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
    private readonly int _transformLocation;
    private readonly int _projectionLocation;
    private readonly int _textureLocation;

    private int _batchCount;
    private uint _currentTexture;
    private Matrix _transform = Matrix.Identity;
    private bool _disposed;

    public Sprite(Device device)
    {
        _device = device;
        _gl = device.GL;
        _shader = CreateShader(out _transformLocation, out _projectionLocation, out _textureLocation);
        (_vao, _vbo) = CreateBuffers();
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
        _currentTexture = 0;
        // Blend factors were just set by the 3D pass (D3D keeps them as device
        // state); honour them here instead of forcing alpha blending, then reset
        // so later sprites use the classic SrcAlpha/1-SrcAlpha.
        _gl.Enable(Silk.NET.OpenGL.EnableCap.Blend);
        _gl.BlendFunc(_device.CurrentSourceBlend, _device.CurrentDestinationBlend);
        _gl.Disable(Silk.NET.OpenGL.EnableCap.DepthTest);
        _gl.Disable(Silk.NET.OpenGL.EnableCap.CullFace);
    }

    public void End() => Flush();

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
        if (_batchCount > 0 && _currentTexture != texture.Handle) Flush();
        if (_batchCount >= MaxBatchSize) Flush();
        _currentTexture = texture.Handle;

        var (u0, v0, u1, v1) = AtlasCoordinates(texture, sourceRect);
        // Atlas rows upload in source order, so v=0 is the image's top row. A GL
        // render target keeps its top row at v=1, so only that source inverts.
        if ((texture.Usage & Usage.RenderTarget) != 0)
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
        _gl.Viewport(viewportX, viewportY, (uint)viewportWidth, (uint)viewportHeight);

        _gl.UseProgram(_shader);
        var projection = Matrix.OrthoOffCenterLH(0, _device.GameWidth, _device.GameHeight, 0, -1, 1);
        _gl.UniformMatrix4(_projectionLocation, 1, false, projection.ToOpenGLMatrix());
        _gl.UniformMatrix4(_transformLocation, 1, false, _transform.ToOpenGLMatrix());

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _currentTexture);
        _gl.Uniform1(_textureLocation, 0);

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
            (uint)(_batchCount * VerticesPerQuad * FloatsPerVertex * sizeof(float)),
            (ReadOnlySpan<float>)_vertices);
        _gl.DrawArrays(Silk.NET.OpenGL.PrimitiveType.Triangles, 0, (uint)(_batchCount * VerticesPerQuad));

        _gl.BindVertexArray(0);
        _gl.UseProgram(0);
        _device.ResetBlendState();
        _batchCount = 0;
    }

    private (int X, int Y, int Width, int Height) FitLogicalSurface(int width, int height)
    {
        float logicalAspect = (float)_device.GameWidth / _device.GameHeight;
        float framebufferAspect = (float)width / height;
        if (framebufferAspect > logicalAspect)
        {
            int scaledWidth = (int)(height * logicalAspect);
            return ((width - scaledWidth) / 2, 0, scaledWidth, height);
        }
        int scaledHeight = (int)(width / logicalAspect);
        return (0, (height - scaledHeight) / 2, width, scaledHeight);
    }

    private uint CreateShader(out int transformLocation, out int projectionLocation, out int textureLocation)
    {
        const string vertexSource = @"#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;
uniform mat4 uTransform;
uniform mat4 uProjection;
out vec2 TexCoord;
out vec4 Color;
void main()
{
    gl_Position = uProjection * uTransform * vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
    Color = aColor;
}";

        const string fragmentSource = @"#version 330 core
in vec2 TexCoord;
in vec4 Color;
out vec4 FragColor;
uniform sampler2D uTexture;
void main()
{
    FragColor = texture(uTexture, TexCoord) * Color;
}";

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

        transformLocation = _gl.GetUniformLocation(program, "uTransform");
        projectionLocation = _gl.GetUniformLocation(program, "uProjection");
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
