using Avalonia.Controls;
using PicoController.Core.Config;
using PicoController.Core.Extensions;

using PicoController.Gui.Helpers;
using System.Threading.Tasks;
using PicoController.Core.BuiltInActions.Other;
using Microsoft.Win32;
using PicoController.Gui.ViewModels.Devices;
using Avalonia.Logging;
using Serilog;
using PicoController.Core;
using PicoController.Core.Devices;
using Device = PicoController.Core.Devices.Device;
using Usb.Events;
using Microsoft.Scripting.Utils;
using System.Collections.ObjectModel;
using System.Management;
using System.IO.Ports;
using PicoController.Core.Misc;
using MsBox.Avalonia;

namespace PicoController.Gui.ViewModels;

public interface IMainWindowViewModel
{
    bool ConfigNotFound { get; set; }
    bool CreateNewConfingEnabled { get; set; }
    DeviceListViewModel Devices { get; set; }
    bool ExpandMenuBar { get; }
    bool MenuButtonToggle { get; set; }
    bool MenuButtonVisible { get; set; }
    DevicesOutputViewModel Output { get; set; }
    bool Run { get; set; }
    bool RunEnabled { get; }
    bool SaveEnabled { get; }
    Device? SelectedDevice { get; set; }
    bool ShowOutput { get; set; }

    Task CreateNewConfig();
    void ReloadPlugins();
    void RequestReload();
    void RestartDevices();
    Task SaveConfigCommand();
    void ToggleRunning();
}

public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
    private readonly string? _customPluginDir = null;
    private readonly IPluginManager _pluginManager;
    private readonly IDeviceManager _deviceManager;
    private readonly IRepositoryHelper _repositoryHelper;
    private readonly Serilog.ILogger? _logger;
    private readonly UsbEventWatcher _usbEventWatcher;
    private readonly Timer _deviceCheckTimer;

    private IConfigRepository ConfigRepository => _repositoryHelper.Repository;

    public MainWindowViewModel(
        IPluginManager pluginManager,
        IDeviceManager deviceManager,
        IRepositoryHelper repositoryHelper,
        ObservableCircularBuffer<LogEventOutput> logEventOutputs,
        Serilog.ILogger? logger)
    {
        _pluginManager = pluginManager;
        _deviceManager = deviceManager;
        _repositoryHelper = repositoryHelper;
        _logger = logger;
        _devices = new(_repositoryHelper, pluginManager);
        _output = new(logEventOutputs);

        const string pluginDirArgName = "-PluginDir=";
        _customPluginDir = Array.Find(
                App.DesktopApplicationLifetime!.Args!,
                x => x.StartsWith(pluginDirArgName))
            ?.Replace(pluginDirArgName, "");

        _repositoryHelper.PropertyChanged += RepositoryHelper_PropertyChanged;

        ConfigNotFound = !ConfigRepository.Exists();
        if (!pluginManager.AreLoaded)
        {
            pluginManager.LoadPlugins(_customPluginDir);
        }

        ToggleRunning();
        PicoControllerActions.ActionRequested += PicoControllerActions_ActionRequested;

        if (OperatingSystem.IsWindows())
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

        _deviceCheckTimer = new Timer(DeviceCheckTimerCallback, null, 100, 100);

        //try
        //{
        //    _usbEventWatcher = new UsbEventWatcher();
        //    _usbEventWatcher.UsbDeviceAdded += UsbEventWatcher_UsbDeviceAdded;
        //}
        //catch (ManagementException ex)
        //{
        //    _logger?.Warning("An error occured while initializing usb watcher {Ex}", ex);
        //}
    }

    private string[] _previousComPorts = Array.Empty<string>();

    private void DeviceCheckTimerCallback(object? state)
    {
        var comPorts = SerialPort.GetPortNames();
        var comDevices = _runningDevices
            ?.Where(x => x.Interface is Core.Devices.Communication.Serial)
            ?.ToArray();

        if (comDevices is not null && comPorts is not null)
        {
            var comDeviceNames = comDevices
                .Select(x => (x.Interface as Core.Devices.Communication.Serial)?.PortName)
                .Where(x => x is not null)
                .ToArray();

            if (comPorts.Length != _previousComPorts.Length ||
                !comPorts.All(x => _previousComPorts.Contains(x)) ||
                !_previousComPorts.All(x => comPorts.Contains(x)))
            {
                RestartDevices();
            }
            _previousComPorts = comPorts;
        }
    }

    private DeviceListViewModel _devices;
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

    private DevicesOutputViewModel _output;
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

    private void UsbEventWatcher_UsbDeviceAdded(object? sender, UsbDevice e)
    {
        if (_devices is null)
            return;

        _usbEventWatcher.Start();
        RestartDevices();
    }

    public async Task CreateNewConfig()
    {
        CreateNewConfingEnabled = false;
        var example = Config.ExampleConfig();
        await ConfigRepository.SaveAsync(example);
        if (App.DesktopApplicationLifetime?.MainWindow is Window win)
            await MessageBoxManager.GetMessageBoxStandard(
                "Config created",
                "An example configuration file was created.")

                .ShowAsPopupAsync(win);
        ConfigNotFound = false;
    }

    public void RequestReload() => _repositoryHelper.RequestReload();

    public async Task SaveConfigCommand()
    {
        await _repositoryHelper.SaveChangesAsync();
    }

    public void ReloadPlugins()
    {
        _pluginManager.UnloadPlugins();
        _pluginManager.LoadPlugins(_customPluginDir);
    }

    public async void ToggleRunning()
    {
        if (Run)
            await StartDevices();
        else
            await StopDevices();
    }

    private async Task StartDevices()
    {
        if (_runningDevices is not null)
            return;

        _logger?.Information("Starting devices");
        var config = _repositoryHelper.SavedConfigCopy;
        if (config is null)
            return;

        var runningDevices = (await _deviceManager.LoadDevicesAsync(config)) ?? Array.Empty<Device>();

        foreach (var device in runningDevices)
        {
            try
            {
                device.Interface.Connect();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "An exception occured while trying to connect to a device");
            }
        }
        _runningDevices = runningDevices.ToList();
    }
    private async Task StopDevices()
    {
        if (_runningDevices is null)
            return;

        _logger?.Information("Stopping devices");
        try
        {
            await _deviceManager.UnloadDevicesAsync();
        }
        finally
        {
            _runningDevices = null;
        }
    }

    public void RestartDevices()
    {
        if (_runningDevices is null)
            return;

        _logger?.Information("Restarting devices");
        foreach (var device in _runningDevices)
        {
            device.Interface.Reconnect();
        }
    }
}
