using Avalonia.Collections;

namespace PicoController.Gui.Models;


public class DeviceConfigModel : ReactiveObject
{
    private string? _id;
    public string? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private DeviceInterfaceConfigModel? _interface;
    public DeviceInterfaceConfigModel? Interface
    {
        get => _interface;
        set => this.RaiseAndSetIfChanged(ref _interface, value);
    }

    private AvaloniaList<DeviceInputConfigModel>? _inputs;
    public AvaloniaList<DeviceInputConfigModel>? Inputs
    {
        get => _inputs;
        set => this.RaiseAndSetIfChanged(ref _inputs, value);
    }

}
