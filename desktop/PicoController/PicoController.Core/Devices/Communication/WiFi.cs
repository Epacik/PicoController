using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
