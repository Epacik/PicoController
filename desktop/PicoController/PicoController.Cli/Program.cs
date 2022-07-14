using System.Text.Json;
using PicoController.Core;
namespace PicoController.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var run = true;
            while (run)
            {
                var config = Core.Config.Config.Read();
                if (config is null)
                {
                    Core.Config.Config.SaveExampleConfig();
                    Console.WriteLine($"No config was found, and empty one was created at {Core.Config.Config.ConfigPath()}");
                    Console.WriteLine("complete the config and reload");
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();

                    continue;
                }
                Plugins.UnloadPlugins();
                Plugins.LoadPlugins();
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
                        }
                        catch (Exception ex)
                        {
                            PrintInColor($"Could not connect to device {device.ToString()}\nException: {ex.Message}", ConsoleColor.Red);
                        }
                    }


                    Console.WriteLine("Press q to quit, press r to reload");
                    while (true)
                    {
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.Q)
                            run = false;

                        if (key.Key == ConsoleKey.R || key.Key == ConsoleKey.Q)
                            break;
                    }

                    foreach (var device in devices)
                    {
                        if (!notLoadedDevices.Contains(device))
                            device.Disconnect();
                    }
                }
                finally
                {
                    foreach (var device in devices)
                        device.Dispose();
                }
            }
        }

        private static void PrintInColor(string message, ConsoleColor foreground, ConsoleColor? background = null)
        {
            var tempFg = Console.ForegroundColor;
            var tempBg = Console.BackgroundColor;
            Console.ForegroundColor = foreground;

            if (background is not null)
                Console.BackgroundColor = (ConsoleColor)background;

            Console.WriteLine(message);
            Console.ForegroundColor = tempFg;
            Console.BackgroundColor = tempBg;
        }
    }
}