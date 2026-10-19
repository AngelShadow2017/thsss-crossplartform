using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;

namespace Shooting;

public static class JoystickDevices
{
    public static bool IsController(IJoystick joystick)
    {
        if (!joystick.IsConnected) return false;
        if (!OperatingSystem.IsLinux() || !Directory.Exists("/sys/class/input")) return true;
        // GLFW can expose Linux touchpads and mice as raw joysticks; DirectInput's GameController class did not.
        foreach (var entry in Directory.EnumerateDirectories("/sys/class/input", "event*"))
        {
            string device = Path.Combine(entry, "device");
            string name = Path.Combine(device, "name");
            if (!File.Exists(name) || File.ReadAllText(name).TrimEnd() != joystick.Name) continue;
            string keys = Path.Combine(device, "capabilities", "key");
            if (File.Exists(keys)) return HasControllerButtons(File.ReadAllText(keys));
        }
        return true;
    }

    public static bool HasControllerButtons(string linuxKeyBits)
    {
        string hex = string.Concat(linuxKeyBits.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.PadLeft(IntPtr.Size * 2, '0')));
        var bits = BigInteger.Parse("0" + hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        for (int key = 0x120; key <= 0x13f; key++)
            if ((bits & (BigInteger.One << key)) != 0) return true;
        return (bits & (new BigInteger(15) << 0x220)) != 0;
    }
}
