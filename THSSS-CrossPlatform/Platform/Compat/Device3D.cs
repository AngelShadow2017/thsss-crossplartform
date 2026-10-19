// Immediate-mode 3D pipeline for Direct3D9.Device.
//
// The game draws 3D backgrounds, stage-title slabs and boss models through
// SetStreamSource + DrawPrimitives with World/View/Projection transforms and
// per-model materials. The sprite batcher cannot express that, so this file owns
// a small GL program with its own dynamic vertex buffer.
using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace SlimDX.Direct3D9;

internal sealed class Device3D : IDisposable
{
    private GL _gl;
    private uint _program;
    private uint _vao;
    private uint _vbo;
    private int _vboCapacity;
    private int _transformLocation;
    private int _viewLocation;
    private int _projectionLocation;
    private int _textureLocation;
    private int _colourLocation;
    private bool _disposed;

    // Fixed-function state the game sets but never reads back.
    private readonly Matrix4x4[] _transforms = new Matrix4x4[4];
    private VertexBuffer _stream;
    private VertexDeclaration _declaration;
    private IndexBuffer _indices;
    private Material _material;
    private Texture _texture0;

    internal Device3D(GL gl)
    {
        _gl = gl;
        for (int i = 0; i < _transforms.Length; i++) _transforms[i] = Matrix4x4.Identity;
        CreateProgram();
        CreateBuffers();
    }

