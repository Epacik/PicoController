using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Gui.Helpers;
using PicoController.Gui.Plugin;
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
        var limitedList = new LimitedAvaloniaList<LogEventOutput>(500);
        var logger = CreateLogger(limitedList);
        Log.Logger = logger;

        Locator.CurrentMutable.RegisterLazySingleton<Serilog.ILogger>(() => logger);
        Locator.CurrentMutable.RegisterLazySingleton<LimitedAvaloniaList<LogEventOutput>>(() => limitedList, "LogList");
        Locator.CurrentMutable.RegisterLazySingleton<IRepositoryHelper>(() => new RepositoryHelper());
        Locator.CurrentMutable.RegisterLazySingleton<Themes.ThemeManager>(Themes.ThemeManager.CreateManager);
        Locator.CurrentMutable.RegisterLazySingleton<IDisplayInfo>(() => new DisplayInfo());
        Locator.CurrentMutable.RegisterLazySingleton<IConfigRepository>(() => new ConfigRepository());

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
            DesktopApplicationLifetime.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static Serilog.ILogger CreateLogger(LimitedAvaloniaList<LogEventOutput> limitedList)
    {
        var cfgPath = ConfigRepository.ConfigDirectory();
        var jsonFormatter = new CompactJsonFormatter();
        var config = new LoggerConfiguration()
            .WriteTo.Async(
                x => x.File(
                    Path.Combine(cfgPath, "Logs", "Text", "log-.log"),
                    LogEventLevel.Information,
                    rollingInterval: RollingInterval.Hour))
            .WriteTo.Async(
                x => x.File(
                    jsonFormatter,
                    Path.Combine(cfgPath, "Logs", "JSON", "log-.json"),
                    LogEventLevel.Information,
                    rollingInterval: RollingInterval.Hour))
            .WriteTo.Async(
                x => x.EventLog("PicoController GUI", restrictedToMinimumLevel: LogEventLevel.Warning))
            .WriteTo.Async(
                x => x.Observers(
                    ev => ev.Do(e => limitedList.Add(new(e)))
                    .Subscribe()));

        if (Environment.GetEnvironmentVariable("SERILOG_DISCORD_WEBHOOK") is string x)
        {
            var (idStr, token) = x.Split('/');
            if (ulong.TryParse(idStr, out ulong id))
                config.WriteTo.Async(x => x.Discord(id, token, restrictedToMinimumLevel: LogEventLevel.Error));
        }

        return config.CreateLogger();
    }
}
