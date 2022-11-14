using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using PicoController.Gui.Converters;
using PicoController.Gui.Themes;
using Splat;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;

namespace PicoController.Gui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.PropertyChanged += MainWindow_PropertyChanged;

        _themeManager = Locator.Current.GetService<Themes.ThemeManager>();
        if (OperatingSystem.IsWindows())
        {
            var build = Environment.OSVersion.Version.Build;
            TransparencyLevelHint = build switch
            {
                > 20000 => WindowTransparencyLevel.Mica,
                > 10000 => WindowTransparencyLevel.AcrylicBlur,
                _ => WindowTransparencyLevel.None,
            };

            Classes.Add(build switch
            {
                > 20000 => "mica",
                > 10000 => "mica", //"acrylic",
                _ => "",
            });
        }
    }

    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if(e.Property.Name == nameof(FrameSize))
        {
            MainGrid.Classes.Clear();
            MainGrid.Classes.Add(((e.NewValue as Size?)?.Width ?? 0) switch
            {
                <= 650 => "small",
                _      => "normal",
                });
        }
    }

    internal void RestoreWindowState()
    {
        Show();
        ShowInTaskbar = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        ShowInTaskbar = false;
        Hide();
        base.OnClosing(e);
    }

    public static readonly BooleanToGridLengthConverterParameter OutputHeight = new()
    {
        ForTrue = new GridLength(150),
        ForFalse = new GridLength(0)
    };
    private ThemeManager? _themeManager;
}
