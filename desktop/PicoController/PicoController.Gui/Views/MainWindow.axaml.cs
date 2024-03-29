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
            TransparencyLevelHint = new List<WindowTransparencyLevel> 
            {
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.None 
            };

            Classes.Add(build switch
            {
                > 20000 => "mica",
               // > 10000 => "acrylic", //"acrylic",
                _ => "",
            });
        }

        Opened += MainWindow_Opened;
    }

    bool firstOpen = true;

    private void MainWindow_Opened(object? sender, EventArgs e)
    {
        if (firstOpen && App.DesktopApplicationLifetime?.Args?.Contains("--hide") == true)
        {
            Hide();
        }
        firstOpen = false;
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

    internal void ToggleWindowVisibility()
    {
        if (IsVisible)
            HideWindow();
        else
            ShowWindow();
    }

    internal void ShowWindow()
    {
        ShowInTaskbar = true;
        Show();
    }

    private void HideWindow()
    {
        ShowInTaskbar = false;
        Hide();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        //_timer.Start();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        HideWindow();
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
