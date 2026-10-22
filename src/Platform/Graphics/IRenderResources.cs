using System;

namespace THSSS.Graphics;

/// <summary>
/// An opaque GPU handle. The compatibility layer passes these around instead of GL
/// names, which is what lets a Vulkan backend exist at all.
/// </summary>
public interface IRenderHandle { }

public interface IRenderTexture : IDisposable
{
    IRenderHandle Handle { get; }
    int Width { get; }
    int Height { get; }
    TextureFormat Format { get; }

    /// <summary>Uploads a whole image; used on creation and for text cache updates.</summary>
    void Upload(ReadOnlySpan<byte> pixels, int rowPitch);

    /// <summary>Sampler state lives with the texture for backends without sampler objects.</summary>
    void SetSamplerState(SamplerStateBlock state);
}

public interface IRenderBuffer : IDisposable
{
    IRenderHandle Handle { get; }
    int SizeInBytes { get; }
    void Update(ReadOnlySpan<byte> data, int offsetInBytes);
}

public interface IRenderShader : IDisposable
{
    IRenderHandle Handle { get; }
    int GetUniformLocation(string name);
    void Use();
    void SetUniform(int location, float value);
    void SetUniform(int location, float x, float y);
    void SetUniform(int location, float x, float y, float z);
    void SetUniform(int location, float x, float y, float z, float w);
    void SetUniform(int location, ReadOnlySpan<float> matrix4);
    void SetUniform(int location, int value);
}

public interface IRenderFramebuffer : IDisposable
{
    IRenderHandle Handle { get; }
    int Width { get; }
    int Height { get; }
    bool IsComplete { get; }
    IRenderTexture? Color { get; }
}

/// <summary>Depth-only surface created by a backend and attached to a render target.</summary>
public interface IRenderDepthSurface : IDisposable
{
    IRenderHandle Handle { get; }
    int Width { get; }
    int Height { get; }

    /// <summary>Backend-native attachment id (a GL renderbuffer, or 0 when not applicable).</summary>
    uint NativeAttachmentId { get; }
}
