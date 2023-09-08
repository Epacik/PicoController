using PicoController.Core;
using System.Management;

namespace PicoController.Core.BuiltInActions.SystemControl;
#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

internal class ScreenBrightness : IPluginAction, IDisposable
{
    private readonly ManagementClass _brightnessMonitoring;
    private readonly ManagementObjectCollection _brightnessMonitoringInstances;
    private bool _disposedValue;

    public ScreenBrightness()
    {
        _brightnessMonitoring = new ManagementClass("WmiMonitorBrightnessMethods")
        {
            Scope = new ManagementScope(@"\\.\root\wmi"),
        };

        _brightnessMonitoringInstances = _brightnessMonitoring.GetInstances();
    }



    public async Task ExecuteAsync(int inputValue, string? data)
    {
        await Task.Yield();

        if (data is null)
            throw new ArgumentNullException("data");

        if (!int.TryParse(data, out int value))
            throw new ArgumentException($"'{data}' is not a valid value, expected an integer");

        foreach (ManagementObject instance in _brightnessMonitoringInstances.OfType<ManagementObject>())
        {
            var currentBrightness = (byte)instance.GetPropertyValue("WmiCurrentBrightness");
            instance.InvokeMethod("WmiSetBrightness", new object[] { 1, currentBrightness + (value * inputValue) });
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _brightnessMonitoringInstances.Dispose();
            _brightnessMonitoring.Dispose();
            _disposedValue = true;
        }
    }

    // //  override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~ScreenBrightness()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS