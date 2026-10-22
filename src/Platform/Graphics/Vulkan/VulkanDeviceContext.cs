using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using THSSS.Graphics;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Core;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkExtent2D = Silk.NET.Vulkan.Extent2D;
using VkFormat = Silk.NET.Vulkan.Format;

namespace THSSS.Graphics.Vulkan;

/// <summary>
/// Minimal Vulkan device bootstrap: instance, physical device, logical device, one
/// graphics/transfer queue and a command pool. Rendering resources are created on top
/// of this; the class intentionally contains no frame presentation yet.
/// </summary>
public sealed unsafe class VulkanDeviceContext : IDisposable
{
    private readonly Vk _vk;
    private bool _disposed;

    internal Vk Vk => _vk;
    internal Instance Instance { get; }
    internal PhysicalDevice PhysicalDevice { get; }
    internal Device Device { get; }
    internal Queue Queue { get; }
    internal uint QueueFamily { get; }
    internal CommandPool CommandPool { get; }
    internal PhysicalDeviceMemoryProperties Memory => _memory;

    private PhysicalDeviceMemoryProperties _memory;

    /// <summary>Native surface handed to the loader, or null for headless use.</summary>
    internal VulkanSurfaceSource? SurfaceSource { get; }

    public VulkanDeviceContext() : this(null) { }

