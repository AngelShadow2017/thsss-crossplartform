using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Shooting.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("Shooting.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap background
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("background", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap FiveStar
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("FiveStar", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Menu_Start
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("Menu_Start", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap MyPlane
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("MyPlane", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Player
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("Player", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Power
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("Power", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Score
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("Score", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Spell
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("Spell", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap 菜单背景
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			object obj = ResourceManager.GetObject("菜单背景", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
