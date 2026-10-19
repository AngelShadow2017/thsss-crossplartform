using System;
using Avalonia;

namespace THSSS.Config;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var workspace = new ConfigWorkspace(ConfigWorkspace.ResolveDataRoot(
                Environment.GetEnvironmentVariable("THSSS_DATA_ROOT")));
            BuildAvaloniaApp(workspace).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.ExitCode = 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp(ConfigWorkspace workspace) => AppBuilder.Configure(() => new ConfigApp(workspace))
        .UsePlatformDetect()
        .LogToTrace();
}
