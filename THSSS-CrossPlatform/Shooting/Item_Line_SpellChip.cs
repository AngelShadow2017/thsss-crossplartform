using System.Drawing;

namespace Shooting;

public class Item_Line_SpellChip : Item_Line
{
	public Item_Line_SpellChip(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, OriginalPosition)
	{
		base.FontType = "ShardNum_";
		Delta = 9;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Value = 0;
		string text = base.MyPlane.SpellChip.ToString();
		int nextSpellChip = base.MyPlane.nextSpellChip;
		base.Text = text + "/" + nextSpellChip;
		base.Text = base.Text.PadLeft(5, ' ');
	}
}
