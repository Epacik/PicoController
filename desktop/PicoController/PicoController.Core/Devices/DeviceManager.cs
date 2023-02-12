using PicoController.Core.Config;
using PicoController.Core.Devices.Communication;
using PicoController.Core.Devices.Inputs;
using PicoController.Core.Extensions;
using Serilog;
using Serilog.Events;
using Usb.Events;

namespace PicoController.Core.Devices;

public interface IDeviceManager : IDisposable
{
    ValueTask<IEnumerable<Device>?> LoadDevicesAsync(CancellationToken? token = null);
    Task<bool> UnloadDevicesAsync(CancellationToken? token = null);
    ValueTask<IEnumerable<Device>?> ReloadDevicesAsync(CancellationToken? token = null);
    Task<IEnumerable<Device>?> LoadDevicesAsync(Config.Config config, CancellationToken? token = null);
}
public class DeviceManager : IDeviceManager
{
    private readonly IPluginManager _pluginLoader;
    private readonly IConfigRepository _repository;
    private readonly ILogger _logger;
    private bool _disposedValue;
    private List<Device>? _devices;
    public IEnumerable<Device>? Devices => _devices;
    public DeviceManager(IPluginManager pluginLoader, IConfigRepository repository, Serilog.ILogger logger)
    {
        _pluginLoader = pluginLoader;
        _repository = repository;
        _logger = logger;
    }

    public async ValueTask<IEnumerable<Device>?> ReloadDevicesAsync(CancellationToken? token = null)
    {

        if (_logger.ExistsAndIsEnabled(LogEventLevel.Information))
            _logger?.Information("Reloading devices");

        if (await UnloadDevicesAsync(token))
            return await LoadDevicesAsync(token);

        return null;
    }

    public Task<IEnumerable<Device>?> LoadDevicesAsync(Config.Config config, CancellationToken? token = null)
    {
        return Task.Run<IEnumerable<Device>?>(() =>
        {
             if (_devices is not null)
                 return _devices;

             if (config is null)
             {
                 return null;
             }
             var devices = config.Devices;
             var result = new List<Devices.Device>();

             for (int deviceId = 0; deviceId < devices.Count; deviceId++)
             {
                 Config.Device device = devices[deviceId];

                 var connectionData = device.Interface.Data;
                 InterfaceBase ifc = device.Interface.Type switch
                 {
                     InterfaceType.COM => new Serial(connectionData, _logger),
                     InterfaceType.WiFi => new WiFi(connectionData, _logger),
                     _ => throw new InvalidDataException($"{device.Interface.Type} is not a valid interface"),
                 };

                 var inputs = new List<Inputs.Input>();
                 foreach (Config.Input input in device.Inputs)
                 {
                     var actions = input.Actions.ToDictionary(x => x.Key, x => _pluginLoader.LookupActions(x.Value));

                     inputs.Add(input.Type switch
                     {
                         Inputs.InputType.Button
                             => new Button(deviceId, input.Id, actions, config.MaxDelayBetweenClicks, _logger),
                         Inputs.InputType.Encoder
                             => Inputs.Encoder.Create(deviceId, input.Id, actions, input.Split, _logger),
                         Inputs.InputType.EncoderWithButton
                             => EncoderWithButton.Create(deviceId, input.Id, actions, config.MaxDelayBetweenClicks, input.Split, _logger),
                         _ => throw new InvalidDataException($"Invalid input type {input.Type}"),
                     });
                 }

                 result.Add(new Device(ifc, inputs));

             }
             _devices = result;
             return result;
         });
    }

    public async ValueTask<IEnumerable<Device>?> LoadDevicesAsync(CancellationToken? token = null)
    {
        if (_devices is not null)
            return _devices;

        var config = await _repository.ReadAsync(token);
        if (config is null)
        {
            return null;
        }

        return await LoadDevicesAsync(config, token);
    }


    public Task<bool> UnloadDevicesAsync(CancellationToken? token = null)
        => Task.Run(() =>
        {
            if (_devices is null)
                return false;
            foreach (var device in _devices)
            {
                try
                {
                    device.Disconnect();
                }
                finally
                {
                    device.Dispose();
                }
            }
            _devices = null;

            return true;
        });

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: clean managed
            }


            // TODO: clean unmanaged
            // TODO: null out big fields

            if (_devices is not null)
            {
                foreach (var device in _devices)
                {
                    try
                    {
                        device.Disconnect();
                    }
                    finally
                    {
                        device.Dispose();
                    }
                }
            }

            _disposedValue = true;
        }
    }

    ~DeviceManager()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
