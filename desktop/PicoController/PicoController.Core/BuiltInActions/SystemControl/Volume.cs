using PicoController.Core;

namespace PicoController.Core.BuiltInActions.SystemControl;

#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

using NAudio.CoreAudioApi;
using OneOf;
using PicoController.Plugin;
using PicoController.Plugin.DisplayInfos;
using SecretNest.TaskSchedulers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Volume : IPluginAction, IDisposable
{
    public IDisplayInfo? DisplayInfo { get; set; }

    private readonly SequentialScheduler _scheduler;
    private readonly TaskFactory _taskFactory;
    private readonly MMDeviceEnumerator _deviceEnumerator;

    public Volume()
    {
        _scheduler = new SequentialScheduler();
        _taskFactory = new TaskFactory(_scheduler);
        _deviceEnumerator = new MMDeviceEnumerator();
    }

    private DateTime _getDeviceTimestamp;
    private MMDevice? _device;
    public MMDevice? Device
    {
        get
        {
            var now = DateTime.Now;
            if (_device is not null && _getDeviceTimestamp.AddSeconds(5) < now)
            {
                _device.Dispose();
                _device = null;
            }
            
            if (_device is null)
            {
                _device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                _getDeviceTimestamp = now;
            }

            return _device;
        }
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Yield();
        _ = _taskFactory.StartNew(() => ExecuteInternal(inputValue, argument));
    }

    private void ExecuteInternal(int inputValue, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        float step = 0.01f * (float)inputValue;
        var device = Device;
        if(device is null)
            return;

        var args = argument.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (args.Length < 2)
        {
            ChangeMasterVolume(argument, step, device);
        }
        else
        {
            ChangeAppVolume(argument, step, device, args);
        }
    }

    private void ChangeMasterVolume(string argument, float step, MMDevice device)
    {

        if (argument.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
            device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;

        else if (int.TryParse(argument, out int value))
        {
            var v = device.AudioEndpointVolume;
            var newValue = NewVolume(v.MasterVolumeLevelScalar + (step * value));
            v.MasterVolumeLevelScalar = newValue;
            if(DisplayInfo is not null)
                DisplayInfo.Display(GetDisplayInfo(device.FriendlyName, newValue));
        }
        else
            throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");
    }

    private void ChangeAppVolume(string? argument, float step, MMDevice device, string[] args)
    {
        var (appName, action) = (args[0], args[1]);

        var sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            string processName = session.GetSessionIdentifier;
            var process = Process.GetProcessById((int)session.GetProcessID);

            if (process.ProcessName.StartsWith("svchost", StringComparison.InvariantCultureIgnoreCase))
                processName = GetServiceName(process);
            else
                processName = process.ProcessName;

            if (string.IsNullOrWhiteSpace(processName))
                continue;

            if (processName.Contains(appName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (action.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
                    session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;

                else if (int.TryParse(args[1], out int value))
                {
                    var v = session.SimpleAudioVolume;
                    var newValue = NewVolume(v.Volume + (step * value));
                    v.Volume = newValue;
                    if (DisplayInfo is not null)
                        DisplayInfo.Display(GetDisplayInfo(processName, newValue));
                }
                else
                    throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");

            }
        }

    }

    private float NewVolume(float newValue) => newValue switch
    {
        < 0 => 0,
        > 1 => 1,
        _ => newValue,
    };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OneOf<Text, ProgressBar>[] GetDisplayInfo(string friendlyName, float value)
    {
        OneOf<Text, ProgressBar>[] infos =
        {
            new Text("Volume", 18, 600),
            new Text(friendlyName),
            new Text($"{100 * value:0}%", fontWeight: 600),
            new ProgressBar(0, 100, 100 * value),
        };

        return infos;
    }

    private static Dictionary<int, (string Name, DateTime DateStamp)> ServicesCache = new();
    private bool disposedValue;

    private static string GetServiceName(Process process)
    {
        var stamp = DateTime.Now;
        if (ServicesCache.ContainsKey(process.Id) && ServicesCache[process.Id].DateStamp < stamp)
        {
            return ServicesCache[process.Id].Name;
        }

        var prc = new Process();
        prc.StartInfo = new ProcessStartInfo("tasklist", $"/svc /fi \"imagename eq svchost.exe\" /FI \"pid eq {process.Id}\" /FO CSV /NH")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        prc.Start();

        var data = prc.StandardOutput.ReadLine();
        prc.WaitForExit();
        if (data is null || !data.StartsWith('"'))
        {
            return "";
        }
        var name = data.Replace("\",\"", ";").Split(';')[2].Trim('"');
        ServicesCache[process.Id] = (name, stamp.AddMilliseconds(500));

        return name;
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
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS