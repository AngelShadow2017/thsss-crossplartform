using System.Drawing;

namespace Shooting;

public class Item_Line_Score : Item_Line
{
	private long scoreValue { get; set; }

	public Item_Line_Score(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, OriginalPosition)
	{
		base.FontType = "ScoreNum_";
	}

	public override void Show()
	{
		base.MaxValue = 0;
		base.Value = 0;
		if (base.MyPlane.Score - scoreValue < 1000)
		{
			scoreValue += 1000L;
		}
		else
		{
			scoreValue += (base.MyPlane.Score - scoreValue) / 5;
		}
		if (scoreValue > base.MyPlane.Score)
		{
			scoreValue = base.MyPlane.Score;
		}
		base.Text = (10000000000L + scoreValue).ToString("N0");
		base.Text = base.Text.Remove(0, 1).PadLeft(10, ' ');
		base.Show();
	}
}
