// Cross-platform cursor visibility control (replaces user32!ShowCursor)
namespace Shooting;

public class MouseCtrl
{
	private static bool _cursorVisible = true;

	public static void DrawCursor(bool cursorShow)
	{
		_cursorVisible = cursorShow;
		CursorState.Visible = cursorShow;
	}

	public static bool IsVisible => _cursorVisible;
}

/// <summary>Shared, backend-agnostic cursor visibility flag consumed by the window layer.</summary>
public static class CursorState
{
	public static bool Visible { get; set; } = true;
}
