using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class MySprite
{
	public Sprite sprite { get; set; }

	public MySprite(Device device)
	{
		sprite = new Sprite(device);
	}

	public void Begin(SpriteFlags flags)
	{
		sprite.Begin(flags);
	}

	public void End()
	{
		sprite.End();
	}

	public void Draw2D(Texture srcTexture, PointF rotationCenter, float rotationAngle, PointF position, Color color)
	{
		Vector3 value = new Vector3(rotationCenter.X, rotationCenter.Y, 0f);
		Vector3 value2 = new Vector3(position.X, position.Y, 0f);
		Color4 color2 = new Color4(color);
		Vector2 vector = new Vector2(position.X, position.Y);
		sprite.Transform = Matrix.Transformation2D(vector, 0f, new Vector2(1f, 1f), vector, rotationAngle, new Vector2(0f, 0f));
		sprite.Draw(srcTexture, value, value2, color2);
		sprite.Transform = Matrix.Identity;
	}

	public void Draw2D(Texture srcTexture, Rectangle srcRectangle, SizeF destinationSize, PointF position, Color color)
	{
		Vector3 value = new Vector3(position.X, position.Y, 0f);
		Color4 color2 = new Color4(color);
		Vector2 vector = new Vector2(position.X, position.Y);
		Vector2 scaling = new Vector2(destinationSize.Width / (float)srcRectangle.Width, destinationSize.Height / (float)srcRectangle.Height);
		sprite.Transform = Matrix.Transformation2D(vector, 0f, scaling, vector, 0f, new Vector2(0f, 0f));
		sprite.Draw(srcTexture, srcRectangle, new Vector3(0f, 0f, 0f), value, color2);
		sprite.Transform = Matrix.Identity;
	}

	public void Draw2D(Texture srcTexture, Rectangle srcRectangle, SizeF destinationSize, PointF rotationCenter, float rotationAngle, PointF position, Color color)
	{
		Vector3 value = new Vector3(rotationCenter.X, rotationCenter.Y, 0f);
		Vector3 value2 = new Vector3(position.X, position.Y, 0f);
		Color4 color2 = new Color4(color);
		Vector2 vector = new Vector2(rotationCenter.X, rotationCenter.Y);
		Vector2 vector2 = new Vector2(position.X, position.Y);
		Vector2 vector3 = vector + vector2;
		Vector2 scaling = new Vector2(destinationSize.Width / (float)srcRectangle.Width, destinationSize.Height / (float)srcRectangle.Height);
		sprite.Transform = Matrix.Transformation2D(vector2, 0f, scaling, vector2, rotationAngle, new Vector2(0f, 0f));
		sprite.Draw(srcTexture, srcRectangle, value, value2, color2);
		sprite.Transform = Matrix.Identity;
	}

	public void Draw2D(TextureObject txtureObject, float scale, float rotationAngle, PointF position, int transpanrent)
	{
		Draw2D(txtureObject, scale, rotationAngle, position, Color.FromArgb(transpanrent, Color.White));
	}

	public void Draw2D(TextureObject txtureObject, float scale, float rotationAngle, PointF position, Color color)
	{
		Draw2D(txtureObject, scale, rotationAngle, position, txtureObject.RotatingCenter, color);
	}

	public void Draw2D(TextureObject txtureObject, float scale, float rotationAngle, PointF position, PointF rotationCenter, Color color)
	{
		Draw2D(txtureObject, scale, rotationAngle, position, rotationCenter, color, mirrored: false);
	}

	public void Draw2D(TextureObject txtureObject, float scale, float rotationAngle, PointF position, PointF rotationCenter, Color color, bool mirrored)
	{
		Draw2D(txtureObject, scale, scale, rotationAngle, position, rotationCenter, color, mirrored);
	}

	public void Draw2D(TextureObject txtureObject, float scaleX, float scaleY, float rotationAngle, PointF position, Color color, bool mirrored)
	{
		Draw2D(txtureObject, scaleX, scaleY, rotationAngle, position, txtureObject.RotatingCenter, color, mirrored);
	}

	public void Draw2D(TextureObject txtureObject, float scaleX, float scaleY, float rotationAngle, PointF position, PointF rotationCenter, Color color, bool mirrored)
	{
		Vector3 value = new Vector3(rotationCenter.X, rotationCenter.Y, 0f);
		Vector3 value2 = new Vector3(position.X, position.Y, 0f);
		Color4 color2 = new Color4(color);
		Vector2 vector = new Vector2(rotationCenter.X, rotationCenter.Y);
		Vector2 vector2 = new Vector2(position.X, position.Y);
		Vector2 vector3 = vector + vector2;
		Vector2 scaling = (mirrored ? new Vector2(0f - scaleX, scaleY) : new Vector2(scaleX, scaleY));
		sprite.Transform = Matrix.Transformation2D(vector2, 0f, scaling, vector2, rotationAngle, new Vector2(0f, 0f));
		sprite.Draw(txtureObject.TXTure, txtureObject.PosRect, value, value2, color2);
		sprite.Transform = Matrix.Identity;
	}

	public void Dispose()
	{
		sprite.Dispose();
	}
}
