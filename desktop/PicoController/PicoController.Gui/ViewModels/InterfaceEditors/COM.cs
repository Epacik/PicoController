using Avalonia.Collections;
using PicoController.Gui.Extensions.InterfaceEditors;
using PicoController.Gui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.InterfaceEditors;

public class COM : InterfaceEditorViewModel
{
    public COM(IEnumerable<ReactiveKeyValuePair<string, JsonElement>>? settings)
    {
        if (settings is null)
            return;

        Port     = settings.GetString("port");
        Rate     = settings.GetInt32("rate");
        DataBits = settings.GetInt32("dataBits");
        StopBits = settings.GetInt32("stopBits");
        Parity   = settings.GetInt32("parity");
    }
    public override Dictionary<string, JsonElement> GetInterfaceSettings() =>
        new()
        {
            { "port", JsonSerializer.SerializeToElement(Port) },
            { "rate", JsonSerializer.SerializeToElement(Rate) },
            { "dataBits", JsonSerializer.SerializeToElement(DataBits) },
            { "stopBits", JsonSerializer.SerializeToElement(StopBits) },
            { "parity", JsonSerializer.SerializeToElement(Parity) },
        };

    private string? _port;
    public string? Port
    {
        get => _port;
        set => this.RaiseAndSetIfChanged(ref _port, value);
    }

    private int _rate;
    public int Rate
    {
        get => _rate;
        set => this.RaiseAndSetIfChanged(ref _rate, value);
    }

    private int _dataBits;
    public int DataBits
    {
        get => _dataBits;
        set => this.RaiseAndSetIfChanged(ref _dataBits, value);
    }

    private int _stopBits;
    public int StopBits
    {
        get => _stopBits;
        set => this.RaiseAndSetIfChanged(ref _stopBits, value);
    }

    private int _parity;
    public int Parity
    {
        get => _parity;
        set => this.RaiseAndSetIfChanged(ref _parity, value);
    }
}
