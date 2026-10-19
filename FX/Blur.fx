float BlurLevel;
Texture xColoredTexture;

sampler ColoredTextureSampler = sampler_state
{ 
	texture = <xColoredTexture>;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR;
	AddressU = mirror;
 	AddressV = mirror;
};

struct VertexOut
{
	float2 textureCoordinates : TEXCOORD0;
};

float4 PixelShaderFunction(VertexOut input) : COLOR0
{
	float4 Color;
	Color = tex2D(ColoredTextureSampler, input.textureCoordinates.xy);
	Color += tex2D(ColoredTextureSampler, input.textureCoordinates.xy + BlurLevel);
	Color += tex2D(ColoredTextureSampler, input.textureCoordinates.xy - BlurLevel);
	Color = Color / 3;
	Color.r -= BlurLevel*50;
	Color.g -= BlurLevel*50;
	Color.b -= BlurLevel*50;
	return Color;
}

technique technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}