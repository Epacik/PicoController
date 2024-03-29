﻿using PicoController.Core.Config;
using Serilog.Formatting.Compact;
using Serilog;
using Splat;
using PicoController.Gui.ViewModels.Devices;
using Serilog.Events;
using System.Reactive.Linq;
using PicoController.Core;
using Serilog.Sinks.Discord;
using PicoController.Core.Extensions;
using PicoController.Gui.ViewModels;
using PicoController.Core.Devices;
using PicoController.Gui.Helpers;
using PicoController.Gui.Plugin;
using PicoController.Plugin;
using CircularBuffer;
using PicoController.Core.Misc;
using Serilog.Core;

namespace PicoController.Gui.DependencyInjection;

public static class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver, string? customMainDir)
    {
        RegisterBasics(services, resolver);
        RegisterLogging(services, resolver);
        Core.DependencyInjection.Bootstrapper.Register(services, resolver, customMainDir);
        RegisterViewModels(services, resolver);

        services.RegisterLazySingleton(Themes.ThemeManager.CreateManager);
        services.RegisterLazySingleton<IDisplayInfo>(() => new DisplayInfo(resolver.GetService<Serilog.ILogger>()));
        services.RegisterLazySingleton<IStorage>(() => new Storage());
        services.InitializeSplat();
    }

    private static void RegisterBasics(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IRepositoryHelper>(() => new RepositoryHelper(
            resolver.GetRequiredService<IConfigRepository>()
        ));
    }

    private static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<IPluginManager>(),
            resolver.GetRequiredService<IDeviceManager>(),
            resolver.GetRequiredService<IRepositoryHelper>(),
            resolver.GetRequiredService<ObservableCircularBuffer<LogEventOutput>>("LogList"),
            resolver.GetService<Serilog.ILogger>(),
            resolver.GetRequiredService<LoggingLevelSwitch>("DisplayLogLevelSwitch")
        ));
    }

    private static void RegisterLogging(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton(() => new ObservableCircularBuffer<LogEventOutput>(500), "LogList");

        services.RegisterLazySingleton(() => new Serilog.Core.LoggingLevelSwitch(), "DisplayLogLevelSwitch");

        services.RegisterLazySingleton<Serilog.ILogger>(() =>
        {
            var repo = resolver.GetRequiredService<ILocationProvider>();
            var logsDir = repo.LogsDirectoryPath;
            var jsonLogsDir = repo.JsonLogsDirectoryPath;

            var jsonFormatter = new CompactJsonFormatter();
            var config = new LoggerConfiguration()
                .WriteTo.Async(
                    x => x.File(
                        Path.Combine(logsDir, "log-.log"),
                        LogEventLevel.Information,
                        rollingInterval: RollingInterval.Hour))

                .WriteTo.Async(
                    x => x.File(
                        jsonFormatter,
                        Path.Combine(jsonLogsDir, "log-.json"),
                        LogEventLevel.Information,
                        rollingInterval: RollingInterval.Hour));

            //.WriteTo.Async(
            //    x => x.EventLog(
            //        "PicoController.GUI",
            //        "PicoController"));

            
            var limitedList = resolver.GetService<ObservableCircularBuffer<LogEventOutput>>("LogList");

            var levelSwitch = resolver.GetService<LoggingLevelSwitch>("DisplayLogLevelSwitch");
            if (limitedList is not null)
            {
                config.WriteTo.Async(
                    x => x.Observers(
                        ev => ev
                            .Do(e => limitedList.PushFront(new(e)))
                            .Subscribe(),
                        LogEventLevel.Verbose,
                        levelSwitch));
            }

            if (Environment.GetEnvironmentVariable("SERILOG_DISCORD_WEBHOOK") is string x)
            {
                var (idStr, token) = x.Split('/');
                if (ulong.TryParse(idStr, out ulong id))
                    config.WriteTo.Async(x => x.Discord(id, token, restrictedToMinimumLevel: LogEventLevel.Error));
            }

            return config.CreateLogger();
        });
    }
}
