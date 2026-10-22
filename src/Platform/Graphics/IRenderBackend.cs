using System;

namespace THSSS.Graphics;

/// <summary>
/// Backend-neutral rendering contract. The compatibility layer (SlimDX look-alike)
/// talks only to this interface, so an OpenGL, Vulkan or future backend can be
/// selected at startup without touching game code.
///
/// The original engine is fixed-function Direct3D 9; this interface deliberately
/// models that state machine (blend factors as D3D9 values, sampler state as
/// integers) so a backend can map it to whatever its API needs.
/// </summary>
public interface IRenderBackend : IDisposable
{
    BackendKind Kind { get; }

    /// <summary>What this backend can do; callers choose paths from these, never from Kind.</summary>
    BackendFeatures Features { get; }

    /// <summary>DPI scale the window layer reports, used for logical-to-pixel sizing.</summary>
    void Resize(int framebufferWidth, int framebufferHeight);

    // ---- resources -------------------------------------------------------
    IRenderTexture CreateTexture(int width, int height, TextureFormat format, ReadOnlySpan<byte> pixels);
    IRenderTexture CreateRenderTexture(int width, int height, TextureFormat format);
    IRenderBuffer CreateBuffer(BufferKind kind, int sizeInBytes);
    IRenderShader CreateShader(ShaderSource source);
    IRenderFramebuffer CreateFramebuffer(IRenderTexture? color, DepthFormat depth, int width, int height);

    /// <summary>
    /// Standalone depth buffer the game attaches to a render target. Creation is the
    /// backend's job; GL used to do it inline with a renderbuffer.
    /// </summary>
    IRenderDepthSurface CreateDepthSurface(int width, int height, DepthFormat format);

    /// <summary>
    /// Creates the presentation swapchain for the surface handed to the backend, or does
    /// nothing when the backend presents through the window itself (GL).
    /// </summary>
    void AttachPresentSurface(int width, int height) { }

    /// <summary>Starts a command batch; immediate backends need no extra work.</summary>
    void BeginFrame() { }

    /// <summary>Completes pending work even when the game skips presentation.</summary>
    void EndFrame() { }

    /// <summary>Presents the frame just rendered. GL swaps via the window, Vulkan here.</summary>
    void Present() { }

    /// <summary>Names the texture holding the finished frame that Present should show.</summary>
    void SetPresentSource(IRenderTexture? texture) { }

    /// <summary>Attaches (or detaches with null) a depth surface to the frame currently bound.</summary>
    void AttachDepth(IRenderDepthSurface? depth);

    /// <summary>Current depth-test and face-culling state, so callers can save/restore it.</summary>
    (bool DepthTest, bool CullFace) QueryRasterState();

    /// <summary>Sets depth testing and face culling together.</summary>
    void SetRasterState(bool depthTest, bool cullFace);

    /// <summary>Depth/scissor/multisample capability switches D3D9 render states map onto.</summary>
    void SetCapability(BackendCapability capability, bool enabled);

    /// <summary>Applies the D3D9 blend state the game configured.</summary>
    void SetBlendState(BlendState state);

    // ---- frame scoped state ---------------------------------------------
    void BindFramebuffer(IRenderFramebuffer? framebuffer);

    /// <summary>
    /// Restores a split draw/read binding pair; the screenshot path saves and restores
    /// whatever the caller had bound rather than assuming a combined binding.
    /// </summary>
    void RestoreFramebufferBindings(IRenderFramebuffer? draw, IRenderFramebuffer? read);

    /// <summary>Selects the read colour buffer of the current framebuffer (0 = default).</summary>
    void SetReadBuffer(int colourAttachment);

    IRenderFramebuffer? CurrentDrawFramebuffer { get; }
    IRenderFramebuffer? CurrentReadFramebuffer { get; }
    void SetViewport(int x, int y, int width, int height, float minDepth, float maxDepth);
    void Clear(ClearMask mask, float red, float green, float blue, float alpha, float depth, int stencil);
    void SetScissor(bool enabled, int x, int y, int width, int height);

