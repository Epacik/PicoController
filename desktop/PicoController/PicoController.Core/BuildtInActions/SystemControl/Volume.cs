using PicoController.Plugin.Interfaces;

namespace PicoController.Core.BuildtInActions.SystemControl;

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
    public void Execute(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        ChangeVolume(argument);
    }

    public async Task ExecuteAsync(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();
        ChangeVolume(argument);
    }

    private void ChangeVolume(string argument)
    {
        float step = 0.01f;
        using var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var processes = Process.GetProcesses();

        var args = argument.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length < 2)
        {
            var volume = device.AudioEndpointVolume;

            if (argument.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
                device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;

            else if (int.TryParse(argument, out int value))
            {
                step *= value;
                if (step > 0 && device.AudioEndpointVolume.MasterVolumeLevelScalar + step > 1)
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                else if (step < 0 && device.AudioEndpointVolume.MasterVolumeLevelScalar + step < 0)
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 0;
                else
                    device.AudioEndpointVolume.MasterVolumeLevelScalar += step;
            }
            else
                throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");
        }
        else
        {
            var appName = args[0];
            var action = args[1];
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
                        step *= value;
                        if (step > 0 && session.SimpleAudioVolume.Volume + step > 1)
                            session.SimpleAudioVolume.Volume = 1;
                        else if (step < 0 && session.SimpleAudioVolume.Volume + step < 0)
                            session.SimpleAudioVolume.Volume = 0;
                        else
                            session.SimpleAudioVolume.Volume += step;
                    }
                    else
                        throw new ArgumentException($"'{argument}' is not a valid value. Expected value was one of: '+X', 'X', '-X', 'ToggleMute', 'AppName;+X', 'AppName;X', 'AppName;-X', 'AppName;ToggleMute', Where X is an integer value and AppName is a name of an application or service volume of which is to be changed ");

                }
            }
        }
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