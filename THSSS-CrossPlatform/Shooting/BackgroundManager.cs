using System.Collections.Generic;

namespace Shooting;

public class BackgroundManager
{
	private int light = 0;

	public int Light
	{
		get
		{
			return light;
		}
		set
		{
			if (value < 0)
			{
				light = 0;
			}
			else if (value > 255)
			{
				light = 255;
			}
			else
			{
				light = value;
			}
		}
	}

	public List<BaseObject> BackgroundList { get; set; }

	public BackgroundManager()
	{
		Light = 255;
		BackgroundList = new List<BaseObject>();
	}

	public void Ctrl()
	{
		foreach (BaseObject x in BackgroundList.ToArray())
		{
			if (x.OutBoundary())
			{
				BackgroundList.Remove(x);
			}
		}
		foreach (BaseObject x in BackgroundList.ToArray())
		{x.Ctrl();
				}
	}

	public void Show(bool Active)
	{
		if (Active)
		{
			foreach (BaseObject x in BackgroundList.ToArray())
		{if (x.Active)
				{
					x.Show();
				}
					}
			return;
		}
		foreach (BaseObject x in BackgroundList.ToArray())
		{if (!x.Active)
			{
				x.Show();
			}
				}
	}

	public void Clear()
	{
		BackgroundList.Clear();
	}
}
