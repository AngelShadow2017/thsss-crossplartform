using THSSS.Graphics;
using THSSS.Graphics.Vulkan;

// Vulkan backend fixture. It creates a real device and drives the shared IRenderBackend
// contract through it, then reads pixels back to prove what actually landed in the target.
// Failures are counted instead of thrown so one broken step does not hide the rest; the
// exit code is nonzero whenever anything failed.
int failures = 0;
void Check(bool condition, string message)
{
    Console.WriteLine((condition ? "PASS: " : "FAIL: ") + message);
    if (!condition) failures++;
}

// 1. Backend selection contract (no device needed).
Check(RenderBackendSelection.Requested is BackendKind.OpenGl or BackendKind.Vulkan,
    "default renderer resolves to a known backend");

// 2. Real Vulkan device bootstrap: instance, physical device, queue, command pool.
// A machine without a usable Vulkan device reports SKIP and exits on the accumulated
// failure count, so setup problems found earlier still fail the run.
VulkanDeviceContext? context = null;
try
{
    context = new VulkanDeviceContext();
    Check(context.QueueFamily < 32, $"vulkan device created with graphics queue family {context.QueueFamily}");
}
catch (PlatformNotSupportedException ex)
{
    Console.WriteLine("SKIP: no usable Vulkan device on this machine — " + ex.Message);
    return failures == 0 ? 0 : 1;
}

