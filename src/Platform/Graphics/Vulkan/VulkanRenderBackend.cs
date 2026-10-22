using System;
using Silk.NET.Vulkan;

namespace THSSS.Graphics.Vulkan;

public sealed unsafe class VulkanRenderBackend : IRenderBackend
{
    private readonly VulkanDeviceContext _context;
    private VulkanSwapchain? _swapchain;
    private bool _presentationAttached, _swapchainResizePending;
    private VulkanFramebuffer? _draw, _read, _backbuffer;
    private VulkanTexture? _backbufferColour, _fallback, _texture;
    private VulkanBuffer? _uniformBuffer;
    private bool _depthTest, _depthWrite = true, _scissorEnabled, _disposed;
    private CullMode _cull;
    private BlendState _blend = new(false, BlendFactor.One, BlendFactor.Zero, BlendEquation.Add);
    private int _width = 640, _height = 480, _presentCount, _drawCount, _clearCount;
    private string _lastHasComposite = "?";
    private string? _lastFrameSignature;
    private int _signatureChanges;
    private (int X, int Y, int W, int H, float Min, float Max) _viewport = (0, 0, 640, 480, 0, 1);
    private (int X, int Y, int W, int H) _scissor;
    private readonly System.Collections.Generic.Dictionary<SamplerStateBlock, Sampler> _samplers = new();
    private SamplerStateBlock? _sampling;

    public VulkanRenderBackend() : this(null) { }
    public VulkanRenderBackend(VulkanSurfaceSource? surface) => _context = new VulkanDeviceContext(surface);
    internal VulkanDeviceContext Context => _context;
    public BackendKind Kind => BackendKind.Vulkan;
    // The compatibility layer uses GL-style texture rows/UVs on both APIs. Presentation
    // performs the single conversion to the window system's top-left image coordinates.
    public BackendFeatures Features => new(false, false, true, 1, "spirv");
    public int PresentedFrames => _presentCount;
    public int SwapchainGeneration { get; private set; }
    public int SwapchainWidth => (int)(_swapchain?.Extent.Width ?? 0);
    public int SwapchainHeight => (int)(_swapchain?.Extent.Height ?? 0);
    public byte[]? LastPresentedPixels { get; private set; }
    private long _frameSubmissionStart, _frameStarted;
    private int _frameDraws, _lastFrameDraws;
    private long _lastFrameSubmissions;
    public long TotalSubmissions => _context.SubmissionCount;
    public long FrameSubmissions => _context.IsBatching ? TotalSubmissions - _frameSubmissionStart : _lastFrameSubmissions;
    public int FrameDraws => _context.IsBatching ? _frameDraws : _lastFrameDraws;
    public double LastFrameMilliseconds { get; private set; }

    public void BeginFrame()
    {
        if (_context.IsBatching)
            throw new InvalidOperationException("A Vulkan frame is already open.");
        if (_presentationAttached && (_swapchainResizePending || _swapchain == null ||
            _swapchain.ShouldRecreate((uint)_width, (uint)_height)))
            ResizeSwapchain();
        _context.BeginFrame();
        _frameSubmissionStart = TotalSubmissions;
        _frameDraws = 0;
        _frameStarted = System.Diagnostics.Stopwatch.GetTimestamp();
    }

    public void EndFrame()
    {
        if (!_context.IsBatching) return;
        _context.EndFrame();
        _lastFrameSubmissions = TotalSubmissions - _frameSubmissionStart;
        _lastFrameDraws = _frameDraws;
        LastFrameMilliseconds = System.Diagnostics.Stopwatch.GetElapsedTime(_frameStarted).TotalMilliseconds;
    }

    private VulkanFramebuffer DefaultFramebuffer
    {
        get
        {
            if (_backbuffer == null)
            {
                _backbufferColour = new VulkanTexture(_context, _width, _height, TextureFormat.Rgba8, true, ReadOnlySpan<byte>.Empty);
                _backbuffer = new VulkanFramebuffer(_context, _backbufferColour, DepthFormat.Depth24, _width, _height);
            }
            return _backbuffer;
        }
    }
    private VulkanFramebuffer DrawTarget => _draw ?? DefaultFramebuffer;
    private VulkanFramebuffer ReadTarget => _read ?? DefaultFramebuffer;

