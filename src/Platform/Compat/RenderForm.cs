using System;
using System.Drawing;
using System.Windows.Forms;
using Silk.NET.Windowing;

namespace SlimDX.Windows;

public class RenderForm : Form
{
    private IWindow _window;
    private bool _focused = true;
    private bool _fullscreen;
    private bool _vsync;
    private int _interval = 1;
    private bool _gameOwnsPacing;
    private Size _lastAspectSize = new(640, 480);
    private Size _lastNonAspectSize;
    public bool AllowAspectCorrection { get; set; } = true;

    public void EnsureWindowAspect()
    {
        if (!AllowAspectCorrection) return;
        if (_window == null || _fullscreen || _window.WindowState is Silk.NET.Windowing.WindowState.Fullscreen or Silk.NET.Windowing.WindowState.Minimized) return;
        var size = ClientSize;
        if (size.Width <= 0 || size.Height <= 0) return;
        if ((long)size.Width * 3 == (long)size.Height * 4)
        {
            _lastAspectSize = size;
            _lastNonAspectSize = Size.Empty;
            return;
        }
        if (size == _lastNonAspectSize) return;
        _lastNonAspectSize = size;
        bool widthChanged = Math.Abs(size.Width - _lastAspectSize.Width) * 3 >= Math.Abs(size.Height - _lastAspectSize.Height) * 4;
        int units = Math.Max(1, (int)Math.Round(widthChanged ? size.Width / 4.0 : size.Height / 3.0));
        ClientSize = new Size(units * 4, units * 3);
    }

    public IWindow Window => _window;
    public override bool Focused => _focused;
    public override string Text
    {
        get => _window?.Title ?? base.Text;
        set { base.Text = value; if (_window != null && _window.Title != value) _window.Title = value; }
    }
    public override Size ClientSize
    {
        get => _window == null ? base.ClientSize : new Size(_window.Size.X, _window.Size.Y);
        set
        {
            base.ClientSize = value;
            if (_window != null && !_fullscreen && ClientSize != value)
                _window.Size = new(value.Width, value.Height);
        }
    }
    public override bool Visible
    {
        get => _window?.IsVisible ?? base.Visible;
        set { base.Visible = value; if (_window != null) _window.IsVisible = value; }
    }

    public event EventHandler Closing;
    public event EventHandler Closed;
    public RenderForm() { }
    public RenderForm(string title) { Text = title; ClientSize = new Size(640, 480); }
    public RenderForm(string title, int width, int height) { Text = title; ClientSize = new Size(width, height); }

    public void AttachWindow(IWindow window)
    {
        if (_window != null) throw new InvalidOperationException("A RenderForm can attach only one window.");
        _window = window ?? throw new ArgumentNullException(nameof(window));
        if (!string.IsNullOrEmpty(base.Text)) window.Title = base.Text;
        if (base.ClientSize.Width > 0 && base.ClientSize.Height > 0) ClientSize = base.ClientSize;
        window.FocusChanged += OnFocus;
        window.Resize += OnResize;
        window.Closing += OnClosing;
        window.Render += OnRender;
        ApplyPresentation();
    }

    public void ConfigurePresentation(bool fullscreen, bool vsync, int interval, bool gameOwnsPacing = false)
    {
        if (interval < 1 || interval > 3) throw new ArgumentOutOfRangeException(nameof(interval), "PresentInterval must be 1, 2 or 3.");
        _fullscreen = fullscreen;
        _vsync = vsync;
        _interval = interval;
        _gameOwnsPacing = gameOwnsPacing;
        Shooting.MouseCtrl.DrawCursor(!fullscreen);
        ApplyPresentation();
    }

    private void ApplyPresentation()
    {
        if (_window == null) return;
        var state = _fullscreen ? Silk.NET.Windowing.WindowState.Fullscreen : Silk.NET.Windowing.WindowState.Normal;
        if (_window.WindowState != state) _window.WindowState = state;
        // Original windowed PresentInterval>1 uses immediate presentation plus
        // the software limiter; only fullscreen uses interval-2/3 vertical sync.
        bool sync = _vsync && (_interval == 1 || _fullscreen);
        _window.VSync = sync;
        _window.FramesPerSecond = sync || _gameOwnsPacing ? 0 : 60.0 / _interval;
        _window.UpdatesPerSecond = 0;
        _window.GLContext?.SwapInterval(sync ? _interval : 0);
    }
    private void OnFocus(bool focused) { _focused = focused; DispatchFocus(focused); }
    private void OnResize(Silk.NET.Maths.Vector2D<int> size) => DispatchResize();
    private void OnRender(double delta) => DispatchPaint();
    private void OnClosing()
    {
        if (!DispatchClosing()) { _window.IsClosing = false; return; }
        Closing?.Invoke(this, EventArgs.Empty);
    }
    public override void Close() { if (_window != null) _window.Close(); else base.Close(); }
    public void CompleteClose() { DispatchClosed(); Closed?.Invoke(this, EventArgs.Empty); }
    public void DetachWindow()
    {
        if (_window == null) return;
        _window.FocusChanged -= OnFocus;
        _window.Resize -= OnResize;
        _window.Closing -= OnClosing;
        _window.Render -= OnRender;
        _window = null;
    }
    protected internal override void Dispose(bool disposing)
    {
        if (disposing) DetachWindow();
        base.Dispose(disposing);
    }
}
