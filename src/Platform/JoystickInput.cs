using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SlimDX.DirectInput;

namespace Shooting;

public sealed record JoystickSource(Guid Id, string Name, Func<JoystickState> Read);

public static class JoystickInput
{
    private static Dictionary<Guid, JoystickSource> _devices = new();
    public static bool Focused { get; set; } = true;
    public static Guid DeviceId(int index, string name) => new Guid(SHA256.HashData(Encoding.UTF8.GetBytes($"{index}:{name}"))[..16]);
    public static void ReplaceDevices(IEnumerable<JoystickSource> devices) => _devices = devices.ToDictionary(d => d.Id);
    public static IEnumerable<DeviceInstance> Enumerate(DeviceClass deviceClass)
    {
        if (deviceClass != DeviceClass.All && deviceClass != DeviceClass.GameController && deviceClass != DeviceClass.Game) yield break;
        foreach (var source in _devices.Values)
            yield return new DeviceInstance { InstanceGuid = source.Id, ProductGuid = source.Id, InstanceName = source.Name, ProductName = source.Name };
    }
    public static JoystickState Read(Guid id) => Focused && _devices.TryGetValue(id, out var source) ? source.Read() : new JoystickState { Buttons = Array.Empty<byte>() };
}
