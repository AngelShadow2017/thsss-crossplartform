float X;
float Y;
float Time;
int ColorKey;
float Radius;

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

float4 PixelShaderFunction(float2 tex : TEXCOORD0) : COLOR0
{
   float x = X + 20 * cos(Time) / 640;
   float y = Y + 30 * sin(1.5*Time) / 640;
 
   float4 Color;
   float deltaX = tex.x - x;
   float deltaY = (tex.y - y) * 3 / 4;
   float dist2 = deltaX * deltaX + deltaY * deltaY;
   float R = Radius;
   float destX, destY;
   destX = tex.x;
   destY = tex.y;

   if( dist2 < R * R)
   {
      float k = 0.3*(acos(sqrt(dist2) / R-1)-acos(0));
      destX -= deltaX * k;
      destY -= deltaY *k/3*4;
   }
         
   x = X + 20 * cos(1.5*Time) / 640;
   y = Y + 30 * sin(1.0*Time) / 640;
   
   deltaX = tex.x - x;
   deltaY = (tex.y - y) * 3 / 4;
   dist2 = deltaX * deltaX + deltaY * deltaY;
   if( dist2 < R * R)
   {
      float k = 0.3*(acos(sqrt(dist2) / R-1)-acos(0));
      destX -= deltaX * k;
      destY -= deltaY *k/3*4;
   }
   
   x = X;
   y = Y;
   
   deltaX = tex.x - x;
   deltaY = (tex.y - y) * 3 / 4;
   dist2 = deltaX * deltaX + deltaY * deltaY;
   if( dist2 < R * R)
   {
      float k = 0.25*(acos(sqrt(dist2) / R-1)-acos(0));
      destX -= deltaX * k;
      destY -= deltaY *k/3*4;
   }
   
   Color = tex2D(ColoredTextureSampler, float2(destX, destY));
   
   R+=0.02;
   if( dist2 < R * R)
   {
      //Çà
      if(ColorKey == 0)
      {
         Color.r -= 10*(R*R-dist2);
      }
      //×Ï
      else if(ColorKey == 1)
      {
         Color.g -= 10*(R*R-dist2);
      }
      //»Æ
      else if(ColorKey == 2)
      {
         Color.b -= 10*(R*R-dist2);
      }
      //À¶
      else if(ColorKey == 3)
      {
         Color.r -= 10*(R*R-dist2);
         Color.g -= 10*(R*R-dist2);
      }
      //ÂÌ
      else if(ColorKey == 4)
      {
         Color.r -= 10*(R*R-dist2);
         Color.b -= 10*(R*R-dist2);
      }
      //ºì
      else if(ColorKey == 5)
      {
         Color.b -= 10*(R*R-dist2);
         Color.g -= 10*(R*R-dist2);
      }
   }   
   
   return Color;
}

technique technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_b PixelShaderFunction();
	}
}