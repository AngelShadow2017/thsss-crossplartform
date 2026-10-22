using System;
using THSSS.Graphics;
using Silk.NET.Vulkan;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkImage = Silk.NET.Vulkan.Image;

namespace THSSS.Graphics.Vulkan;

/// <summary>Vulkan texture with host-visible staging upload and optional render usage.</summary>
internal sealed unsafe class VulkanTexture : IRenderTexture
{
    private readonly VulkanDeviceContext _ctx;
    private readonly bool _renderTarget;
    private bool _disposed;
    internal Silk.NET.Vulkan.Format NativeFormat => Silk.NET.Vulkan.Format.R8G8B8A8Unorm;
    internal SamplerStateBlock SamplerState { get; private set; } = new(1, 1, 2, 2, 0, 1, 0, 0, 0, 0, 0, false);
    private Vk Vk => _ctx.Vk;

    internal VkImage Image { get; }
    internal ImageView View { get; private set; }
    internal DeviceMemory Memory { get; }
    internal ImageLayout Layout { get; set; } = ImageLayout.Undefined;

    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public TextureFormat Format { get; }

    internal VulkanTexture(VulkanDeviceContext ctx, int width, int height, TextureFormat format,
        bool renderTarget, ReadOnlySpan<byte> pixels)
    {
        _ctx = ctx;
        _renderTarget = renderTarget;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Format = format;
        var vkFormat = NativeFormat;

        var usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit;
        if (renderTarget) usage |= ImageUsageFlags.ColorAttachmentBit;

        var info = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = vkFormat,
            Extent = new Extent3D((uint)Width, (uint)Height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
        };
        if (Vk.CreateImage(_ctx.Device, &info, null, out var image) != Result.Success)
            throw new InvalidOperationException("Vulkan image creation failed.");
        Image = image;

        try
        {
            Vk.GetImageMemoryRequirements(_ctx.Device, Image, out var reqs);
            Memory = _ctx.AllocateMemory(reqs.Size, reqs.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            var bindResult = Vk.BindImageMemory(_ctx.Device, Image, Memory, 0);
            if (bindResult != Result.Success) throw new InvalidOperationException($"Vulkan texture memory bind failed: {bindResult}.");

            var viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = Image,
                ViewType = ImageViewType.Type2D,
                Format = Silk.NET.Vulkan.Format.R8G8B8A8Unorm,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
            };
            if (Vk.CreateImageView(_ctx.Device, &viewInfo, null, out var view) != Result.Success)
                throw new InvalidOperationException("Vulkan image view creation failed.");
            View = view;
            Handle = new VulkanHandle(image.Handle, "image");
            if (!pixels.IsEmpty) UploadImmediate(pixels, Width * (Format == TextureFormat.Rgb8 ? 3 : 4));
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Upload(ReadOnlySpan<byte> pixels, int rowPitch)
    {
        if (pixels.IsEmpty) return;
        UploadImmediate(pixels, rowPitch);
    }

    /// <summary>
    /// Stage through a host-visible buffer, copy, then hand the image to whichever layout
    /// the next use needs. Kept simple: one-shot submit and wait.
    /// </summary>
    private void UploadImmediate(ReadOnlySpan<byte> pixels, int rowPitch)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        int sourcePixelSize = Format == TextureFormat.Rgb8 ? 3 : 4;
        int sourceRowSize = checked(Width * sourcePixelSize);
        if (rowPitch < sourceRowSize || pixels.Length < checked((Height - 1) * rowPitch + sourceRowSize))
            throw new ArgumentException("Texture pixels and row pitch do not cover the image.", nameof(pixels));
        ulong size = (ulong)(Width * 4) * (ulong)Height;
        var staging = _ctx.CreateBuffer(size, BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        try
        {
            void* mapped;
            var mapResult = Vk.MapMemory(_ctx.Device, staging.Memory, 0, size, 0, &mapped);
            if (mapResult != Result.Success)
                throw new InvalidOperationException($"Vulkan texture staging map failed: {mapResult}.");
            try
            {
                for (int y = 0; y < Height; y++)
                {
                    var src = pixels.Slice(y * rowPitch, sourceRowSize);
                    var dst = new Span<byte>((byte*)mapped + y * Width * 4, Width * 4);
                    if (Format == TextureFormat.Rgba8) { src.CopyTo(dst); continue; }
                    for (int x = 0; x < Width; x++)
                    {
                        int s = x * sourcePixelSize, d = x * 4;
                        dst[d] = src[s + (Format == TextureFormat.Bgra8 ? 2 : 0)];
                        dst[d + 1] = src[s + 1];
                        dst[d + 2] = src[s + (Format == TextureFormat.Bgra8 ? 0 : 2)];
                        dst[d + 3] = sourcePixelSize == 4 ? src[s + 3] : (byte)255;
                    }
                }
            }
            finally { Vk.UnmapMemory(_ctx.Device, staging.Memory); }

            _ctx.RunOnce(cmd =>
            {
                Transition(cmd, ImageLayout.TransferDstOptimal);
                var region = new BufferImageCopy
                {
                    BufferOffset = 0,
                    BufferRowLength = 0,
                    BufferImageHeight = 0,
                    ImageSubresource = new ImageSubresourceLayers
                    { AspectMask = ImageAspectFlags.ColorBit, MipLevel = 0, BaseArrayLayer = 0, LayerCount = 1 },
                    ImageOffset = new Offset3D(0, 0, 0),
                    ImageExtent = new Extent3D((uint)Width, (uint)Height, 1),
                };
                Vk.CmdCopyBufferToImage(cmd, staging.Buffer, Image, ImageLayout.TransferDstOptimal, 1, &region);
                Transition(cmd, _renderTarget ? ImageLayout.ColorAttachmentOptimal : ImageLayout.ShaderReadOnlyOptimal);
            });
        }
        finally
        {
            Vk.DestroyBuffer(_ctx.Device, staging.Buffer, null);
            Vk.FreeMemory(_ctx.Device, staging.Memory, null);
        }
    }


    internal void Transition(CommandBuffer cmd, ImageLayout target)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Layout == target && target == ImageLayout.ShaderReadOnlyOptimal) return;
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = Layout,
            NewLayout = target,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = Image,
            SubresourceRange = new ImageSubresourceRange
            { AspectMask = ImageAspectFlags.ColorBit, BaseMipLevel = 0, LevelCount = 1, BaseArrayLayer = 0, LayerCount = 1 },
            SrcAccessMask = Layout == ImageLayout.Undefined ? 0 : AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit,
            DstAccessMask = AccessFlags.MemoryReadBit | AccessFlags.MemoryWriteBit,
        };
        Vk.CmdPipelineBarrier(cmd, Layout == ImageLayout.Undefined ? PipelineStageFlags.TopOfPipeBit : PipelineStageFlags.AllCommandsBit, PipelineStageFlags.AllCommandsBit,
            0, 0, null, 0, null, 1, &barrier);
        // Undefined is a write-only starting point: the first transition from it discards
        // the image contents, so remember the real layout once any barrier has run.
        Layout = target;
    }

    public void SetSamplerState(SamplerStateBlock state) => SamplerState = state;

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.FlushPending();
        _disposed = true;
        if (View.Handle != 0) Vk.DestroyImageView(_ctx.Device, View, null);
        Vk.DestroyImage(_ctx.Device, Image, null);
        if (Memory.Handle != 0) Vk.FreeMemory(_ctx.Device, Memory, null);
    }
}

internal sealed unsafe class VulkanBuffer : IRenderBuffer
{
    private readonly VulkanDeviceContext _ctx;
    private bool _disposed;
    private void* _mapped;
    internal VkBuffer Buffer { get; }
    internal DeviceMemory Memory { get; }

    public IRenderHandle Handle { get; }
    public int SizeInBytes { get; }

    internal VulkanBuffer(VulkanDeviceContext ctx, BufferKind kind, int sizeInBytes)
    {
        _ctx = ctx;
        SizeInBytes = Math.Max(1, sizeInBytes);
        var usage = kind switch
        {
            BufferKind.Index => BufferUsageFlags.IndexBufferBit,
            BufferKind.Uniform => BufferUsageFlags.UniformBufferBit,
            _ => BufferUsageFlags.VertexBufferBit,
        };
        usage |= BufferUsageFlags.TransferDstBit | BufferUsageFlags.TransferSrcBit |
            BufferUsageFlags.VertexBufferBit | BufferUsageFlags.UniformBufferBit;
        var created = _ctx.CreateBuffer((ulong)SizeInBytes, usage, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        Buffer = created.Buffer;
        Memory = created.Memory;
        Handle = new VulkanHandle(Buffer.Handle, "buffer");
        void* mapped;
        var result = _ctx.Vk.MapMemory(_ctx.Device, Memory, 0, (ulong)SizeInBytes, 0, &mapped);
        if (result != Result.Success)
        {
            Dispose();
            throw new InvalidOperationException($"Vulkan buffer map failed: {result}.");
        }
        _mapped = mapped;
    }

    public void Update(ReadOnlySpan<byte> data, int offsetInBytes)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (offsetInBytes < 0 || offsetInBytes > SizeInBytes || data.Length > SizeInBytes - offsetInBytes)
            throw new ArgumentOutOfRangeException(nameof(offsetInBytes), "Buffer update exceeds the allocation.");
        if (data.IsEmpty) return;
        data.CopyTo(new Span<byte>((byte*)_mapped + offsetInBytes, data.Length));
    }

