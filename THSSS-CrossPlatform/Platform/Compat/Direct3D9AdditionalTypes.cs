// Additional DirectX 9 types for cross-platform compatibility
using System;
using System.Runtime.InteropServices;

namespace SlimDX.Direct3D9
{
    // Color is System.Drawing.Color (provided by the framework)

    // Blend states
    public enum Blend
    {
        Zero = 0,
        One = 1,
        SourceColor = 2,
        InvSourceColor = 3,
        SourceAlpha = 4,
        InvSourceAlpha = 5,
        DestAlpha = 6,
        InvDestAlpha = 7,
        DestColor = 8,
        InvDestColor = 9,
        SourceAlphaSaturate = 10,
        InverseSourceAlpha = 5,
        InverseSourceColor = 3,
        InverseDestinationAlpha = 7,
        InverseDestinationColor = 9,
        DestinationAlpha = 6,
        DestinationColor = 8,
        BlendFactor = 27,
        InverseBlendFactor = 28,
        BotSourceAlpha = 11,
        BotDestinationAlpha = 12,
        BotSourceColor = 13,
        BotDestinationColor = 14,
        BotInverseSourceAlpha = 15,
        BotInverseDestinationAlpha = 16,
        BotInverseSourceColor = 17,
        BotInverseDestinationColor = 18,
        BothSourceAlpha = 19,
        BothDestinationAlpha = 20,
        BothInverseSourceAlpha = 21,
        BothInverseDestinationAlpha = 22,
        BothSourceColor = 23,
        BothDestinationColor = 24,
        BothInverseSourceColor = 25,
        BothInverseDestinationColor = 26,
        FactorColor = 27,
        InvFactorColor = 28,
        FactorAlpha = 29,
        InvFactorAlpha = 30,
    }

    // Blend operations
    public enum BlendOperation
    {
        Add = 1,
        Subtract = 2,
        RevSubtract = 3,
        Min = 4,
        Max = 5,
        ReverseSubtract = 6,
    }

    // Texture address modes
    public enum TextureAddress
    {
        Wrap = 1,
        Mirror = 2,
        Clamp = 3,
        Border = 4,
        MirrorOnce = 5,
    }

    // Compare functions
    public enum Compare
    {
        Never = 1,
        Less = 2,
        Equal = 3,
        LessEqual = 4,
        Greater = 5,
        NotEqual = 6,
        GreaterEqual = 7,
        Always = 8,
    }

    // Fill modes
    public enum FillMode
    {
        Point = 1,
        Wireframe = 2,
        Solid = 3,
    }

    // Shade modes
    public enum ShadeMode
    {
        Flat = 1,
        Gouraud = 2,
        Phong = 3,
    }

    // Cull modes
    public enum Cull
    {
        None = 1,
        Clockwise = 2,
        Counterclockwise = 3,
    }

    // Transform types
    public enum TransformType
    {
        None = 0,
        World = 1,
        View = 2,
        Projection = 3,
        Texture0 = 4,
        Texture1 = 5,
        Texture2 = 6,
        Texture3 = 7,
        World1 = 8,
        World2 = 9,
        World3 = 10,
    }

    // Light types
    public enum LightType
    {
        Point = 1,
        Spot = 2,
        Directional = 3,
    }

    // Primitive types
    public enum PrimitiveType
    {
        PointList = 1,
        LineList = 2,
        LineStrip = 3,
        TriangleList = 4,
        TriangleStrip = 5,
        TriangleFan = 6,
        Triangles = 4,
        Quads = 7,
    }

