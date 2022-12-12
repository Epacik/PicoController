using Avalonia.Collections;
using PicoController.Gui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.InterfaceEditors;

internal class WiFi : InterfaceEditorViewModel
{
    public WiFi(AvaloniaList<ReactiveKeyValuePair<string, JsonElement>>? data)
    {
        if (data is null)
            return;

        var element = data.FirstOrDefault(x => x.Key == "ip")?.Value;
        if (element?.ValueKind == JsonValueKind.String)
            IP = element?.GetString();
    }

    public override Dictionary<string, JsonElement> GetInterfaceSettings() =>
        new()
        {
            { "ip", JsonSerializer.SerializeToElement(IP) },
        };

    private string? _ip;
    public string? IP
    {
        get => _ip;
        set => this.RaiseAndSetIfChanged(ref _ip, value);
    }

}
