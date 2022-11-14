using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.RemoveLater;

public static class Output
{
    public static event EventHandler<OutputMessageEventArgs>? MessageSent;

    internal static void SendMessage(int deviceId, byte inputId, string actionName)
    {
        MessageSent?.Invoke(null, new OutputMessageEventArgs(deviceId, inputId, actionName));
    }
}

public class OutputMessageEventArgs : EventArgs
{
    public OutputMessageEventArgs(int deviceId, byte inputId, string actionName)
    {
        DeviceId = deviceId;
        InputId = inputId;
        ActionName = actionName;
    }

    public int DeviceId { get; }
    public byte InputId { get; }
    public string ActionName { get; }
}
