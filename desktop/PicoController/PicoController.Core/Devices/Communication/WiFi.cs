using System.Text.Json;

namespace PicoController.Core.Devices.Communication;

internal class WiFi : InterfaceBase
{
    public WiFi(Dictionary<string, JsonElement> connectionData) : base(connectionData)
    {
    }

    public override void Dispose()
    {
    }

    public override void Connect()
    {
    }

    public override void Disconnect()
    {
    }
}
