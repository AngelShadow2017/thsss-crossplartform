using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace THSSS.Config;

/// <summary>
/// Cross-platform replacement for the original WinForms Config dialog. Same widget
/// set, Chinese labels and Setting.INI contract; Avalonia is used so one executable
/// runs on Linux, macOS and Windows.
/// </summary>
internal sealed class ConfigWindow : Window
{
    private static readonly (string Field, string Label)[] KeyRows =
    {
        ("Left", "左"), ("Right", "右"), ("Up", "上"), ("Down", "下"),
        ("Slow", "低速移动"), ("Shoot", "射击/确认"), ("Bomb", "Bomb/取消"),
    };

    private static readonly (string Field, string Label)[] JoyRows =
    {
        ("Skip", "跳过对话"), ("Pause", "暂停"), ("Slow", "低速移动"),
        ("Shoot", "射击/确认"), ("Bomb", "Bomb/取消"),
    };

    private readonly ConfigWorkspace _workspace;
    private readonly ConfigSettings _settings;
    private readonly Dictionary<string, TextBox> _keyBoxes = new();
    private readonly Dictionary<string, ComboBox> _joyBoxes = new();
    private readonly RadioButton _interval1 = new() { Content = "每次", GroupName = "interval" };
    private readonly RadioButton _interval2 = new() { Content = "1/2次", GroupName = "interval" };
    private readonly RadioButton _interval3 = new() { Content = "1/3次", GroupName = "interval" };
    private readonly CheckBox _vsync = new() { Content = "启用垂直同步" };
    private readonly CheckBox _full = new() { Content = " 全屏启动游戏" };
    private readonly CheckBox _directInput = new() { Content = "使用DirectInput输入" };
    private readonly ComboBox _font = new() { ItemsSource = ConfigSettings.FontChoices, Width = 120 };
    private readonly TextBlock _fontPreview = new() { FontSize = 16, Margin = new Thickness(0, 4, 0, 0) };
    private readonly TextBlock _status = new() { Margin = new Thickness(0, 6, 0, 0), Foreground = Brushes.DimGray };
    private Border _controllerSection = null!;
    private TextBox? _capturing;

    public ConfigWindow(ConfigWorkspace workspace)
    {
        _workspace = workspace;
        _settings = ConfigSettings.Load(IniFile.Open(_workspace.IniPath));
        Title = "Config";
        Width = 640;
        Height = 400;
        MinWidth = 625;
        MinHeight = 380;
        Content = BuildLayout();
        LoadFromSettings();
        ApplyDirectInputLayout();
        Closing += (_, _) => Persist(launchGame: false, announce: false);
    }