    public VulkanDeviceContext(VulkanSurfaceSource? surfaceSource)
    {
        SurfaceSource = surfaceSource;
        _vk = Vk.GetApi();
        Instance = CreateInstance();
        if (SurfaceSource is not null) CreateSurface();
        (PhysicalDevice, QueueFamily) = PickPhysicalDevice();
        Device = CreateLogicalDevice();
        _vk.GetDeviceQueue(Device, QueueFamily, 0, out var queue);
        Queue = queue;
        CommandPool = CreateCommandPool();
        _vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, out _memory);
        _vk.GetPhysicalDeviceProperties(PhysicalDevice, out var properties);
        UploadAlignment = Math.Max(16UL, properties.Limits.MinUniformBufferOffsetAlignment);
        CreateDescriptors();
        var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
        CheckResult(_vk.CreateFence(Device, &fenceInfo, null, out _completionFence), "create completion fence");
    }

    /// <summary>
    /// Instantiation outside of a platform library also needs the pre-instance API; the
    /// loader exports vkGetInstanceProcAddr from the platform loader that Silk resolves.
    /// </summary>
    private Instance CreateInstance()
    {
        var app = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("THSSS"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("THSSS"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13,
        };
        string[] wanted = SurfaceSource is null
            ? Array.Empty<string>()
            : VulkanSurfaceExtensions.RequiredExtensions(SurfaceSource.Platform);
        IntPtr[] names = Array.ConvertAll(wanted, n => (IntPtr)Marshal.StringToHGlobalAnsi(n));
        var info = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &app,
        };
        try
        {
            fixed (IntPtr* pNames = names)
            {
                info.EnabledExtensionCount = (uint)names.Length;
                info.PpEnabledExtensionNames = (byte**)pNames;
                CheckResult(_vk.CreateInstance(&info, null, out var instance), "create instance");
                return instance;
            }
        }
        finally
        {
            foreach (var name in names) Marshal.FreeHGlobal(name);
            Marshal.FreeHGlobal((IntPtr)app.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)app.PEngineName);
        }
    }

    internal SurfaceKHR Surface { get; private set; }
    internal KhrSurface? SurfaceApi { get; private set; }

    /// <summary>Creates the presentation surface from the platform handles, if any.</summary>
    private void CreateSurface()
    {
        var src = SurfaceSource!;
        if (!_vk.TryGetInstanceExtension(Instance, out KhrSurface surfaceApi))
            throw new PlatformNotSupportedException("VK_KHR_surface is not available.");
        SurfaceApi = surfaceApi;
        switch (src.Platform)
        {
            case VulkanPlatform.X11:
            {
                var info = new XlibSurfaceCreateInfoKHR
                {
                    SType = StructureType.XlibSurfaceCreateInfoKhr,
                    Dpy = (nint*)src.Display,
                    Window = (nint)src.Window,
                };
                if (!_vk.TryGetInstanceExtension(Instance, out KhrXlibSurface xlib))
                    throw new PlatformNotSupportedException("VK_KHR_xlib_surface is not available.");
                if (xlib.CreateXlibSurface(Instance, &info, null, out var surface) != Result.Success)
                    throw new PlatformNotSupportedException("Vulkan X11 surface creation failed.");
                Surface = surface;
                break;
            }
            case VulkanPlatform.Wayland:
            {
                var info = new WaylandSurfaceCreateInfoKHR
                {
                    SType = StructureType.WaylandSurfaceCreateInfoKhr,
                    Display = (nint*)src.Display,
                    Surface = (nint*)src.Window,
                };
                if (!_vk.TryGetInstanceExtension(Instance, out KhrWaylandSurface wayland))
                    throw new PlatformNotSupportedException("VK_KHR_wayland_surface is not available.");
                if (wayland.CreateWaylandSurface(Instance, &info, null, out var surface) != Result.Success)
                    throw new PlatformNotSupportedException("Vulkan Wayland surface creation failed.");
                Surface = surface;
                break;
            }
            case VulkanPlatform.Win32:
            {
                var info = new Win32SurfaceCreateInfoKHR
                {
                    SType = StructureType.Win32SurfaceCreateInfoKhr,
                    Hinstance = src.Instance,
                    Hwnd = src.Window,
                };
                if (!_vk.TryGetInstanceExtension(Instance, out KhrWin32Surface win32))
                    throw new PlatformNotSupportedException("VK_KHR_win32_surface is not available.");
                if (win32.CreateWin32Surface(Instance, &info, null, out var surface) != Result.Success)
                    throw new PlatformNotSupportedException("Vulkan Win32 surface creation failed.");
                Surface = surface;
                break;
            }
            default:
                throw new PlatformNotSupportedException(
                    $"Vulkan presentation on {src.Platform} is not wired up in this build.");
        }
    }

    private bool SupportsPresent(PhysicalDevice device, uint family)
    {
        if (SurfaceApi is null) return true;
        return SurfaceApi.GetPhysicalDeviceSurfaceSupport(device, family, Surface, out var supported) == Result.Success
            && supported;
    }

    private (PhysicalDevice, uint) PickPhysicalDevice()
    {
        uint count = 0;
        if (_vk.EnumeratePhysicalDevices(Instance, ref count, null) != Result.Success || count == 0)
            throw new PlatformNotSupportedException("No Vulkan physical device found.");

        var devices = new PhysicalDevice[count];
        fixed (PhysicalDevice* p = devices)
            _vk.EnumeratePhysicalDevices(Instance, ref count, p);

        foreach (var candidate in devices)
        {
            uint familyCount = 0;
            _vk.GetPhysicalDeviceQueueFamilyProperties(candidate, ref familyCount, null);
            var families = new QueueFamilyProperties[familyCount];
            fixed (QueueFamilyProperties* p = families)
                _vk.GetPhysicalDeviceQueueFamilyProperties(candidate, ref familyCount, p);
            for (uint i = 0; i < familyCount; i++)
                if ((families[i].QueueFlags & QueueFlags.GraphicsBit) != 0 && SupportsPresent(candidate, i))
                    return (candidate, i);
        }
        throw new PlatformNotSupportedException("No Vulkan device with a graphics queue found.");
    }

    private Device CreateLogicalDevice()
    {
        float priority = 1f;
        var queueCreate = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = QueueFamily,
            QueueCount = 1,
            PQueuePriorities = &priority,
        };
        var features = new PhysicalDeviceFeatures();
        var features13 = new PhysicalDeviceVulkan13Features
        {
            SType = StructureType.PhysicalDeviceVulkan13Features,
            DynamicRendering = true,
            Synchronization2 = true,
        };
        // Presenting needs the swapchain device extension; enable it only when a surface
        // was requested so headless use keeps a minimal device.
        string[] deviceExtensions = SurfaceSource is null
            ? Array.Empty<string>()
            : new[] { "VK_KHR_swapchain" };
        IntPtr[] deviceExtensionNames = Array.ConvertAll(deviceExtensions,
            n => (IntPtr)Marshal.StringToHGlobalAnsi(n));
        var info = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreate,
            PEnabledFeatures = &features,
            PNext = &features13,
        };
        try
        {
            fixed (IntPtr* pExtensions = deviceExtensionNames)
            {
                info.EnabledExtensionCount = (uint)deviceExtensionNames.Length;
                info.PpEnabledExtensionNames = (byte**)pExtensions;
                CheckResult(_vk.CreateDevice(PhysicalDevice, &info, null, out var device), "create logical device");
                return device;
            }
        }
        finally
        {
            foreach (var name in deviceExtensionNames) Marshal.FreeHGlobal(name);
        }
    }

    private CommandPool CreateCommandPool()
    {
        var info = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = QueueFamily,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
        };
        if (_vk.CreateCommandPool(Device, &info, null, out var pool) != Result.Success)
            throw new PlatformNotSupportedException("Vulkan command pool creation failed.");
        return pool;
    }

    private DescriptorSetLayout _setLayout;
    private readonly List<DescriptorPool> _descriptorPools = new();
    private int _descriptorCount;
    private const int SetsPerPool = 256;
    private CommandBuffer _batchCommand;
    private bool _hasPendingCommands;
    private Fence _completionFence;
    private VulkanUploadArena? _uploads;
    internal bool IsBatching { get; private set; }
    public long SubmissionCount { get; private set; }
    internal ulong UploadAlignment { get; private set; }

    /// <summary>
    /// Set 0 matches the shader: binding 0 is the std140 block, binding 1 the sampler.
    /// Both are visible to every stage because GLSL lets either stage read them.
    /// </summary>
    internal DescriptorSetLayout SetLayout => _setLayout;

    internal void CreateDescriptors()
    {
        var bindings = stackalloc DescriptorSetLayoutBinding[2];
        bindings[0] = new DescriptorSetLayoutBinding
        {
            Binding = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
        };
        bindings[1] = new DescriptorSetLayoutBinding
        {
            Binding = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
        };
        var layoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 2,
            PBindings = bindings,
        };
        if (_vk.CreateDescriptorSetLayout(Device, &layoutInfo, null, out _setLayout) != Result.Success)
            throw new InvalidOperationException("Vulkan descriptor set layout creation failed.");

    }

    private DescriptorPool CreateDescriptorPool()
    {
        var sizes = stackalloc DescriptorPoolSize[2];
        sizes[0] = new DescriptorPoolSize { Type = DescriptorType.UniformBuffer, DescriptorCount = SetsPerPool };
        sizes[1] = new DescriptorPoolSize { Type = DescriptorType.CombinedImageSampler, DescriptorCount = SetsPerPool };
        var poolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = SetsPerPool,
            PoolSizeCount = 2,
            PPoolSizes = sizes,
        };
        CheckResult(_vk.CreateDescriptorPool(Device, &poolInfo, null, out var pool), "create descriptor pool");
        return pool;
    }

    private Sampler _linear;

    /// <summary>Linear clamp sampler shared by every textured draw.</summary>
    internal Sampler LinearSampler
    {
        get
        {
            if (_linear.Handle != 0) return _linear;
            var info = new SamplerCreateInfo
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                AddressModeU = SamplerAddressMode.ClampToEdge,
                AddressModeV = SamplerAddressMode.ClampToEdge,
                AddressModeW = SamplerAddressMode.ClampToEdge,
                MaxLod = 1,
            };
            if (_vk.CreateSampler(Device, &info, null, out _linear) != Result.Success)
                throw new InvalidOperationException("Vulkan sampler creation failed.");
            return _linear;
        }
    }

    internal void ResetDescriptors()
    {
        foreach (var pool in _descriptorPools)
            CheckResult(_vk.ResetDescriptorPool(Device, pool, 0), "reset descriptor pool");
        _descriptorCount = 0;
    }

    internal DescriptorSet AllocateSet()
    {
        int poolIndex = _descriptorCount / SetsPerPool;
        if (poolIndex == _descriptorPools.Count) _descriptorPools.Add(CreateDescriptorPool());
        var layouts = stackalloc DescriptorSetLayout[1] { _setLayout };
        var info = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _descriptorPools[poolIndex],
            DescriptorSetCount = 1,
            PSetLayouts = layouts,
        };
        if (_vk.AllocateDescriptorSets(Device, &info, out var set) != Result.Success)
            throw new InvalidOperationException("Vulkan descriptor set allocation failed.");
        _descriptorCount++;
        return set;
    }

    /// <summary>Creates a host-visible buffer bound to its own memory.</summary>
    internal (VkBuffer Buffer, DeviceMemory Memory) CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags memory)
    {
        var info = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        };
        if (_vk.CreateBuffer(Device, &info, null, out var buffer) != Result.Success)
            throw new InvalidOperationException("Vulkan buffer creation failed.");
        _vk.GetBufferMemoryRequirements(Device, buffer, out var reqs);
        var memoryHandle = AllocateMemory(reqs.Size, reqs.MemoryTypeBits, memory);
        _vk.BindBufferMemory(Device, buffer, memoryHandle, 0);
        return (buffer, memoryHandle);
    }

    /// <summary>Records and submits one command buffer synchronously.</summary>
    internal void RunOnce(Action<CommandBuffer> record)
    {
        FlushPending();
        RunCommands(record, default, default);
    }

    internal void BeginFrame()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsBatching) throw new InvalidOperationException("A Vulkan frame is already open.");
        IsBatching = true;
    }

    internal void EndFrame()
    {
        try { FlushPending(); }
        finally { IsBatching = false; }
    }

    internal (VulkanBuffer Buffer, ulong Offset) Snapshot(VulkanBuffer source, int offset, int size)
        => (_uploads ??= new VulkanUploadArena(this)).Snapshot(source, offset, size);

    internal void Record(Action<CommandBuffer> record)
    {
        if (!IsBatching) { RunOnce(record); return; }
        if (!_hasPendingCommands)
        {
            if (_batchCommand.Handle == 0) _batchCommand = AllocateCommand();
            else CheckResult(_vk.ResetCommandBuffer(_batchCommand, 0), "reset frame command buffer");
            BeginCommand(_batchCommand);
            _hasPendingCommands = true;
        }
        record(_batchCommand);
    }

    internal void FlushPending()
    {
        if (!_hasPendingCommands) return;
        SubmitAndWait(_batchCommand, default, default);
        RecycleBatch();
    }

    private void RecycleBatch()
    {
        _hasPendingCommands = false;
        ResetDescriptors();
        _uploads?.Reset();
    }

    internal void RunPresent(Action<CommandBuffer> record, Semaphore acquireSemaphore, Semaphore renderSemaphore)
    {
        if (acquireSemaphore.Handle == 0 || renderSemaphore.Handle == 0)
            throw new ArgumentException("Presentation requires acquire and render semaphores.");
        if (!IsBatching) { RunCommands(record, acquireSemaphore, renderSemaphore); return; }
        Record(record);
        SubmitAndWait(_batchCommand, acquireSemaphore, renderSemaphore);
        RecycleBatch();
    }

    /// <summary>Set once the window system destroys the native surface; it cannot be reused.</summary>
    internal bool SurfaceLost { get; private set; }

    internal void CheckResult(Result result, string operation)
    {
        if (result == Result.ErrorSurfaceLostKhr) SurfaceLost = true;
        if (result != Result.Success)
            throw new InvalidOperationException($"Vulkan {operation} failed: {result}.");
    }

    private CommandBuffer AllocateCommand()
    {
        var alloc = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        if (_vk.AllocateCommandBuffers(Device, &alloc, out var cmd) != Result.Success)
            throw new InvalidOperationException("Vulkan command buffer allocation failed.");
        return cmd;
    }

    private void BeginCommand(CommandBuffer cmd)
    {
        var begin = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        CheckResult(_vk.BeginCommandBuffer(cmd, &begin), "begin command buffer");
    }

    private void RunCommands(Action<CommandBuffer> record, Semaphore acquireSemaphore, Semaphore renderSemaphore)
    {
        var cmd = AllocateCommand();
        try
        {
            BeginCommand(cmd);
            record(cmd);
            SubmitAndWait(cmd, acquireSemaphore, renderSemaphore);
        }
        finally
        {
            _vk.FreeCommandBuffers(Device, CommandPool, 1, &cmd);
        }
    }

    private void SubmitAndWait(CommandBuffer cmd, Semaphore acquireSemaphore, Semaphore renderSemaphore)
    {
        CheckResult(_vk.EndCommandBuffer(cmd), "end command buffer");
        var waitStage = PipelineStageFlags.TransferBit;
        var submit = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = acquireSemaphore.Handle == 0 ? 0u : 1u,
            PWaitSemaphores = &acquireSemaphore,
            PWaitDstStageMask = &waitStage,
            CommandBufferCount = 1,
            PCommandBuffers = &cmd,
            SignalSemaphoreCount = renderSemaphore.Handle == 0 ? 0u : 1u,
            PSignalSemaphores = &renderSemaphore,
        };
        var fence = _completionFence;
        CheckResult(_vk.ResetFences(Device, 1, &fence), "reset completion fence");
        CheckResult(_vk.QueueSubmit(Queue, 1, &submit, fence), "submit commands");
        SubmissionCount++;
        CheckResult(_vk.WaitForFences(Device, 1, &fence, true, ulong.MaxValue), "wait for submitted commands");
    }

    /// <summary>Picks a memory type satisfying the required flags.</summary>
    internal uint FindMemoryType(uint typeBits, MemoryPropertyFlags required)
    {
        for (uint i = 0; i < _memory.MemoryTypeCount; i++)
        {
            if ((typeBits & (1u << (int)i)) == 0) continue;
            if ((_memory.MemoryTypes[(int)i].PropertyFlags & required) == required) return i;
        }
        throw new InvalidOperationException("No Vulkan memory type matches the requested properties.");
    }

    internal DeviceMemory AllocateMemory(ulong size, uint typeBits, MemoryPropertyFlags flags)
    {
        var alloc = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = size,
            MemoryTypeIndex = FindMemoryType(typeBits, flags),
        };
        if (_vk.AllocateMemory(Device, &alloc, null, out var memory) != Result.Success)
            throw new OutOfMemoryException("Vulkan memory allocation failed.");
        return memory;
    }

    public void Dispose()
    {
        if (_disposed) return;
        EndFrame();
        _uploads?.Dispose();
        CheckResult(_vk.DeviceWaitIdle(Device), "wait before destroying device");
        _disposed = true;
        if (_linear.Handle != 0) _vk.DestroySampler(Device, _linear, null);
        foreach (var pool in _descriptorPools) _vk.DestroyDescriptorPool(Device, pool, null);
        if (_completionFence.Handle != 0) _vk.DestroyFence(Device, _completionFence, null);
        if (_setLayout.Handle != 0) _vk.DestroyDescriptorSetLayout(Device, _setLayout, null);
        _vk.DestroyCommandPool(Device, CommandPool, null);
        _vk.DestroyDevice(Device, null);
        if (Surface.Handle != 0) SurfaceApi!.DestroySurface(Instance, Surface, null);
        SurfaceApi?.Dispose();
        _vk.DestroyInstance(Instance, null);
        _vk.Dispose();
    }
}
