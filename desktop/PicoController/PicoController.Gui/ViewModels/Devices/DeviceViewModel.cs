using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using PicoController.Core.Config;
using PicoController.Core.Extensions;
using PicoController.Gui.Helpers;
using ReactiveUI;
using System.Reactive;

namespace PicoController.Gui.ViewModels.Devices;

public class DeviceViewModel : ViewModelBase
{
    public DeviceViewModel() : this(Config.ExampleConfig(1).Devices[0]) { }

    public DeviceViewModel(Device device)
    {
        Device = device;
        Inputs = new(device.Inputs);
        _repositoryHelper = Locator.Current.GetRequiredService<IRepositoryHelper>();
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;
        SelectedInputChangedCommand = ReactiveCommand.Create<SelectionChangedEventArgs>(SelectedInputChanged);
    }

    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        {
            this.RaisePropertyChanged(nameof(Device));
            Inputs = new(Device.Inputs);
            this.RaisePropertyChanged(nameof(Inputs));
        }
    }

    public Device Device { get; }

    //public Input[] Inputs => Device.Inputs;

    private AvaloniaList<Input>? _inputs;
    public AvaloniaList<Input>? Inputs
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

        var handler = (KeyValuePair<string, InputAction>)args.AddedItems[0]!;
       
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
            _repositoryHelper.AddChanges(Device.Id, input?.Id ?? 0, handler.Key, content.GetHandler().Value);
        }

    }
}
