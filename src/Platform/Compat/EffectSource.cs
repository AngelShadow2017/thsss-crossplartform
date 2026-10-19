using System;
using System.Text.RegularExpressions;

namespace SlimDX.Direct3D9;

internal enum EffectKind { Filter, Blur, Warp }

internal static class EffectSource
{
    private const string Sampler = @"Texture xColoredTexture;
        sampler ColoredTextureSampler = sampler_state {
        texture = <xColoredTexture>; magfilter = LINEAR; minfilter = LINEAR;
        mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };";
    private const string Vertex = @"struct VertexOut { float2 textureCoordinates : TEXCOORD0; };";
    private const string Filter = @"float4 PixelShaderFunction(VertexOut input) : COLOR0 {
        float4 Color; Color = tex2D(ColoredTextureSampler, input.textureCoordinates.xy); return Color; }";
    private const string Blur = @"float4 PixelShaderFunction(VertexOut input) : COLOR0 {
        float4 Color; Color = tex2D(ColoredTextureSampler, input.textureCoordinates.xy);
        Color += tex2D(ColoredTextureSampler, input.textureCoordinates.xy + BlurLevel);
        Color += tex2D(ColoredTextureSampler, input.textureCoordinates.xy - BlurLevel);
        Color = Color / 3; Color.r -= BlurLevel*50; Color.g -= BlurLevel*50;
        Color.b -= BlurLevel*50; return Color; }";
    private const string Warp = @"float4 PixelShaderFunction(float2 tex : TEXCOORD0) : COLOR0 {
        float x = X + 20 * cos(Time) / 640;
        float y = Y + 30 * sin(1.5*Time) / 640;
        float4 Color; float deltaX = tex.x - x; float deltaY = (tex.y - y) * 3 / 4;
        float dist2 = deltaX * deltaX + deltaY * deltaY; float R = Radius;
        float destX, destY; destX = tex.x; destY = tex.y;
        if( dist2 < R * R) {
        float k = 0.3*(acos(sqrt(dist2) / R-1)-acos(0));
        destX -= deltaX * k; destY -= deltaY *k/3*4; }
        x = X + 20 * cos(1.5*Time) / 640;
        y = Y + 30 * sin(1.0*Time) / 640;
        deltaX = tex.x - x; deltaY = (tex.y - y) * 3 / 4;
        dist2 = deltaX * deltaX + deltaY * deltaY;
        if( dist2 < R * R) {
        float k = 0.3*(acos(sqrt(dist2) / R-1)-acos(0));
        destX -= deltaX * k; destY -= deltaY *k/3*4; }
        x = X; y = Y;
        deltaX = tex.x - x; deltaY = (tex.y - y) * 3 / 4;
        dist2 = deltaX * deltaX + deltaY * deltaY;
        if( dist2 < R * R) {
        float k = 0.25*(acos(sqrt(dist2) / R-1)-acos(0));
        destX -= deltaX * k; destY -= deltaY *k/3*4; }
        Color = tex2D(ColoredTextureSampler, float2(destX, destY));
        R+=0.02;
        if( dist2 < R * R) {
        if(ColorKey == 0) { Color.r -= 10*(R*R-dist2); }
        else if(ColorKey == 1) { Color.g -= 10*(R*R-dist2); }
        else if(ColorKey == 2) { Color.b -= 10*(R*R-dist2); }
        else if(ColorKey == 3) { Color.r -= 10*(R*R-dist2); Color.g -= 10*(R*R-dist2); }
        else if(ColorKey == 4) { Color.r -= 10*(R*R-dist2); Color.b -= 10*(R*R-dist2); }
        else if(ColorKey == 5) { Color.b -= 10*(R*R-dist2); Color.g -= 10*(R*R-dist2); }
        } return Color; }";

    internal static EffectKind Parse(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Effect source is empty.", nameof(source));
        string normalized = Normalize(source);
        foreach (EffectKind kind in Enum.GetValues<EffectKind>())
        {
            string body = kind switch {
                EffectKind.Blur => "float BlurLevel;" + Sampler + Vertex + Blur,
                EffectKind.Warp => "float X;float Y;float Time;int ColorKey;float Radius;" + Sampler + Warp,
                _ => Sampler + Vertex + Filter
            };
            string profile = kind == EffectKind.Warp ? "ps_2_b" : "ps_2_0";
            string technique = "technique technique1 { pass Pass0 { PixelShader = compile " + profile + " PixelShaderFunction(); } }";
            if (normalized == Normalize(body + technique)) return kind;
        }
        throw new NotSupportedException("Unsupported or invalid FX source. Only the shipped Blur, Filter and Warp algorithms are supported, not arbitrary HLSL.");
    }

    private static string Normalize(string source)
    {
        string uncommented = Regex.Replace(source, @"/\*.*?\*/|//[^\r\n]*", "", RegexOptions.Singleline);
        return Regex.Replace(uncommented, @"\s+", "").Trim('\uFEFF');
    }
}
