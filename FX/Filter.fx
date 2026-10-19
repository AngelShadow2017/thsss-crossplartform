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
	return Color;
}

technique technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}