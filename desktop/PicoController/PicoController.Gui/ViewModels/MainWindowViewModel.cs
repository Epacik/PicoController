using Avalonia.Controls;
using PicoController.Core.Config;
using PicoController.Core.Extensions;

using MessageBox.Avalonia;
using PicoController.Gui.Helpers;
using System.Threading.Tasks;
using PicoController.Core.BuiltInActions.Other;
using Microsoft.Win32;
using PicoController.Gui.ViewModels.Devices;

namespace PicoController.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly string? _customPluginDir = null;
    public MainWindowViewModel()
    {
        const string pluginDirArgName = "-PluginDir=";
        _customPluginDir = App.DesktopApplicationLifetime!.Args
            !.FirstOrDefault(x => x.StartsWith(pluginDirArgName))
            ?.Replace(pluginDirArgName, "");

        _repository = Locator.Current.GetRequiredService<IConfigRepository>();
        _repositoryHelper = Locator.Current.GetRequiredService<IRepositoryHelper>();
        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;

        ConfigNotFound = !_repository.Exists();
        if (!Core.Plugins.AreLoaded)
        {
            Core.Plugins.LoadPlugins(_customPluginDir);
        }

        ToggleRunning();
        PicoControllerActions.ActionRequested += PicoControllerActions_ActionRequested;

        if (OperatingSystem.IsWindows())
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    private readonly IConfigRepository _repository;
    private readonly IRepositoryHelper _repositoryHelper;

    private HandlersViewModel _handlers = new();
    public HandlersViewModel Handlers
    {
        get => _handlers;
        set => this.RaiseAndSetIfChanged(ref _handlers, value);
    }


    private DeviceListViewModel _devices = new();
    public DeviceListViewModel Devices
    {
        get => _devices;
        set => this.RaiseAndSetIfChanged(ref _devices, value);
    }

    private bool _menuButtonToggle;
    public bool MenuButtonToggle
    {
        get => _menuButtonToggle;
        set
        {
            this.RaiseAndSetIfChanged(ref _menuButtonToggle, value);
            this.RaisePropertyChanged(nameof(ExpandMenuBar));
        }
    }

    private bool _menuButtonVisible = true;
    public bool MenuButtonVisible
    {
        get => _menuButtonVisible;
        set
        {
            this.RaiseAndSetIfChanged(ref _menuButtonVisible, value);
            this.RaisePropertyChanged(nameof(ExpandMenuBar));
        }
    }

    public bool ExpandMenuBar => MenuButtonVisible && MenuButtonToggle;

    private bool run = true;
    public bool Run
    {
        get => run;
        set
        {
            this.RaiseAndSetIfChanged(ref run, value);
            this.RaisePropertyChanged(nameof(SaveEnabled));
        }
    }

    private bool showOutput = true;
    public bool ShowOutput
    {
        get => showOutput;
        set => this.RaiseAndSetIfChanged(ref showOutput, value);
    }

    private bool _configNotFound;
    public bool ConfigNotFound
    {
        get => _configNotFound;
        set
        {
            this.RaiseAndSetIfChanged(ref _configNotFound, value);
            this.RaisePropertyChanged(nameof(RunEnabled));
        }
    }

    private bool _createNewConfingEnabled = true;
    public bool CreateNewConfingEnabled
    {
        get => _createNewConfingEnabled;
        set => this.RaiseAndSetIfChanged(ref _createNewConfingEnabled, value);
    }

    private Device? _selectedDevice;
    private List<Core.Devices.Device>? _runningDevices;

    public Device? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    private DevicesOutputViewModel _output = new();
    public DevicesOutputViewModel Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }

    public bool RunEnabled => !ConfigNotFound && !_repositoryHelper.IsDirty;
    public bool SaveEnabled => !Run && _repositoryHelper.IsDirty;

    private void RepositoryHelper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(SaveEnabled));
        this.RaisePropertyChanged(nameof(RunEnabled));
    }

    private void Device_ActionThrownAnException(object? sender, Core.PluginActionExceptionEventArgs e)
    {
        Output.ActionThrownAnException(e);
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
            return;
        
        if (e.Mode == PowerModes.Resume)
        {
            RestartDevices();
        }
    }

    private void PicoControllerActions_ActionRequested(object? sender, RequestedActionEventArgs e)
    {
        switch (e.RequestedAction)
        {
            case RequestedAction.Quit:
                App.DesktopApplicationLifetime?.Shutdown();
                break;
            case RequestedAction.Reload:
                RestartDevices();
                break;
        }
    }


    public async Task CreateNewConfig()
    {
        CreateNewConfingEnabled = false;
        var example = Config.ExampleConfig();
        await _repository.SaveAsync(example);
        if (App.DesktopApplicationLifetime?.MainWindow is Window win)
            await MessageBoxManager.GetMessageBoxStandardWindow(
                "Config created",
                "An example configuration file was created.")

                .ShowDialog(win);
        ConfigNotFound = false;
    }

    public void RequestReload() => _repositoryHelper.RequestReload();

    public async Task SaveConfigCommand()
    {
        await _repositoryHelper.SaveChangesAsync();
    }

    public void ReloadPlugins()
    {
        Core.Plugins.UnloadPlugins();
        Core.Plugins.LoadPlugins(_customPluginDir);

        Handlers.Reload();
    }

    public void ToggleRunning()
    {
        if (Run)
            StartDevices();
        else
            StopDevices();
    }

    private void StartDevices()
    {
        if (_runningDevices is not null)
            return;

        var config = _repositoryHelper.SavedConfigCopy;
        if (config is null)
            return;

        var runningDevices = Core.Devices.Device.FromConfig(config);
        
        foreach(var device in runningDevices)
        {
            try
            {
                device.Connect();
                device.ActionThrownAnException += Device_ActionThrownAnException;
            }
            catch (Exception ex)
            {
                Output.OtherException(ex);
            }
        }
        _runningDevices = runningDevices;
    }
    private void StopDevices()
    {
        if (_runningDevices is null)
            return;
        
        try
        {
            foreach (var device in _runningDevices)
            {
                device.ActionThrownAnException -= Device_ActionThrownAnException;
                device.Disconnect();
            }
        }
        finally
        {
            foreach (var device in _runningDevices)
            {
                device.Dispose();
            }
            _runningDevices = null;
        }
    }

    public void RestartDevices()
    {
        StopDevices();
        StartDevices();
    }
}
