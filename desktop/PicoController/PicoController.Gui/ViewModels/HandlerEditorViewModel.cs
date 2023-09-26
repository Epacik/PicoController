using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Gui.Controls.Editors;
using PicoController.Gui.Models;
using PicoController.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class HandlerEditorViewModel : ViewModelBase
{
    public HandlerEditorViewModel(ReactiveKeyValuePair<string, DeviceInputActionConfigModel> handler, IPluginManager pluginManager)
    {
        _pluginManager     = pluginManager;
        Handler            = handler;
        HandlerName        = handler.Key;
        HandlerId          = handler.Value!.Handler;
        HandlerData        = handler.Value!.Data;
        OverrideValue      = handler.Value!.InputValueOverride is not null;
        InputValueOverride = handler.Value!.InputValueOverride ?? 0;

        OpenEditorCommand = ReactiveCommand.Create(OpenEditor);

        Reload();
    }
    private readonly IPluginManager _pluginManager;

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
        set
        {
            this.RaiseAndSetIfChanged(ref _handlerId, value);
            HasEditor = value is not null && GetEditorForHandler(value) is not null;
        }
    }

    private bool _hasEditor = false;
    public bool HasEditor
    {
        get => _hasEditor;
        set => this.RaiseAndSetIfChanged(ref _hasEditor, value);
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
        new(HandlerName ?? "", new(HandlerId ?? "", HandlerData ?? "", OverrideValue ? InputValueOverride : null));

    private AvaloniaList<string> _handlers = new();
    public AvaloniaList<string> Handlers
    {
        get => _handlers;
        set => this.RaiseAndSetIfChanged(ref _handlers, value);
    }
    
    public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }

    private async void OpenEditor()
    {
        var type = GetEditorForHandler(HandlerId);

        var ctor = type.GetConstructors()
            .FirstOrDefault(x =>
            {
                var param = x.GetParameters();
                if (param?.Length != 1)
                    return false;

                if (param[0].ParameterType != typeof(string))
                    return false;
                return true;
            });

        if (ctor is null)
            return;

        var control = ctor.Invoke(new[] { HandlerData }) as IEditor;

        if (control is null) 
            return;

        var window = new Window()
        {
            Title = "PicoController: Value editor",
            Content = control,
        };

#if DEBUG
        window.AttachDevTools();
#endif

        await window.ShowDialog(App.MainWindow!);

        HandlerData = control.GetValue();
    }

    public void Reload()
    {
        Handlers.Clear();
        Handlers.AddRange(_pluginManager.GetAllAvailableActions());
    }


    private Type? GetEditorForHandler(string? handlerName)
    {
        var builtIn = _pluginManager.GetBuiltInActions();

        if (builtIn.Contains(handlerName))
        {
            return GetEditorForBuiltInHandler(handlerName);
        }

        return null;
    }

    private Type? GetEditorForBuiltInHandler(string? handlerName)
        => handlerName switch
        {
            "/IronPython" => typeof(IronPythonCodeEditor),
            _ => null,
        };
}