    // ---- fixed-function state (D3D9 semantics) --------------------------
    void SetDepthTest(bool enabled);
    void SetDepthWrite(bool enabled);
    void SetCullMode(CullMode mode);
    void SetBlend(BlendState blend);

    /// <summary>Sampler state for texture unit 0; D3D9 values, mapped by the backend.</summary>
    void SetSamplerState(SamplerStateBlock state);

    /// <summary>
    /// Uploads a std140 uniform block. Both backends read the same block layout, which is
    /// what lets one shader source serve GL and Vulkan.
    /// </summary>
    void SetUniformBlock(int binding, IRenderBuffer? buffer, ReadOnlySpan<byte> data);

    // ---- binding ---------------------------------------------------------
    /// <summary>Binds a texture to slot 0 for the next draw; null unbinds.</summary>
    /// <remarks>Callers that only need a read source should also bind it as the read
    /// framebuffer; see <see cref="RestoreFramebufferBindings"/>.</remarks>
    void BindTexture(IRenderTexture? texture);

    // ---- drawing ---------------------------------------------------------
    void Draw(IRenderShader shader, IRenderBuffer vertices, VertexLayout layout,
              PrimitiveKind kind, int startVertex, int vertexCount);
    byte[] ReadPixels(int x, int y, int width, int height);
    bool TryGetError(out string message);
}

public enum BackendKind { OpenGl, Vulkan }

/// <summary>Capabilities a caller may branch on instead of assuming a backend.</summary>
public readonly record struct BackendFeatures(
    bool SupportsSamplerObjects,
    bool SupportsRgb8Textures,
    bool OriginBottomLeft,
    int MaxColourAttachments,
    string ShaderLanguage);

public enum TextureFormat { Rgba8, Rgb8, Bgra8 }

public enum DepthFormat { None, Depth16, Depth24 }

public enum BufferKind { Vertex, Index, Uniform }

public enum PrimitiveKind { TriangleList, TriangleStrip, LineList }

public enum BackendCapability { DepthTest, ScissorTest, Multisample, CullFace }


public enum ClearMask { Color = 1, Depth = 2, Stencil = 4 }

public enum CullMode { None, Clockwise, CounterClockwise }

public readonly record struct BlendState(
    bool Enabled,
    BlendFactor Source,
    BlendFactor Destination,
    BlendEquation Equation);

/// <summary>D3D9 D3DBLEND* values, so the mapping table stays in one place.</summary>
public enum BlendFactor : int
{
    Zero = 1,
    One = 2,
    SourceColor = 3,
    InverseSourceColor = 4,
    SourceAlpha = 5,
    InverseSourceAlpha = 6,
    DestinationAlpha = 7,
    InverseDestinationAlpha = 8,
    DestinationColor = 9,
    InverseDestinationColor = 10,
    SourceAlphaSaturate = 11,
}

public enum BlendEquation { Add = 1, Subtract = 2, ReverseSubtract = 3, Min = 4, Max = 5 }

/// <summary>Packed per-slot sampler state; values follow D3DTEXF/D3DTADDRESS.</summary>
public readonly record struct SamplerStateBlock(
    int AddressU,
    int AddressV,
    int MinFilter,
    int MagFilter,
    int MipFilter,
    int MaxAnisotropy,
    float MipLodBias,
    float BorderRed,
    float BorderGreen,
    float BorderBlue,
    float BorderAlpha,
    bool BorderEnabled);

/// <summary>Vertex attribute description; offsets are in bytes within the stride.</summary>
public readonly record struct VertexLayout(
    int Stride,
    ReadOnlyMemory<VertexAttribute> Attributes);

public readonly record struct VertexAttribute(
    int Location,
    int ComponentCount,
    VertexAttributeType Type,
    bool Normalized,
    int ByteOffset);

public enum VertexAttributeType { Float, UnsignedByte }

public sealed class ShaderSource
{
    public required string Vertex { get; init; }
    public required string Fragment { get; init; }
    public required string[] VertexInputs { get; init; }
}
