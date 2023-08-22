using Serilog;
using System.IO.Ports;
using System.Text.Json;

namespace PicoController.Core.Devices.Communication;
public class Serial : DeviceInterface
{
    private bool _isDisposed;
    private SerialPort? _port;
    private readonly ILogger _logger;
    public string PortName { get; }
    private readonly int _baudRate;
    private readonly int _dataBits;
    private readonly StopBits _stopBits;
    private readonly Parity _parity;
    private readonly bool _dtrEnable;

    public Serial(Dictionary<string, JsonElement> connectionData, ILogger _logger) : base(connectionData)
    {
        PortName  = connectionData["port"].GetString() ?? "COM1";
        _baudRate  = connectionData["rate"].GetInt32();
        _dataBits  = connectionData["dataBits"].GetInt32();
        _stopBits  = (StopBits)connectionData["stopBits"].GetInt32();
        _parity    = (Parity)connectionData["parity"].GetInt32();
        _dtrEnable = true;
        
        this._logger = _logger;
    }

    private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        
        var data = _port!.ReadExisting()?.Trim() ?? "";

        if(_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            _logger.Verbose("Message received over serial port, {Data}", data);
        var lines = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach(var line in lines)
        {
            var message = Inputs.InputMessage.Parse(line);
            OnNewMessage(message);
        }
    }

    public override void Connect()
    {
        var availablePorts = SerialPort.GetPortNames();
        if (availablePorts is null || !availablePorts.Contains(PortName))
            return;

        var cleanup = false;
        try
        {
            _port = new SerialPort
            {
                PortName = PortName,
                BaudRate = _baudRate,
                DataBits = _dataBits,
                StopBits = _stopBits,
                Parity = _parity,
                DtrEnable = _dtrEnable,
            };
            _port.DataReceived += Port_DataReceived;
            _port.Open();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An exception occured while starting COM port");
            cleanup = true;
        }
        finally
        {
            if (cleanup && _port is not null)
            {
                _port.DataReceived -= Port_DataReceived;
                _port?.Close();
                _port?.Dispose();
            }
        }
    }

    public override void Disconnect()
    {
        if (_port is null)
            throw new InvalidOperationException("Serial interface wasn't connected");

        _port.DataReceived -= Port_DataReceived;
        _port?.Close();
        _port?.Dispose();
    }

    public override void Reconnect()
    {
        if (_port is not null)
            Disconnect();

        Connect();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _port?.Dispose();
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
