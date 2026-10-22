using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using THSSS.Graphics;

namespace THSSS.Graphics.Vulkan;

/// <summary>
/// Vulkan shader: real SPIR-V modules plus a pipeline built for the vertex layout the
/// draw call supplies. Pipelines are cached per layout because the interface hands the
/// layout at draw time rather than at creation.
/// </summary>
internal sealed unsafe class VulkanShader : IRenderShader
{
    private readonly VulkanDeviceContext _ctx;
    private readonly ShaderModule _vertex;
    private readonly ShaderModule _fragment;
    private readonly record struct PipelineKey(int Stride, string Attributes, PrimitiveKind Primitive,
        BlendState Blend, bool DepthTest, bool DepthWrite, CullMode Cull, Format ColorFormat, Format DepthFormat);
    private readonly Dictionary<PipelineKey, Pipeline> _pipelines = new();
    private readonly Dictionary<string, int> _uniforms = new(StringComparer.Ordinal);
    private readonly List<PipelineLayout> _layouts = new();
    private readonly Dictionary<PipelineKey, PipelineLayout> _pipelineLayouts = new();
    private bool _disposed;

    public IRenderHandle Handle { get; }

    internal VulkanShader(VulkanDeviceContext ctx, ShaderSource source)
    {
        _ctx = ctx;
        var vs = SpirVCompiler.Compile(source.Vertex, vertexStage: true);
        var fs = SpirVCompiler.Compile(source.Fragment, vertexStage: false);
        _vertex = CreateModule(vs);
        try { _fragment = CreateModule(fs); }
        catch
        {
            _ctx.Vk.DestroyShaderModule(_ctx.Device, _vertex, null);
            throw;
        }
        Handle = new VulkanHandle(_vertex.Handle, "shader");
        for (int i = 0; i < source.VertexInputs.Length; i++) _uniforms[source.VertexInputs[i]] = i;
    }

