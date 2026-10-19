using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace SlimDX.Direct3D9;

internal sealed class Device3D : IDisposable
{
    private readonly Device _device;
    private readonly GL _gl;
    private uint _program;
    private uint _vao;
    private int _worldLocation, _viewLocation, _projectionLocation, _textureLocation, _colourLocation;
    private readonly Matrix4x4[] _transforms = new Matrix4x4[4];
    private VertexBuffer _stream;
    private VertexDeclaration _declaration;
    private Material _material;
    private Texture _texture0;
    private bool _depthTest;
    private Cull _cull = Cull.None;
    private bool _fogEnabled, _lighting, _specularEnabled;
    private bool _localViewer = true;
    private bool _normalizeNormals;
    private bool _colorVertex = true;
    private int _diffuseSource = 1, _specularSource = 2, _ambientSource, _emissiveSource;
    private float _fogStart, _fogEnd = 1, _fogDensity = 1;
    private FogMode _fogMode;
    private System.Drawing.Color _fogColour = System.Drawing.Color.Black;
    private readonly System.Collections.Generic.Dictionary<int, Light> _lights = new();
    private readonly System.Collections.Generic.HashSet<int> _enabledLights = new();
    private System.Drawing.Color _ambient = System.Drawing.Color.Black;
    private int _diagnosticDraw;

    internal Device3D(Device device, GL gl)
    {
        _device = device;
        _gl = gl;
        for (int i = 0; i < _transforms.Length; i++) _transforms[i] = Matrix4x4.Identity;
        CreateProgram();
        _vao = _gl.GenVertexArray();
    }

