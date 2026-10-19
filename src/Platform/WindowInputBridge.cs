using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Silk.NET.Input;
using SlimDX.DirectInput;
using SlimDX.Windows;
using SilkKey = Silk.NET.Input.Key;

namespace Shooting;

public sealed class WindowInputBridge : IDisposable
{
    private readonly RenderForm _form;
    private readonly IInputContext _input;
    private readonly HashSet<IKeyboard> _keyboards = new();
    private readonly HashSet<(IKeyboard Keyboard, SilkKey Key)> _pressed = new();
    private bool _focused = true;
    public event Action DevicesChanged;

    public WindowInputBridge(RenderForm form, IInputContext input)
    {
        _form = form;
        _input = input;
        input.ConnectionChanged += OnConnection;
        form.Window.FocusChanged += SetFocus;
        foreach (var keyboard in input.Keyboards) ConnectKeyboard(keyboard);
        foreach (var mouse in input.Mice)
        {
            mouse.MouseDown += MouseDown;
            mouse.MouseUp += MouseUp;
            mouse.MouseMove += MouseMove;
        }
        RefreshJoysticks();
    }

    private void ConnectKeyboard(IKeyboard keyboard)
    {
        if (!_keyboards.Add(keyboard)) return;
        keyboard.KeyDown += KeyDown;
        keyboard.KeyUp += KeyUp;
    }
    private void KeyDown(IKeyboard keyboard, SilkKey key, int scan) => PublishKey(keyboard, key, true);
    private void KeyUp(IKeyboard keyboard, SilkKey key, int scan) => PublishKey(keyboard, key, false);
    public void PublishKey(IKeyboard keyboard, SilkKey key, bool down)
    {
        if (!_focused && down) return;
        var legacy = LegacyKeyMap.Translate(key);
        bool wasDown = _pressed.Any(p => LegacyKeyMap.Translate(p.Key) == legacy);
        if (down) _pressed.Add((keyboard, key)); else _pressed.Remove((keyboard, key));
        var snapshotKey = KeyMap.Translate(key);
        KeyboardInput.Set(snapshotKey, _pressed.Any(p => KeyMap.Translate(p.Key) == snapshotKey));
        bool isDown = _pressed.Any(p => LegacyKeyMap.Translate(p.Key) == legacy);
        if (legacy != Keys.None && wasDown != isDown) _form.DispatchKey(legacy, isDown);
        if (down && key == SilkKey.F4 && (_pressed.Any(p => p.Key == SilkKey.AltLeft || p.Key == SilkKey.AltRight)))
            _form.Close();
    }
    public void SetFocus(bool focused)
    {
        _focused = focused;
        JoystickInput.Focused = focused;
        if (focused) return;
        foreach (var key in _pressed.Select(p => LegacyKeyMap.Translate(p.Key)).Distinct().ToArray())
            if (key != Keys.None) _form.DispatchKey(key, false);
        _pressed.Clear();
        KeyboardInput.Clear();
    }
    private void OnConnection(IInputDevice device, bool connected)
    {
        if (device is IKeyboard keyboard)
        {
            if (connected) ConnectKeyboard(keyboard);
            else
            {
                foreach (var key in _pressed.Where(p => p.Keyboard == keyboard).Select(p => p.Key).ToArray())
                    PublishKey(keyboard, key, false);
                keyboard.KeyDown -= KeyDown;
                keyboard.KeyUp -= KeyUp;
                _keyboards.Remove(keyboard);
            }
        }
        RefreshJoysticks();
        DevicesChanged?.Invoke();
    }
    private void RefreshJoysticks()
    {
        JoystickInput.ReplaceDevices(_input.Joysticks.Where(JoystickDevices.IsController).Select(j =>
            new JoystickSource(JoystickInput.DeviceId(j.Index, j.Name), j.Name, () => Capture(j))));
    }
    public static JoystickState Capture(IJoystick joystick)
    {
        var axes = joystick.Axes;
        int Axis(int index) => axes.Count > index ? (int)Math.Round(Math.Clamp(axes[index].Position, -1, 1) * 1000) : 0;
        int count = joystick.Buttons.Count == 0 ? 0 : joystick.Buttons.Max(b => b.Index) + 1;
        var buttons = new byte[count];
        foreach (var button in joystick.Buttons) buttons[button.Index] = button.Pressed ? (byte)128 : (byte)0;
        return new JoystickState { X = Axis(0), Y = Axis(1), Z = Axis(2), RotationX = Axis(3), RotationY = Axis(4), RotationZ = Axis(5), Buttons = buttons };
    }
    public void ApplyCursor()
    {
        var mode = !_focused || CursorState.Visible ? CursorMode.Normal : CursorMode.Hidden;
        foreach (var mouse in _input.Mice)
            if (mouse.Cursor.CursorMode != mode) mouse.Cursor.CursorMode = mode;
    }
    private void MouseMove(IMouse mouse, System.Numerics.Vector2 pos) => _form.DispatchMouse(new MouseEventArgs(MouseButtons.None, 0, (int)pos.X, (int)pos.Y), 0);
    private void MouseDown(IMouse mouse, MouseButton button) => MouseButtonEvent(mouse, button, 1);
    private void MouseUp(IMouse mouse, MouseButton button) => MouseButtonEvent(mouse, button, 2);
    private void MouseButtonEvent(IMouse mouse, MouseButton button, int kind)
    {
        var legacy = button switch { MouseButton.Left => MouseButtons.Left, MouseButton.Right => MouseButtons.Right, MouseButton.Middle => MouseButtons.Middle, _ => MouseButtons.None };
        _form.DispatchMouse(new MouseEventArgs(legacy, 1, (int)mouse.Position.X, (int)mouse.Position.Y), kind);
    }
    public void Dispose()
    {
        SetFocus(false);
        _input.ConnectionChanged -= OnConnection;
        if (_form.Window != null) _form.Window.FocusChanged -= SetFocus;
        foreach (var keyboard in _keyboards) { keyboard.KeyDown -= KeyDown; keyboard.KeyUp -= KeyUp; }
        foreach (var mouse in _input.Mice) { mouse.MouseDown -= MouseDown; mouse.MouseUp -= MouseUp; mouse.MouseMove -= MouseMove; }
        JoystickInput.ReplaceDevices(Array.Empty<JoystickSource>());
    }
}
