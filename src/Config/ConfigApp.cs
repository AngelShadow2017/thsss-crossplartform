using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

namespace THSSS.Config;

internal sealed class ConfigApp : Application
{
    private readonly ConfigWorkspace _workspace;

    public ConfigApp(ConfigWorkspace workspace) => _workspace = workspace;

    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new ConfigWindow(_workspace);
        base.OnFrameworkInitializationCompleted();
    }
}
