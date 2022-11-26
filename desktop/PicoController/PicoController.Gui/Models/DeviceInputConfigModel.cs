using Avalonia.Collections;
using PicoController.Core.Devices.Inputs;

namespace PicoController.Gui.Models;

public class DeviceInputConfigModel : ReactiveObject
{
    private byte _id;
    public byte Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private InputType _type;
    public InputType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    private bool _split;
    public bool Split
    {
        get => _split;
        set => this.RaiseAndSetIfChanged(ref _split, value);
    }

    private AvaloniaList<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>? _actions;
    public AvaloniaList<ReactiveKeyValuePair<string, DeviceInputActionConfigModel>>? Actions
    {
        get => _actions;
        set => this.RaiseAndSetIfChanged(ref _actions, value);
    }

}
