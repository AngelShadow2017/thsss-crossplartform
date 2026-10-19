using System.Reflection;

public class Fake : DispatchProxy
{
    public Dictionary<string, object> Values = new();
    public Dictionary<string, Delegate> Events = new();
    public List<(string Name, object[] Args)> Calls = new();
    public Action<string, object[]> OnCall;
    public static T Create<T>() where T : class => DispatchProxy.Create<T, Fake>();
    public static Fake Of(object value) => (Fake)value;
    public void Raise(string name, params object[] args)
    {
        if (Events.TryGetValue(name, out var handler)) handler?.DynamicInvoke(args);
    }
    protected override object Invoke(MethodInfo method, object[] args)
    {
        Calls.Add((method.Name, args));
        OnCall?.Invoke(method.Name, args);
        var name = method.Name;
        if (name.StartsWith("add_") || name.StartsWith("remove_"))
        {
            string key = name[4..];
            if (name.StartsWith("remove_")) key = name[7..];
            Events.TryGetValue(key, out var previous);
            Events[key] = name.StartsWith("add_") ? Delegate.Combine(previous, (Delegate)args[0]) : Delegate.Remove(previous, (Delegate)args[0]);
            return null;
        }
        if (name.StartsWith("set_")) { Values[name[4..]] = args[0]; return null; }
        if (name.StartsWith("get_") && Values.TryGetValue(name[4..], out var value)) return value;
        return method.ReturnType == typeof(void) ? null : method.ReturnType.IsValueType ? Activator.CreateInstance(method.ReturnType) : null;
    }
}