    private void CreateProgram()
    {
        const string vertexSource = @"#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
uniform mat4 uWorld;
uniform mat4 uView;
uniform mat4 uProjection;
out vec2 TexCoord;
void main()
{
    gl_Position = uProjection * uView * uWorld * vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
}";

        const string fragmentSource = @"#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
uniform vec4 uColour;
void main()
{
    FragColor = texture(uTexture, TexCoord) * uColour;
}";

        uint vs = Compile(ShaderType.VertexShader, vertexSource);
        uint fs = Compile(ShaderType.FragmentShader, fragmentSource);
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vs);
        _gl.AttachShader(_program, fs);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linked);
        if (linked == 0) throw new InvalidOperationException($"3D shader link failed: {_gl.GetProgramInfoLog(_program)}");
        _gl.DetachShader(_program, vs);
        _gl.DetachShader(_program, fs);
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);

        _transformLocation = _gl.GetUniformLocation(_program, "uWorld");
        _viewLocation = _gl.GetUniformLocation(_program, "uView");
        _projectionLocation = _gl.GetUniformLocation(_program, "uProjection");
        _textureLocation = _gl.GetUniformLocation(_program, "uTexture");
        _colourLocation = _gl.GetUniformLocation(_program, "uColour");
    }

    private uint Compile(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compiled);
        if (compiled == 0) throw new InvalidOperationException($"3D {type} failed: {_gl.GetShaderInfoLog(shader)}");
        return shader;
    }

    private void CreateBuffers()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        const uint stride = 5 * sizeof(float); // pos(3) + uv(2)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        _gl.EnableVertexAttribArray(1);
        _gl.BindVertexArray(0);
    }

    internal void SetTransform(TransformState state, Matrix matrix) => _transforms[(int)state] = matrix.ToSystemMatrix();

    internal Matrix4x4 World => _transforms[(int)TransformState.World];
    internal Matrix4x4 View => _transforms[(int)TransformState.View];
    internal Matrix4x4 Projection => _transforms[(int)TransformState.Projection];

    internal void SetStreamSource(VertexBuffer buffer) => _stream = buffer;
    internal void SetVertexDeclaration(VertexDeclaration declaration) => _declaration = declaration;
    internal void SetIndices(IndexBuffer indices) => _indices = indices;
    internal void SetMaterial(Material material) => _material = material;
    internal void SetTexture(Texture texture) => _texture0 = texture;

    /// <summary>
    /// Reads the bound vertex buffer back from OpenGL. The game's models upload
    /// through DataStream, so the buffer contents (not the CPU-side struct) are
    /// the authoritative geometry.
    /// </summary>
    private float[] _scratch = Array.Empty<float>();

    internal unsafe void Draw(PrimitiveType type, int startVertex, int primitiveCount)
    {
        if (_stream == null || _stream.Handle == 0 || primitiveCount <= 0) return;

        var glType = ToGlPrimitive(type);
        int vertexCount = VerticesFor(type, primitiveCount);
        if (vertexCount <= 0) return;

        int stride = _stream.Stride;
        int offsetBytes = startVertex * stride;
        int byteCount = vertexCount * stride;

        var raw = new byte[byteCount];
        fixed (byte* dst = raw)
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _stream.Handle);
            _gl.GetBufferSubData(BufferTargetARB.ArrayBuffer, offsetBytes, (uint)byteCount, dst);
        }

        // Position(3 floats) + uv(2 floats) is the only layout the game's models use.
        int floats = vertexCount * 5;
        if (_scratch.Length < floats) _scratch = new float[floats];
        for (int i = 0; i < vertexCount; i++)
        {
            int src = i * stride;
            _scratch[i * 5 + 0] = BitConverter.ToSingle(raw, src + 0);
            _scratch[i * 5 + 1] = BitConverter.ToSingle(raw, src + 4);
            _scratch[i * 5 + 2] = BitConverter.ToSingle(raw, src + 8);
            _scratch[i * 5 + 3] = BitConverter.ToSingle(raw, src + 12);
            _scratch[i * 5 + 4] = BitConverter.ToSingle(raw, src + 16);
        }

        Upload(floatCount: floats);

        _gl.UseProgram(_program);
        UploadMatrix(_transformLocation, World);
        UploadMatrix(_viewLocation, View);
        UploadMatrix(_projectionLocation, Projection);

        var diffuse = _material.Diffuse;
        _gl.Uniform4(_colourLocation, diffuse.Red, diffuse.Green, diffuse.Blue, diffuse.Alpha);

        if (_texture0 != null && _texture0.Handle != 0)
        {
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture0.Handle);
            _gl.Uniform1(_textureLocation, 0);
        }

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.DrawArrays(glType, 0, (uint)vertexCount);
        _gl.BindVertexArray(0);
        _gl.UseProgram(0);
    }

    private unsafe void Upload(int floatCount)
    {
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        int bytes = floatCount * sizeof(float);
        if (bytes > _vboCapacity)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)bytes, (void*)0, BufferUsageARB.DynamicDraw);
            _vboCapacity = bytes;
        }
        fixed (float* ptr = _scratch)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)bytes, ptr);
        }
    }

    private void UploadMatrix(int location, Matrix4x4 m)
    {
        if (location < 0) return;
        Span<float> values = stackalloc float[16];
        // Row-major input, column-major uniform (transpose = true).
        values[0] = m.M11; values[1] = m.M21; values[2] = m.M31; values[3] = m.M41;
        values[4] = m.M12; values[5] = m.M22; values[6] = m.M32; values[7] = m.M42;
        values[8] = m.M13; values[9] = m.M23; values[10] = m.M33; values[11] = m.M43;
        values[12] = m.M14; values[13] = m.M24; values[14] = m.M34; values[15] = m.M44;
        _gl.UniformMatrix4(location, 1, false, values);
    }

    private static Silk.NET.OpenGL.PrimitiveType ToGlPrimitive(PrimitiveType type) => type switch
    {
        PrimitiveType.PointList => Silk.NET.OpenGL.PrimitiveType.Points,
        PrimitiveType.LineList or PrimitiveType.LineStrip => Silk.NET.OpenGL.PrimitiveType.Lines,
        PrimitiveType.TriangleStrip => Silk.NET.OpenGL.PrimitiveType.TriangleStrip,
        PrimitiveType.TriangleFan => Silk.NET.OpenGL.PrimitiveType.TriangleFan,
        _ => Silk.NET.OpenGL.PrimitiveType.Triangles,
    };

    private static int VerticesFor(PrimitiveType type, int primitiveCount) => type switch
    {
        PrimitiveType.PointList => primitiveCount,
        PrimitiveType.LineList => primitiveCount * 2,
        PrimitiveType.LineStrip => primitiveCount + 1,
        PrimitiveType.TriangleList or PrimitiveType.Triangles => primitiveCount * 3,
        PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan => primitiveCount + 2,
        _ => primitiveCount * 3,
    };

    public void Dispose()
    {
        if (_disposed) return;
        if (_program != 0) _gl.DeleteProgram(_program);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        _disposed = true;
    }
}
