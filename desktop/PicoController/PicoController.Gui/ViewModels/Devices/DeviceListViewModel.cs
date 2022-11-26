using Avalonia.Collections;
using PicoController.Core.Config;
using System.Diagnostics;
using PicoController.Gui.Helpers;
using PicoController.Core.Extensions;
using PicoController.Gui.Models;
using Mapster;

namespace PicoController.Gui.ViewModels.Devices;

public class DeviceListViewModel : ViewModelBase
{
    public DeviceListViewModel()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            Debug.WriteLine("Design mode!");
            PopulateDesignData();
            return;
        }
        _repositoryHelper = Locator.Current.GetRequiredService<IRepositoryHelper>();
        _repositoryHelper.ReloadRequested += RepositoryHelper_ReloadRequested;
        _repositoryHelper.PropertyChanging += RepositoryHelper_PropertyChanging;
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;

        IEnumerable<Device>? dev = _repositoryHelper?.WorkingConfigCopy?.Devices;
        dev ??= Array.Empty<Device>();
        Devices = new(dev.Select(x => x.Adapt<DeviceConfigModel>()));
    }

    private IRepositoryHelper? _repositoryHelper;
    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        {
            this.RaisePropertyChanging(nameof(Devices));
            //this.RaisePropertyChanging(nameof(SelectedDevice));
        }
    }

    private void RepositoryHelper_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        {
            this.RaisePropertyChanged(nameof(Devices));
            //this.RaisePropertyChanged(nameof(SelectedDevice));
        }
    }

    private void RepositoryHelper_ReloadRequested(object? sender, EventArgs e)
    {
        IEnumerable<Device>? dev = _repositoryHelper?.WorkingConfigCopy?.Devices;
        dev ??= Array.Empty<Device>();
        Devices = new(dev.Select(x => x.Adapt<DeviceConfigModel>()));
    }

    private bool expandMenuBar = true;
    public bool ExpandMenuBar
    {
        get => expandMenuBar;
        set => this.RaiseAndSetIfChanged(ref expandMenuBar, value);
    }
    private AvaloniaList<DeviceConfigModel>? _devices;
    public AvaloniaList<DeviceConfigModel>? Devices
    {
        get => _devices;
        set => this.RaiseAndSetIfChanged(ref _devices, value);
    }

    private DeviceConfigModel? _selectedDevice;

    public DeviceConfigModel? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    private void PopulateDesignData()
    {
        Debug.WriteLine("Showing example data");
        Devices = new(Config.ExampleConfig(5).Devices.Select(x => x.Adapt<DeviceConfigModel>()));
    }
}
