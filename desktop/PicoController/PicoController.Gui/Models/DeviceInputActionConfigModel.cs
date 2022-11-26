namespace PicoController.Gui.Models;

public class DeviceInputActionConfigModel : ReactiveObject
{
    public DeviceInputActionConfigModel() { }
    public DeviceInputActionConfigModel(string? handler, string? data, int? inputValueOverride)
    {
        Handler = handler;
        Data = data;
        InputValueOverride = inputValueOverride;
    }

    private string? _handler;
    public string? Handler
    {
        get => _handler;
        set => this.RaiseAndSetIfChanged(ref _handler, value);
    }

    private string? _Data;
    public string? Data
    {
        get => _Data;
        set => this.RaiseAndSetIfChanged(ref _Data, value);
    }

    private int? _inputValueOverride;
    public int? InputValueOverride
    {
        get => _inputValueOverride;
        set => this.RaiseAndSetIfChanged(ref _inputValueOverride, value);
    }
}