using PicoController.Core;

namespace PicoController.Core.BuiltInActions.SystemControl;

#if OS_WINDOWS
#pragma warning disable CA1416 // Walidacja zgodności z platformą

using NAudio.CoreAudioApi;
using System.Diagnostics;

internal class Volume : IPluginAction
{
    private readonly MMDeviceEnumerator deviceEnumerator;

    public Volume()
    {
        deviceEnumerator = new MMDeviceEnumerator();
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();

        float step = 0.01f;
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

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
            SetVolume(
                x => v.MasterVolumeLevel = x,
                v.MasterVolumeLevelScalar,
                step * value);
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
                    SetVolume(
                        x => v.Volume = x,
                        v.Volume,
                        step * value);
                }
                else
                    throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");

            }
        }

    }

    private void SetVolume(Action<float> set, float currentValue, float step)
    {

        if (step > 0 && currentValue + step > 1)
            set(1);

        else if (step < 0 && currentValue + step < 0)
            set(0);

        else
            set(currentValue + step);
    }

    private static Dictionary<int, (string Name, DateTime DateStamp)> ServicesCache = new();
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
}

#pragma warning restore CA1416 // Walidacja zgodności z platformą
#endif //OS_WINDOWS