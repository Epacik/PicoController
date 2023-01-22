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
using System.ComponentModel;
using System.Reactive.Linq;

namespace PicoController.Gui;

public class App : Application
{
    private TrayIcon _trayIcon;

    public App()
    {
        DataContext = this;
        _clickResetTimer = new()
        {
            Interval = 400,
            AutoReset = false,
        };
        _clickResetTimer.Elapsed += _clickResetTimer_Elapsed;
    }

    private void _clickResetTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _clickResetTimer.Stop();
        _openWindowAfterNextClick = false;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public IMainWindowViewModel? MainWindowViewModel { get; set; }
    public static IClassicDesktopStyleApplicationLifetime? DesktopApplicationLifetime => Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    private static MainWindow? MainWindow => DesktopApplicationLifetime?.MainWindow as MainWindow;
    private System.Timers.Timer _clickResetTimer;

    private bool _openWindowAfterNextClick = false;
    public void OpenMenu_Click(object? sender, EventArgs e)
    {
        if (MainWindow?.IsVisible == true)
            return;

        if (!_openWindowAfterNextClick)
        {
            _clickResetTimer.Start();
            _openWindowAfterNextClick = true;
        }
        else
        {
            MainWindow?.ShowWindow();
        }
    }

    public void ShowWindow_Click(object? sender, EventArgs e)
    {
        MainWindow?.ToggleWindowVisibility();
    }

    public void ToggleDevices_Click(object? sender, EventArgs e)
    {
        if (MainWindowViewModel is null)
            return;

        MainWindowViewModel.Run = !MainWindowViewModel.Run;
        MainWindowViewModel?.ToggleRunning();

        if (sender is NativeMenuItem item)
        {
            item.Header = (MainWindowViewModel!.Run ? "Stop" : "Start") + " devices";
        }
    }

    public void RestartDevices_Click(object? sender, EventArgs e)
    {
        if(MainWindowViewModel?.Run != true) 
            return;

        MainWindowViewModel?.RestartDevices();
    }

    public async void ExitApp_Click(object? sender, EventArgs e)
    {
        var box = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
            new()
            {
                ContentTitle          = "Are you sure?",
                ContentMessage        = "Do you want to close PicoController?",
                ButtonDefinitions     = MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            });

        var result = await box.ShowDialog(MainWindow!);
        if(result == MessageBox.Avalonia.Enums.ButtonResult.Yes)
        {
            DesktopApplicationLifetime?.Shutdown();
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (DesktopApplicationLifetime is not null)
        {
            MainWindowViewModel = Resolver.GetRequiredService<IMainWindowViewModel>();
            var mainWindow = new MainWindow(Resolver.GetRequiredService<ThemeManager>())
            {
                DataContext = MainWindowViewModel,
            };

            DesktopApplicationLifetime.MainWindow = mainWindow;

            if (MainWindowViewModel is INotifyPropertyChanged mwVmNpc)
            {
                mwVmNpc.PropertyChanged += MainWindowViewModel_PropertyChanged;
            }

            if (MainWindow is INotifyPropertyChanged mwNpc)
            {
                mwNpc.PropertyChanged += MainWindow_PropertyChanged;
            }

            _trayIcon = TrayIcon.GetIcons(this)[0];
            UpdateTrayMenu();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void MainWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindow.IsVisible))
        {
            UpdateTrayMenu();
        }
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IMainWindowViewModel.Run))
        {
            UpdateTrayMenu();
        }
    }

    private void UpdateTrayMenu()
    {
        //// show window
        //if (_trayIcon.Menu!.Items[0] is NativeMenuItem showWindow)
        //{
        //    showWindow.IsEnabled = MainWindow?.IsVisible != true;
        //}

        // Restart Devices
        if (
            _trayIcon.Menu!.Items[1] is NativeMenuItem devices &&
            devices.Menu!.Items[1] is NativeMenuItem restartDevices)
        {
            restartDevices.IsEnabled = MainWindowViewModel?.Run == true;
        }

        // Reload plugins
        if (_trayIcon.Menu!.Items[2] is NativeMenuItem reloadPlugins)
        {
            reloadPlugins.IsEnabled = MainWindowViewModel?.Run != true;
        }
    }

    static IReadonlyDependencyResolver Resolver => Locator.Current;
}
