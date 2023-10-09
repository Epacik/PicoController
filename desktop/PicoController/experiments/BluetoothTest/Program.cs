// See https://aka.ms/new-console-template for more information
using InTheHand.Bluetooth;
using System.Reflection.PortableExecutable;

Console.WriteLine("Hello, World!");

var uuid = new Guid("95C26871-655A-11EE-B1A8-108EAB4F90E5");
BluetoothDevice? device = null;
GattCharacteristic? characteristic = null;

Bluetooth.AdvertisementReceived += Bluetooth_AdvertisementReceived;


async void Bluetooth_AdvertisementReceived(object? sender, BluetoothAdvertisingEvent e)
{
    if (e?.Device?.Name is null)
        return;
    var isPico = e.Device.Name.StartsWith("Pico") || e.Device.Id == "28CDC10DA7A7";
    if (!isPico || e.Device.Gatt.IsConnected || device is not null)
        return;

    device = e.Device;
    device.GattServerDisconnected += Device_GattServerDisconnected;
    await device.Gatt.ConnectAsync();

    var service = await device.Gatt.GetPrimaryServiceAsync(uuid);
    characteristic = await service.GetCharacteristicAsync(uuid);
    characteristic.CharacteristicValueChanged += Characteristic_CharacteristicValueChanged;
    await characteristic.StartNotificationsAsync();
}

void Characteristic_CharacteristicValueChanged(object? sender, GattCharacteristicValueChangedEventArgs e)
{
    Console.WriteLine(System.Text.Encoding.ASCII.GetString(e.Value));
}

async void Device_GattServerDisconnected(object? sender, EventArgs e)
{
    var ch = characteristic;
    var de = device;
    characteristic = null;
    device = null;
    if (ch is not null)
    {
        ch.CharacteristicValueChanged -= Characteristic_CharacteristicValueChanged;
        try
        {
            await ch.StopNotificationsAsync();
        }
        catch {}
        ch = null;
    }
    if (de is not null)
    {
        de.GattServerDisconnected -= Device_GattServerDisconnected;
        de = null;
    }
}

var options = new BluetoothLEScanOptions()
{
    AcceptAllAdvertisements = true,
};
var scan = await Bluetooth.RequestLEScanAsync(options);

//var discoveredDevices = await Bluetooth.ScanForDevicesAsync();
//Console.WriteLine($"found {discoveredDevices?.Count} devices\n\nDevices:");

//var device = discoveredDevices?.FirstOrDefault(x => x.Name == "PicoController");

//if (device is null)
//    return;


//await device.Gatt.ConnectAsync();
//var srv = await device.Gatt.GetPrimaryServicesAsync();

//var services = srv.Select(x => x.Uuid.ToString());

//Console.WriteLine($"Name: {device.Name}\nId: {device.Id}\nServices\n\t{string.Join("\n\t", services)}");

//var service = srv.FirstOrDefault(x => x.Uuid == uuid);

//if (service is not null)
//{
//    //service.
//}

//device.Gatt.Disconnect();

Console.ReadLine();

scan.Stop();