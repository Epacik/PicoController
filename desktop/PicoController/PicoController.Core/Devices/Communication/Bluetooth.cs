using InTheHand.Bluetooth;
using IronPython.Modules;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BT = InTheHand.Bluetooth.Bluetooth;

namespace PicoController.Core.Devices.Communication;

public class Bluetooth : DeviceInterface
{
    private static readonly object _locker = new();
    
    private readonly ILogger _logger;
    private string? _deviceName;
    private readonly Guid _communicationServiceGuid = new("95C26871-655A-11EE-B1A8-108EAB4F90E5");
    private BluetoothLEScan? _scan;
    private BluetoothDevice? _device;
    private bool _connected = false;
    private GattCharacteristic? _characteristic;

    public Bluetooth(Dictionary<string, JsonElement> connectionData, Serilog.ILogger logger)
        : base(connectionData)
    {
        _logger = logger;
        _deviceName = connectionData["deviceName"].GetString();
    }
    public override void Connect()
    {
        BT.AdvertisementReceived += Bluetooth_AdvertisementReceived;
        Task.Run(async () =>
        {
            if (!(await BT.GetAvailabilityAsync()))
                return;

            _logger.Verbose("Starting bluetooth scan");
            _scan = await BT.RequestLEScanAsync();
        })
        .GetAwaiter()
        .GetResult();
    }

    private async void Bluetooth_AdvertisementReceived(object? sender, BluetoothAdvertisingEvent e)
    {
        if (e.Device is null)
            return;

        _logger.Verbose(
            "Advertisement Received for device: {Name} ({Id})",
            e.Device.Name,
            e.Device.Id);


        if (!_connected && e.Device.Name.Trim().Equals(_deviceName))
        {
            _connected = true;
            var device = e.Device;
            device.GattServerDisconnected += Device_GattServerDisconnected;
            

            await device.Gatt.ConnectAsync();
            var service = await device.Gatt.GetPrimaryServiceAsync(_communicationServiceGuid);
            var characteristic = await service.GetCharacteristicAsync(_communicationServiceGuid);
            characteristic.CharacteristicValueChanged += Characteristic_CharacteristicValueChanged;
            await characteristic.StartNotificationsAsync();

            _device = device;
            _characteristic = characteristic;
        }
    }

    private void Characteristic_CharacteristicValueChanged(object? sender, GattCharacteristicValueChangedEventArgs e)
    {
        var value = Encoding.ASCII.GetString(e.Value)?.Trim();
        if (value is null)
            return;
        var message = Inputs.InputMessage.Parse(value);
        OnNewMessage(message);
    }

    private void Device_GattServerDisconnected(object? sender, EventArgs e)
    {
        DisconnectInternal();
    }
    public override void Disconnect()
    {
        _scan?.Stop();
        DisconnectInternal();
    }

    private void DisconnectInternal()
    {
        Task.Run(async () =>
        {
            var ch = _characteristic;
            var de = _device;

            _characteristic = null;
            _device = null;

            _connected = false;

            if (ch is not null)
            {
                ch.CharacteristicValueChanged -= Characteristic_CharacteristicValueChanged;
                try
                {
                    await ch.StopNotificationsAsync();
                }
                catch { }
                ch = null;
            }
            if (de is not null)
            {
                de.GattServerDisconnected -= Device_GattServerDisconnected;
                de = null;
            }
        })
        .GetAwaiter()
        .GetResult();


    }

    public override void Dispose()
    {
    }

    public override void Reconnect()
    {
        _scan?.Stop();
        DisconnectInternal();
        Connect();
    }
}
