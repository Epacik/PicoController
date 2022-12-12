using Avalonia.Collections;
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
    public COM(AvaloniaList<ReactiveKeyValuePair<string, JsonElement>>? data)
    {
        if (data is null)
            return;

        var element = data.FirstOrDefault(x => x.Key == "port")?.Value;
        if (element?.ValueKind == JsonValueKind.String)
            Port = element?.GetString();

        element = data.FirstOrDefault(x => x.Key == "rate")?.Value;
        if (element?.ValueKind == JsonValueKind.Number && element?.TryGetInt32(out int value) == true)
            Rate = value;

        element = data.FirstOrDefault(x => x.Key == "dataBits")?.Value;
        if (element?.ValueKind == JsonValueKind.Number && element?.TryGetInt32(out value) == true)
            DataBits = value;
        
        element = data.FirstOrDefault(x => x.Key == "stopBits")?.Value;
        if (element?.ValueKind == JsonValueKind.Number && element?.TryGetInt32(out value) == true)
            StopBits = value;

        element = data.FirstOrDefault(x => x.Key == "parity")?.Value;
        if (element?.ValueKind == JsonValueKind.Number && element?.TryGetInt32(out value) == true)
            Parity = value;
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
