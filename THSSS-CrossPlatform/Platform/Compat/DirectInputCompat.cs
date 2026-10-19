// DirectInput compatibility layer for cross-platform
using System;
using System.Collections.Generic;

namespace SlimDX.DirectInput
{
    // Key codes - complete set matching SlimDX.DirectInput.Key
    public enum Key
    {
        Unknown = 0,
        Escape = 1,
        D1 = 2, D2 = 3, D3 = 4, D4 = 5, D5 = 6, D6 = 7, D7 = 8, D8 = 9, D9 = 10, D0 = 11,
        Minus = 12, Equals = 13, Back = 14, Tab = 15,
        Q = 16, W = 17, E = 18, R = 19, T = 20, Y = 21, U = 22, I = 23, O = 24, P = 25,
        LeftBracket = 26, RightBracket = 27, Return = 28,
        LeftControl = 29,
        A = 30, S = 31, D = 32, F = 33, G = 34, H = 35, J = 36, K = 37, L = 38,
        Semicolon = 39, Apostrophe = 40, Grave = 41,
        LeftShift = 42, Backslash = 43,
        Z = 44, X = 45, C = 46, V = 47, B = 48, N = 49, M = 50,
        Comma = 51, Period = 52, Slash = 53,
        RightShift = 54,
        LeftAlt = 55, Space = 56, CapsLock = 57,
        F1 = 58, F2 = 59, F3 = 60, F4 = 61, F5 = 62, F6 = 63, F7 = 64, F8 = 65, F9 = 66, F10 = 67,
        NumLock = 68, ScrollLock = 69,
        Numpad7 = 70, Numpad8 = 71, Numpad9 = 72, NumpadMinus = 73,
        Numpad4 = 74, Numpad5 = 75, Numpad6 = 76, NumpadPlus = 77,
        Numpad1 = 78, Numpad2 = 79, Numpad3 = 80, Numpad0 = 81, NumpadPeriod = 82,
        F11 = 83, F12 = 84,
        F13 = 85, F14 = 86, F15 = 87,
        Kana = 88, Kanji = 89,
        RightAlt = 90,
        RightControl = 91,
        Up = 92, Down = 93, Left = 94, Right = 95,
        Insert = 96, Delete = 97, Home = 98, End = 99,
        PageUp = 100, PageDown = 101,
        PrintScreen = 102, Pause = 103,
        LWin = 104, RWin = 105,
        LMenu = 106, RMenu = 107,
        LShift = LeftShift,
        RShift = RightShift,
        LControl = LeftControl,
        RControl = RightControl,
        LAlt = LeftAlt,
        RAlt = RightAlt,
        UpArrow = 92,
        DownArrow = 93,
        LeftArrow = 94,
        RightArrow = 95,
        LeftShift2 = 42,
        RightShift2 = 54,
        LeftControl2 = 29,
        RightControl2 = 91,

    }

    // Joystick offset
    public enum JoystickOffset
    {
        X = 0, Y = 4, Z = 8,
        RotationX = 12, RotationY = 16, RotationZ = 20,
        Sliders0 = 24, Sliders1 = 28,
        PointOfViewControllers0 = 32, PointOfViewControllers1 = 36,
        PointOfViewControllers2 = 40, PointOfViewControllers3 = 44,
        Buttons0 = 48, Buttons1 = 49, Buttons2 = 50, Buttons3 = 51,
        Buttons4 = 52, Buttons5 = 53, Buttons6 = 54, Buttons7 = 55,
        Buttons8 = 56, Buttons9 = 57, Buttons10 = 58, Buttons11 = 59,
        Buttons12 = 60, Buttons13 = 61, Buttons14 = 62, Buttons15 = 63,
    }

    // Device class
    public enum DeviceClass
    {
        All = 0,
        Device = 1,
        Game = 2,
        Keyboard = 3,
        Mouse = 4,
        Pointer = 5,
        GameController = 6,
    }

    // Device enumeration flags
    [Flags]
    public enum DeviceEnumerationFlags
    {
        AllDevices = 0,
        Attached = 1,
        ForceFeedback = 2,
        AttachedAndDefaultDevice = 4,
        ForceFeedbackAttack = 8,
        AttachedOnly = 1,
    }

    // Cooperative level
    [Flags]
    public enum CooperativeLevel
    {
        Normal = 0,
        Exclusive = 1,
        Fullscreen = 2,
        SetFocus = 4,
        AllowReboot = 8,
        NoWinChanges = 16,
        NoChanges = 32,
        NoWinKey = 64,
        Foreground = 128,
        Background = 256,
    }

    // Device instance
    public struct DeviceInstance
    {
        public string InstanceName;
        public string ProductName;
        public Guid InstanceGuid;
        public Guid ProductGuid;
        public int Usage;
        public int UsagePage;
    }

