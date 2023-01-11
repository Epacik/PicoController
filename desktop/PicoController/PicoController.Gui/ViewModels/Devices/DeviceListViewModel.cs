using Avalonia.Collections;
using PicoController.Core.Config;
using System.Diagnostics;
using PicoController.Gui.Helpers;
using PicoController.Core.Extensions;
using PicoController.Gui.Models;
using Mapster;
using PicoController.Core;
using System.Linq;
using PicoController.Gui.Design;

namespace PicoController.Gui.ViewModels.Devices;

public class DeviceListViewModel : ViewModelBase
{
    public DeviceListViewModel() : this(new DesignRepositoryHelper(), new DesignPluginManager())
    { }
    public DeviceListViewModel(IRepositoryHelper repositoryHelper, IPluginManager pluginManager)
    {
        _repositoryHelper = repositoryHelper;
        _pluginManager = pluginManager;

        if (Avalonia.Controls.Design.IsDesignMode)
        {
            PopulateDesignData();
            return;
        }
        
        _repositoryHelper.ReloadRequested += RepositoryHelper_ReloadRequested;
        _repositoryHelper.PropertyChanging += RepositoryHelper_PropertyChanging;
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;

        IEnumerable<Device>? dev = _repositoryHelper?.SavedConfigCopy?.Devices;
        dev ??= Array.Empty<Device>();
        Devices = new(
            dev.Select(
                x => new DeviceViewModel(
                    x.Adapt<DeviceConfigModel>(),
                    repositoryHelper,
                    pluginManager)));
    }

    private IRepositoryHelper? _repositoryHelper;
    private readonly IPluginManager _pluginManager;

    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //if (e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        //{
        //    this.RaisePropertyChanging(nameof(Devices));
        //    //this.RaisePropertyChanging(nameof(SelectedDevice));
        //}
    }

    private void RepositoryHelper_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
    {
        //if (e.PropertyName == nameof(_repositoryHelper.WorkingConfigCopy))
        //{
        //    this.RaisePropertyChanged(nameof(Devices));
        //    //this.RaisePropertyChanged(nameof(SelectedDevice));
        //}
    }

    private void RepositoryHelper_ReloadRequested(object? sender, EventArgs e)
    {
        IEnumerable<Device>? dev = _repositoryHelper?.SavedConfigCopy?.Devices;
        dev ??= Array.Empty<Device>();
        Devices = new(
            dev.Select(
                x => new DeviceViewModel(
                    x.Adapt<DeviceConfigModel>(),
                    _repositoryHelper!,
                    _pluginManager)));
    }

    private bool expandMenuBar = true;
    public bool ExpandMenuBar
    {
        get => expandMenuBar;
        set => this.RaiseAndSetIfChanged(ref expandMenuBar, value);
    }
    private AvaloniaList<DeviceViewModel>? _devices;
    public AvaloniaList<DeviceViewModel>? Devices
    {
        get => _devices;
        set => this.RaiseAndSetIfChanged(ref _devices, value);
    }

    private DeviceViewModel? _selectedDevice;

    public DeviceViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    private void PopulateDesignData()
    {
        Debug.WriteLine("Showing example data");
        var dev = Config.ExampleConfig(5).Devices;
        Devices = new(
            dev.Select(
                x => new DeviceViewModel(
                    x.Adapt<DeviceConfigModel>(),
                    _repositoryHelper!,
                    _pluginManager)));
    }

    
}
