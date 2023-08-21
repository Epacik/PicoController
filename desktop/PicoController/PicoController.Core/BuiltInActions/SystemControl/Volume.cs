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
using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal class Volume : IPluginAction, IDisposable
{
    public IDisplayInfo? DisplayInfo { get; set; }

    private readonly SequentialScheduler _scheduler;
    private readonly TaskFactory _taskFactory;
    //private MMDeviceEnumerator _deviceEnumerator;
    private readonly ILogger? _logger;

    private readonly Timer _cacheTimer;
    
    public Volume(ILogger? logger, IDisplayInfo? displayInfo)
    {
        _scheduler = new SequentialScheduler();
        _taskFactory = new TaskFactory(_scheduler);
        //_deviceEnumerator = new MMDeviceEnumerator();
        _logger = logger;
        DisplayInfo = displayInfo;
        _cacheTimer = new Timer(CacheTimerCallback, null, 5000, 5000);
    }
    private void CacheTimerCallback(object? state)
    {
        // get new device 

        using var enumerator = new MMDeviceEnumerator();
        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var deviceName = device.FriendlyName;
        var deviceCache = new DeviceCache(device, deviceName);

        // get new sessions

        var sessions = device.AudioSessionManager.Sessions;
        var sessionControls = new List<SessionInfo>();
        for (int id = 0; id < sessions.Count; id++)
        {
            var session = sessions[id];
            if (session is not null)
            {
                var sessionId = session.GetSessionIdentifier;
                var process = session.GetProcessID;
                sessionControls.Add(new SessionInfo(sessionId, process, session));
            }
        }

        // get processes

        var processes = Process.GetProcesses()
            .Select(x => new ProcessInfo(x.Id, x.ProcessName, x.MainWindowTitle, x))
            .ToList();

        // get services

        List<ServiceInfo> services = new();
        ManagementObjectSearcher searcher = new("SELECT PROCESSID, NAME, DISPLAYNAME FROM WIN32_SERVICE");
        foreach (ManagementObject mngntObj in searcher.Get().Cast<ManagementObject>())
        {
            services.Add(new((uint)mngntObj["PROCESSID"], (string)mngntObj["NAME"], (string)mngntObj["DISPLAYNAME"]));
        }


        // create new cache 

        var cache = GetCache();

        lock (this) { _cache = new Cache(deviceCache, sessionControls, processes, services, _logger); }

        // cleanup old cache

        if (cache is null)
            return;

        cache.Dispose();
    }

    private Cache? _cache;

    private Cache? GetCache()
    {
        lock (this) { return _cache; }
    }


    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();
        
        float step = 0.01f * (float)inputValue;

        var cache = GetCache();
        if (cache is null)
            return;
        var device = cache.Device;

        var args = argument.Split(';', StringSplitOptions.RemoveEmptyEntries);

        try
        {
            if (args.Length < 2)
            {
                ChangeMasterVolume(argument, step, device);
            }
            else
            {
                ChangeAppVolume(argument, step, device, cache.Sessions, cache.Processes, cache.Services, args);
            }
        }
        catch (InvalidComObjectException ex)
        {
            _logger?.Warning(ex, "An exception occured while changing volume");
        }
    }

    private void ChangeMasterVolume(string argument, float step, DeviceCache device)
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
    private bool ChangeAppVolume(
        string? argument,
        float step,
        DeviceCache device,
        List<SessionInfo> sessions,
        List<ProcessInfo> processes,
        List<ServiceInfo> services,
        string[] args)
    {
        var (dev, _) = device;
        var (appName, action) = (args[0], args[1]);
        bool exact = args.Contains("!") || args.Contains("EXACT");
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            var sessionId = session.SessionId;
            (string? name, string? displayName) proc = (session.SessionId, session.SessionId);
            int processId = (int)session.ProcessId;

            var process = processes.Find(x => x.Id == processId);

            if (process is null)
            {
                continue;
            }


            if (process.Name.StartsWith("svchost", StringComparison.InvariantCultureIgnoreCase))
            {
                var service = services.Find(x => x.ProcessId == process.Id);
                if (service is null)
                    return false;
                proc = (service.Name, service.DisplayName);

            }
            else
                proc = (
                    process.Name,
                    string.IsNullOrWhiteSpace(process.MainWindowTitle) ? process.Name : process.MainWindowTitle
                    );

            var displayName = proc.displayName ?? proc.name;

            if (string.IsNullOrWhiteSpace(displayName))
                continue;

            if ((exact && proc.name == appName)
                ||
                (!exact && proc.name.Contains(appName, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (action.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
                    session.SessionControl.SimpleAudioVolume.Mute = !session.SessionControl.SimpleAudioVolume.Mute;

                else if (int.TryParse(args[1], out int value))
                {
                    var v = session.SessionControl.SimpleAudioVolume;
                    var newValue = NewVolume(v.Volume + (step * value));
                    v.Volume = newValue;
                    Display(displayName, newValue);
                }
                else
                    Throw(argument);

            }
        }

        return true;
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
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {}
            _scheduler.Dispose();
            _cacheTimer.Dispose();
            GetCache()?.Dispose();
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

    private record struct DeviceCache(MMDevice Dev, string Name) : IDisposable
    {
        public void Deconstruct(out MMDevice dev)
        {
            dev = Dev;
        }
        public void Deconstruct(out MMDevice dev, out string name)
        {
            dev = Dev;
            name = Name;
        }

        public void Dispose()
        {
            if (Dev is not null)
            {
                try
                {
                    Dev.Dispose();
                }
                catch /*(Exception ex)*/
                {
                    //_logger?.Warning("Problem disposing audio device {Ex}", ex);
                }
            }
        }
    }

    private record class SessionInfo(string? SessionId, uint ProcessId, AudioSessionControl SessionControl);
    private record class ProcessInfo(int Id, string Name, string? MainWindowTitle, Process Process);
    private record class ServiceInfo(uint ProcessId, string Name, string DisplayName);

    private record class Cache(DeviceCache Device, List<SessionInfo> Sessions, List<ProcessInfo> Processes, List<ServiceInfo> Services, ILogger? Logger) : IDisposable
    {
        public void Dispose()
        {
            foreach (var process in Processes)
            {
                try
                {
                    process.Process.Dispose();
                }
                catch (Exception ex)
                {
                    Logger?.Warning(ex, "An exception occured while disposing a process");
                }
            }

            foreach (var session in Sessions)
            {
                try
                {
                    session.SessionControl.Dispose();
                }
                catch (Exception ex)
                {
                    Logger?.Warning(ex, "An exception occured while disposing a process");
                }
            }

            try
            {
                Device.Dispose();
            }
            catch (Exception ex)
            {
                Logger?.Warning(ex, "An exception occured while disposing a process");
            }
        }
    }
}




#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS