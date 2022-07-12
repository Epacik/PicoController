using PicoController.Core.Devices.Inputs;
using System.Text.Json;

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
