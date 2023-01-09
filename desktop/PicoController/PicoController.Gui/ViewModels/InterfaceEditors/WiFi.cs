using Avalonia.Collections;
using PicoController.Gui.Extensions.InterfaceEditors;
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
    public WiFi(IEnumerable<ReactiveKeyValuePair<string, JsonElement>>? settings)
    {
        if (settings is null)
            return;

        IP = settings.GetString("ip");
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
