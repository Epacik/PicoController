using PicoController.Core.Devices.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Core.Devices.Communication;

public abstract class InterfaceBase : IDisposable
{
    private readonly Dictionary<string, JsonElement> _connectionData;

    protected InterfaceBase(Dictionary<string, JsonElement> connectionData)
    {
        _connectionData = connectionData;
    }

    public abstract void Connect();
    public abstract void Disconnect();
    public abstract void Dispose();

    public event EventHandler<InterfaceMessageEventArgs>? NewMessage;

    protected void OnNewMessage(InputMessage message)
    {
        NewMessage?.Invoke(this, new InterfaceMessageEventArgs(message));
    }
}

public class InterfaceMessageEventArgs : EventArgs
{
    public readonly Inputs.InputMessage Message;

    public InterfaceMessageEventArgs(InputMessage message)
    {
        Message = message;
    }
}
