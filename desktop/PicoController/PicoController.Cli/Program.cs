using System.Text.Json;
using PicoController.Core;
namespace PicoController.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = Core.Config.Config.Read();
            var devices = Core.Devices.Device.FromConfig(config.Devices);
            try
            {
                foreach (var device in devices)
                    device.Connect();

                Console.WriteLine("Press Enter to quit");
                Console.ReadLine();

                foreach (var device in devices)
                    device.Disconnect();
            }
            finally
            {
                foreach (var device in devices)
                    device.Dispose();
            }
        }
    }
}