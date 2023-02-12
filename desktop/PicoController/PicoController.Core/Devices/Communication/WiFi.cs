using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using static IronPython.Runtime.Profiler;

namespace PicoController.Core.Devices.Communication;

internal class WiFi : InterfaceBase
{
    private CancellationTokenSource? _calcallationTokenSource;
    private Thread? _listeningThread;
    private readonly int _port;
    private readonly IPAddress _address;
    private readonly ILogger _logger;

    public WiFi(Dictionary<string, JsonElement> connectionData, Serilog.ILogger _logger) : base(connectionData)
    {
        _port = connectionData["port"].TryGetInt32(out int value) ? value : 9420;
        _address = IPAddress.Parse(connectionData["ip"].GetString() ?? "127.0.0.1");
        this._logger = _logger;
    }

    public override void Dispose()
    {
    }

    public override void Connect()
    {
        if (_listeningThread is not null)
            return;

        _calcallationTokenSource = new CancellationTokenSource();
        _listeningThread = new Thread(Listen)
        {
            IsBackground = true,
        };
        _listeningThread.Start();
    }

    public override void Disconnect()
    {
        if (_listeningThread is null)
            return;

        _calcallationTokenSource?.Cancel();
    }

    private async void Listen(object? obj)
    {
        var token = _calcallationTokenSource!.Token;
        var listener = new TcpListener(_address, _port);
        listener.Start();
        var utf8 = Encoding.UTF8;
        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(token);
                var buffer = new byte[4096];
                var stream = client.GetStream();
                var length = stream.Read(buffer, 0, buffer.Length);
                var data = utf8.GetString(buffer, 0, length);

                if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                    _logger.Verbose("Message received over network, {Data}", data);

                var lines = data.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (token.IsCancellationRequested)
                        return;
                    var message = Inputs.InputMessage.Parse(line);
                    OnNewMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch(Exception ex)
            {
                _logger.Warning("An error occured while receiving data over network {Ex}", ex);
            }
        }
    }
}