    // Render states
    public enum RenderState
    {
        ShadeMode = 2,
        FillMode = 3,
        DitherEnable = 4,
        SpecularEnable = 5,
        FogEnable = 28,
        ZWriteEnable = 14,
        AlphaTestEnable = 15,
        SrcBlend = 19,
        DestBlend = 20,
        CullMode = 22,
        ZFunc = 23,
        AlphaFunc = 24,
        AlphaBlendEnable = 27,
        FogTableMode = 28,
        FogStart = 36,
        FogEnd = 37,
        FogDensity = 38,
        FogVertexMode = 39,
        Lighting = 137,
        ColorVertex = 141,
        NormalizeNormals = 142,
        FogColor = 34,
        BlendOperation = 171,
        ScissorTestEnable = 174,
        ColorWriteEnable = 168,
        SrcBlendAlpha = 200,
        DestBlendAlpha = 201,
        SeparateAlphaBlendEnable = 202,
        // SlimDX names
        SourceBlend = 19,
        DestinationBlend = 20,
        ZEnable = 7,
        MultisampleAntialias = 161,
        FogMode = 28,
        TextureFactor = 60,
        Ambient = 139,
        AmbientMaterialSource = 140,
        DiffuseMaterialSource = 145,
        SpecularMaterialSource = 146,
        EmissiveMaterialSource = 147,
        IndexedVertexBlendEnable = 226,
        VertexBlend = 151,
        PointSize = 154,
        PointScaleEnable = 156,
        PointSpriteEnable = 156,
        PointScaleA = 155,
        PointScaleB = 156,
        PointScaleC = 157,
        PatchEdgeStyle = 0,
        AntialiasedLineEnable = 176,
        WrapU = 128,
        WrapV = 129,
        WrapW = 130,
        BorderColor = 143,
        FogVertexMode2 = 39,
        LocalViewer = 138,
        RangeFogEnable = 40,
        SlopeScaleDepthBias = 175,
        StencilEnable = 52,
        StencilFail = 53,
        StencilFunc = 54,
        StencilMask = 55,
        StencilPass = 56,
        StencilRef = 57,
        StencilWriteMask = 58,
        StencilZFail = 59,
        TwoSidedStencilMode = 185,
        ColorWriteEnable1 = 168,
        ColorWriteEnable2 = 169,
        ColorWriteEnable3 = 170,
        BlendFactor = 193,
        SrgbWriteEnable = 194,
        DepthBias = 195,
        ScissorLeft = 0,
        MultipassAntialias = 0,
        ClipPlaneEnable = 144,
        PointScale = 0,
        AlphaRef = 24,
        FogTableMode2 = 28,
    }

    // Sampler states
    public enum SamplerState
    {
        AddressU = 1,
        AddressV = 2,
        AddressW = 3,
        BorderColor = 4,
        MagFilter = 5,
        MinFilter = 6,
        MipFilter = 7,
        MipMapLodBias = 8,
        MaxMipLevel = 9,
        MaxAnisotropy = 10,
        SrgbTexture = 11,
    }

    // Texture filter types
    public enum TextureFilter
    {
        None = 0,
        Point = 1,
        Linear = 2,
        Anisotropic = 3,
        FlatCubic = 4,
        GaussianCubic = 5,
    }

    // Texture stage states
    public enum TextureStage
    {
        ColorOperation = 1,
        ColorArgument1 = 2,
        ColorArgument2 = 3,
        AlphaOperation = 4,
        AlphaArgument1 = 5,
        AlphaArgument2 = 6,
        TexCoordIndex = 11,
        TextureTransform = 12,
        TextureTransformFlags = 13,
        ColorOp = 1,
        ColorArg1 = 2,
        ColorArg2 = 3,
        AlphaOp = 4,
        AlphaArg1 = 5,
        AlphaArg2 = 6,
    }

    // Texture operation types
    public enum TextureOperation
    {
        Disable = 1,
        SelectArg1 = 2,
        SelectArg2 = 3,
        Modulate = 4,
        Modulate2X = 5,
        Modulate4X = 6,
        Add = 7,
        AddSigned = 8,
        AddSigned2X = 9,
        Subtract = 10,
        AddSmooth = 11,
        BlendDiffuseAlpha = 12,
        BlendTextureAlpha = 13,
        BlendFactorAlpha = 14,
        BlendTextureAlphaPM = 15,
        BlendCurrentAlpha = 16,
        PreModulate = 17,
        ModulateAlphaAddColor = 18,
        ModulateColorAddAlpha = 19,
        ModulateInvAlphaAddColor = 20,
        ModulateInvColorAddAlpha = 21,
        BumpEnvMap0 = 22,
        BumpEnvMap1 = 23,
        BumpEnvMap2 = 24,
        BumpEnvMap3 = 25,
        BumpEnvMap4 = 26,
        BumpEnvMap5 = 27,
        BumpEnvMap6 = 28,
        BumpEnvMap7 = 29,
    }

    // Texture argument constants
    public enum TextureArgument
    {
        Current = 0,
        Diffuse = 1,
        Texture = 2,
        TFactor = 3,
        Specular = 4,
        Temp = 5,
        Constant = 6,
    }

    // Vertex formats
    [Flags]
    public enum VertexFormat
    {
        None = 0,
        Position = 0x002,
        Normal = 0x010,
        Diffuse = 0x040,
        Specular = 0x080,
        Texture0 = 0x000,
        Texture1 = 0x100,
        Texture2 = 0x200,
        Texture3 = 0x300,
        Texture4 = 0x400,
        Texture5 = 0x500,
        Texture6 = 0x600,
        Texture7 = 0x700,
        Texture8 = 0x800,
        Xyz = 0x002,
        XyzRhw = 0x004,
        XyzB1 = 0x006,
        XyzB2 = 0x008,
    }

