using PicoController.Core;
using PicoController.Core.BuiltInActions.Other;

namespace PicoController.Cli;

internal static class DefaultBehavior
{
    internal static void Run()
    {
        var run = true;

        PicoControllerActions.ActionRequested += PicoControllerActions_ActionRequested;
#if OS_WINDOWS
#pragma warning disable CA1416 // Platform compatibility validation
        Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
#pragma warning restore CA1416 // Platform compatibility validation
#endif

        while (run)
        {
            var rep = new Core.Config.ConfigRepository();
            var config = rep.Read();
            if (config is null)
            {
                rep.Save(Core.Config.Config.ExampleConfig());

                Console.WriteLine($"No config was found, and empty one was created at {Core.Config.ConfigRepository.ConfigPath()}");
                Console.WriteLine("complete the config and reload");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                continue;
            }

            Plugins.UnloadPlugins();
            Plugins.LoadPlugins();

            _requestedAction = RequestedAction.None;
            _resumeFromSleep = false;

            var devices = Core.Devices.Device.FromConfig(config);
            var notLoadedDevices = new List<Core.Devices.Device>();
            try
            {
                Console.Clear();
                foreach (var device in devices)
                {
                    try
                    {
                        device.Connect();
                        device.ActionThrownAnException += Device_ActionThrownAnException;
                    }
                    catch (Exception ex)
                    {
                        Program.PrintInColor($"Could not connect to device {device.ToString()}\nException: {ex.Message}", ConsoleColor.Red);
                    }
                }


                Console.WriteLine("Press q to quit, press r to reload");
                while (true)
                {
                    Task<RequestedAction>[] tasks =
                    {
                        GetKeyboard(),
                        GetOnResume(),
                        GetAction(),
                    };

                    var finished = Task.WaitAny(tasks);

                    var result = tasks[finished].GetAwaiter().GetResult();

                    if (result == RequestedAction.Quit)
                        run = false;

                    if (result == RequestedAction.Reload || result == RequestedAction.Quit)
                        break;
                }

                foreach (var device in devices)
                {
                    if (!notLoadedDevices.Contains(device))
                    {
                        device.ActionThrownAnException -= Device_ActionThrownAnException;
                        device.Disconnect();
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

    private static void Device_ActionThrownAnException(object? sender, PluginActionExceptionEventArgs e)
    {
        Program.PrintInColor("An action thrown an exception!\n" +
            $"Device:    {e.DeviceNumber}, Input: {e.InputId}\n" +
            $"Action:    {e.ActionName}\n" +
            $"Exception: {e.Exception.Message}\n",

            ConsoleColor.Yellow);
    }
}
