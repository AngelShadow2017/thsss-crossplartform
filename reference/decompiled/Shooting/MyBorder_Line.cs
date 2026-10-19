using System.Drawing;

namespace Shooting;

public class MyBorder_Line : Life_Line
{
	public MyBorder_Line(StageDataPackage StageData, string textureName, PointF OriginalPosition)
		: base(StageData, textureName, OriginalPosition)
	{
		size = 16f;
		maxLife = 11;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Life = base.MyPlane.EnchantmentCount;
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF(size, size);
		PointF position;
		for (int i = 0; i < base.MyPlane.EnchantmentCountNeeded; i++)
		{
			TextureObject textureObject = base.TextureObjectDictionary["Border0d"];
			position = new PointF(base.OriginalPosition.X + (float)(i * 12), base.OriginalPosition.Y);
			base.SpriteMain.Draw2D(textureObject.TXTure, textureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
		}
		for (int i = 0; i < base.Life; i++)
		{
			TextureObject textureObject = base.TextureObjectDictionary["Border"];
			position = new PointF(base.OriginalPosition.X + (float)(i * 12), base.OriginalPosition.Y);
			base.SpriteMain.Draw2D(textureObject.TXTure, textureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
		}
	}
}