    private void CreateProgram()
    {
        const string vertex = @"#version 330 core
layout(location=0) in vec3 aPosition;
layout(location=1) in vec2 aTexCoord;
layout(location=2) in vec3 aNormal;
layout(location=3) in vec4 aColour;
layout(location=4) in vec4 aSecondary;
uniform bool uHasColour, uPackedColour, uHasNormal, uLighting;
uniform bool uHasSecondary, uPackedSecondary, uColorVertex, uNormalizeNormals, uSpecularEnabled;
uniform int uDiffuseSource, uAmbientSource, uEmissiveSource, uSpecularSource;
uniform vec4 uColour;
uniform vec3 uAmbientLight;
uniform vec4 uMaterialAmbient, uMaterialEmissive;
uniform vec3 uSpecular;
uniform float uPower;
uniform int uLightCount;
uniform vec3 uLightDirection[8];
uniform vec3 uLightDiffuse[8];
uniform vec3 uLightSpecular[8];
out vec4 VertexColour;
out vec3 VertexSpecular;
uniform bool uLocalViewer;
uniform mat4 uWorld;
uniform mat4 uView;
uniform mat4 uProjection;
out vec2 TexCoord;
out float FogDepth;
vec4 sourceColour(int source, vec4 material, vec4 primary, vec4 secondary) {
    if (uColorVertex && source == 1 && uHasColour) return primary;
    if (uColorVertex && source == 2 && uHasSecondary) return secondary;
    return material;
}
void main() {
    vec4 eye = uView * uWorld * vec4(aPosition, 1.0);
    FogDepth = abs(eye.z);
    vec4 clip = uProjection * eye;
    clip.z = 2.0 * clip.z - clip.w;
    gl_Position = clip;
    TexCoord = aTexCoord;
    vec4 vertex = uPackedColour ? aColour.bgra : aColour;
    vec4 secondary = uPackedSecondary ? aSecondary.bgra : aSecondary;
    VertexColour = uHasColour ? vertex : uColour;
    VertexSpecular = uSpecularEnabled && uHasSecondary ? secondary.rgb : vec3(0);
    if (uLighting) {
        vec4 diffuse = sourceColour(uDiffuseSource, uColour, vertex, secondary);
        vec3 ambient = sourceColour(uAmbientSource, uMaterialAmbient, vertex, secondary).rgb;
        vec3 emission = sourceColour(uEmissiveSource, uMaterialEmissive, vertex, secondary).rgb;
        vec3 specular = sourceColour(uSpecularSource, vec4(uSpecular,1.0), vertex, secondary).rgb;
        vec3 lit = ambient * uAmbientLight + emission;
        VertexSpecular = vec3(0);
        if (uHasNormal) {
            vec3 normal = transpose(inverse(mat3(uView * uWorld))) * aNormal;
            if (uNormalizeNormals && dot(normal,normal) > 0.0) normal = normalize(normal);
            for (int i=0; i<uLightCount; i++) {
                vec3 light = normalize(-uLightDirection[i]);
                float amount = max(dot(normal, light), 0.0);
                lit += diffuse.rgb * uLightDiffuse[i] * amount;
                if (amount > 0.0 && uPower > 0.0) {
                    vec3 viewer = uLocalViewer ? normalize(-eye.xyz) : vec3(0,0,-1);
                    vec3 halfVector = normalize(light + viewer);
                    VertexSpecular += specular * uLightSpecular[i] * pow(max(dot(normal,halfVector),0.0),uPower);
                }
            }
        }
        VertexColour = vec4(clamp(lit,0.0,1.0),diffuse.a);
        VertexSpecular = clamp(VertexSpecular,0.0,1.0);
    }
}";
        const string fragment = @"#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
uniform sampler2D uTexture;
in vec4 VertexColour;
in vec3 VertexSpecular;
in float FogDepth;
uniform int uFogMode;
uniform vec3 uFogColour;
uniform vec3 uFogParams;
void main() {
    vec4 colour = texture(uTexture, TexCoord) * VertexColour;
    colour.rgb += VertexSpecular;
    float visibility = 1.0;
    if (uFogMode == 3) visibility = clamp((uFogParams.y - FogDepth) / (uFogParams.y - uFogParams.x), 0.0, 1.0);
    if (uFogMode == 1) visibility = clamp(exp(-uFogParams.z * FogDepth), 0.0, 1.0);
    if (uFogMode == 2) visibility = clamp(exp(-pow(uFogParams.z * FogDepth, 2.0)), 0.0, 1.0);
    FragColor = vec4(mix(uFogColour, colour.rgb, visibility), colour.a);
}";
        uint vs = Compile(ShaderType.VertexShader, vertex);
        uint fs = Compile(ShaderType.FragmentShader, fragment);
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vs);
        _gl.AttachShader(_program, fs);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linked);
        if (linked == 0) throw new InvalidOperationException(_gl.GetProgramInfoLog(_program));
        _gl.DetachShader(_program, vs);
        _gl.DetachShader(_program, fs);
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
        _worldLocation = _gl.GetUniformLocation(_program, "uWorld");
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
        if (compiled == 0) throw new InvalidOperationException(_gl.GetShaderInfoLog(shader));
        return shader;
    }

    internal void SetTransform(TransformState state, Matrix matrix) => _transforms[(int)state] = matrix.ToSystemMatrix();
    internal Matrix4x4 World => _transforms[(int)TransformState.World];
    internal Matrix4x4 View => _transforms[(int)TransformState.View];
    internal Matrix4x4 Projection => _transforms[(int)TransformState.Projection];
    private int _offset;
    private int _stride;
    internal void SetStreamSource(VertexBuffer buffer, int offset, int stride)
    {
        _stream = buffer;
        _offset = offset;
        _stride = stride;
    }
    internal void SetVertexDeclaration(VertexDeclaration declaration) => _declaration = declaration;
    internal void SetIndices(IndexBuffer indices) { }
    internal void SetMaterial(Material material) => _material = material;
    internal void SetTexture(Texture texture) => _texture0 = texture;
    internal void SetDepthTest(bool enabled) => _depthTest = enabled;
    internal void SetLight(int index, Light light) => _lights[index] = light;
    internal void EnableLight(int index, bool enabled) { if (enabled) _enabledLights.Add(index); else _enabledLights.Remove(index); }
    internal void SetState(RenderState state, int value)
    {
        switch (state)
        {
            case RenderState.CullMode: _cull = (Cull)value; break;
            case RenderState.FogEnable: _fogEnabled = value != 0; break;
            case RenderState.FogTableMode: _fogMode = (FogMode)value; break;
            case RenderState.FogColor: _fogColour = System.Drawing.Color.FromArgb(value); break;
            case RenderState.Lighting: _lighting = value != 0; break;
            case RenderState.SpecularEnable: _specularEnabled = value != 0; break;
            case RenderState.LocalViewer: _localViewer = value != 0; break;
            case RenderState.NormalizeNormals: _normalizeNormals = value != 0; break;
            case RenderState.ColorVertex: _colorVertex = value != 0; break;
            case RenderState.DiffuseMaterialSource: _diffuseSource = CheckedSource(value); break;
            case RenderState.SpecularMaterialSource: _specularSource = CheckedSource(value); break;
            case RenderState.AmbientMaterialSource: _ambientSource = CheckedSource(value); break;
            case RenderState.EmissiveMaterialSource: _emissiveSource = CheckedSource(value); break;
            case RenderState.Ambient: _ambient = System.Drawing.Color.FromArgb(value); break;
        }
    }
    private static int CheckedSource(int value)
        => value is >= 0 and <= 2 ? value : throw new ArgumentOutOfRangeException(nameof(value),"Material source must be Material=0, Color1=1 or Color2=2.");

    internal void SetState(RenderState state, float value)
    {
        switch (state)
        {
            case RenderState.FogStart: _fogStart = value; break;
            case RenderState.FogEnd: _fogEnd = value; break;
            case RenderState.FogDensity: _fogDensity = value; break;
        }
    }

    internal unsafe void Draw(PrimitiveType type, int startVertex, int primitiveCount)
    {
        if (_stream == null || primitiveCount <= 0) return;
        int vertexCount = type switch
        {
            PrimitiveType.PointList => primitiveCount,
            PrimitiveType.LineList => primitiveCount * 2,
            PrimitiveType.LineStrip => primitiveCount + 1,
            PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan => primitiveCount + 2,
            _ => primitiveCount * 3,
        };
        var glType = type switch
        {
            PrimitiveType.PointList => Silk.NET.OpenGL.PrimitiveType.Points,
            PrimitiveType.LineList => Silk.NET.OpenGL.PrimitiveType.Lines,
            PrimitiveType.LineStrip => Silk.NET.OpenGL.PrimitiveType.LineStrip,
            PrimitiveType.TriangleStrip => Silk.NET.OpenGL.PrimitiveType.TriangleStrip,
            PrimitiveType.TriangleFan => Silk.NET.OpenGL.PrimitiveType.TriangleFan,
            _ => Silk.NET.OpenGL.PrimitiveType.Triangles,
        };
        _device.ApplyBlendState();
        if (_cull == Cull.None) _gl.Disable(Silk.NET.OpenGL.EnableCap.CullFace);
        else
        {
            _gl.Enable(Silk.NET.OpenGL.EnableCap.CullFace);
            _gl.FrontFace(FrontFaceDirection.Ccw);
            _gl.CullFace(_cull == Cull.Clockwise ? GLEnum.Front : GLEnum.Back);
        }
        _gl.DepthMask(_device.DepthWriteEnabled);
        if (_depthTest) { _gl.Enable(Silk.NET.OpenGL.EnableCap.DepthTest); _gl.DepthFunc(DepthFunction.Lequal); }
        else _gl.Disable(Silk.NET.OpenGL.EnableCap.DepthTest);
        _gl.UseProgram(_program);
        UploadMatrix(_worldLocation, World);
        UploadMatrix(_viewLocation, View);
        UploadMatrix(_projectionLocation, Projection);
        var diffuse = _material.Diffuse;
        if (_lighting)
        {
            float r = _ambient.R / 255f, g = _ambient.G / 255f, b = _ambient.B / 255f;
            foreach (int index in _enabledLights)
                if (_lights.TryGetValue(index, out var light))
                { r += light.Ambient.R / 255f; g += light.Ambient.G / 255f; b += light.Ambient.B / 255f; }
            _gl.Uniform3(_gl.GetUniformLocation(_program, "uAmbientLight"),r,g,b);
        }
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uLighting"), _lighting ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uLocalViewer"), _localViewer ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uNormalizeNormals"), _normalizeNormals ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uColorVertex"), _colorVertex ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uSpecularEnabled"), _specularEnabled ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uDiffuseSource"), _diffuseSource);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uAmbientSource"), _ambientSource);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uEmissiveSource"), _emissiveSource);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uSpecularSource"), _specularSource);
        _gl.Uniform4(_gl.GetUniformLocation(_program, "uMaterialAmbient"),_material.Ambient.Red,_material.Ambient.Green,_material.Ambient.Blue,_material.Ambient.Alpha);
        _gl.Uniform4(_gl.GetUniformLocation(_program, "uMaterialEmissive"),_material.Emissive.Red,_material.Emissive.Green,_material.Emissive.Blue,_material.Emissive.Alpha);
        _gl.Uniform3(_gl.GetUniformLocation(_program, "uSpecular"), _material.Specular.Red, _material.Specular.Green, _material.Specular.Blue);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uPower"), _specularEnabled ? _material.Power : 0f);
        int lightIndex = 0;
        foreach (int index in _enabledLights)
        {
            if (lightIndex == 8) break;
            if (!_lights.TryGetValue(index, out var light) || light.Type != LightType.Directional) continue;
            var direction = System.Numerics.Vector3.TransformNormal(light.Direction, View);
            _gl.Uniform3(_gl.GetUniformLocation(_program, $"uLightDirection[{lightIndex}]"),direction.X,direction.Y,direction.Z);
            _gl.Uniform3(_gl.GetUniformLocation(_program, $"uLightDiffuse[{lightIndex}]"),light.Diffuse.R / 255f,light.Diffuse.G / 255f,light.Diffuse.B / 255f);
            _gl.Uniform3(_gl.GetUniformLocation(_program, $"uLightSpecular[{lightIndex}]"),light.Specular.R / 255f,light.Specular.G / 255f,light.Specular.B / 255f);
            lightIndex++;
        }
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uLightCount"), lightIndex);
        _gl.Uniform1(_gl.GetUniformLocation(_program, "uFogMode"), _fogEnabled ? (int)_fogMode : 0);
        _gl.Uniform3(_gl.GetUniformLocation(_program, "uFogColour"), _fogColour.R / 255f, _fogColour.G / 255f, _fogColour.B / 255f);
        _gl.Uniform3(_gl.GetUniformLocation(_program, "uFogParams"), _fogStart, _fogEnd, _fogDensity);
        _gl.Uniform4(_colourLocation, diffuse.Red, diffuse.Green, diffuse.Blue, diffuse.Alpha);
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture0?.Handle ?? 0);
        _gl.Uniform1(_textureLocation, 0);
        _device.ApplySamplerState();
        int width = _device.RenderTarget?.Width ?? Math.Max(1, _device.FramebufferWidth);
        int height = _device.RenderTarget?.Height ?? Math.Max(1, _device.FramebufferHeight);
        var viewport = _device.Viewport;
        int logicalWidth = _device.RenderTarget?.Width ?? _device.GameWidth;
        int logicalHeight = _device.RenderTarget?.Height ?? _device.GameHeight;
        float scale = _device.RenderTarget != null ? 1f : Math.Min((float)width / logicalWidth, (float)height / logicalHeight);
        int left = (int)((width - logicalWidth * scale) / 2 + viewport.X * scale);
        int bottom = (int)((height - logicalHeight * scale) / 2 + (logicalHeight - viewport.Y - viewport.Height) * scale);
        _gl.Viewport(left, bottom, (uint)(viewport.Width * scale), (uint)(viewport.Height * scale));
        _gl.DepthRange(viewport.MinZ, viewport.MaxZ);
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _stream.Handle);
        int stride = _stride > 0 ? _stride : _stream.Stride;
        ConfigureAttributes(stride);
        bool probe = Environment.GetEnvironmentVariable("THSSS_DIAG3D") == "1"
            && ++_diagnosticDraw >= 100 && _diagnosticDraw <= 1000 && _diagnosticDraw % 20 == 0;
        byte[] before = probe ? ReadTarget(width, height) : null;
        _gl.DrawArrays(glType, startVertex, (uint)vertexCount);
        if (probe)
        {
            var after = ReadTarget(width, height);
            int changed = 0;
            for (int i = 0; i < after.Length; i += 4)
                if (after[i] != before[i] || after[i+1] != before[i+1] || after[i+2] != before[i+2]) changed++;
            Console.WriteLine($"[3D-PIXELS] draw={_diagnosticDraw} changedRGB={changed} texture={_texture0?.Handle} materialAlpha={diffuse.Alpha} error={_gl.GetError()}");
        }
        _gl.BindVertexArray(0);
        _gl.BindSampler(0,0);
        _gl.UseProgram(0);
    }

    private unsafe void ConfigureAttributes(int stride)
    {
        int position = 0, normal = -1, uv = -1, colour = -1, secondary = -1;
        bool packed = true, secondaryPacked = true;
        if (_declaration != null)
        {
            foreach (var element in _declaration.Elements)
            {
                if (element.Type == DeclarationType.Unused || element.Stream != 0) continue;
                switch (element.Usage)
                {
                    case DeclarationUsage.Position: position = element.Offset; break;
                    case DeclarationUsage.Normal: normal = element.Offset; break;
                    case DeclarationUsage.TexCoord when element.UsageIndex == 0: uv = element.Offset; break;
                    case DeclarationUsage.Color when element.UsageIndex == 0:
                        colour = element.Offset; packed = element.Type == DeclarationType.Color; break;
                    case DeclarationUsage.Color when element.UsageIndex == 1:
                        secondary = element.Offset; secondaryPacked = element.Type == DeclarationType.Color; break;
                }
            }
        }
        else
        {
            int offset = 12;
            var format = _device.VertexFormat;
            if ((format & VertexFormat.Normal) != 0) { normal = offset; offset += 12; }
            if ((format & VertexFormat.Diffuse) != 0) { colour = offset; offset += 4; }
            if ((format & VertexFormat.Specular) != 0) { secondary = offset; offset += 4; }
            if (offset + 8 <= stride) uv = offset;
        }
        _gl.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,(uint)stride,(void*)(_offset+position));
        _gl.EnableVertexAttribArray(0);
        SetAttribute(1,2,uv,VertexAttribPointerType.Float,false,stride);
        SetAttribute(2,3,normal,VertexAttribPointerType.Float,false,stride);
        SetAttribute(3,4,colour,packed ? VertexAttribPointerType.UnsignedByte : VertexAttribPointerType.Float,packed,stride);
        SetAttribute(4,4,secondary,secondaryPacked ? VertexAttribPointerType.UnsignedByte : VertexAttribPointerType.Float,secondaryPacked,stride);
        if (secondary < 0) _gl.VertexAttrib4(4,0f,0f,0f,0f);
        if (normal < 0) _gl.VertexAttrib3(2,0f,0f,0f);
        if (colour < 0) _gl.VertexAttrib4(3,1f,1f,1f,1f);
        _gl.Uniform1(_gl.GetUniformLocation(_program,"uHasNormal"),normal>=0 ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program,"uHasColour"),colour>=0 ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program,"uPackedColour"),packed ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program,"uHasSecondary"),secondary>=0 ? 1 : 0);
        _gl.Uniform1(_gl.GetUniformLocation(_program,"uPackedSecondary"),secondaryPacked ? 1 : 0);
    }

    private unsafe void SetAttribute(uint location,int size,int offset,VertexAttribPointerType type,bool normalized,int stride)
    {
        if (offset < 0) { _gl.DisableVertexAttribArray(location); return; }
        _gl.VertexAttribPointer(location,size,type,normalized,(uint)stride,(void*)(_offset+offset));
        _gl.EnableVertexAttribArray(location);
    }

    private unsafe byte[] ReadTarget(int width, int height)
    {
        var pixels = new byte[width * height * 4];
        fixed (byte* p = pixels)
            _gl.ReadPixels(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        return pixels;
    }

    private void UploadMatrix(int location, Matrix4x4 m)
    {
        var values = WorldViewProjection.ColumnMajor(m);
        _gl.UniformMatrix4(location, 1, false, values);
    }

    public void Dispose()
    {
        if (_program != 0) _gl.DeleteProgram(_program);
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        _program = _vao = 0;
    }
}
