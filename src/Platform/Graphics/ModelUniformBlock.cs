namespace THSSS.Graphics;

/// <summary>
/// std140 offsets for the model shader block. The layout is declared once in
/// Device3D's GLSL; these constants mirror that declaration and must move together.
/// Order: projection, colour, fog mode, world, view, 15 flags, light count, then the
/// vec3/vec4 members (each aligning to 16) and three light arrays with a 16-byte stride.
/// </summary>
public static class ModelUniformBlock
{
    public const int Size = 576;

    public const int Projection = 0;
    public const int Colour = 64;
    public const int FogMode = 80;
    public const int World = 96;
    public const int View = 160;

    public const int Lighting = 224;
    public const int HasColour = 228;
    public const int PackedColour = 232;
    public const int HasNormal = 236;
    public const int HasSecondary = 240;
    public const int PackedSecondary = 244;
    public const int ColorVertex = 248;
    public const int NormalizeNormals = 252;
    public const int SpecularEnabled = 256;
    public const int DiffuseSource = 260;
    public const int AmbientSource = 264;
    public const int EmissiveSource = 268;
    public const int SpecularSource = 272;
    public const int LocalViewer = 276;
    public const int LightCount = 280;

    public const int AmbientLight = 288;
    public const int Specular = 304;
    public const int Power = 316;
    public const int FogColour = 320;
    public const int FogParams = 336;
    public const int MaterialAmbient = 352;
    public const int MaterialEmissive = 368;

    public const int LightDirection = 384;
    public const int LightDiffuse = 448;
    public const int LightSpecular = 512;

    /// <summary>std140 array elements stride one vec4 per light.</summary>
    public const int LightStride = 16;

    /// <summary>Lights the block can carry; more are dropped, matching the GLSL array.</summary>
    public const int MaxLights = 4;
}