    // Declaration types
    public enum DeclarationType
    {
        Float1 = 0,
        Float2 = 1,
        Float3 = 2,
        Float4 = 3,
        D3dcolor = 4,
        Ubyte4 = 5,
        Short2 = 6,
        Short4 = 7,
        Ubyte4N = 8,
        Short2N = 9,
        Short4N = 10,
        Ushort2N = 11,
        Ushort4N = 12,
        Udec3 = 13,
        Dec3N = 14,
        Float16_2 = 15,
        Float16_4 = 16,
        Unused = 17,
        Color = 4,
        UByte4 = 5,
    }

    // Declaration usage
    public enum DeclarationUsage
    {
        Position = 0,
        BlendWeight = 1,
        BlendIndices = 2,
        Normal = 3,
        PSize = 4,
        TexCoord = 5,
        TextureCoordinate = 5,
        Tangent = 6,
        Binormal = 7,
        TessellateFactor = 8,
        PositionT = 9,
        Color = 10,
        Fog = 11,
        Depth = 12,
        Sample = 13,
    }

    // Vertex element
    public struct VertexElement
    {
        public short Stream;
        public short Offset;
        public DeclarationType Type;
        public DeclarationMethod Method;
        public DeclarationUsage Usage;
        public byte UsageIndex;
        public int FormatSize;
        public DeclarationType FormatType;
        public DeclarationUsage FormatUsage;
        public int UsageIndex2;

        public VertexElement(short stream, short offset, DeclarationType type,
                             DeclarationMethod method, DeclarationUsage usage, byte usageIndex)
        {
            Stream = stream; Offset = offset; Type = type;
            Method = method; Usage = usage; UsageIndex = usageIndex;
            FormatSize = 0; FormatType = type; FormatUsage = usage; UsageIndex2 = usageIndex;
        }

        public static VertexElement VertexDeclarationEnd = new VertexElement
        {
            Stream = 0xFF,
            Offset = 0,
            Type = DeclarationType.Unused,
            Method = DeclarationMethod.Default,
            Usage = DeclarationUsage.Position,
            UsageIndex = 0
        };
    }

    // Declaration method
    public enum DeclarationMethod
    {
        Default = 0,
        PartialU = 1,
        PartialV = 2,
        CrossUV = 3,
        UV = 4,
        Lookup = 5,
        LookupPresampled = 6,
    }

    // Enable capabilities
    public enum EnableCap
    {
        Lighting = 0,
        NormalizeNormals = 1,
        CullFace = 2,
        Texture2D = 3,
        DepthTest = 4,
        Blend = 5,
        AlphaTest = 6,
        ScissorTest = 7,
        StencilTest = 8,
        Fog = 9,
        ColorMaterial = 10,
    }

    // Texture stage operation
    public enum TextureStageOperation
    {
        Disable = 1,
        SelectArg1 = 2,
        SelectArg2 = 3,
        Modulate = 4,
        Add = 5,
    }

    // NOTE: Color/Color4/VertexElement/DeclarationMethod live elsewhere
    // (System.Drawing.Color in AdditionalTypes.cs, SlimDX.Color4 in AdditionalTypes.cs,
    //  VertexElement/DeclarationMethod in Verbuffer.cs). Do NOT redefine.

    // Result is in SlimDX namespace (AdditionalTypes.cs)

    // Description box
    public static class DescriptionBox
    {
        public static void Show(string text) { }
        public static void Show(string text, string caption) { }
        public static void Hide() { }
    }

    // Image file format
    public enum ImageFileFormat
    {
        Bmp = 0,
        Jpg = 1,
        Tga = 2,
        Png = 3,
        Dds = 4,
        Ppm = 5,
        Dib = 6,
        Hdr = 7,
        Dpf = 8,
    }

    // Present flags
    public enum Present
    {
        None = 0,
        Test = 1,
        DoNotFlip = 2,
        DoNotWait = 4,
    }

    [Flags]
    public enum PresentFlags
    {
        None = 0,
        LockableBackBuffer = 1,
        DiscardDepthStencil = 2,
        DeviceClip = 4,
        Video = 16,
    }

