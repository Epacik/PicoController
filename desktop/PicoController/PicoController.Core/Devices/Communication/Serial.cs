using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Core.Devices.Communication;
public class Serial : InterfaceBase
{
    private bool _isDisposed;
    private readonly SerialPort _port;

    public Serial(Dictionary<string, JsonElement> connectionData) : base(connectionData)
    {
        _port = new SerialPort
        {
            PortName  = connectionData["port"].GetString() ?? "",
            BaudRate  = connectionData["rate"].GetInt32(),
            DataBits  = connectionData["dataBits"].GetInt32(),
            StopBits  = (StopBits)connectionData["stopBits"].GetInt32(),
            Parity    = (Parity)connectionData["parity"].GetInt32(),
            DtrEnable = true,
        };
    }

    public override void Connect()
    {
        _port.DataReceived += _port_DataReceived;
        _port.Open();
    }

    private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        
        var data = _port.ReadExisting()?.Trim() ?? "";
        var lines = data.Split(new[] { "\r\n" }, StringSplitOptions.None);
        foreach(var line in lines)
        {
            var message = Inputs.InputMessage.Parse(line);
            OnNewMessage(message);
        }
    }

    public override void Disconnect()
    {
        _port.Close();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _port.DataReceived -= _port_DataReceived;
                _port.Dispose();
            }

            // TODO: Zwolnić niezarządzane zasoby (niezarządzane obiekty) i przesłonić finalizator
            // TODO: Ustawić wartość null dla dużych pól
            _isDisposed = true;
        }
    }

    ~Serial()
    {
        Dispose(disposing: false);
    }

    public override void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    
}
