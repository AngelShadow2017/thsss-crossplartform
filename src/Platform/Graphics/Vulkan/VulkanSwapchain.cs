using System;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace THSSS.Graphics.Vulkan;

internal sealed unsafe class VulkanSwapchain : IDisposable
{
    private readonly VulkanDeviceContext _ctx;
    private readonly KhrSwapchain _swapchainApi;
    private readonly SwapchainKHR _swapchain;
    private readonly Image[] _images = [];
    private readonly ImageView[] _views = [];
    private readonly ImageLayout[] _layouts = [];
    private readonly Semaphore _imageAvailable;
    private readonly Semaphore[] _renderFinished = [];
    private readonly SurfaceTransformFlagsKHR _transform;
    private bool _outOfDate;
    private int _acquiredImage = -1;
    private bool _disposed;

    internal Format Format { get; }
    internal Extent2D Extent { get; }
    internal int ImageCount => _images.Length;
    internal bool SupportsReadback { get; }
    internal bool NeedsRecreation { get; private set; }
    internal Result LastAcquireResult { get; private set; } = Result.Success;
    internal int ReadbackSize => checked((int)(Extent.Width * (ulong)Extent.Height * 4));

    internal static VulkanSwapchain? TryCreate(VulkanDeviceContext ctx, uint width, uint height, VulkanSwapchain? previous)
    {
        var chain = new VulkanSwapchain(ctx, width, height, previous);
        if (chain._swapchain.Handle != 0) return chain;
        chain.Dispose();
        return null;
    }

    internal bool ShouldRecreate(uint width, uint height)
    {
        if (!NeedsRecreation) return false;
        if (_outOfDate) return true;
        _ctx.CheckResult(_ctx.SurfaceApi!.GetPhysicalDeviceSurfaceCapabilities(
            _ctx.PhysicalDevice, _ctx.Surface, out var caps), "query surface capabilities");
        var extent = PickExtent(caps, width, height);
        return extent.Width != Extent.Width || extent.Height != Extent.Height || caps.CurrentTransform != _transform;
    }

