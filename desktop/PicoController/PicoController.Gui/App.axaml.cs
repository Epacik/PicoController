using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Core.Extensions;
using PicoController.Gui.Helpers;
using PicoController.Gui.Plugin;
using PicoController.Gui.Themes;
using PicoController.Gui.ViewModels;
using PicoController.Gui.ViewModels.Devices;
using PicoController.Gui.Views;
using PicoController.Plugin;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Discord;
using Splat;
using System;
using System.Reactive.Linq;

namespace PicoController.Gui;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static IClassicDesktopStyleApplicationLifetime? DesktopApplicationLifetime => Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    private static MainWindow? MainWindow => DesktopApplicationLifetime?.MainWindow as MainWindow;

    public void ShowWindow_Click(object? sender, EventArgs e)
    {
        MainWindow?.RestoreWindowState();
    }

    public void StartStopDevices_Click(object? sender, EventArgs e)
    {
        //var isRunning = MainWindow?.DevicesRunning ?? false;
        //var startStop = this
        //if (isRunning)
        //{
    
        //    StartStopMenuItem.Header == "Start Devices";
        //} 
    }

    public void RestartDevices_Click(object? sender, EventArgs e)
    {
        MainWindow?.RestoreWindowState();
    }

    public void ExitApp_Click(object? sender, EventArgs e)
    {
        DesktopApplicationLifetime?.Shutdown();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (DesktopApplicationLifetime is not null)
        {
            DesktopApplicationLifetime.MainWindow = new MainWindow(Resolver.GetRequiredService<ThemeManager>())
            {
                DataContext = Resolver.GetRequiredService<IMainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    static IReadonlyDependencyResolver Resolver => Locator.Current;
}
