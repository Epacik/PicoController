using PicoController.Core;
using System.Management;

namespace PicoController.Core.BuiltInActions.SystemControl;
#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

internal class ScreenBrightness : IPluginAction, IDisposable
{
    private readonly ManagementClass _brightnessMonitoring;
    private readonly ManagementObjectCollection _brightnessMonitoringInstances;

    public ScreenBrightness()
    {
        _brightnessMonitoring = new ManagementClass("WmiMonitorBrightnessMethods")
        {
            Scope = new ManagementScope(@"\\.\root\wmi"),
        };

        _brightnessMonitoringInstances = _brightnessMonitoring.GetInstances();
    }

    

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Yield();

        if (argument is null)
            throw new ArgumentNullException("data");

        if (!int.TryParse(argument, out int value))
            throw new ArgumentException($"'{argument}' is not a valid value, expected an integer");

        foreach (ManagementObject instance in _brightnessMonitoringInstances)
        {
            var currentBrightness = (byte)instance.GetPropertyValue("WmiCurrentBrightness");
            instance.InvokeMethod("WmiSetBrightness", new object[] { 1, currentBrightness + (value * inputValue) });
        }
    }

    public void Dispose()
    {
        _brightnessMonitoringInstances.Dispose();
        _brightnessMonitoring.Dispose();
    }
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS