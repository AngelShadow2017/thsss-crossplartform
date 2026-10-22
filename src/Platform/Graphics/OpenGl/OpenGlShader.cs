using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using THSSS.Graphics;

namespace THSSS.Graphics.OpenGl;

/// <summary>
/// GLSL program. Differences between the GL backend's 330 core and a Vulkan SPIR-V
/// backend stay behind this type: callers only set uniforms by name.
/// </summary>
internal sealed class OpenGlShader : IRenderShader
{
    private readonly GL _gl;
    private readonly uint _program;
    private readonly Dictionary<string, int> _uniforms = new(StringComparer.Ordinal);
    private bool _disposed;

    internal OpenGlShader(GL gl, ShaderSource source)
    {
        _gl = gl;
        uint vertex = Compile(ShaderType.VertexShader, source.Vertex);
        uint fragment = Compile(ShaderType.FragmentShader, source.Fragment);
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertex);
        _gl.AttachShader(_program, fragment);
        BindAttributes(source.VertexInputs);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linked);
        string log = _gl.GetProgramInfoLog(_program);
        _gl.DetachShader(_program, vertex);
        _gl.DetachShader(_program, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
        if (linked == 0)
            throw new InvalidOperationException("Shader link failed: " + log);
        Handle = new OpenGlHandle(_program);
    }

    internal uint Program => _program;

    public IRenderHandle Handle { get; }

    private uint Compile(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        string log = _gl.GetShaderInfoLog(shader);
        if (status == 0)
        {
            _gl.DeleteShader(shader);
            throw new InvalidOperationException($"{type} compile failed: {log}");
        }
        return shader;
    }

    private void BindAttributes(string[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
            _gl.BindAttribLocation(_program, (uint)i, inputs[i]);
    }

    public void Use() => _gl.UseProgram(_program);

    public int GetUniformLocation(string name)
    {
        if (_uniforms.TryGetValue(name, out int cached)) return cached;
        int location = _gl.GetUniformLocation(_program, name);
        _uniforms[name] = location;
        return location;
    }

    public void SetUniform(int location, float value)
    {
        if (location >= 0) _gl.Uniform1(location, value);
    }

    public void SetUniform(int location, float x, float y)
    {
        if (location >= 0) _gl.Uniform2(location, x, y);
    }

    public void SetUniform(int location, float x, float y, float z)
    {
        if (location >= 0) _gl.Uniform3(location, x, y, z);
    }

    public void SetUniform(int location, float x, float y, float z, float w)
    {
        if (location >= 0) _gl.Uniform4(location, x, y, z, w);
    }

    public void SetUniform(int location, ReadOnlySpan<float> matrix4)
    {
        if (location < 0 || matrix4.Length < 16) return;
        _gl.UniformMatrix4(location, 1, false, matrix4);
    }

    public void SetUniform(int location, int value)
    {
        if (location >= 0) _gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_program != 0) _gl.DeleteProgram(_program);
    }
}
