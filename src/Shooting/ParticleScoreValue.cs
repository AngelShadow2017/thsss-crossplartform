using System;
using System.Drawing;

namespace Shooting;

internal class ParticleScoreValue : BaseEffect
{
	private char[] ScoreCharArray { get; set; }

	public ParticleScoreValue(StageDataPackage StageData, PointF OriginalPosition, long ScoreValue)
		: base(StageData, null, new PointF(0f, 0f), 1f, -1.5707963705062866)
	{
		base.LifeTime = 50;
		base.Active = false;
		base.OriginalPosition = OriginalPosition;
		ScoreCharArray = ScoreValue.ToString().ToCharArray();
	}

	public override void Ctrl()
	{
		base.Angle = 0.0 - base.Direction + Math.PI / 2.0;
		base.Ctrl();
		if (base.Time > 0)
		{
			base.TransparentValueF -= 255 / base.LifeTime;
		}
	}

	public override void Show()
	{
		int num = base.TextureObjectDictionary["Score_+"].Width;
		for (int i = 0; i < ScoreCharArray.Length; i++)
		{
			num += base.TextureObjectDictionary[ScoreCharArray[i].ToString()].Width;
		}
		float num2 = 0.6f;
		PointF position = new PointF(base.OriginalPosition.X - (float)num * num2 / 2f, base.OriginalPosition.Y);
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Score_+"], num2, 0f, position, Color.FromArgb(base.TransparentValue, base.ColorValue));
		position = new PointF(position.X + (float)base.TextureObjectDictionary["Score_+"].Width * num2 / 2f, position.Y);
		for (int i = 0; i < ScoreCharArray.Length; i++)
		{
			position = new PointF(position.X + (float)base.TextureObjectDictionary["Score_" + ScoreCharArray[i]].Width * num2 / 2f, position.Y);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["Score_" + ScoreCharArray[i]], num2, 0f, position, Color.FromArgb(base.TransparentValue, base.ColorValue));
			position = new PointF(position.X + (float)base.TextureObjectDictionary["Score_" + ScoreCharArray[i]].Width * num2 / 2f, position.Y);
		}
	}
}
