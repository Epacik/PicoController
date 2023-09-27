using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Gui.Controls.Editors;
using PicoController.Gui.Models;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        var editor = GetEditorForHandler(HandlerId);

        if (editor is null) 
            return;

        try
        {
            var window = new Window()
            {
                Title = "PicoController: Value editor",
                Content = editor,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };

            if (editor is Control control)
            {
                window.MinHeight = control.MinHeight;
                window.MinWidth = control.MinWidth;
                window.Height = control.Height;
                window.Width = control.Width;
                window.MaxHeight = control.MaxHeight;
                window.MaxWidth = control.MaxWidth;

                if (window.MaxHeight - window.MinHeight < double.Epsilon ||
                    window.MaxWidth - window.MinWidth < double.Epsilon)
                {
                    window.CanResize = false;
                }

                control.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            }



#if DEBUG
        window.AttachDevTools();
#endif

            await window.ShowDialog(App.MainWindow!);

            HandlerData = editor.GetValue();
        }
        finally
        {
            if(editor is IDisposable disposable)
                disposable.Dispose();
        }
    }

    public void Reload()
    {
        Handlers.Clear();
        Handlers.AddRange(_pluginManager.GetAllAvailableActions());
    }


    private IEditor? GetEditorForHandler(string? handlerName)
    {
        var builtIn = _pluginManager.GetBuiltInActions();

        if (builtIn.Contains(handlerName))
        {
            return GetEditorForBuiltInHandler(handlerName);
        }

        return GetEditorForPluginHandler(handlerName);
    }

    private IEditor? GetEditorForBuiltInHandler(string? handlerName)
    {
        return handlerName switch
        {
            "/IronPython" => new IronPythonCodeEditor(HandlerData!),
            "/IronPythonFile" => new IronPythonFileCodeEditor(HandlerData!),
            "/CsScript" => new CodeEditor(HandlerData!, ".cs"),
            "/CsScriptFile" => new FileCodeEditor(HandlerData!, ".cs"),
            "/NeoLua" => new CodeEditor(HandlerData!, ".lua"),
            "/NeoLuaFile" => new FileCodeEditor(HandlerData!, ".lua"),
            "/RunProgram" => new RunProgramEditor(HandlerData!),
            _ => null,
        };
    }

    private IEditor? GetEditorForPluginHandler(string? handlerName)
    {
        var typeInfo = _pluginManager.GetPluginHandlerInfo(handlerName);

        if (typeInfo is null)
            return null;

        var attributes = typeInfo.CustomAttributes;

        var editor = attributes
            .FirstOrDefault(x => x.AttributeType == typeof(CodeEditorAttribute));
        if (editor is not null)
        {
            var extension = editor.ConstructorArguments.FirstOrDefault().Value as string;
            return extension is not null ? new CodeEditor(HandlerData!, extension) : null;
        }

        editor = attributes
            .FirstOrDefault(x => x.AttributeType == typeof(FileCodeEditorAttribute));
        if (editor is not null)
        {
            var extension = editor.ConstructorArguments.FirstOrDefault().Value as string;
            return extension is not null ? new FileCodeEditor(HandlerData!, extension) : null;
        }

        return null;
    }
}