    private static Border Section(string header, Control content) => new()
    {
        BorderBrush = Brushes.LightGray,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(3),
        Padding = new Thickness(8, 4, 8, 8),
        Child = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = header, FontWeight = FontWeight.Bold },
                content,
            },
        },
    };

    private Control BuildLayout()
    {
        var interval = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { _interval1, _interval2, _interval3 },
        };
        var display = Section("显示设置", new StackPanel
        {
            Spacing = 6,
            Children = { _vsync, _full, Section("绘制周期", interval) },
        });

        var keyRows = new StackPanel { Spacing = 4 };
        foreach (var (field, label) in KeyRows)
        {
            var box = new TextBox { Width = 120, IsReadOnly = true, Watermark = "点击后按键" };
            box.KeyDown += (_, e) => CaptureKey(box, e);
            box.PointerPressed += (_, _) => BeginCapture(box);
            _keyBoxes[field] = box;
            keyRows.Children.Add(Row(label, box));
        }

        var joyRows = new StackPanel { Spacing = 4 };
        foreach (var (field, label) in JoyRows)
        {
            var combo = new ComboBox { ItemsSource = ConfigSettings.ControllerButtons, Width = 120 };
            _joyBoxes[field] = combo;
            joyRows.Children.Add(Row(label, combo));
        }
        _controllerSection = Section("手柄设置", joyRows);

        _font.SelectionChanged += (_, _) => UpdateFontPreview();
        var fontSection = Section("字体设置", new StackPanel { Children = { _font, _fontPreview } });

        var apply = new Button { Content = "应用", MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
        apply.Click += (_, _) => Persist(launchGame: false, announce: true);
        var applyAndStart = new Button { Content = "应用并启动", MinWidth = 110 };
        applyAndStart.Click += (_, _) => Persist(launchGame: true, announce: true);
        _directInput.IsCheckedChanged += (_, _) => ApplyDirectInputLayout();

        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 8,
                Children =
                {
                    display,
                    Section("键设置", keyRows),
                    _directInput,
                    _controllerSection,
                    fontSection,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children = { apply, applyAndStart },
                    },
                    _status,
                    new TextBlock
                    {
                        Text = $"配置文件：{_workspace.IniPath}",
                        FontSize = 11,
                        Foreground = Brushes.Gray,
                        TextWrapping = TextWrapping.Wrap,
                    },
                },
            },
        };
    }

    private static Control Row(string label, Control control) => new Grid
    {
        ColumnDefinitions = new ColumnDefinitions("90,*"),
        Children =
        {
            new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center },
            InColumn(control, 1, 0),
        },
    };

    private static Control InColumn(Control control, int column, int row)
    {
        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        return control;
    }

    private void LoadFromSettings()
    {
        foreach (var (field, _) in KeyRows) _keyBoxes[field].Text = GetKey(field);
        foreach (var (field, _) in JoyRows)
            _joyBoxes[field].SelectedIndex = Math.Clamp(GetJoy(field), 0, ConfigSettings.ControllerButtons.Length - 1);
        _vsync.IsChecked = _settings.VerticalSync;
        _full.IsChecked = _settings.FullWindow;
        _directInput.IsChecked = _settings.UseDirectInput;
        _interval1.IsChecked = _settings.PresentInterval == 1;
        _interval2.IsChecked = _settings.PresentInterval == 2;
        _interval3.IsChecked = _settings.PresentInterval == 3;
        _font.SelectedItem = ConfigSettings.FontChoices.Contains(_settings.FontType)
            ? _settings.FontType
            : ConfigSettings.FontChoices[0];
        UpdateFontPreview();
    }

    private string GetKey(string field) => field switch
    {
        "Left" => _settings.Left, "Right" => _settings.Right, "Up" => _settings.Up,
        "Down" => _settings.Down, "Slow" => _settings.Slow, "Shoot" => _settings.Shoot,
        _ => _settings.Bomb,
    };

    private void SetKey(string field, string value)
    {
        switch (field)
        {
            case "Left": _settings.Left = value; break;
            case "Right": _settings.Right = value; break;
            case "Up": _settings.Up = value; break;
            case "Down": _settings.Down = value; break;
            case "Slow": _settings.Slow = value; break;
            case "Shoot": _settings.Shoot = value; break;
            default: _settings.Bomb = value; break;
        }
    }

    private int GetJoy(string field) => field switch
    {
        "Skip" => _settings.JoySkip, "Pause" => _settings.JoyPause, "Slow" => _settings.JoySlow,
        "Shoot" => _settings.JoyShoot, _ => _settings.JoyBomb,
    };

    private void SetJoy(string field, int value)
    {
        switch (field)
        {
            case "Skip": _settings.JoySkip = value; break;
            case "Pause": _settings.JoyPause = value; break;
            case "Slow": _settings.JoySlow = value; break;
            case "Shoot": _settings.JoyShoot = value; break;
            default: _settings.JoyBomb = value; break;
        }
    }

    private void BeginCapture(TextBox box)
    {
        _capturing = box;
        _status.Text = "请按下要绑定的按键…";
    }

    private void CaptureKey(TextBox box, KeyEventArgs e)
    {
        if (!ReferenceEquals(_capturing, box)) return;
        string? name = KeyNameMapper.ToGameName(e.Key);
        if (name == null) return;
        var field = _keyBoxes.First(kv => ReferenceEquals(kv.Value, box)).Key;
        SetKey(field, name);
        box.Text = name;
        _capturing = null;
        e.Handled = true;
        _status.Text = $"已设置 {name}（点“应用”写入文件）";
    }

    private void ApplyDirectInputLayout()
    {
        bool useDirect = _directInput.IsChecked == true;
        foreach (var box in _keyBoxes.Values) box.Opacity = useDirect ? 0.5 : 1;
        _controllerSection.IsEnabled = useDirect;
    }

    private void UpdateFontPreview()
    {
        string family = _font.SelectedItem as string ?? ConfigSettings.FontChoices[0];
        _fontPreview.Text = "夏夜祭E比星莲船E难";
        // Avalonia falls back to an installed CJK family when the requested one is absent.
        _fontPreview.FontFamily = new FontFamily(
            $"'{family}', Noto Sans CJK SC, Noto Sans SC, Microsoft YaHei, SimHei, sans-serif");
    }

    private void Persist(bool launchGame, bool announce)
    {
        _settings.PresentInterval = _interval3.IsChecked == true ? 3 : _interval2.IsChecked == true ? 2 : 1;
        _settings.VerticalSync = _vsync.IsChecked == true;
        _settings.FullWindow = _full.IsChecked == true;
        _settings.UseDirectInput = _directInput.IsChecked == true;
        _settings.FontType = _font.SelectedItem as string ?? ConfigSettings.FontChoices[0];
        foreach (var (field, _) in JoyRows) SetJoy(field, Math.Max(0, _joyBoxes[field].SelectedIndex));
        ApplyDirectInputLayout();

        try
        {
            var ini = IniFile.Open(_workspace.IniPath);
            _settings.Save(ini);
            ini.Save();
            if (announce) _status.Text = "配置已保存到 Setting.INI。";
        }
        catch (Exception ex)
        {
            _status.Text = "读取配置文件失败，悲剧啊！" + ex.Message;
            return;
        }

        if (!launchGame) return;
        bool launched = _workspace.LaunchGame(out string message);
        _status.Text = message;
        if (launched) Dispatcher.UIThread.Post(Close);
    }
}
