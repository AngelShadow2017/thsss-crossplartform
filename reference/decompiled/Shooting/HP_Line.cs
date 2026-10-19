using System.Drawing;

namespace Shooting;

public class HP_Line : BaseObject
{
	private int hp = 0;

	public int HP
	{
		get
		{
			return hp;
		}
		set
		{
			if (value < 0)
			{
				hp = 0;
			}
			else if (value > MaxHP)
			{
				hp = MaxHP;
			}
			else
			{
				hp = value;
			}
		}
	}

	public int MaxHP { get; set; }

	public HP_Line(StageDataPackage StageData, string textureName, PointF OriginalPosition)
	{
		base.StageData = StageData;
		base.TxtureObject = base.TextureObjectDictionary[textureName];
		base.OriginalPosition = OriginalPosition;
		MaxHP = 1;
		base.TransparentValueF = 0f;
	}

	public override void Ctrl()
	{
		if (base.Boss == null)
		{
			base.TransparentValueF -= 25f;
			return;
		}
		base.Ctrl();
		MaxHP = base.Boss.MaxHP;
		if (HP < (int)base.Boss.HealthPoint)
		{
			HP += 300;
		}
		else
		{
			HP = (int)base.Boss.HealthPoint;
		}
		if (base.MyPlane.OriginalPosition.Y < 30f)
		{
			base.TransparentValueF -= 10f;
			if (base.TransparentValueF < 50f)
			{
				base.TransparentValueF = 50f;
			}
		}
		else
		{
			base.TransparentValueF += 10f;
		}
	}

	public override void Show()
	{
		int num = base.TxtureObject.Width * HP / MaxHP;
		num = ((num <= 1) ? 1 : num);
		Size size = new Size(num, base.TxtureObject.Height);
		Rectangle srcRectangle = new Rectangle(new Point(0, 0), size);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, srcRectangle, size, base.TxtureObject.RotatingCenter, 0f, base.OriginalPosition, Color.FromArgb(base.TransparentValue * 10 / 10, Color.White));
		if (base.Boss != null && !base.Boss.OnSpell && base.Boss.SpellcardHP != 0)
		{
			Point point = new Point((int)base.OriginalPosition.X - base.TxtureObject.Width / 2 + base.TxtureObject.Width * base.Boss.SpellcardHP / MaxHP, (int)base.OriginalPosition.Y);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["HP_Flag"], 1f, 0f, point, Color.FromArgb(base.TransparentValue, Color.White));
		}
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["HP_Line2"], 1f, 0f, base.OriginalPosition, Color.FromArgb(base.TransparentValue, Color.White));
	}
}
