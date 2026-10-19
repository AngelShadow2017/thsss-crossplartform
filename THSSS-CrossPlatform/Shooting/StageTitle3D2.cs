using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class StageTitle3D2 : StageTitle3D
{
	public StageTitle3D2(StageDataPackage StageData, string TextureName)
		: base(StageData, TextureName)
	{
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2 - 80);
	}

	public override void SetupVertex()
	{
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		float num = 0f - base.TxtureObject.RotatingCenter.X;
		float num2 = (float)base.TxtureObject.Width - base.TxtureObject.RotatingCenter.X;
		float y = 0f - base.TxtureObject.RotatingCenter.Y;
		float y2 = (float)base.TxtureObject.Height - base.TxtureObject.RotatingCenter.Y;
		float num3 = (float)base.TxtureObject.PosRect.X / (float)base.TxtureObject.SrcWidth;
		float num4 = (float)(base.TxtureObject.PosRect.X + base.TxtureObject.Width) / (float)base.TxtureObject.SrcWidth;
		float y3 = (float)base.TxtureObject.PosRect.Y / (float)base.TxtureObject.SrcHeight;
		float y4 = (float)(base.TxtureObject.PosRect.Y + base.TxtureObject.Height) / (float)base.TxtureObject.SrcHeight;
		TexturedColoredVertex[] array = new TexturedColoredVertex[2 * Length];
		for (int i = 0; i < Length; i++)
		{
			int num5 = -10 * i + base.Time * 3;
			if (num5 < 0)
			{
				num5 = 0;
			}
			else if (num5 > base.TransparentValue)
			{
				num5 = base.TransparentValue;
			}
			ref TexturedColoredVertex reference = ref array[2 * i];
			reference = new TexturedColoredVertex(new Vector3(num + (num2 - num) * (float)i / (float)Length, y, 0f), new Vector2(num3 + (num4 - num3) * (float)i / (float)Length, y3), Color.FromArgb(num5, base.ColorValue).ToArgb());
			ref TexturedColoredVertex reference2 = ref array[2 * i + 1];
			reference2 = new TexturedColoredVertex(new Vector3(num + (num2 - num) * (float)i / (float)Length, y2, 0f), new Vector2(num3 + (num4 - num3) * (float)i / (float)Length, y4), Color.FromArgb(num5, base.ColorValue).ToArgb());
		}
		dataStream.WriteRange(array);
		vertexBuffer.Unlock();
	}
}
