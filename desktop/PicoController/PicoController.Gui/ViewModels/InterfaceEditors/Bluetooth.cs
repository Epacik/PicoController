using Avalonia.Collections;
using PicoController.Gui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.InterfaceEditors;

public class Bluetooth : InterfaceEditorViewModel
{
    public Bluetooth(AvaloniaList<ReactiveKeyValuePair<string, JsonElement>>? data)
    {
    }

    public override Dictionary<string, JsonElement> GetInterfaceSettings() =>
        new()
        {
        };
}