    public void AttachPresentSurface(int width, int height)
    {
        if (_context.SurfaceSource == null)
            throw new InvalidOperationException("Vulkan window presentation requires a surface; use the parameterless backend only for offscreen tests.");
        Resize(width, height);
        _presentationAttached = true;
        if (_swapchain == null) ResizeSwapchain();
        if (_swapchain != null && Environment.GetEnvironmentVariable("THSSS_DIAG_VK") == "1")
            Console.WriteLine($"[VK] swapchain created images={_swapchain.ImageCount} extent={_swapchain.Extent.Width}x{_swapchain.Extent.Height}");
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0 || (_width == width && _height == height)) return;
        if (_context.IsBatching)
            throw new InvalidOperationException("Resize the Vulkan framebuffer before BeginFrame.");
        _context.FlushPending();
        _context.CheckResult(_context.Vk.DeviceWaitIdle(_context.Device), "wait before resizing framebuffer");
        _width = width; _height = height;
        if (ReferenceEquals(_draw, _backbuffer)) _draw = null;
        if (ReferenceEquals(_read, _backbuffer)) _read = null;
        if (ReferenceEquals(_texture, _backbufferColour)) _texture = null;
        if (ReferenceEquals(_presentSource, _backbufferColour)) _presentSource = null;
        _backbuffer?.Dispose(); _backbuffer = null;
        _backbufferColour?.Dispose(); _backbufferColour = null;
        LastPresentedPixels = null;
        if (_presentationAttached) ResizeSwapchain();
    }

    private VulkanTexture? _presentSource;

    /// <summary>
    /// Frame manager output. Menus and the HUD composite into an offscreen target, so
    /// presenting the back buffer there would show an empty image.
    /// </summary>
    internal void NotePresentState() => _lastHasComposite = _presentSource == null ? "no" : "yes";

    public void SetPresentSource(IRenderTexture? texture)
    {
        _presentSource = texture as VulkanTexture;
    }

    public void Present()
    {
        try { PresentCore(); }
        finally { EndFrame(); }
    }

    private void PresentCore()
    {
        if (_swapchain == null || _swapchainResizePending) return; // Offscreen or minimized surface.
        if (Environment.GetEnvironmentVariable("THSSS_DIAG_VK_STATE") == "1")
        {
            var target = (_draw ?? DefaultFramebuffer).ColorTexture!;
            var signature = $"target={target.Width}x{target.Height}/{target.Layout} viewport={_viewport.W}x{_viewport.H} blend={_blend.Enabled}/{_blend.Source}/{_blend.Destination} depth={_depthTest}/{_depthWrite}/{(_draw ?? DefaultFramebuffer).DepthSurface?.Format} cull={_cull}";
            if (signature != _lastFrameSignature)
            {
                _signatureChanges++;
                var px = ReadPixels(_width / 2, _height / 2, 1, 1);
                Console.WriteLine($"[VK-STATE] frame={_presentCount} change#{_signatureChanges} centre={px[0]},{px[1]},{px[2]} | {signature}");
                _lastFrameSignature = signature;
            }
        }
        if (!_swapchain.TryAcquire(out int index)) return;
        var source = _presentSource ?? DefaultFramebuffer.ColorTexture!;
        if (Environment.GetEnvironmentVariable("THSSS_DIAG_PRESENT") == "1" && _presentCount % 60 == 0)
        {
            int w = source.Width, h = source.Height;
            var centre = ReadPixels(w / 2 - 4, h / 2 - 4, 8, 8);
            int lit = 0;
            for (int i = 0; i < centre.Length; i += 4) if ((centre[i] | centre[i + 1] | centre[i + 2]) > 8) lit++;
            Console.WriteLine($"[PRESENT] frame={_presentCount} {w}x{h} centreLit={lit}/64 srcHash={System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(source)} lastDrawHash={System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(DrawTarget.ColorTexture!)} hasComposite={_lastHasComposite}");
        }
        _context.RunPresent(cmd =>
        {
            source.Transition(cmd, ImageLayout.TransferSrcOptimal);
            _swapchain.TransitionToTransferDst(cmd, index);
            var black = new ClearColorValue(0f, 0f, 0f, 1f);
            var range = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1);
            _context.Vk.CmdClearColorImage(cmd, _swapchain.Image(index), ImageLayout.TransferDstOptimal,
                &black, 1, &range);
            var clearBarrier = new MemoryBarrier
            {
                SType = StructureType.MemoryBarrier,
                SrcAccessMask = AccessFlags.TransferWriteBit,
                DstAccessMask = AccessFlags.TransferWriteBit,
            };
            _context.Vk.CmdPipelineBarrier(cmd, PipelineStageFlags.TransferBit, PipelineStageFlags.TransferBit,
                0, 1, &clearBarrier, 0, null, 0, null);
            var destination = _presentSource == null || ReferenceEquals(source, _backbufferColour)
                ? new PixelViewport(0, 0, SwapchainWidth, SwapchainHeight)
                : PresentationGeometry.Fit(SwapchainWidth, SwapchainHeight);
            var region = new ImageBlit
            {
                SrcSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
                DstSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            };
            region.SrcOffsets[0] = new Offset3D(0, source.Height, 0);
            region.SrcOffsets[1] = new Offset3D(source.Width, 0, 1);
            region.DstOffsets[0] = new Offset3D(destination.X, destination.Y, 0);
            region.DstOffsets[1] = new Offset3D(destination.X + destination.Width, destination.Y + destination.Height, 1);
            _context.Vk.CmdBlitImage(cmd, source.Image, ImageLayout.TransferSrcOptimal,
                _swapchain.Image(index), ImageLayout.TransferDstOptimal, 1, &region, Filter.Linear);
            _swapchain.TransitionToPresent(cmd, index);
        }, _swapchain.ImageAvailable, _swapchain.RenderFinished);
        if (Environment.GetEnvironmentVariable("THSSS_DIAG_VK_PIXELS") == "1" && (Environment.GetEnvironmentVariable("THSSS_DIAG_VK_ALL") == "1" || _presentCount % 40 == 0))
        {
            LastPresentedPixels = _swapchain.ReadImage(index, _context);
            {
                int lit = 0;
                for (int i = 0; i < LastPresentedPixels.Length; i += 4)
                    if ((LastPresentedPixels[i] | LastPresentedPixels[i + 1] | LastPresentedPixels[i + 2]) > 8) lit++;
                Console.WriteLine($"[VK-PIXELS] frame={_presentCount} nonBlack={lit} source={(_presentSource is null ? "backbuffer" : $"{_presentSource.Width}x{_presentSource.Height}")}");
            }
        }
        var result = _swapchain.Present(index);
        if (Environment.GetEnvironmentVariable("THSSS_DIAG_VK_STATE") == "1" && _presentCount % 40 == 0)
            Console.WriteLine($"[VK-PRESENT] frame={_presentCount} result={result}");
        if (result is not (Result.Success or Result.ErrorOutOfDateKhr or Result.SuboptimalKhr))
            throw new InvalidOperationException("Vulkan present failed: " + result);
        _presentCount++;
    }
    private void ResizeSwapchain()
    {
        _context.FlushPending();
        _context.CheckResult(_context.Vk.DeviceWaitIdle(_context.Device), "wait before recreating swapchain");
        if (_context.SurfaceLost)
            throw new PlatformNotSupportedException(
                "The presentation surface was lost by the window system. Run with THSSS_RENDERER=gl.");
        var replacement = VulkanSwapchain.TryCreate(_context, (uint)_width, (uint)_height, _swapchain);
        _swapchainResizePending = replacement == null;
        if (replacement == null) return;
        _swapchain?.Dispose();
        _swapchain = replacement;
        SwapchainGeneration++;
        LastPresentedPixels = null;
    }

    public IRenderTexture CreateTexture(int width, int height, TextureFormat format, ReadOnlySpan<byte> pixels)
        => new VulkanTexture(_context, width, height, format, false, pixels);
    public IRenderTexture CreateRenderTexture(int width, int height, TextureFormat format)
        => new VulkanTexture(_context, width, height, format, true, ReadOnlySpan<byte>.Empty);
    public IRenderBuffer CreateBuffer(BufferKind kind, int size) => new VulkanBuffer(_context, kind, size);
    public IRenderShader CreateShader(ShaderSource source) => new VulkanShader(_context, source);
    public IRenderDepthSurface CreateDepthSurface(int width, int height, DepthFormat format)
        => new VulkanDepthSurface(_context, width, height, format);
    public IRenderFramebuffer CreateFramebuffer(IRenderTexture? color, DepthFormat depth, int width, int height)
        => new VulkanFramebuffer(_context, color, depth, width, height);
    public void BindFramebuffer(IRenderFramebuffer? framebuffer)
    {
        _draw = (VulkanFramebuffer?)framebuffer;
        _read = _draw;
    }

    public void RestoreFramebufferBindings(IRenderFramebuffer? draw, IRenderFramebuffer? read)
    { _draw = (VulkanFramebuffer?)draw; _read = (VulkanFramebuffer?)read; }
    public IRenderFramebuffer? CurrentDrawFramebuffer => _draw;
    public IRenderFramebuffer? CurrentReadFramebuffer => _read;
    public void SetReadBuffer(int colourAttachment)
    { if (colourAttachment != 0) throw new NotSupportedException("Only colour attachment zero is supported."); }
    public void AttachDepth(IRenderDepthSurface? depth) => DrawTarget.SetDepthSurface((VulkanDepthSurface?)depth);
    public void SetViewport(int x, int y, int width, int height, float minDepth, float maxDepth)
        => _viewport = (x, y, width, height, minDepth, maxDepth);
    public void SetScissor(bool enabled, int x, int y, int width, int height)
    { _scissorEnabled = enabled; _scissor = (x, y, width, height); }
    public void SetCapability(BackendCapability capability, bool enabled)
    {
        switch (capability)
        {
            case BackendCapability.DepthTest: _depthTest = enabled; break;
            case BackendCapability.CullFace: _cull = enabled ? CullMode.CounterClockwise : CullMode.None; break;
            case BackendCapability.ScissorTest: _scissorEnabled = enabled; break;
            case BackendCapability.Multisample: break; // All current attachments are single-sample.
        }
    }
    public (bool DepthTest, bool CullFace) QueryRasterState() => (_depthTest, _cull != CullMode.None);
    public void SetRasterState(bool depthTest, bool cullFace)
    { _depthTest = depthTest; _cull = cullFace ? CullMode.CounterClockwise : CullMode.None; }
    public void SetBlendState(BlendState state) => _blend = state;
    public void SetBlend(BlendState state) => _blend = state;
    public void SetDepthTest(bool enabled) => _depthTest = enabled;
    public void SetDepthWrite(bool enabled) => _depthWrite = enabled;
    public void SetCullMode(CullMode mode) => _cull = mode;
    public void SetSamplerState(SamplerStateBlock state) => _sampling = state;
    public void BindTexture(IRenderTexture? texture) { _texture = (VulkanTexture?)texture; _sampling = null; }
    public void SetUniformBlock(int binding, IRenderBuffer? buffer, ReadOnlySpan<byte> data)
    {
        if (binding != 0) throw new NotSupportedException("Current shaders use uniform block binding zero.");
        _uniformBuffer = (VulkanBuffer?)buffer;
        if (!data.IsEmpty) _uniformBuffer!.Update(data, 0);
    }

    public void Clear(ClearMask mask, float red, float green, float blue, float alpha, float depth, int stencil)
    {
        var frame = DrawTarget;
        _clearCount++;
        _context.Record(cmd =>
        {
            var color = frame.ColorTexture!;
            color.Transition(cmd, ImageLayout.ColorAttachmentOptimal);
            var attachment = new RenderingAttachmentInfo
            {
                SType = StructureType.RenderingAttachmentInfo, ImageView = frame.View,
                ImageLayout = ImageLayout.ColorAttachmentOptimal,
                LoadOp = (mask & ClearMask.Color) != 0 ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store, ClearValue = new ClearValue { Color = new ClearColorValue(red, green, blue, alpha) },
            };
            var depthSurface = frame.DepthSurface;
            var depthAttachment = DepthAttachment(cmd, depthSurface, (mask & ClearMask.Depth) != 0, depth);
            var info = new RenderingInfo
            {
                SType = StructureType.RenderingInfo,
                RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)frame.Width, (uint)frame.Height)),
                LayerCount = 1, ColorAttachmentCount = 1, PColorAttachments = &attachment,
                PDepthAttachment = depthSurface == null ? null : &depthAttachment,
            };
            _context.Vk.CmdBeginRendering(cmd, &info);
            _context.Vk.CmdEndRendering(cmd);
        });
    }

    private RenderingAttachmentInfo DepthAttachment(CommandBuffer cmd, VulkanDepthSurface? surface, bool clear, float value)
    {
        if (surface == null) return default;
        surface.Transition(cmd, ImageLayout.DepthStencilAttachmentOptimal);
        return new RenderingAttachmentInfo
        {
            SType = StructureType.RenderingAttachmentInfo, ImageView = surface.View,
            ImageLayout = ImageLayout.DepthStencilAttachmentOptimal,
            LoadOp = clear ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
            StoreOp = AttachmentStoreOp.Store,
            ClearValue = new ClearValue { DepthStencil = new ClearDepthStencilValue(value, 0) },
        };
    }

    private Sampler GetSampler(SamplerStateBlock state)
    {
        if (_samplers.TryGetValue(state, out var value)) return value;
        SamplerAddressMode Address(int v) => v switch
        { 2 => SamplerAddressMode.MirroredRepeat, 3 => SamplerAddressMode.ClampToEdge, 4 => SamplerAddressMode.ClampToBorder, _ => SamplerAddressMode.Repeat };
        var info = new SamplerCreateInfo
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = state.MagFilter == 1 ? Filter.Nearest : Filter.Linear,
            MinFilter = state.MinFilter == 1 ? Filter.Nearest : Filter.Linear,
            AddressModeU = Address(state.AddressU), AddressModeV = Address(state.AddressV), AddressModeW = SamplerAddressMode.ClampToEdge,
            MinLod = 0, MaxLod = 0, BorderColor = BorderColor.FloatTransparentBlack,
        };
        var result = _context.Vk.CreateSampler(_context.Device, &info, null, out value);
        if (result != Result.Success) throw new InvalidOperationException("Vulkan sampler creation failed: " + result);
        _samplers.Add(state, value); return value;
    }

    public void Draw(IRenderShader shader, IRenderBuffer vertices, VertexLayout layout, PrimitiveKind kind, int startVertex, int vertexCount)
    {
        if (vertexCount <= 0) return;
        _drawCount++;
        if (_context.IsBatching) _frameDraws++;
        var program = (VulkanShader)shader;
        var vertexBuffer = (VulkanBuffer)vertices;
        var frame = DrawTarget;
        var depth = frame.DepthSurface;
        var sampled = _texture ?? (_fallback ??= new VulkanTexture(_context, 1, 1, TextureFormat.Rgba8, false, new byte[] {255,255,255,255}));
        if (ReferenceEquals(frame.ColorTexture, sampled)) throw new InvalidOperationException("Cannot sample the active colour attachment.");
        if (_uniformBuffer == null) throw new InvalidOperationException("A uniform block must be bound before drawing.");
        if (!_context.IsBatching) _context.ResetDescriptors();
        if (startVertex < 0) throw new ArgumentOutOfRangeException(nameof(startVertex));
        int vertexSize = checked((startVertex + vertexCount) * layout.Stride);
        var vertexSnapshot = _context.IsBatching
            ? _context.Snapshot(vertexBuffer, 0, vertexSize)
            : (Buffer: vertexBuffer, Offset: 0UL);
        var uniformSnapshot = _context.IsBatching
            ? _context.Snapshot(_uniformBuffer, 0, _uniformBuffer.SizeInBytes)
            : (Buffer: _uniformBuffer, Offset: 0UL);
        uint firstVertex = (uint)startVertex;
        var set = _context.AllocateSet();
        var bufferInfo = new DescriptorBufferInfo { Buffer = uniformSnapshot.Buffer.Buffer, Offset = uniformSnapshot.Offset, Range = (ulong)_uniformBuffer.SizeInBytes };
        var imageInfo = new DescriptorImageInfo { ImageLayout = ImageLayout.ShaderReadOnlyOptimal, ImageView = sampled.View, Sampler = GetSampler(_sampling ?? sampled.SamplerState) };
        var writes = stackalloc WriteDescriptorSet[2];
        writes[0] = new WriteDescriptorSet { SType = StructureType.WriteDescriptorSet, DstSet = set, DstBinding = 0, DescriptorCount = 1, DescriptorType = DescriptorType.UniformBuffer, PBufferInfo = &bufferInfo };
        writes[1] = new WriteDescriptorSet { SType = StructureType.WriteDescriptorSet, DstSet = set, DstBinding = 1, DescriptorCount = 1, DescriptorType = DescriptorType.CombinedImageSampler, PImageInfo = &imageInfo };
        _context.Vk.UpdateDescriptorSets(_context.Device, 2, writes, 0, null);
        var pipeline = program.GetPipeline(layout, kind, _blend, _depthTest, _depthWrite, _cull,
            Format.R8G8B8A8Unorm, depth?.Format ?? Format.Undefined, out var pipelineLayout);
        _context.Record(cmd =>
        {
            sampled.Transition(cmd, ImageLayout.ShaderReadOnlyOptimal);
            frame.ColorTexture!.Transition(cmd, ImageLayout.ColorAttachmentOptimal);
            var colorAttachment = new RenderingAttachmentInfo
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = frame.View,
                ImageLayout = ImageLayout.ColorAttachmentOptimal,
                // Preserve whatever earlier layers of this frame already drew.
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
            };
            var depthAttachment = DepthAttachment(cmd, depth, false, 1);
            var info = new RenderingInfo
            {
                SType = StructureType.RenderingInfo, RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)frame.Width, (uint)frame.Height)),
                LayerCount = 1, ColorAttachmentCount = 1, PColorAttachments = &colorAttachment, PDepthAttachment = depth == null ? null : &depthAttachment,
            };
            var viewport = new Viewport(_viewport.X, _viewport.Y, _viewport.W, _viewport.H, _viewport.Min, _viewport.Max);
            var scissor = _scissorEnabled
                ? new Rect2D(new Offset2D(_scissor.X, _scissor.Y), new Extent2D((uint)_scissor.W, (uint)_scissor.H))
                : new Rect2D(new Offset2D(0, 0), new Extent2D((uint)frame.Width, (uint)frame.Height));
            _context.Vk.CmdBeginRendering(cmd, &info);
            _context.Vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, pipeline);
            var descriptor = set;
            _context.Vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, &descriptor, 0, null);
            _context.Vk.CmdSetViewport(cmd, 0, 1, &viewport);
            _context.Vk.CmdSetScissor(cmd, 0, 1, &scissor);
            var buffer = vertexSnapshot.Buffer.Buffer; ulong offset = vertexSnapshot.Offset;
            _context.Vk.CmdBindVertexBuffers(cmd, 0, 1, &buffer, &offset);
            _context.Vk.CmdDraw(cmd, (uint)vertexCount, 1, firstVertex, 0);
            _context.Vk.CmdEndRendering(cmd);
        });
    }

    public byte[] ReadPixels(int x, int y, int width, int height)
    {
        var target = ReadTarget.ColorTexture!;
        if (x < 0 || y < 0 || width <= 0 || height <= 0 || x + width > target.Width || y + height > target.Height)
            throw new ArgumentOutOfRangeException(nameof(width), "Readback region is outside the framebuffer.");
        using var staging = new VulkanBuffer(_context, BufferKind.Vertex, checked(width * height * 4));
        _context.RunOnce(cmd =>
        {
            target.Transition(cmd, ImageLayout.TransferSrcOptimal);
            var region = new BufferImageCopy
            {
                ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
                ImageOffset = new Offset3D(x, y, 0), ImageExtent = new Extent3D((uint)width, (uint)height, 1),
            };
            _context.Vk.CmdCopyImageToBuffer(cmd, target.Image, ImageLayout.TransferSrcOptimal, staging.Buffer, 1, &region);
            // Leaving the image in TransferSrcOptimal would make the next colour attachment
            // transition discard what was just rendered.
            target.Transition(cmd, ImageLayout.ColorAttachmentOptimal);
        });
        return staging.ReadAll();
    }
    public bool TryGetError(out string message) { message = ""; return false; }
    public void Dispose()
    {
        if (_disposed) return; _disposed = true;
        _context.FlushPending();
        _context.Vk.DeviceWaitIdle(_context.Device);
        _swapchain?.Dispose(); _backbuffer?.Dispose(); _backbufferColour?.Dispose(); _fallback?.Dispose();
        foreach (var sampler in _samplers.Values) _context.Vk.DestroySampler(_context.Device, sampler, null);
        _context.Dispose();
    }
}