    /// <summary>Reads the host-visible mapping; the buffer must be host visible.</summary>
    internal byte[] ReadAll()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _ctx.FlushPending();
        return new ReadOnlySpan<byte>(_mapped, SizeInBytes).ToArray();
    }

    internal void CopyTo(VulkanBuffer destination, int sourceOffset, int destinationOffset, int size)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        new ReadOnlySpan<byte>((byte*)_mapped + sourceOffset, size)
            .CopyTo(new Span<byte>((byte*)destination._mapped + destinationOffset, size));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.FlushPending();
        _disposed = true;
        if (_mapped != null) _ctx.Vk.UnmapMemory(_ctx.Device, Memory);
        _ctx.Vk.DestroyBuffer(_ctx.Device, Buffer, null);
        _ctx.Vk.FreeMemory(_ctx.Device, Memory, null);
    }
}

internal sealed class VulkanHandle : IRenderHandle
{
    internal VulkanHandle(ulong id, string kind) { Id = id; Kind = kind; }
    internal ulong Id { get; }
    internal string Kind { get; }
    public override string ToString() => $"vk:{Kind}:{Id}";
}

/// <summary>Colour (and optional depth) render target composed from Vulkan views.</summary>
internal sealed unsafe class VulkanFramebuffer : IRenderFramebuffer
{
    private readonly VulkanDeviceContext _ctx;
    private readonly VulkanDepthSurface? _ownedDepth;
    private bool _disposed;

    internal VulkanDepthSurface? DepthSurface { get; private set; }
    internal ImageView DepthView => DepthSurface?.View ?? default;

    internal void SetDepthSurface(VulkanDepthSurface? depth)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (depth is not null && (depth.Width != Width || depth.Height != Height))
            throw new ArgumentException("Depth attachment dimensions must match the framebuffer.", nameof(depth));
        // Detaching an external surface must fall back to the depth this target owns, or
        // later draws would silently lose depth testing.
        DepthSurface = depth ?? _ownedDepth;
    }

    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsComplete { get; }
    public IRenderTexture? Color { get; }

    internal VulkanTexture? ColorTexture => Color as VulkanTexture;
    internal Framebuffer Framebuffer => default;
    internal ImageView View => ColorTexture!.View;

    internal VulkanFramebuffer(VulkanDeviceContext ctx, IRenderTexture? color, DepthFormat depth, int width, int height)
    {
        _ctx = ctx;
        Color = color;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);

        if (color is not VulkanTexture target)
            throw new PlatformNotSupportedException("Vulkan framebuffers require a Vulkan render texture.");
        Width = target.Width;
        Height = target.Height;

        if (depth != DepthFormat.None)
        {
            _ownedDepth = new VulkanDepthSurface(ctx, Width, Height, depth);
            DepthSurface = _ownedDepth;
        }
        IsComplete = true;
        Handle = new VulkanHandle(target.View.Handle, "framebuffer");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.FlushPending();
        _disposed = true;
        _ownedDepth?.Dispose();
        DepthSurface = null;
    }
}

/// <summary>Depth-only Vulkan image used as a render target attachment.</summary>
internal sealed unsafe class VulkanDepthSurface : IRenderDepthSurface
{
    private readonly VulkanDeviceContext _ctx;
    private readonly Silk.NET.Vulkan.Image _image;
    private readonly DeviceMemory _memory;
    private bool _disposed;

    internal Silk.NET.Vulkan.Format Format { get; }
    internal ImageLayout Layout { get; private set; } = ImageLayout.Undefined;
    internal VkImage Image => _image;
    internal ImageView View { get; }
    public IRenderHandle Handle { get; }
    public int Width { get; }
    public int Height { get; }
    public uint NativeAttachmentId => 0;

