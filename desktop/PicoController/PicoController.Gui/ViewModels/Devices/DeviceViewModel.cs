using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Mapster;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Core.Extensions;
using PicoController.Gui.Helpers;
using PicoController.Gui.Models;
using PicoController.Gui.ViewModels.InterfaceEditors;
using ReactiveUI;
using System.Reactive;

namespace PicoController.Gui.ViewModels.Devices;

public class DeviceViewModel : ViewModelBase
{
    public DeviceViewModel(IRepositoryHelper repositoryHelper, IPluginManager pluginManager)
        : this(Config.ExampleConfig(1).Devices[0].Adapt<DeviceConfigModel>(), repositoryHelper, pluginManager) { }

    public DeviceViewModel(DeviceConfigModel device, IRepositoryHelper repositoryHelper, IPluginManager pluginManager)
    {
        Device = device;
        Inputs = device.Inputs!;
        _repositoryHelper = repositoryHelper;
        _pluginManager = pluginManager;
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;
        SelectedInputChangedCommand = ReactiveCommand.Create<SelectionChangedEventArgs>(SelectedInputChanged);
        SwitchChangedCommand = ReactiveCommand.Create<DeviceInputConfigModel>(SwitchChanged);
    }

    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //if(e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        //{
        //    this.RaisePropertyChanged(nameof(Device));
        //    Device.Inputs = _repositoryHelper.WorkingConfigCopy?.Devices
        //        ?.FirstOrDefault(x => x.Id == Device.Id)?.Inputs
        //        ?.Adapt<AvaloniaList<DeviceInputConfigModel>>();

        //    Inputs = Device.Inputs;
        //    this.RaisePropertyChanged(nameof(Inputs));
        //}
    }

    public DeviceConfigModel Device { get; }
    private readonly IRepositoryHelper _repositoryHelper;
    private readonly IPluginManager _pluginManager;

    //public Input[] Inputs => Device.Inputs;

    private AvaloniaList<DeviceInputConfigModel>? _inputs;
    public AvaloniaList<DeviceInputConfigModel>? Inputs
    {
        get => _inputs;
        set => this.RaiseAndSetIfChanged(ref _inputs, value);
    }

    public InterfaceType[] InterfaceTypes => _interfaceTypes;
    private InterfaceType[] _interfaceTypes =
    {
        InterfaceType.Bluetooth,
        InterfaceType.COM,
        InterfaceType.WiFi,
    };

    private InterfaceType _selectedInterfaceType;
    public InterfaceType SelectedInterfaceType
    {
        get => _selectedInterfaceType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedInterfaceType, value);

            var data = Device.Interface?.Data;

            InterfaceEditor = _selectedInterfaceType switch
            {
                InterfaceType.COM => new COM(data),
                InterfaceType.WiFi => new WiFi(data),
                InterfaceType.Bluetooth => new Bluetooth(data),
                _ => null,
            };
        }
    }

    private InterfaceEditorViewModel? _interfaceEditor;
    public InterfaceEditorViewModel? InterfaceEditor
    {
        get => _interfaceEditor;
        set => this.RaiseAndSetIfChanged(ref _interfaceEditor, value);
    }

    public ReactiveCommand<SelectionChangedEventArgs, Unit> SelectedInputChangedCommand { get; }
    public async void SelectedInputChanged(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0 || args.AddedItems[0] is not ReactiveKeyValuePair<string, DeviceInputActionConfigModel>)
        {
            return;
        }

        args.Handled = true;
        (args.Source as ListBox)!.UnselectAll();

        var handler = (ReactiveKeyValuePair<string, DeviceInputActionConfigModel>)args.AddedItems[0]!;
       
        var input = (args.Source as ListBox)?.FindAncestorOfType<ListBoxItem>(false)?.Content as DeviceInputConfigModel;
        var content = new HandlerEditorViewModel(handler, _pluginManager);

        var dialog = new ContentDialog
        {
            Title = handler.Key,
            Content = content,
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Ok",
            IsPrimaryButtonEnabled = true,
            CornerRadius = new CornerRadius(5),
            VerticalContentAlignment = VerticalAlignment.Stretch,
        };
        var result = await dialog.ShowAsync();

        if(result != ContentDialogResult.Primary)
        {
            return;
        }

        var inputId = input?.Id ?? 0;
        var newValue = content.GetHandler().Value;
        _repositoryHelper.AddChanges(
            Device.Id!,
            inputId,
            handler.Key!,
            newValue!.Adapt<InputAction>());

        await _repositoryHelper.SaveChangesAsync();

        var inp = Device.Inputs?.FirstOrDefault(x => x.Id == inputId);
        if (inp is null)
        {
            return;
        }

        var action = inp.Actions?.FirstOrDefault(x => x.Key == handler.Key);
        if (action is null)
        {
            return;
        }

        if (action.Value is null)
            action.Value = new();

        action.Value.Data = newValue?.Data;
        action.Value.Handler = newValue?.Handler;
        action.Value.InputValueOverride = newValue?.InputValueOverride;
    }

    public ReactiveCommand<DeviceInputConfigModel, Unit> SwitchChangedCommand { get; }
    public async void SwitchChanged(DeviceInputConfigModel input)
    {
        if (input is null)
            return;

        var actions = input.GetPossibleActions();
        var actions1 = actions.Where(x => !input.AllActions!.Any(y => y.Key == x));
        foreach (var action in actions1)
        {
            input.AllActions!.Add(new(action, new()));
        }

        _repositoryHelper.AddChanges(Device.Id!, input!.Id, input!.Split);
        await _repositoryHelper.SaveChangesAsync();
    }
}
