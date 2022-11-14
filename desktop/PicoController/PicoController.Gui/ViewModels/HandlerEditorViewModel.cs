using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class HandlerEditorViewModel : ViewModelBase
{
    public HandlerEditorViewModel(KeyValuePair<string, InputAction> handler)
    {
        Handler = handler;
        HandlerName = handler.Key;
        HandlerId = handler.Value.Handler;
        HandlerData = handler.Value.Data;
    }

    public KeyValuePair<string, InputAction> Handler { get; }

    private string? _handlerName;
    public string? HandlerName
    {
        get => _handlerName;
        set => this.RaiseAndSetIfChanged(ref _handlerName, value);
    }

    private string? _handlerId;
    public string? HandlerId
    {
        get => _handlerId;
        set => this.RaiseAndSetIfChanged(ref _handlerId, value);
    }

    private string? _handlerData;
    public string? HandlerData
    {
        get => _handlerData;
        set => this.RaiseAndSetIfChanged(ref _handlerData, value);
    }

    public KeyValuePair<string, InputAction> GetHandler() => new(HandlerName ?? "", new(HandlerId ?? "", HandlerData ?? ""));

}