    private VulkanSwapchain(VulkanDeviceContext ctx, uint width, uint height, VulkanSwapchain? previous)
    {
        _ctx = ctx;
        if (!ctx.Vk.TryGetDeviceExtension(ctx.Instance, ctx.Device, out KhrSwapchain api))
            throw new PlatformNotSupportedException("VK_KHR_swapchain is not available on this device.");
        _swapchainApi = api;
        var surfaceApi = ctx.SurfaceApi ?? throw new InvalidOperationException("A presentation surface is required.");
        _ctx.CheckResult(surfaceApi.GetPhysicalDeviceSurfaceCapabilities(
            ctx.PhysicalDevice, ctx.Surface, out var caps), "query surface capabilities");
        Extent = PickExtent(caps, width, height);
        _transform = caps.CurrentTransform;
        if (Extent.Width == 0 || Extent.Height == 0) return;
        uint formatCount = 0;
        _ctx.CheckResult(surfaceApi.GetPhysicalDeviceSurfaceFormats(
            ctx.PhysicalDevice, ctx.Surface, &formatCount, null), "query surface format count");
        var formats = new SurfaceFormatKHR[formatCount];
        _ctx.CheckResult(surfaceApi.GetPhysicalDeviceSurfaceFormats(
            ctx.PhysicalDevice, ctx.Surface, &formatCount, formats), "query surface formats");
        var selectedFormat = PickFormat(formats);
        Format = selectedFormat.Format;

        var usage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit;
        if ((caps.SupportedUsageFlags & usage) != usage)
            throw new PlatformNotSupportedException("The surface must support transfer-destination and color-attachment images.");
        SupportsReadback = (caps.SupportedUsageFlags & ImageUsageFlags.TransferSrcBit) != 0;
        if (SupportsReadback) usage |= ImageUsageFlags.TransferSrcBit;
        uint imageCount = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount) imageCount = caps.MaxImageCount;
        var create = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = ctx.Surface,
            MinImageCount = imageCount,
            ImageFormat = Format,
            ImageColorSpace = selectedFormat.ColorSpace,
            ImageExtent = Extent,
            ImageArrayLayers = 1,
            ImageUsage = usage,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = caps.CurrentTransform,
            CompositeAlpha = PickCompositeAlpha(caps.SupportedCompositeAlpha),
            PresentMode = PresentModeKHR.FifoKhr,
            Clipped = true,
            OldSwapchain = previous?._swapchain ?? default,
        };
        var result = _swapchainApi.CreateSwapchain(ctx.Device, &create, null, out _swapchain);
        if (result == Result.ErrorOutOfDateKhr) return;
        _ctx.CheckResult(result, "create swapchain");
        try
        {
            uint count = 0;
            _ctx.CheckResult(_swapchainApi.GetSwapchainImages(ctx.Device, _swapchain,
                &count, null), "query swapchain image count");
            _images = new Image[count];
            _views = new ImageView[count];
            _layouts = new ImageLayout[count];
            _renderFinished = new Semaphore[count];
            _ctx.CheckResult(_swapchainApi.GetSwapchainImages(ctx.Device, _swapchain,
                &count, _images), "query swapchain images");
            for (int i = 0; i < _images.Length; i++)
            {
                var viewInfo = new ImageViewCreateInfo
                {
                    SType = StructureType.ImageViewCreateInfo,
                    Image = _images[i],
                    ViewType = ImageViewType.Type2D,
                    Format = Format,
                    SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
                };
                _ctx.CheckResult(ctx.Vk.CreateImageView(ctx.Device, &viewInfo, null,
                    out _views[i]), "create swapchain image view");
                _layouts[i] = ImageLayout.Undefined;
                _renderFinished[i] = CreateSemaphore(ctx);
            }
            _imageAvailable = CreateSemaphore(ctx);
        }
        catch
        {
            DestroyResources();
            throw;
        }
    }

    private static Semaphore CreateSemaphore(VulkanDeviceContext ctx)
    {
        var info = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        ctx.CheckResult(ctx.Vk.CreateSemaphore(ctx.Device, &info, null,
            out var semaphore), "create presentation semaphore");
        return semaphore;
    }

    private static SurfaceFormatKHR PickFormat(ReadOnlySpan<SurfaceFormatKHR> formats)
    {
        if (formats.Length == 0)
            throw new PlatformNotSupportedException("The surface exposes no image formats.");
        if (formats.Length == 1 && formats[0].Format == Format.Undefined)
            return new SurfaceFormatKHR(Format.B8G8R8A8Unorm, formats[0].ColorSpace);
        foreach (var candidate in formats)
            if (candidate.Format is Format.B8G8R8A8Unorm or Format.R8G8B8A8Unorm)
                return candidate;
        throw new PlatformNotSupportedException("The surface exposes no supported RGBA8/BGRA8 UNORM format.");
    }

    private static CompositeAlphaFlagsKHR PickCompositeAlpha(CompositeAlphaFlagsKHR supported)
    {
        foreach (var candidate in new[] { CompositeAlphaFlagsKHR.OpaqueBitKhr,
            CompositeAlphaFlagsKHR.PreMultipliedBitKhr, CompositeAlphaFlagsKHR.PostMultipliedBitKhr,
            CompositeAlphaFlagsKHR.InheritBitKhr })
            if ((supported & candidate) != 0) return candidate;
        throw new PlatformNotSupportedException("The surface exposes no composite-alpha mode.");
    }

    private static Extent2D PickExtent(SurfaceCapabilitiesKHR caps, uint width, uint height)
    {
        if (caps.CurrentExtent.Width != uint.MaxValue) return caps.CurrentExtent;
        return new Extent2D(
            Math.Clamp(width, caps.MinImageExtent.Width, caps.MaxImageExtent.Width),
            Math.Clamp(height, caps.MinImageExtent.Height, caps.MaxImageExtent.Height));
    }

    private Fence CreateFence()
    {
        var info = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
        _ctx.CheckResult(_ctx.Vk.CreateFence(_ctx.Device, &info, null, out var fence), "create acquire fence");
        return fence;
    }

    internal bool TryAcquire(out int index, ulong timeoutNanoseconds = 1_000_000_000)
    {
        if (_acquiredImage >= 0)
            throw new InvalidOperationException("Present the acquired image before acquiring another.");
        index = -1;
        uint acquired = 0;
        var fence = CreateFence();
        Result result;
        try
        {
            result = _swapchainApi.AcquireNextImage(_ctx.Device, _swapchain, timeoutNanoseconds,
                _imageAvailable, fence, &acquired);
            if (result is Result.Success or Result.SuboptimalKhr)
                _ctx.CheckResult(_ctx.Vk.WaitForFences(_ctx.Device, 1, &fence, true, ulong.MaxValue), "wait for acquire fence");
        }
        finally
        {
            _ctx.Vk.DestroyFence(_ctx.Device, fence, null);
        }
        LastAcquireResult = result;
        if (result is Result.Timeout or Result.NotReady) return false;
        if (result == Result.ErrorOutOfDateKhr)
        {
            NeedsRecreation = _outOfDate = true;
            return false;
        }
        if (result == Result.SuboptimalKhr) NeedsRecreation = true;
        else _ctx.CheckResult(result, "acquire swapchain image");
        index = _acquiredImage = checked((int)acquired);
        return true;
    }

    internal int Acquire()
    {
        if (TryAcquire(out var index)) return index;
        if (LastAcquireResult is Result.Timeout or Result.NotReady)
            throw new TimeoutException($"Vulkan swapchain acquisition did not complete: {LastAcquireResult}.");
        throw new InvalidOperationException("Vulkan swapchain is out of date; recreate it before acquiring.");
    }

    internal ImageView View(int index) => _views[index];
    internal Image Image(int index) => _images[index];
    internal Semaphore ImageAvailable => _imageAvailable;
    internal Semaphore RenderFinished => _acquiredImage >= 0 ? _renderFinished[_acquiredImage]
        : throw new InvalidOperationException("Acquire an image before requesting its render semaphore.");

    internal void TransitionToRender(CommandBuffer cmd, int index)
        => Transition(cmd, index, ImageLayout.ColorAttachmentOptimal);

    internal void TransitionToTransferDst(CommandBuffer cmd, int index)
        => Transition(cmd, index, ImageLayout.TransferDstOptimal);

    internal void TransitionToTransferDestination(CommandBuffer cmd, int index)
        => TransitionToTransferDst(cmd, index);

    internal void TransitionToPresent(CommandBuffer cmd, int index)
        => Transition(cmd, index, ImageLayout.PresentSrcKhr);

    private void RequireAcquired(int index)
    {
        if (index < 0 || index != _acquiredImage)
            throw new InvalidOperationException("Only the currently acquired swapchain image may be accessed.");
    }

    private static (PipelineStageFlags Stage, AccessFlags Access) Synchronization(ImageLayout layout)
        => layout switch
        {
            ImageLayout.Undefined => (PipelineStageFlags.TopOfPipeBit, 0),
            ImageLayout.PresentSrcKhr => (PipelineStageFlags.TransferBit, 0),
            ImageLayout.TransferDstOptimal => (PipelineStageFlags.TransferBit, AccessFlags.TransferWriteBit),
            ImageLayout.TransferSrcOptimal => (PipelineStageFlags.TransferBit, AccessFlags.TransferReadBit),
            ImageLayout.ColorAttachmentOptimal => (PipelineStageFlags.ColorAttachmentOutputBit,
                AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit),
            _ => throw new InvalidOperationException($"Unsupported swapchain image layout: {layout}."),
        };

    private void Transition(CommandBuffer cmd, int index, ImageLayout layout)
    {
        RequireAcquired(index);
        var oldLayout = _layouts[index];
        if (oldLayout == layout) return;
        var source = Synchronization(oldLayout);
        var destination = Synchronization(layout);
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = layout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = _images[index],
            SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
            SrcAccessMask = source.Access,
            DstAccessMask = destination.Access,
        };
        var destinationStage = layout == ImageLayout.PresentSrcKhr
            ? PipelineStageFlags.BottomOfPipeBit : destination.Stage;
        _ctx.Vk.CmdPipelineBarrier(cmd, source.Stage, destinationStage,
            0, 0, null, 0, null, 1, &barrier);
        _layouts[index] = layout;
    }

    internal void RecordReadback(CommandBuffer cmd, int index, VulkanBuffer staging)
    {
        RequireAcquired(index);
        if (!SupportsReadback)
            throw new PlatformNotSupportedException("The surface does not support transfer-source readback.");
        Transition(cmd, index, ImageLayout.TransferSrcOptimal);
        var region = new BufferImageCopy
        {
            ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            ImageExtent = new Extent3D(Extent.Width, Extent.Height, 1),
        };
        _ctx.Vk.CmdCopyImageToBuffer(cmd, _images[index], ImageLayout.TransferSrcOptimal,
            staging.Buffer, 1, &region);
        var hostBarrier = new BufferMemoryBarrier
        {
            SType = StructureType.BufferMemoryBarrier,
            SrcAccessMask = AccessFlags.TransferWriteBit,
            DstAccessMask = AccessFlags.HostReadBit,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Buffer = staging.Buffer,
            Size = (ulong)ReadbackSize,
        };
        _ctx.Vk.CmdPipelineBarrier(cmd, PipelineStageFlags.TransferBit, PipelineStageFlags.HostBit,
            0, 0, null, 1, &hostBarrier, 0, null);
    }

    internal byte[] ReadImage(int index, VulkanDeviceContext ctx)
    {
        RequireAcquired(index);
        var previousLayout = _layouts[index];
        using var staging = new VulkanBuffer(ctx, BufferKind.Vertex, ReadbackSize);
        ctx.RunOnce(cmd =>
        {
            RecordReadback(cmd, index, staging);
            Transition(cmd, index, previousLayout);
        });
        return staging.ReadAll();
    }

    internal Result Present(int index)
    {
        RequireAcquired(index);
        if (_layouts[index] != ImageLayout.PresentSrcKhr)
            throw new InvalidOperationException("Transition the acquired image to PresentSrcKhr before presenting.");
        var wait = _renderFinished[index];
        var chain = _swapchain;
        uint imageIndex = checked((uint)index);
        var info = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = &wait,
            SwapchainCount = 1,
            PSwapchains = &chain,
            PImageIndices = &imageIndex,
        };
        var result = _swapchainApi.QueuePresent(_ctx.Queue, &info);
        _acquiredImage = -1;
        if (result == Result.ErrorOutOfDateKhr) NeedsRecreation = _outOfDate = true;
        else if (result == Result.SuboptimalKhr) NeedsRecreation = true;
        else _ctx.CheckResult(result, "present swapchain image");
        _ctx.CheckResult(_ctx.Vk.QueueWaitIdle(_ctx.Queue), "wait for presentation queue");
        return result;
    }

    private void DestroyResources()
    {
        if (_views is not null)
            foreach (var view in _views)
                if (view.Handle != 0) _ctx.Vk.DestroyImageView(_ctx.Device, view, null);
        if (_renderFinished is not null)
            foreach (var semaphore in _renderFinished)
                if (semaphore.Handle != 0) _ctx.Vk.DestroySemaphore(_ctx.Device, semaphore, null);
        if (_imageAvailable.Handle != 0) _ctx.Vk.DestroySemaphore(_ctx.Device, _imageAvailable, null);
        if (_swapchain.Handle != 0) _swapchainApi.DestroySwapchain(_ctx.Device, _swapchain, null);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.CheckResult(_ctx.Vk.DeviceWaitIdle(_ctx.Device), "wait before destroying swapchain");
        _disposed = true;
        DestroyResources();
        _swapchainApi.Dispose();
    }
}
