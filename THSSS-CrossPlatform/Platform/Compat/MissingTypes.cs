// Missing types for cross-platform compatibility
// NOTE: Effect, Sprite, Texture, VertexBuffer, Surface, Mesh, Image, Font, Device, SourceVoice,
//       WaveFormat, FilterParameters, FilterType, WaveFormatExtensible, Speakers are defined
//       in their own dedicated files. Do NOT redefine them here.
using System;
using System.Collections.Generic;

namespace SlimDX.Direct3D9
{
    // Data rectangle
    public struct DataRectangle
    {
        public IntPtr DataPointer;
        public int Pitch;
        public DataStream Data;
    }

    // Lock flags
    [Flags]
    public enum LockFlags
    {
        None = 0,
        Discard = 1,
        NoOverwrite = 2,
        ReadOnly = 4,
        NoDirtyUpdate = 8,
    }

    // Index buffer
    public class IndexBuffer : IDisposable
    {
        public IndexBuffer(Device device, int size, Usage usage, Pool pool) { }
        public void Dispose() { }
        public DataStream Lock(int offset, int size, LockFlags flags) => new DataStream(size, true, true);
        public void Unlock() { }
    }

    // Lighting state used by the 3D background/environment helpers.
    public struct Light
    {
        public LightType Type;
        public System.Drawing.Color Diffuse;
        public System.Drawing.Color Ambient;
        public System.Drawing.Color Specular;
        public Vector3 Direction;
        public Vector3 Position;
    }

    /// <summary>D3DCLEAR_* bits.</summary>
    [Flags]
    public enum ClearFlags
    {
        None = 0,
        Target = 1,
        ZBuffer = 2,
        Stencil = 4,
    }

    public class VertexShader : IDisposable
    {
        public void Dispose() { }
    }

    public class PixelShader : IDisposable
    {
        public void Dispose() { }
    }

    // Handle
    public struct Handle
    {
        public IntPtr Value;
    }

    // Sprite flags
    [Flags]
    public enum SpriteFlags
    {
        None = 0,
        AlphaBlend = 1,
        SortTexture = 2,
        SortDepthBackToFront = 4,
        SortDepthFrontToBack = 8,
        DoNotSaveState = 16,
        DoNotModifyRenderState = 32,
    }

    // Font enums
    public enum FontWeight { Normal = 0, Bold = 700 }
    public enum CharacterSet { Default = 1, Ansi = 0 }
    public enum Precision { Default = 0 }
    public enum FontQuality { Default = 0, Antialiased = 4 }
    public enum FontPitch { Default = 0 }

    // Present parameters
    public struct PresentParameters
    {
        public int BackBufferWidth;
        public int BackBufferHeight;
        public Format BackBufferFormat;
        public int BackBufferCount;
        public MultiSampleType MultiSampleType;
        public int MultiSampleQuality;
        public SwapEffect SwapEffect;
        public IntPtr DeviceWindowHandle;
        public bool Windowed;
        public bool EnableAutoDepthStencil;
        public Format AutoDepthStencilFormat;
        public PresentFlags PresentFlags;
        public int FullScreenRefreshRateInHz;
        public PresentInterval PresentationInterval;
        public int PresentIntervalValue;
    }

    // Swap effect
    public enum SwapEffect
    {
        Discard = 1,
        Flip = 2,
        Copy = 3,
    }

    // Multi-sample type
    public enum MultiSampleType
    {
        None = 0,
        NonMaskable = 1,
        TwoSamples = 2,
        ThreeSamples = 3,
        FourSamples = 4,
    }

    // AdapterInformation is in Direct3D9AdditionalTypes.cs

    // Adapter information
    public struct AdapterInformation
    {
        public int Adapter;
        public string Description;
        public string DriverName;
        public Guid DriverGuid;
        public DisplayMode CurrentDisplayMode;
    }

    // Display mode
    public struct DisplayMode
    {
        public int Width;
        public int Height;
        public int RefreshRate;
        public Format Format;
    }

    // Manager
    public static class Manager
    {
        public static int AdapterCount => 1;
        public static AdapterInformation GetAdapterInformation(int adapter) => new AdapterInformation();
        public static DisplayMode[] GetAdapterDisplayMode(int adapter) => new DisplayMode[] { new DisplayMode() };
        public static Capabilities GetDeviceCapabilities(int adapter, DeviceType deviceType) => new Capabilities();
        public static bool CheckDeviceFormat(int adapter, DeviceType deviceType, Format adapterFormat, Usage usage, ResourceType resourceType, Format checkFormat) => true;
        public static bool CheckDeviceType(int adapter, DeviceType deviceType, Format adapterFormat, Format backBufferFormat, bool windowed) => true;
    }

    // Resource type
    public enum ResourceType
    {
        Texture = 0,
        VolumeTexture = 1,
        CubeTexture = 2,
        VertexBuffer = 3,
        IndexBuffer = 4,
    }

    // State block
    public class StateBlock : IDisposable
    {
        public StateBlock(Device device, StateBlockType type) { }
        public void Capture() { }
        public void Apply() { }
        public void Dispose() { }
    }

    public enum StateBlockType
    {
        All = 1,
        PixelState = 2,
        VertexState = 3,
    }

}

namespace SlimDX.Direct3D9
{
    /// <summary>Direct3D font (cross-platform stub backed by no-ops).</summary>
    public class Font : IDisposable
    {
        public Font(Device device, System.Drawing.Font font) { }
        public Font(Device device, int height, int width, FontWeight weight, int mipLevels,
                    bool italic, CharacterSet charSet, Precision precision,
                    FontQuality quality, FontPitch pitch, string faceName) { }
        public System.Drawing.Size MeasureString(string text) => System.Drawing.Size.Empty;
        public System.Drawing.Size MeasureString(string text, int width, int height) => System.Drawing.Size.Empty;
        public System.Drawing.Size MeasureString(string text, DrawTextFormat flags) => System.Drawing.Size.Empty;
        public System.Drawing.Size MeasureString(Sprite sprite, string text, DrawTextFormat flags) => System.Drawing.Size.Empty;
        public System.Drawing.Size MeasureString(Sprite sprite, string text, DrawTextFormat flags, int width) => System.Drawing.Size.Empty;
        public void DrawText(Sprite sprite, string text, System.Drawing.Rectangle rect,
                             DrawTextFormat flags, System.Drawing.Color color) { }
        public void DrawText(Sprite sprite, string text, int x, int y,
                             System.Drawing.Color color) { }
        public void DrawString(Sprite sprite, string text, int x, int y, System.Drawing.Color color) { }
        public void DrawString(Sprite sprite, string text, int x, int y, SlimDX.Direct3D9.Color4 color) { }
        public void Dispose() { }
    }

    /// <summary>Vertex element descriptor (SlimDX-compatible).</summary>
    public struct VertexElementDx
    {
    }
}
