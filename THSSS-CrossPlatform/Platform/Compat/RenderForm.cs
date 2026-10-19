// Cross-platform window using Silk.NET.Windowing (replaces SlimDX.Windows.RenderForm)
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Silk.NET.Windowing;
using Silk.NET.Input;

namespace SlimDX.Windows
{
    /// <summary>Cross-platform game window (replaces SlimDX.Windows.RenderForm)</summary>
    public class RenderForm : Form
    {
        private IWindow _window;
        private bool _running;

        public IWindow Window => _window;
        public bool IsDisposed => _window == null && _disposed;
        private bool _disposed;

        public string Text
        {
            get => _window?.Title ?? "";
            set { if (_window != null) _window.Title = value; }
        }

        public Size ClientSize
        {
            get => _window != null ? new Size(_window.Size.X, _window.Size.Y) : new Size(0, 0);
            set { if (_window != null) _window.Size = new Silk.NET.Maths.Vector2D<int>(value.Width, value.Height); }
        }

        public bool Visible
        {
            get => _window?.IsVisible ?? false;
            set { if (_window != null) _window.IsVisible = value; }
        }

        public bool TopMost
        {
            get => _window?.WindowBorder == WindowBorder.Resizable;
            set { if (_window != null) _window.WindowBorder = value ? WindowBorder.Fixed : WindowBorder.Resizable; }
        }

        public event EventHandler Closing;
        public event EventHandler Closed;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event EventHandler Resize;
        public event PaintEventHandler Paint;

        public RenderForm() : base() { }

        public RenderForm(string title) : base()
        {
            var options = WindowOptions.Default;
            options.Title = title;
            options.Size = new Silk.NET.Maths.Vector2D<int>(640, 480);
            options.API = GraphicsAPI.Default;
            options.FramesPerSecond = 60;
            options.UpdatesPerSecond = 60;

            _window = Silk.NET.Windowing.Window.Create(options);
            SetupEvents();
        }

        public RenderForm(string title, int width, int height) : base()
        {
            var options = WindowOptions.Default;
            options.Title = title;
            options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
            options.API = GraphicsAPI.Default;
            options.FramesPerSecond = 60;
            options.UpdatesPerSecond = 60;

            _window = Silk.NET.Windowing.Window.Create(options);
            SetupEvents();
        }

        private void SetupEvents()
        {
            if (_window == null) return;

            _window.Load += () => { };
            _window.Update += dt => { };
            _window.Render += dt => Paint?.Invoke(this, new PaintEventArgs(null, Rectangle.Empty));
            _window.Closing += () => Closing?.Invoke(this, EventArgs.Empty);
            _window.Resize += (size) => Resize?.Invoke(this, EventArgs.Empty);

            var input = _window.CreateInput();
            var keyboard = input.Keyboards[0];
            keyboard.KeyDown += (kb, key, sc) => KeyDown?.Invoke(this, new KeyEventArgs(ConvertKey(key)));
            keyboard.KeyUp += (kb, key, sc) => KeyUp?.Invoke(this, new KeyEventArgs(ConvertKey(key)));

            if (input.Mice.Count > 0)
            {
                var mouse = input.Mice[0];
                mouse.MouseDown += (m, btn) => MouseDown?.Invoke(this, new MouseEventArgs(ConvertButton(btn), 1, (int)mouse.Position.X, (int)mouse.Position.Y));
                mouse.MouseUp += (m, btn) => MouseUp?.Invoke(this, new MouseEventArgs(ConvertButton(btn), 1, (int)mouse.Position.X, (int)mouse.Position.Y));
                mouse.MouseMove += (m, pos) => MouseMove?.Invoke(this, new MouseEventArgs(0, 0, (int)pos.X, (int)pos.Y));
            }
        }

        private System.Windows.Forms.Keys ConvertKey(Silk.NET.Input.Key key) => key switch
        {
            Silk.NET.Input.Key.A => System.Windows.Forms.Keys.A,
            Silk.NET.Input.Key.B => System.Windows.Forms.Keys.B,
            Silk.NET.Input.Key.C => System.Windows.Forms.Keys.C,
            Silk.NET.Input.Key.D => System.Windows.Forms.Keys.D,
            Silk.NET.Input.Key.E => System.Windows.Forms.Keys.E,
            Silk.NET.Input.Key.F => System.Windows.Forms.Keys.F,
            Silk.NET.Input.Key.G => System.Windows.Forms.Keys.G,
            Silk.NET.Input.Key.H => System.Windows.Forms.Keys.H,
            Silk.NET.Input.Key.I => System.Windows.Forms.Keys.I,
            Silk.NET.Input.Key.J => System.Windows.Forms.Keys.J,
            Silk.NET.Input.Key.K => System.Windows.Forms.Keys.K,
            Silk.NET.Input.Key.L => System.Windows.Forms.Keys.L,
            Silk.NET.Input.Key.M => System.Windows.Forms.Keys.M,
            Silk.NET.Input.Key.N => System.Windows.Forms.Keys.N,
            Silk.NET.Input.Key.O => System.Windows.Forms.Keys.O,
            Silk.NET.Input.Key.P => System.Windows.Forms.Keys.P,
            Silk.NET.Input.Key.Q => System.Windows.Forms.Keys.Q,
            Silk.NET.Input.Key.R => System.Windows.Forms.Keys.R,
            Silk.NET.Input.Key.S => System.Windows.Forms.Keys.S,
            Silk.NET.Input.Key.T => System.Windows.Forms.Keys.T,
            Silk.NET.Input.Key.U => System.Windows.Forms.Keys.U,
            Silk.NET.Input.Key.V => System.Windows.Forms.Keys.V,
            Silk.NET.Input.Key.W => System.Windows.Forms.Keys.W,
            Silk.NET.Input.Key.X => System.Windows.Forms.Keys.X,
            Silk.NET.Input.Key.Y => System.Windows.Forms.Keys.Y,
            Silk.NET.Input.Key.Z => System.Windows.Forms.Keys.Z,
            Silk.NET.Input.Key.Space => System.Windows.Forms.Keys.Space,
            Silk.NET.Input.Key.Enter => System.Windows.Forms.Keys.Enter,
            Silk.NET.Input.Key.Escape => System.Windows.Forms.Keys.Escape,
            Silk.NET.Input.Key.ShiftLeft => System.Windows.Forms.Keys.ShiftKey,
            Silk.NET.Input.Key.ShiftRight => System.Windows.Forms.Keys.ShiftKey,
            Silk.NET.Input.Key.ControlLeft => System.Windows.Forms.Keys.ControlKey,
            Silk.NET.Input.Key.ControlRight => System.Windows.Forms.Keys.ControlKey,
            Silk.NET.Input.Key.Up => System.Windows.Forms.Keys.Up,
            Silk.NET.Input.Key.Down => System.Windows.Forms.Keys.Down,
            Silk.NET.Input.Key.Left => System.Windows.Forms.Keys.Left,
            Silk.NET.Input.Key.Right => System.Windows.Forms.Keys.Right,
            _ => System.Windows.Forms.Keys.None,
        };

        private int ConvertButton(MouseButton btn) => btn switch
        {
            MouseButton.Left => 0,
            MouseButton.Right => 1,
            MouseButton.Middle => 2,
            _ => 0,
        };

        public void Show()
        {
            _window?.Run();
        }

        public new void Close()
        {
            _window?.Close();
        }

        protected internal override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _window?.Dispose();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