    private ShaderModule CreateModule(byte[] spirv)
    {
        fixed (byte* code = spirv)
        {
            var info = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)spirv.Length,
                PCode = (uint*)code,
            };
            if (_ctx.Vk.CreateShaderModule(_ctx.Device, &info, null, out var module) != Result.Success)
                throw new InvalidOperationException("Vulkan shader module creation failed.");
            return module;
        }
    }

    public int GetUniformLocation(string name) => _uniforms.TryGetValue(name, out var i) ? i : -1;
    public void Use() { }
    public void SetUniform(int location, float value) { }
    public void SetUniform(int location, float x, float y) { }
    public void SetUniform(int location, float x, float y, float z) { }
    public void SetUniform(int location, float x, float y, float z, float w) { }
    public void SetUniform(int location, ReadOnlySpan<float> matrix4) { }
    public void SetUniform(int location, int value) { }

    internal Pipeline GetPipeline(VertexLayout layout, out PipelineLayout pipelineLayout)
        => GetPipeline(layout, PrimitiveKind.TriangleList,
            new BlendState(true, THSSS.Graphics.BlendFactor.SourceAlpha, THSSS.Graphics.BlendFactor.InverseSourceAlpha, BlendEquation.Add),
            false, false, CullMode.None, Format.R8G8B8A8Unorm, Format.Undefined, out pipelineLayout);

    internal Pipeline GetPipeline(VertexLayout layout, PrimitiveKind primitive, BlendState blend,
        bool depthTest, bool depthWrite, CullMode cullMode, Format colorFormat, Format depthFormat,
        out PipelineLayout pipelineLayout)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var attributeKey = string.Join(";", Array.ConvertAll(layout.Attributes.ToArray(),
            a => $"{a.Location},{a.ComponentCount},{(int)a.Type},{a.Normalized},{a.ByteOffset}"));
        var key = new PipelineKey(layout.Stride, attributeKey, primitive, blend,
            depthTest, depthWrite, cullMode, colorFormat, depthFormat);
        if (_pipelines.TryGetValue(key, out var cached)) { pipelineLayout = _pipelineLayouts[key]; return cached; }

        var bindings = new VertexInputBindingDescription[1]
        {
            new() { Binding = 0, Stride = (uint)layout.Stride, InputRate = VertexInputRate.Vertex },
        };
        var attributes = new VertexInputAttributeDescription[layout.Attributes.Length];
        for (int i = 0; i < attributes.Length; i++)
        {
            var a = layout.Attributes.Span[i];
            attributes[i] = new VertexInputAttributeDescription
            {
                Location = (uint)a.Location,
                Binding = 0,
                Format = AttributeFormat(a),
                Offset = (uint)a.ByteOffset,
            };
        }

        fixed (VertexInputBindingDescription* pBinding = bindings)
        fixed (VertexInputAttributeDescription* pAttributes = attributes)
        {
            var vertexInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = pBinding,
                VertexAttributeDescriptionCount = (uint)attributes.Length,
                PVertexAttributeDescriptions = pAttributes,
            };
            var inputAssembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = primitive switch
                {
                    PrimitiveKind.TriangleList => PrimitiveTopology.TriangleList,
                    PrimitiveKind.TriangleStrip => PrimitiveTopology.TriangleStrip,
                    PrimitiveKind.LineList => PrimitiveTopology.LineList,
                    _ => throw new ArgumentOutOfRangeException(nameof(primitive)),
                },
            };
            var viewportState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1,
            };
            var rasterizer = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = cullMode == CullMode.None ? CullModeFlags.None : CullModeFlags.BackBit,
                FrontFace = cullMode == CullMode.Clockwise ? FrontFace.Clockwise : FrontFace.CounterClockwise,
                LineWidth = 1f,
            };
            var multisample = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = SampleCountFlags.Count1Bit,
            };
            var colourBlend = new PipelineColorBlendAttachmentState
            {
                BlendEnable = blend.Enabled,
                SrcColorBlendFactor = ToBlendFactor(blend.Source),
                DstColorBlendFactor = ToBlendFactor(blend.Destination),
                SrcAlphaBlendFactor = ToBlendFactor(blend.Source),
                DstAlphaBlendFactor = ToBlendFactor(blend.Destination),
                ColorBlendOp = ToBlendOp(blend.Equation),
                AlphaBlendOp = ToBlendOp(blend.Equation),
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                                 ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            };
            var colourBlendState = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colourBlend,
            };
            var depthState = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = depthTest && depthFormat != Format.Undefined,
                DepthWriteEnable = depthWrite && depthTest && depthFormat != Format.Undefined,
                DepthCompareOp = CompareOp.LessOrEqual,
            };
            DynamicState* dynamicStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates,
            };

            var setLayout = _ctx.SetLayout;
            var layoutInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = &setLayout,
            };
            if (_ctx.Vk.CreatePipelineLayout(_ctx.Device, &layoutInfo, null, out var createdLayout) != Result.Success)
                throw new InvalidOperationException("Vulkan pipeline layout creation failed.");
            _layouts.Add(createdLayout);
            pipelineLayout = createdLayout;

            var stages = stackalloc PipelineShaderStageCreateInfo[2];
            stages[0] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = _vertex,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main"),
            };
            stages[1] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = _fragment,
                PName = (byte*)Marshal.StringToHGlobalAnsi("main"),
            };

            Format* colourFormats = stackalloc Format[1] { colorFormat };
            var rendering = new PipelineRenderingCreateInfo
            {
                SType = StructureType.PipelineRenderingCreateInfo,
                ColorAttachmentCount = 1,
                PColorAttachmentFormats = colourFormats,
                DepthAttachmentFormat = depthFormat,
            };

            var info = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = stages,
                PVertexInputState = &vertexInput,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisample,
                PColorBlendState = &colourBlendState,
                PDepthStencilState = &depthState,
                PDynamicState = &dynamicState,
                Layout = createdLayout,
                PNext = &rendering,
            };
            var result = _ctx.Vk.CreateGraphicsPipelines(_ctx.Device, default, 1, &info, null, out var pipeline);
            Marshal.FreeHGlobal((nint)stages[0].PName);
            Marshal.FreeHGlobal((nint)stages[1].PName);
            if (result != Result.Success)
            {
                if (pipeline.Handle != 0) _ctx.Vk.DestroyPipeline(_ctx.Device, pipeline, null);
                _ctx.Vk.DestroyPipelineLayout(_ctx.Device, createdLayout, null);
                _layouts.Remove(createdLayout);
                throw new InvalidOperationException($"Vulkan pipeline creation failed: {result}.");
            }
            _pipelines[key] = pipeline;
            _pipelineLayouts[key] = createdLayout;
            return pipeline;
        }
    }

    private static Format AttributeFormat(VertexAttribute attribute) => (attribute.Type, attribute.ComponentCount, attribute.Normalized) switch
    {
        (VertexAttributeType.Float, 1, _) => Format.R32Sfloat,
        (VertexAttributeType.Float, 2, _) => Format.R32G32Sfloat,
        (VertexAttributeType.Float, 3, _) => Format.R32G32B32Sfloat,
        (VertexAttributeType.Float, 4, _) => Format.R32G32B32A32Sfloat,
        (VertexAttributeType.UnsignedByte, 1, true) => Format.R8Unorm,
        (VertexAttributeType.UnsignedByte, 2, true) => Format.R8G8Unorm,
        (VertexAttributeType.UnsignedByte, 3, true) => Format.R8G8B8Unorm,
        (VertexAttributeType.UnsignedByte, 4, true) => Format.R8G8B8A8Unorm,
        (VertexAttributeType.UnsignedByte, 1, false) => Format.R8Uscaled,
        (VertexAttributeType.UnsignedByte, 2, false) => Format.R8G8Uscaled,
        (VertexAttributeType.UnsignedByte, 3, false) => Format.R8G8B8Uscaled,
        (VertexAttributeType.UnsignedByte, 4, false) => Format.R8G8B8A8Uscaled,
        _ => throw new ArgumentException("Unsupported Vulkan vertex attribute format.", nameof(attribute)),
    };

    private static Silk.NET.Vulkan.BlendFactor ToBlendFactor(THSSS.Graphics.BlendFactor factor) => factor switch
    {
        THSSS.Graphics.BlendFactor.Zero => Silk.NET.Vulkan.BlendFactor.Zero,
        THSSS.Graphics.BlendFactor.One => Silk.NET.Vulkan.BlendFactor.One,
        THSSS.Graphics.BlendFactor.SourceColor => Silk.NET.Vulkan.BlendFactor.SrcColor,
        THSSS.Graphics.BlendFactor.InverseSourceColor => Silk.NET.Vulkan.BlendFactor.OneMinusSrcColor,
        THSSS.Graphics.BlendFactor.SourceAlpha => Silk.NET.Vulkan.BlendFactor.SrcAlpha,
        THSSS.Graphics.BlendFactor.InverseSourceAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha,
        THSSS.Graphics.BlendFactor.DestinationAlpha => Silk.NET.Vulkan.BlendFactor.DstAlpha,
        THSSS.Graphics.BlendFactor.InverseDestinationAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusDstAlpha,
        THSSS.Graphics.BlendFactor.DestinationColor => Silk.NET.Vulkan.BlendFactor.DstColor,
        THSSS.Graphics.BlendFactor.InverseDestinationColor => Silk.NET.Vulkan.BlendFactor.OneMinusDstColor,
        THSSS.Graphics.BlendFactor.SourceAlphaSaturate => Silk.NET.Vulkan.BlendFactor.SrcAlphaSaturate,
        _ => throw new ArgumentOutOfRangeException(nameof(factor)),
    };

    private static BlendOp ToBlendOp(BlendEquation equation) => equation switch
    {
        BlendEquation.Add => BlendOp.Add,
        BlendEquation.Subtract => BlendOp.Subtract,
        BlendEquation.ReverseSubtract => BlendOp.ReverseSubtract,
        BlendEquation.Min => BlendOp.Min,
        BlendEquation.Max => BlendOp.Max,
        _ => throw new ArgumentOutOfRangeException(nameof(equation)),
    };

    public void Dispose()
    {
        if (_disposed) return;
        _ctx.FlushPending();
        _disposed = true;
        foreach (var pipeline in _pipelines.Values) _ctx.Vk.DestroyPipeline(_ctx.Device, pipeline, null);
        foreach (var layout in _layouts) _ctx.Vk.DestroyPipelineLayout(_ctx.Device, layout, null);
        _ctx.Vk.DestroyShaderModule(_ctx.Device, _vertex, null);
        _ctx.Vk.DestroyShaderModule(_ctx.Device, _fragment, null);
    }
}
