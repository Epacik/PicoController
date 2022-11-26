using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Mapster;
using PicoController.Core.Config;
using PicoController.Core.Extensions;
using PicoController.Gui.Helpers;
using PicoController.Gui.Models;
using ReactiveUI;
using System.Reactive;

namespace PicoController.Gui.ViewModels.Devices;

public class DeviceViewModel : ViewModelBase
{
    public DeviceViewModel() : this(Config.ExampleConfig(1).Devices[0].Adapt<DeviceConfigModel>()) { }

    public DeviceViewModel(DeviceConfigModel device)
    {
        Device = device;
        Inputs = device.Inputs!;
        _repositoryHelper = Locator.Current.GetRequiredService<IRepositoryHelper>();
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;
        SelectedInputChangedCommand = ReactiveCommand.Create<SelectionChangedEventArgs>(SelectedInputChanged);
    }

    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        {
            this.RaisePropertyChanged(nameof(Device));
            Inputs = Device.Inputs;
            this.RaisePropertyChanged(nameof(Inputs));
        }
    }

    public DeviceConfigModel Device { get; }

    //public Input[] Inputs => Device.Inputs;

    private AvaloniaList<DeviceInputConfigModel>? _inputs;
    public AvaloniaList<DeviceInputConfigModel>? Inputs
    {
        get => _inputs;
        set => this.RaiseAndSetIfChanged(ref _inputs, value);
    }

    private readonly IRepositoryHelper _repositoryHelper;
    public ReactiveCommand<SelectionChangedEventArgs, Unit> SelectedInputChangedCommand { get; }

    public async void SelectedInputChanged(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0 && args.AddedItems[0] is not KeyValuePair<string, InputAction>)
        {
            return;
        }

        args.Handled = true;
        (args.Source as ListBox)!.UnselectAll();

        var handler = (ReactiveKeyValuePair<string, DeviceInputActionConfigModel>)args.AddedItems[0]!;
       
        var input = (args.Source as ListBox)?.FindAncestorOfType<ListBoxItem>(false)?.Content as Input;
        var content = new HandlerEditorViewModel(handler);

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
        var result = await dialog.ShowAsync(ContentDialogPlacement.Popup);

        if(result == ContentDialogResult.Primary)
        {
            _repositoryHelper.AddChanges(
                Device.Id!,
                input?.Id ?? 0,
                handler.Key!,
                content.GetHandler().Value!.Adapt<InputAction>());
        }

    }
}