    // Device object instance
    public struct DeviceObjectInstance
    {
        public string Name;
        public int Offset;
        public int Type;
        public int Flags;
        public Guid ObjectTypeGuid;
        public ObjectDeviceType ObjectType { get => (ObjectDeviceType)Type; }
    }

    // DirectInput device
    public class Device : IDisposable
    {
        public Device() { }
        public Device(Guid guid) { }
        public void SetCooperativeLevel(System.Windows.Forms.Control control, SlimDX.DirectInput.CooperativeLevel level) { }
        public void SetCooperativeLevel(IntPtr handle, SlimDX.DirectInput.CooperativeLevel level) { }
        public void SetDataFormat(ObjectDataFormat format) { }
        public Result Acquire() => SlimDX.Result.Ok;
        public Result Unacquire() => SlimDX.Result.Ok;
        public JoystickState GetCurrentState() => new JoystickState();
        public Result Poll() => SlimDX.Result.Ok;
        public void Dispose() { }
        public bool IsDisposed => false;

        /// <summary>
        /// DirectInput enumerates physical devices. There is no cross-platform
        /// gamepad backend, so report none: JoystickCapture already tolerates an
        /// empty list and leaves the keyboard-derived KeyClass untouched.
        /// </summary>
        public static IEnumerable<DeviceInstance> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags)
        {
            return new List<DeviceInstance>();
        }
    }

    // Joystick state
    public struct JoystickState
    {
        public int X, Y, Z;
        public int RotationX, RotationY, RotationZ;
        public int[] Sliders;
        public int[] PointOfViewControllers;
        public byte[] Buttons;
        public bool IsPressed(int button) => false;
        public bool IsPressed(JoystickOffset offset) => false;
    }

    public enum ObjectDeviceType
    {
        All = 0,
        Button = 1,
        Key = 2,
        Axis = 3,
        Slider = 4,
        POV = 5,
    }

    // Object data format
    public struct ObjectDataFormat
    {
        public Guid ObjectGuid;
        public int Offset;
        public int Type;
        public int Flags;
    }

    // Mouse state
    public struct MouseState
    {
        public int X, Y, Z;
        public byte[] Buttons;
    }

    // Keyboard state
    public class KeyboardState
    {
        public byte[] PressedKeys = new byte[256];

        /// <summary>
        /// Reflects the snapshot the window published for this frame. The original
        /// DirectInput state was a per-frame copy too: KeyboardCapture keeps the
        /// previous one to detect rising/falling edges.
        /// </summary>
        public bool IsPressed(Key key) => PressedKeys != null && PressedKeys.Length > (int)key && PressedKeys[(int)key] != 0;
        public bool IsReleased(Key key) => !IsPressed(key);

        internal static KeyboardState Capture()
        {
            var state = new KeyboardState();
            foreach (var key in KeyboardInput.Snapshot())
                if ((int)key >= 0 && (int)key < state.PressedKeys.Length)
                    state.PressedKeys[(int)key] = 1;
            return state;
        }
    }

    // Keyboard device
    public class Keyboard : Device
    {
        public Keyboard() { }
        public Keyboard(Guid guid) : base(guid) { }
        public Keyboard(DirectInput directInput) { }
        public new KeyboardState GetCurrentState() => KeyboardState.Capture();
    }

    // Joystick device
    public class Joystick : Device
    {
        public Joystick() { }
        public Joystick(Guid guid) : base(guid) { }
        public Joystick(DirectInput directInput, Guid guid) : base(guid) { }
        public new JoystickState GetCurrentState() => new JoystickState();
        public DeviceObjectInstance[] GetObjects() => new DeviceObjectInstance[0];
        public DeviceObjectInstance GetObjectPropertiesById(Guid guid) => new DeviceObjectInstance();
        public DeviceObjectInstance GetObjectPropertiesById(int dataFormatOffset, int dataFormatType) => new DeviceObjectInstance();
        public DeviceObjectProperties GetObjectPropertiesById(int offset) => new DeviceObjectProperties();
    }

    // DirectInput class
    public class DirectInput : IDisposable
    {
        public DirectInput() { }
        public IEnumerable<DeviceInstance> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags)
        {
            return new List<DeviceInstance>();
        }
        public void Dispose() { }
    }
}

// Result comes from SlimDX namespace (shared)

namespace SlimDX.DirectInput
{
    public class DirectInputException : Exception
    {
        public DirectInputException(string message) : base(message) { }
        public DirectInputException() { }
    }
}

namespace SlimDX.DirectInput
{
    public class DeviceObjectProperties
    {
        public void SetRange(int min, int max) { }
    }
}