    // Device capabilities
    public struct Capabilities
    {
        public int MaxTextureWidth;
        public int MaxTextureHeight;
        public int MaxTextureRepeat;
        public int MaxTextureAspectRatio;
        public int MaxAnisotropy;
        public int MaxVertexW;
        public float GuardBandLeft;
        public float GuardBandTop;
        public float GuardBandRight;
        public float GuardBandBottom;
        public float ExtentsAdjust;
        public int StencilCaps;
        public int TextureCaps;
        public int TextureFilterCaps;
        public int CubeTextureFilterCaps;
        public int VolumeTextureFilterCaps;
        public int MaxTextureBlendStages;
        public int MaxSimultaneousTextures;
        public int VertexProcessingCaps;
        public int MaxActiveLights;
        public int MaxUserClipPlanes;
        public int MaxVertexBlendMatrices;
        public int MaxVertexBlendMatrixIndex;
        public float MaxPointSize;
        public int MaxPrimitiveCount;
        public int MaxVertexIndex;
        public int MaxStreams;
        public int MaxStreamStride;
        public int VertexShaderVersion;
        public int MaxVertexShaderConst;
        public int PixelShaderVersion;
        public float MaxPixelShaderValue;
        public DeviceCaps DeviceCaps;
    }

    // Device type
    public enum DeviceType
    {
        Null = 0,
        Hardware = 1,
        Reference = 2,
        Software = 3,
    }

    // Result code
    public static class ResultCode
    {
        public static Result Success = new Result(0);
        public static Result Failure = new Result(-1);
        public static Result DeviceLost = new Result(-2);
        public static Result DeviceNotReset = new Result(-3);
        public static Result NotAvailable = new Result(-4);
        public static Result InvalidCall = new Result(-5);
        public static Result OutOfMemory = new Result(-6);
    }

    // Create flags
    [Flags]
    public enum CreateFlags
    {
        None = 0,
        FpuPreserve = 1,
        Multithreaded = 2,
        PureDevice = 4,
        SoftwareVertexProcessing = 8,
        HardwareVertexProcessing = 16,
        MixedVertexProcessing = 32,
        DisableDriverManagement = 64,
        AdapterGroupDevice = 128,
    }

    // Device caps
    [Flags]
    public enum DeviceCaps
    {
        None = 0,
        ExecuteSystemMemory = 1,
        ExecuteVideoMemory = 2,
        TlVertexSystemMemory = 4,
        TlVertexVideoMemory = 8,
        TextureSystemMemory = 16,
        TextureVideoMemory = 32,
        DrawPrimitives2 = 64,
        CanBltSystemToVideoMemory = 128,
        CanBltVideoToSystemMemory = 256,
        CanBltVideoToVideoMemory = 512,
        HWTransformAndLight = 16,
    }

    // Surface description
    public struct SurfaceDescription
    {
        public int Width;
        public int Height;
        public Format Format;
        public int MultiSampleType;
        public int MultiSampleQuality;
        public Usage Usage;
        public Pool Pool;
        public int Level;
    }

    // Material
    public struct Material
    {
        public Color4 Diffuse;
        public Color4 Ambient;
        public Color4 Specular;
        public Color4 Emissive;
        public float Power;
    }

    // Text drawing / brush helpers
    [Flags]
    public enum DrawTextFormat
    {
        Center = 0x00000001,
        Left = 0x00000002,
        Right = 0x00000004,
        Top = 0x00000008,
        Bottom = 0x00000010,
        VCenter = 0x00000020,
        WordBreak = 0x00000040,
        SingleLine = 0x00000080,
        NoClip = 0x00000100,
    }

    // Brushes moved to System.Drawing.Brushes

    // Cooperative level
    [Flags]
    public enum CooperativeLevel
    {
        Normal = 0,
        Exclusive = 1,
        Fullscreen = 2,
        SetFocus = 4,
        AllowReboot = 8,
        NoWinChanges = 16,
        NoChanges = 32,
    }

    // Cull face mode
    public enum CullFaceMode
    {
        Back = 0,
        Front = 1,
        FrontAndBack = 2,
    }

    // Material face
    public enum MaterialFace
    {
        Front = 0,
        Back = 1,
        FrontAndBack = 2,
    }

    // Object device type
    public enum ObjectDeviceType
    {
        All = 0,
        Button = 1,
        Key = 2,
        Axis = 3,
        Slider = 4,
        POV = 5,
    }

    // Texture filter type
    public enum TextureFilterType
    {
        None = 0,
        Point = 1,
        Linear = 2,
        Anisotropic = 3,
        FlatCubic = 4,
        GaussianCubic = 5,
    }

