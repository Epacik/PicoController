using Avalonia.Collections;
using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Gui.Models;

public class DeviceInterfaceConfigModel : ReactiveObject
{
    private InterfaceType? _type;
    public InterfaceType? Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private AvaloniaList<ReactiveKeyValuePair<string, JsonElement>>? _data;
    public AvaloniaList<ReactiveKeyValuePair<string, JsonElement>>? Data
    {
        get => _data;
        set => this.RaiseAndSetIfChanged(ref _data, value);
    }
}