try
{
    using var backend = new VulkanRenderBackend();
    Check(backend.Kind == BackendKind.Vulkan, "backend reports Vulkan kind");
    // The compatibility layer above assumes GL conventions; the backend must declare them.
    Check(backend.Features.ShaderLanguage == "spirv", "backend advertises SPIR-V shaders");
    Check(backend.Features.OriginBottomLeft, "Vulkan exposes GL-compatible texture/readback rows to the compatibility layer");

    // Offscreen clear + readback through the backend interface (dynamic rendering).
    using var target = backend.CreateRenderTexture(8, 8, TextureFormat.Rgba8);
    using var framebuffer = backend.CreateFramebuffer(target, DepthFormat.None, 8, 8);
    Check(framebuffer.IsComplete, "vulkan framebuffer created");
    backend.BindFramebuffer(framebuffer);
    backend.SetViewport(0, 0, 8, 8, 0, 1);
    backend.Clear(ClearMask.Color, 0f, 0f, 1f, 1f, 1f, 0);
    var cleared = backend.ReadPixels(4, 4, 1, 1);
    Check(cleared[2] == 255 && cleared[0] == 0, "Clear produces blue before any draw");
    // Real pipeline: three vertices, a shader compiled from the project's own GLSL.
    // The uniform block follows std140, hence vec4 for a single colour.
    const string vertexGlsl = @"#version 330 core
layout(location=0) in vec3 aPosition;
layout(location=1) in vec2 aTexCoord;
layout (std140) uniform ProjectionBlock { vec4 uColour; };
out vec2 TexCoord;
void main() { gl_Position = vec4(aPosition, 1.0); TexCoord = aTexCoord; }";
    const string fragmentGlsl = @"#version 330 core
in vec2 TexCoord;
out vec4 FragColor;
layout (std140) uniform ProjectionBlock { vec4 uColour; };
void main() { FragColor = uColour; }";
    using var shader = backend.CreateShader(new ShaderSource
    {
        Vertex = vertexGlsl, Fragment = fragmentGlsl, VertexInputs = new[] { "aPosition", "aTexCoord" },
    });
    // Interleaved vertex: 5 floats = 20 bytes per vertex, position then UV.
    using var vertexBuffer = backend.CreateBuffer(BufferKind.Vertex, 60);
    var vertices = new float[]
    {
        -1, -1, 0, 0, 0,
         1, -1, 0, 0, 0,
         0,  1, 0, 0, 0,
    };
    var vertexBytes = new byte[vertices.Length * 4];
    Buffer.BlockCopy(vertices, 0, vertexBytes, 0, vertexBytes.Length);
    vertexBuffer.Update(vertexBytes, 0);
    // One std140 vec4 = 16 bytes: red, green, blue, alpha as floats.
    using var uniform = backend.CreateBuffer(BufferKind.Uniform, 16);
    var green = new byte[16];
    BitConverter.TryWriteBytes(green.AsSpan(0, 4), 1f);
    BitConverter.TryWriteBytes(green.AsSpan(4, 4), 0f);
    BitConverter.TryWriteBytes(green.AsSpan(8, 4), 0f);
    BitConverter.TryWriteBytes(green.AsSpan(12, 4), 1f);
    backend.SetUniformBlock(0, uniform, green);
    backend.BindTexture(null);
    backend.Draw(shader, vertexBuffer,
        new VertexLayout(20, new[]
        {
            new VertexAttribute(0, 3, VertexAttributeType.Float, false, 0),
            new VertexAttribute(1, 2, VertexAttributeType.Float, false, 12),
        }),
        PrimitiveKind.TriangleList, 0, 3);
    var drawn = backend.ReadPixels(4, 4, 1, 1);
    // The uniform colour must reach the shader: the centre pixel turns red over the blue clear.
    Check(drawn[0] > 240 && drawn[1] < 15 && drawn[2] < 15,
        $"Draw changes the blue centre to uniform red: {string.Join(',', drawn)}");

    // Readback must be side-effect free, otherwise polling a pixel changes the frame.
    var pixel = backend.ReadPixels(4, 4, 1, 1);
    Check(pixel.AsSpan().SequenceEqual(drawn), "ReadPixels does not clear or change the rendered image");
    // A corner outside the triangle keeps the clear colour, proving rasterization happened
    // rather than the whole target being overwritten.
    var outside = backend.ReadPixels(0, 7, 1, 1);
    Check(outside[2] == 255 && outside[0] == 0, "Outside the triangle remains blue, proving rasterization rather than clear output");

    // A second frame without a clear must keep the previous contents: the original engine
    // renders stacked layers into one target and only clears where it asks to.
    backend.Draw(shader, vertexBuffer,
        new VertexLayout(20, new[]
        {
            new VertexAttribute(0, 3, VertexAttributeType.Float, false, 0),
            new VertexAttribute(1, 2, VertexAttributeType.Float, false, 12),
        }),
        PrimitiveKind.TriangleList, 0, 3);
    var second = backend.ReadPixels(4, 4, 1, 1);
    Check(second[0] > 240, $"Second draw without clear keeps the frame populated: {string.Join(',', second)}");

    // The frame manager draws into the default frame, switches to an offscreen layer for
    // a pass, then returns to the default frame to composite. A round trip must not
    // discard what the default frame already holds.
    backend.BindFramebuffer(null);
    backend.Clear(ClearMask.Color, 0f, 0f, 1f, 1f, 1f, 0);
    using var layer = backend.CreateRenderTexture(8, 8, TextureFormat.Rgba8);
    using var layerFbo = backend.CreateFramebuffer(layer, DepthFormat.None, 8, 8);
    backend.BindFramebuffer(layerFbo);
    backend.Clear(ClearMask.Color, 0f, 1f, 0f, 1f, 1f, 0);
    backend.BindFramebuffer(null);
    var afterSwitch = backend.ReadPixels(4, 4, 1, 1);
    Check(afterSwitch[2] > 240 && afterSwitch[1] < 15,
        $"Returning from an offscreen target keeps the default frame contents: {string.Join(',', afterSwitch)}");

    // Detaching an external depth surface must restore the depth the target was built
    // with; otherwise every later draw on that target loses depth testing.
    using var external = backend.CreateDepthSurface(8, 8, DepthFormat.Depth24);
    backend.BindFramebuffer(layerFbo);
    backend.AttachDepth(external);
    backend.AttachDepth(null);
    backend.SetDepthTest(true);
    backend.SetDepthWrite(true);
    backend.Clear(ClearMask.Color | ClearMask.Depth, 0f, 1f, 0f, 1f, 1f, 0);
    backend.Draw(shader, vertexBuffer,
        new VertexLayout(20, new[]
        {
            new VertexAttribute(0, 3, VertexAttributeType.Float, false, 0),
            new VertexAttribute(1, 2, VertexAttributeType.Float, false, 12),
        }),
        PrimitiveKind.TriangleList, 0, 3);
    var afterDepthDetach = backend.ReadPixels(4, 4, 1, 1);
    Check(afterDepthDetach[0] > 240,
        $"Detached depth falls back to the target's own depth and still draws: {string.Join(',', afterDepthDetach)}");
    // Leave the state neutral for the sprite pass and for anything after the fixture.
    backend.SetDepthTest(false);
    backend.BindFramebuffer(null);
    SpriteChecks.Run(backend);
}
finally
{
    // The device context is separate from the backend and owns the Vulkan instance.
    context.Dispose();
}

Console.WriteLine(failures == 0
    ? "PASS VulkanBackend: device bootstrap and backend contract verified"
    : $"FAIL VulkanBackend: {failures} failure(s)");
return failures == 0 ? 0 : 1;
