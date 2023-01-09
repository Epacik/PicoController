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
    }
    public MainWindow(Themes.ThemeManager themeManager) : this()
    {

        this.PropertyChanged += MainWindow_PropertyChanged;

        _themeManager = themeManager;
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        //_timer.Start();
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
    //private int index = 0;
    //private readonly DispatcherTimer? _timer;
    private ThemeManager? _themeManager;

}
