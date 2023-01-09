using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Gui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class HandlerEditorViewModel : ViewModelBase
{
    public HandlerEditorViewModel(ReactiveKeyValuePair<string, DeviceInputActionConfigModel> handler, IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        Handler            = handler;
        HandlerName        = handler.Key;
        HandlerId          = handler.Value!.Handler;
        HandlerData        = handler.Value!.Data;
        OverrideValue      = handler.Value!.InputValueOverride is not null;
        InputValueOverride = handler.Value!.InputValueOverride ?? 0;
    }

    public ReactiveKeyValuePair<string, DeviceInputActionConfigModel> Handler { get; }

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

    private bool _overrideValue;
    public bool OverrideValue
    {
        get => _overrideValue;
        set => this.RaiseAndSetIfChanged(ref _overrideValue, value);
    }

    private int _inputValueOverride;
    public int InputValueOverride
    {
        get => _inputValueOverride;
        set => this.RaiseAndSetIfChanged(ref _inputValueOverride, value);
    }


    public ReactiveKeyValuePair<string, DeviceInputActionConfigModel> GetHandler() =>
        new(HandlerName ?? "",new(HandlerId ?? "", HandlerData ?? "", OverrideValue ? InputValueOverride : null));

    private readonly IPluginManager _pluginManager;

    private AvaloniaList<string> _handlers = new();
    public AvaloniaList<string> Handlers
    {
        get => _handlers;
        set => this.RaiseAndSetIfChanged(ref _handlers, value);
    }

    public void Reload()
    {
        Handlers.Clear();
        Handlers.AddRange(_pluginManager.AllAvailableActions());
    }

}
