using PicoController.Core;
using PicoController.Core.BuiltInActions.Other;
using PicoController.Core.Config;
using Serilog.Formatting.Compact;
using Serilog;
using Serilog.Events;
using System.CommandLine.Invocation;
using PicoController.Core.Devices;

namespace PicoController.Cli;

internal static class App
{
    internal static async Task Run(
        DirectoryInfo? pluginDir,
        IPluginManager pluginManager,
        IDeviceManager deviceManager,
        IConfigRepository configRepository,
        ILocationProvider locationProvider,
        Serilog.ILogger logger)
    {
        var run = true;

        PicoControllerActions.ActionRequested += PicoControllerActions_ActionRequested;
        if (OperatingSystem.IsWindows())
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        var pluginsPath = pluginDir?.FullName;
        while (run)
        {
            var rep = configRepository;
            var config = rep.Read();
            if (config is null)
            {
                await rep.SaveAsync(Config.ExampleConfig());
;
                string path = locationProvider.ConfigPath;
                logger.Error("No config was found, and empty one was created at {Path}", path);
                logger.Error("complete the config and reload");
                logger.Error("Press any key to continue");
                Console.ReadKey();

                continue;
            }

            pluginManager.UnloadPlugins();
            pluginManager.LoadPlugins(pluginsPath);

            _requestedAction = RequestedAction.None;
            _resumeFromSleep = false;

            var devices = await deviceManager.LoadDevicesAsync();
            if (devices is null)
                throw new InvalidOperationException("could not load devices");
            var notLoadedDevices = new List<Core.Devices.Device>();
            try
            {
                Console.Clear();
                foreach (var device in devices)
                {
                    try
                    {
                        device.Connect();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Could not connect to device {Device}\nException: {Ex}", device.ToString(), ex);
                    }
                }


                Log.Logger.Information("Press q to quit, press r to reload");
                while (true)
                {
                    Task<RequestedAction>[] tasks =
                    {
                        GetKeyboard(),
                        GetOnResume(),
                        GetAction(),
                    };

                    var finished = await Task.WhenAny(tasks);

                    var result = await finished;

                    if (result == RequestedAction.Quit)
                        run = false;

                    if (result == RequestedAction.Reload || result == RequestedAction.Quit)
                        break;
                }

                foreach (var device in devices)
                {
                    if (!notLoadedDevices.Contains(device))
                    {
                        try
                        {
                            device.Disconnect();
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error("An error occured while disconnecting from device {Ex}", ex);
                        }
                    }
                }
            }
            finally
            {
                foreach (var device in devices)
                    device.Dispose();
            }
        }
    }

#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą
    private static void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
        if (e.Mode == Microsoft.Win32.PowerModes.Resume)
            _resumeFromSleep = true;

    }
#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif

    private static void PicoControllerActions_ActionRequested(object? sender, RequestedActionEventArgs e)
    {
        _requestedAction = e.RequestedAction;
    }

    static RequestedAction _requestedAction = RequestedAction.None;
    private static async Task<RequestedAction> GetAction()
    {
        await Task.Yield();
        while (_requestedAction == RequestedAction.None) { await Task.Delay(500); }
        return _requestedAction;
    }

    static bool _resumeFromSleep;
    private static async Task<RequestedAction> GetOnResume()
    {
        await Task.Yield();
        while (!_resumeFromSleep) { await Task.Delay(500); }
        return RequestedAction.Reload;
    }

    private static async Task<RequestedAction> GetKeyboard()
    {
        await Task.Yield();
        var key = Console.ReadKey();
        return key.Key switch
        {
            ConsoleKey.R => RequestedAction.Reload,
            ConsoleKey.Q => RequestedAction.Quit,
            _ => RequestedAction.None,
        };
    }
}
