using System;

namespace THSSS.Graphics;

/// <summary>Backend-neutral D3D9-style resource, state and frame contract.</summary>
public interface IRenderBackend : IDisposable
{
    BackendKind Kind { get; }
    BackendFeatures Features { get; }
    void Resize(int framebufferWidth, int framebufferHeight);
    IRenderTexture CreateTexture(int width, int height, TextureFormat format, ReadOnlySpan<byte> pixels);
    IRenderTexture CreateRenderTexture(int width, int height, TextureFormat format);
    IRenderBuffer CreateBuffer(BufferKind kind, int sizeInBytes);
    IRenderShader CreateShader(ShaderSource source);
    IRenderFramebuffer CreateFramebuffer(IRenderTexture? color, DepthFormat depth, int width, int height);
    IRenderDepthSurface CreateDepthSurface(int width, int height, DepthFormat format);
    void AttachPresentSurface(int width, int height) { }
    void BeginFrame() { }
    void EndFrame() { }
    void Present() { }
    void SetPresentSource(IRenderTexture? texture) { }
    void AttachDepth(IRenderDepthSurface? depth);
    (bool DepthTest, bool CullFace) QueryRasterState();
    void SetRasterState(bool depthTest, bool cullFace);
    void SetCapability(BackendCapability capability, bool enabled);
    void SetBlendState(BlendState state);
    void BindFramebuffer(IRenderFramebuffer? framebuffer);
    void RestoreFramebufferBindings(IRenderFramebuffer? draw, IRenderFramebuffer? read);
    void SetReadBuffer(int colourAttachment);
    IRenderFramebuffer? CurrentDrawFramebuffer { get; }
    IRenderFramebuffer? CurrentReadFramebuffer { get; }
    void SetViewport(int x, int y, int width, int height, float minDepth, float maxDepth);
    void Clear(ClearMask mask, float red, float green, float blue, float alpha, float depth, int stencil);
    void SetScissor(bool enabled, int x, int y, int width, int height);
    void SetDepthTest(bool enabled);
    void SetDepthWrite(bool enabled);
    void SetCullMode(CullMode mode);
    void SetBlend(BlendState blend);
    void SetSamplerState(SamplerStateBlock state);
    /// <summary>Uploads the shared std140 block; backend conversion must preserve its layout.</summary>
    void SetUniformBlock(int binding, IRenderBuffer? buffer, ReadOnlySpan<byte> data);
    void BindTexture(IRenderTexture? texture);
    void Draw(IRenderShader shader, IRenderBuffer vertices, VertexLayout layout,
              PrimitiveKind kind, int startVertex, int vertexCount);
    byte[] ReadPixels(int x, int y, int width, int height);
    bool TryGetError(out string message);
}

public enum BackendKind { OpenGl, Vulkan, Metal }
public readonly record struct BackendFeatures(bool SupportsSamplerObjects, bool SupportsRgb8Textures,
    bool OriginBottomLeft, int MaxColourAttachments, string ShaderLanguage);
public enum TextureFormat { Rgba8, Rgb8, Bgra8 }
public enum DepthFormat { None, Depth16, Depth24 }
public enum BufferKind { Vertex, Index, Uniform }
public enum PrimitiveKind { TriangleList, TriangleStrip, LineList }
public enum BackendCapability { DepthTest, ScissorTest, Multisample, CullFace }
public enum ClearMask { Color = 1, Depth = 2, Stencil = 4 }
public enum CullMode { None, Clockwise, CounterClockwise }
public readonly record struct BlendState(bool Enabled, BlendFactor Source, BlendFactor Destination, BlendEquation Equation);
/// <summary>D3D9 D3DBLEND values.</summary>
public enum BlendFactor : int
{
    Zero = 1, One = 2, SourceColor = 3, InverseSourceColor = 4,
    SourceAlpha = 5, InverseSourceAlpha = 6, DestinationAlpha = 7,
    InverseDestinationAlpha = 8, DestinationColor = 9, InverseDestinationColor = 10,
    SourceAlphaSaturate = 11,
}
public enum BlendEquation { Add = 1, Subtract = 2, ReverseSubtract = 3, Min = 4, Max = 5 }
/// <summary>Address and filter values follow D3DTEXF/D3DTADDRESS.</summary>
public readonly record struct SamplerStateBlock(int AddressU, int AddressV, int MinFilter, int MagFilter,
    int MipFilter, int MaxAnisotropy, float MipLodBias, float BorderRed, float BorderGreen,
    float BorderBlue, float BorderAlpha, bool BorderEnabled);
public readonly record struct VertexLayout(int Stride, ReadOnlyMemory<VertexAttribute> Attributes);
public readonly record struct VertexAttribute(int Location, int ComponentCount, VertexAttributeType Type,
    bool Normalized, int ByteOffset);
public enum VertexAttributeType { Float, UnsignedByte }
public sealed class ShaderSource
{
    public required string Vertex { get; init; }
    public required string Fragment { get; init; }
    public required string[] VertexInputs { get; init; }
}
