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
    Task<IEnumerable<Device>?> LoadDevicesAsync(Config.Config config, CancellationToken? token = null);
    Task<bool> UnloadDevicesAsync(CancellationToken? token = null);
    ValueTask<IEnumerable<Device>?> ReloadDevicesAsync(CancellationToken? token = null);
}
public class DeviceManager : IDeviceManager
{
    private readonly IPluginManager _pluginLoader;
    private readonly IConfigRepository _repository;
    private readonly IHandlerProvider _handlerProvider;
    private readonly ILogger _logger;
    private bool _disposedValue;
    private List<Device>? _devices;
    public IEnumerable<Device>? Devices => _devices;
    public DeviceManager(
        IPluginManager pluginLoader,
        IConfigRepository repository,
        IHandlerProvider handlerProvider,
        Serilog.ILogger logger)
    {
        _pluginLoader = pluginLoader;
        _repository = repository;
        _handlerProvider = handlerProvider;
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
                Communication.DeviceInterface ifc = device.Interface.Type switch
                {
                    InterfaceType.COM => new Serial(connectionData, _logger),
                    InterfaceType.WiFi => new WiFi(connectionData, _logger),
                    InterfaceType.Bluetooth => new Bluetooth(connectionData, _logger),
                     _ => throw new InvalidDataException($"{device.Interface.Type} is not a valid interface"),
                };

                 var inputs = new List<Inputs.Input>();
                 foreach (Config.Input input in device.Inputs)
                 {
                     inputs.Add(input.Type switch
                     {
                         Inputs.InputType.Button
                             => new Button(
                                 device.Id,
                                 input.Id,
                                 config.MaxDelayBetweenClicks,
                                 _handlerProvider,
                                 _pluginLoader.GetAction,
                                 _logger),

                         Inputs.InputType.Encoder
                             => new Inputs.Encoder(
                                 device.Id,
                                 input.Id,
                                 _handlerProvider,
                                 _pluginLoader.GetAction,
                                 input.Split,
                                 _logger),

                         Inputs.InputType.EncoderWithButton
                             => new EncoderWithButton(
                                 device.Id,
                                 input.Id,
                                 _handlerProvider,
                                 _pluginLoader.GetAction,
                                 config.MaxDelayBetweenClicks,
                                 input.Split,
                                 _logger),

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
                    device.Interface.Disconnect();
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
                // TO DO: clean managed
            }


            // TO DO: clean unmanaged
            // TO DO: null out big fields

            if (_devices is not null)
            {
                foreach (var device in _devices)
                {
                    try
                    {
                        device.Interface.Disconnect();
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
