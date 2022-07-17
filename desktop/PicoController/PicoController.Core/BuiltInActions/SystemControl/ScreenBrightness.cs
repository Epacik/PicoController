using PicoController.Plugin;
using System.Management;

namespace PicoController.Core.BuiltInActions.SystemControl;
#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

internal class ScreenBrightness : IPluginAction
{
    public void Execute(string? argument)
    {
        ChangeBrightness(argument);
    }

    public async Task ExecuteAsync(string? argument)
    {
        await Task.Yield();
        ChangeBrightness(argument);
    }

    private void ChangeBrightness(string? argument)
    {
        if (argument is null)
            throw new ArgumentNullException("data");

        if (!int.TryParse(argument, out int value))
            throw new ArgumentException($"'{argument}' is not a valid value, expected an integer");

        using var brightnessMonitoring = new ManagementClass("WmiMonitorBrightnessMethods")
        {
            Scope = new ManagementScope(@"\\.\root\wmi"),
        };

        using var instances = brightnessMonitoring.GetInstances();

        foreach (ManagementObject instance in instances)
        {
            var currentBrightness = (byte)instance.GetPropertyValue("WmiCurrentBrightness");
            instance.InvokeMethod("WmiSetBrightness", new object[] { 1, currentBrightness + value });
        }

    }
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS