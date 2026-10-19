using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SlimDX;
using SlimDX.Direct3D9;
using Texture = SlimDX.Direct3D9.Texture;

var options = WindowOptions.Default;
options.Size = new Silk.NET.Maths.Vector2D<int>(64, 64);
options.IsVisible = false;
using var window = Window.Create(options);
window.Initialize();
using var gl = window.CreateOpenGL();
var device = new Device(gl, 64, 64) { GameWidth = 64, GameHeight = 64 };
using var image = new Image<Rgba32>(2, 2);
image[0, 0] = new Rgba32(255, 0, 0, 255);
image[1, 0] = new Rgba32(0, 255, 0, 255);
image[0, 1] = new Rgba32(0, 0, 255, 255);
image[1, 1] = new Rgba32(255, 255, 255, 255);
using var texture = Texture.FromImage(device, image);
// GetTexImage writes the ENTIRE texture: allocate all 2*2*4 bytes, not one pixel.
var bytes = new byte[16];
gl.BindTexture(TextureTarget.Texture2D, texture.Handle);
unsafe { fixed (byte* p = bytes) gl.GetTexImage(TextureTarget.Texture2D, 0, Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, p); }
if (bytes[0] != 255 || bytes[1] != 0 || bytes[2] != 0)
    throw new Exception($"Texture row zero must retain source top-left red; got {bytes[0]},{bytes[1]},{bytes[2]}");
using var target = new Texture(device, 64, 64, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
using var surface = target.GetSurfaceLevel(0);
if (surface.FBO == 0) throw new Exception("Render target has no FBO");
if (gl.GetInteger(GLEnum.FramebufferBinding) != 0) throw new Exception("Surface construction leaked binding");
device.SetRenderTarget(0, surface);
using var sprite = new Sprite(device);
sprite.Begin(SpriteFlags.AlphaBlend);
sprite.Transform = Matrix.Scaling(32, 32, 1);
sprite.Draw(texture, null, null, Vector3.Zero, new SlimDX.Direct3D9.Color4(1f, 1f, 1f, 1f));
sprite.Transform = Matrix.Identity;
sprite.End();
var pixel = new byte[4];
unsafe { fixed (byte* p = pixel) gl.ReadPixels(8, 56, 1, 1, Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, p); }
if (pixel[0] < 240 || pixel[1] > 15 || pixel[2] > 15)
    throw new Exception($"Rendered top-left must be red; got {string.Join(',', pixel)}");
device.SetRenderTarget(0, null);
if (gl.GetInteger(GLEnum.FramebufferBinding) != 0) throw new Exception("Backbuffer restoration failed");
using var secondTarget = new Texture(device, 64, 64, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
using var secondSurface = secondTarget.GetSurfaceLevel(0);
device.SetRenderTarget(0, secondSurface);
sprite.Begin(SpriteFlags.AlphaBlend);
sprite.Draw(target, null, null, Vector3.Zero, new SlimDX.Direct3D9.Color4(1f, 1f, 1f, 1f));
sprite.End();
unsafe { fixed (byte* p = pixel) gl.ReadPixels(8, 56, 1, 1, Silk.NET.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, p); }
if (pixel[0] < 240 || pixel[1] > 15 || pixel[2] > 15)
    throw new Exception($"Composited render target top-left must remain red; got {string.Join(',', pixel)}");
device.SetRenderTarget(0, null);
Console.WriteLine("PASS: texture row orientation, FBO creation/restoration, transformed sprite pixel");