    internal VulkanDepthSurface(VulkanDeviceContext ctx, int width, int height, DepthFormat format)
    {
        _ctx = ctx;
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        var vkFormat = format switch
        {
            DepthFormat.Depth16 => Silk.NET.Vulkan.Format.D16Unorm,
            DepthFormat.Depth24 => Silk.NET.Vulkan.Format.D32Sfloat,
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Depth surface requires a depth format."),
        };
        Format = vkFormat;

        var info = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = vkFormat,
            Extent = new Extent3D((uint)Width, (uint)Height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = ImageUsageFlags.DepthStencilAttachmentBit,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
        };
        if (ctx.Vk.CreateImage(ctx.Device, &info, null, out _image) != Result.Success)
            throw new InvalidOperationException("Vulkan depth image creation failed.");
        try
        {
            ctx.Vk.GetImageMemoryRequirements(ctx.Device, _image, out var reqs);
            _memory = ctx.AllocateMemory(reqs.Size, reqs.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit);
            var bindResult = ctx.Vk.BindImageMemory(ctx.Device, _image, _memory, 0);
            if (bindResult != Result.Success) throw new InvalidOperationException($"Vulkan depth memory bind failed: {bindResult}.");

            var viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _image,
                ViewType = ImageViewType.Type2D,
                Format = vkFormat,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.DepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
            };
            if (ctx.Vk.CreateImageView(ctx.Device, &viewInfo, null, out var view) != Result.Success)
                throw new InvalidOperationException("Vulkan depth view creation failed.");
            View = view;
            Handle = new VulkanHandle(view.Handle, "depth");
        }
        catch
        {
            Dispose();
            throw;
        }
    }


    internal void Transition(CommandBuffer cmd, ImageLayout target)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Layout == target && target == ImageLayout.ShaderReadOnlyOptimal) return;
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = Layout,
            NewLayout = target,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _image,
            SubresourceRange = new ImageSubresourceRange
            { AspectMask = ImageAspectFlags.DepthBit, LevelCount = 1, LayerCount = 1 },
            SrcAccessMask = Layout == ImageLayout.Undefined ? 0 : AccessFlags.DepthStencilAttachmentReadBit | AccessFlags.DepthStencilAttachmentWriteBit,
            DstAccessMask = AccessFlags.DepthStencilAttachmentReadBit | AccessFlags.DepthStencilAttachmentWriteBit,
        };
        _ctx.Vk.CmdPipelineBarrier(cmd,
            Layout == ImageLayout.Undefined ? PipelineStageFlags.TopOfPipeBit : PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
            0, 0, null, 0, null, 1, &barrier);
        // Undefined is a write-only starting point: the first transition from it discards
        // the image contents, so remember the real layout once any barrier has run.
        Layout = target;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.FlushPending();
        _disposed = true;
        if (View.Handle != 0) _ctx.Vk.DestroyImageView(_ctx.Device, View, null);
        _ctx.Vk.DestroyImage(_ctx.Device, _image, null);
        if (_memory.Handle != 0) _ctx.Vk.FreeMemory(_ctx.Device, _memory, null);
    }
}

internal sealed class VulkanUploadArena : IDisposable
{
    private readonly VulkanDeviceContext _context;
    private readonly System.Collections.Generic.List<VulkanBuffer> _pages = new();
    private int _pageIndex;
    private ulong _offset;

    internal VulkanUploadArena(VulkanDeviceContext context) => _context = context;

    internal (VulkanBuffer Buffer, ulong Offset) Snapshot(VulkanBuffer source, int sourceOffset, int size)
    {
        if (sourceOffset < 0 || size <= 0 || sourceOffset > source.SizeInBytes - size)
            throw new ArgumentOutOfRangeException(nameof(size), "Draw snapshot exceeds the source buffer.");
        ulong alignment = _context.UploadAlignment;
        while (true)
        {
            if (_pageIndex == _pages.Count)
                _pages.Add(new VulkanBuffer(_context, BufferKind.Uniform, Math.Max(4 * 1024 * 1024, size)));
            var page = _pages[_pageIndex];
            ulong offset = (_offset + alignment - 1) / alignment * alignment;
            if (offset + (ulong)size <= (ulong)page.SizeInBytes)
            {
                source.CopyTo(page, sourceOffset, (int)offset, size);
                _offset = offset + (ulong)size;
                return (page, offset);
            }
            _pageIndex++;
            _offset = 0;
        }
    }

    internal void Reset() { _pageIndex = 0; _offset = 0; }

    public void Dispose()
    {
        foreach (var page in _pages) page.Dispose();
        _pages.Clear();
    }
}
