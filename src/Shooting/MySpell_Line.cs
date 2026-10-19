using System.Drawing;

namespace Shooting;

public class MySpell_Line : Life_Line
{
	public MySpell_Line(StageDataPackage StageData, string textureName, PointF OriginalPosition)
		: base(StageData, textureName, OriginalPosition)
	{
		size = 16f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Life = base.MyPlane.Spell;
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF(size, size);
		PointF position;
		for (int i = 0; i < 8; i++)
		{
			TextureObject textureObject = base.TextureObjectDictionary["Bomb0d"];
			position = new PointF(base.OriginalPosition.X + (float)i * size, base.OriginalPosition.Y);
			base.SpriteMain.Draw2D(textureObject.TXTure, textureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
		}
		for (int i = 0; i < base.Life; i++)
		{
			TextureObject textureObject = base.TextureObjectDictionary["Bomb"];
			position = new PointF(base.OriginalPosition.X + (float)i * size, base.OriginalPosition.Y);
			base.SpriteMain.Draw2D(textureObject.TXTure, textureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
		}
		if (base.Life < 8)
		{
			int num = base.MyPlane.SpellChip * 4 / base.MyPlane.nextSpellChip;
			if (num > 0)
			{
				TextureObject textureObject = base.TextureObjectDictionary["Bomb" + (num - 1).ToString("D2")];
				position = new PointF(base.OriginalPosition.X + (float)base.Life * size, base.OriginalPosition.Y);
				base.SpriteMain.Draw2D(textureObject.TXTure, textureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
			}
		}
	}
}
