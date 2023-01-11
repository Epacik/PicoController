using PicoController.Core;

namespace PicoController.Core.BuiltInActions.SystemControl;

using IronPython.Compiler.Ast;

#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using OneOf;
using PicoController.Plugin;
using PicoController.Plugin.DisplayInfos;
using SecretNest.TaskSchedulers;
using Serilog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Volume : IPluginAction, IDisposable
{
    public IDisplayInfo? DisplayInfo { get; set; }

    private readonly SequentialScheduler _scheduler;
    private readonly TaskFactory _taskFactory;
    private MMDeviceEnumerator _deviceEnumerator;
    private readonly NotificationClient _notificationClient;
    private readonly ILogger? _logger;

    public Volume(ILogger? logger, IDisplayInfo? displayInfo)
    {
        _notificationClient = new NotificationClient();
        _notificationClient.DeviceNotification += NotificationClient_DeviceNotification;
        _scheduler = new SequentialScheduler();
        _taskFactory = new TaskFactory(_scheduler);
        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
        _logger = logger;
        DisplayInfo = displayInfo;
    }

    private void NotificationClient_DeviceNotification(object? sender, EventArgs e)
    {
        if(Device.HasValue)
        {
            Device.Value.dev.Dispose();
        }
        _device = null;
    }

    private (MMDevice dev, string name)? _device;
    public (MMDevice dev, string name)? Device
    {
        get
        {
            try
            {
                if (_device is null)
                {
                    var dev = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    _device = (dev, dev.FriendlyName);
                }

                return _device;
            }
            catch (Exception ex) 
            {
                _logger?.Error("An error occured while obaining a sound device {Ex}", ex);
                _deviceEnumerator?.Dispose();
                _deviceEnumerator = new MMDeviceEnumerator();
                
                return Device;
            }
        }
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();
        
        float step = 0.01f * (float)inputValue;
        var device = Device;
        if (device is null)
            return;

        var args = argument.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (args.Length < 2)
        {
            ChangeMasterVolume(argument, step, device.Value);
        }
        else
        {
            ChangeAppVolume(argument, step, device.Value, args);
        }
    }


    private void ChangeMasterVolume(string argument, float step, (MMDevice dev, string name) device)
    {
        var (dev, name) = device;
        if (argument.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
            dev.AudioEndpointVolume.Mute = !dev.AudioEndpointVolume.Mute;

        else if (int.TryParse(argument, out int value))
        {
            var newValue = NewVolume(dev.AudioEndpointVolume.MasterVolumeLevelScalar + (step * value));
            dev.AudioEndpointVolume.MasterVolumeLevelScalar = newValue;
            Display(name, newValue);
        }
        else
        {
            Throw(argument);
        }
    }
    private void ChangeAppVolume(string? argument, float step, (MMDevice dev, string name) device, string[] args)
    {
        var (dev, _) = device;
        var (appName, action) = (args[0], args[1]);

        var sessions = dev.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var sessionId = session.GetSessionIdentifier;
            (string name, string? displayName) proc = (session.GetSessionIdentifier, session.GetSessionIdentifier);
            var process = Process.GetProcessById((int)session.GetProcessID);

            if (process.ProcessName.StartsWith("svchost", StringComparison.InvariantCultureIgnoreCase))
            {
                var p = GetServiceName(process.Id);
                if (p is null)
                    return;
                proc = p.Value;

            }
            else
                proc = (
                    process.ProcessName,
                    string.IsNullOrWhiteSpace(process.MainWindowTitle) ? process.ProcessName : process.MainWindowTitle
                    );

            var displayName = proc.displayName ?? proc.name;

            if (string.IsNullOrWhiteSpace(displayName))
                continue;

            if (proc.name.Contains(appName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (action.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
                    session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;

                else if (int.TryParse(args[1], out int value))
                {
                    var v = session.SimpleAudioVolume;
                    var newValue = NewVolume(v.Volume + (step * value));
                    v.Volume = newValue;
                    Display(displayName, newValue);
                }
                else
                    Throw(argument);

            }
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NewVolume(float newValue) => newValue switch
    {
        < 0 => 0,
        > 1 => 1,
        _ => newValue,
    };

    private static void Throw(string? argument)
    {
        throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");
    }

    private void Display(string displayName, float value)
    {
        if (DisplayInfo is not null)
            DisplayInfo.Display(GetDisplayInfo(displayName, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DisplayInformations[] GetDisplayInfo(string friendlyName, float value)
    {
        DisplayInformations[] infos =
        {
            new Text("Volume", 18, 600),
            new Text(friendlyName),
            new Text($"{100 * value:0}%", fontWeight: 600),
            new ProgressBar(0, 100, 100 * value),
        };

        return infos;
    }

    private bool disposedValue;

    private (string name, string displayName)? GetServiceName(int processId)
    {
        var service = Services?.FirstOrDefault(x => x.id == processId);

        if (service == null)
            return null;

        return (service.Value.name, service.Value.displayName);
    }

    private (DateTime timestamp, IEnumerable<(uint id, string name, string displayName)> services)? _servicesCache;
    private IEnumerable<(uint id, string name, string displayName)> Services
    {
        get
        {
            var now = DateTime.Now;

            if(_servicesCache is not null && _servicesCache?.timestamp >= now)
            {
                return _servicesCache.Value.services;
            }

            List<(uint id, string name, string displayname)> services = new();
            string qry = $"SELECT PROCESSID, NAME, DISPLAYNAME FROM WIN32_SERVICE";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(qry);
            foreach (System.Management.ManagementObject mngntObj in searcher.Get())
            {
                services.Add(((uint)mngntObj["PROCESSID"], (string)mngntObj["NAME"], (string)mngntObj["DISPLAYNAME"]));
            }

            _servicesCache = (now.AddSeconds(20), services);

            return services;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {}
            _scheduler.Dispose();
            disposedValue = true;
        }
    }

    ~Volume()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    private class NotificationClient : IMMNotificationClient
    {
        public event EventHandler? DeviceNotification;
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            DeviceNotification?.Invoke(this, EventArgs.Empty);
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            DeviceNotification?.Invoke(this, EventArgs.Empty);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            DeviceNotification?.Invoke(this, EventArgs.Empty);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }
    }
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS