using Serilog;
using System.IO.Ports;
using System.Text.Json;

namespace PicoController.Core.Devices.Communication;
public class Serial : InterfaceBase
{
    private bool _isDisposed;
    private readonly SerialPort _port;
    private readonly ILogger _logger;

    public Serial(Dictionary<string, JsonElement> connectionData, Serilog.ILogger _logger) : base(connectionData)
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
        this._logger = _logger;
    }

    public override void Connect()
    {
        _port.DataReceived += Port_DataReceived;
        _port.Open();
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        
        var data = _port.ReadExisting()?.Trim() ?? "";

        if(_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            _logger.Verbose("Message received over serial port, {Data}", data);
        var lines = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach(var line in lines)
        {
            var message = Inputs.InputMessage.Parse(line);
            OnNewMessage(message);
        }
    }

    public override void Disconnect()
    {
        _port.DataReceived -= Port_DataReceived;
        _port.Close();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _port.Dispose();
            }

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