    // Mesh
    public class Mesh : IDisposable
    {
        public VertexFormat VertexFormat { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }
        public bool IsDisposed => false;
        public VertexBuffer VertexBuffer => new VertexBuffer((Device)null, 0, Usage.None, VertexFormat.None, Pool.Default);
        public IndexBuffer IndexBuffer => new IndexBuffer((Device)null, 0, Usage.None, Pool.Default);
        public void Dispose() { }
        public void DrawSubset(int i) { }
        public static Mesh FromFile(Device device, string filename, out ExtendedMaterial[] materials) { materials = new ExtendedMaterial[0]; return new Mesh(); }
        public static Mesh FromFile(Device device, string filename, MeshFlags flags) => new Mesh();
        public static Mesh FromFile(Device device, string filename, MeshFlags flags, Device adjacency) => new Mesh();
        public ExtendedMaterial[] GetMaterials() => new ExtendedMaterial[0];
        public void ComputeNormals() { }
        public Mesh Clone(Device device, MeshFlags flags, VertexElement[] declaration) => new Mesh();
        public Mesh Clone(Device device, MeshFlags flags, VertexFormat format) => new Mesh();
    }

    // Extended material
    public struct ExtendedMaterial
    {
        public Material Material;
        public string TextureFilename;
        public string TextureFileName { get => TextureFilename; set => TextureFilename = value; }
        public Material MaterialD3D { get => Material; set => Material = value; }
    }
}

// Format enum
namespace SlimDX.Direct3D9
{
    public enum Format
    {
        Unknown = 0,
        A8R8G8B8 = 21,
        X8R8G8B8 = 22,
        R5G6B5 = 23,
        X1R5G5B5 = 24,
        A1R5G5B5 = 25,
        A4R4G4B4 = 26,
        A8 = 28,
        R8G8B8 = 20,
        A2R10G10B10 = 31,
        D16 = 80,
        D24S8 = 75,
        D32 = 71,
        Index16 = 101,
        Index32 = 102,
    }
}

// Usage flags
namespace SlimDX.Direct3D9
{
    [Flags]
    public enum Usage
    {
        None = 0,
        RenderTarget = 1,
        DepthStencil = 2,
        Dynamic = 512,
        WriteOnly = 8,
        SoftwareProcessing = 16,
        DoNotClip = 32,
        Points = 64,
        RtPatches = 128,
        Npatches = 256,
    }
}

// Pool
namespace SlimDX.Direct3D9
{
    public enum Pool
    {
        Default = 0,
        Managed = 1,
        SystemMemory = 2,
        Scratch = 3,
    }
}

// Additional SlimDX enums used by the game
namespace SlimDX.Direct3D9
{
    public enum TransformState
    {
        World = 0,
        View = 1,
        Projection = 2,
        Texture = 4,
        World1 = 8,
        World2 = 9,
        World3 = 10,
    }

    public enum PresentInterval
    {
        Default = 0,
        Immediate = 1,
        One = 2,
        Two = 3,
        Three = 4,
        Four = 5,
    }

    public enum FogMode
    {
        None = 0,
        Exponential = 1,
        ExponentialSquared = 2,
        Linear = 3,
    }





    public enum ImageFileFormatEx
    {
        Bmp = 0,
        Jpg = 1,
        Tga = 2,
        Png = 3,
        Dds = 4,
    }
}

namespace SlimDX.Direct3D9
{
    public enum ImageLayout
    {
        None = 0,
        Tile = 1,
        Center = 2,
        Stretch = 3,
        Zoom = 4,
    }
}

namespace SlimDX.Direct3D9
{
    public class AdapterCollection
    {
        public AdapterInformation DefaultAdapter => new AdapterInformation();
        public AdapterInformation this[int index] => new AdapterInformation();
        public int Count => 1;
    }

    public class Direct3D : IDisposable
    {
        public AdapterCollection Adapters { get; } = new AdapterCollection();
        public Capabilities GetDeviceCaps(int adapter, DeviceType deviceType) => new Capabilities();
        public void Dispose() { }
    }

    public class Direct3D9Exception : Exception
    {
        public SlimDX.Result ResultCode { get; set; }
        public Direct3D9Exception(string message) : base(message) { }
        public Direct3D9Exception() { }
    }

    [Flags]
    public enum MeshFlags
    {
        None = 0,
        Dynamic = 1,
        Managed = 2,
        SystemMemory = 4,
        Use32Bit = 32,
    }

    public enum ObjectDeviceTypeEx
    {
        All = 0,
    }
}

namespace SlimDX.Direct3D9
{
    public enum MultisampleType
    {
        None = 0,
        NonMaskable = 1,
        TwoSamples = 2,
        ThreeSamples = 3,
        FourSamples = 4,
    }
}
