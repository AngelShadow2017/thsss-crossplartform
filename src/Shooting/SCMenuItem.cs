using System.Drawing;

namespace Shooting;

public class SCMenuItem(StageDataPackage StageData, string Name) : DescriptionMenuItem(StageData, Name)
{
	public int OffX = 50;

	public int OffY = -5;

	public override void Show()
	{
		base.Show();
		if (base.TxtureObject != null)
		{
			base.SpriteMain.Draw2D(base.TxtureObject, base.ScaleWidth, base.ScaleLength, 0f, new PointF(base.Position.X + (float)base.TxtureObject.Width * base.Scale / 2f + (float)OffX, base.Position.Y + (float)OffY + (float)base.TxtureObject.Height * base.Scale / 2f), Color.FromArgb(base.TransparentValue, base.ColorValue), base.Mirrored);
		}
	}
}
