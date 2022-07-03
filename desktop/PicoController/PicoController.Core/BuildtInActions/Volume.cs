using PicoController.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.BuildtInActions;

#if OS_WINDOWS
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
internal class Volume : IPluginAction
{
    private readonly MMDevice device;

    public Volume()
    {
        var deviceEnumerator = new MMDeviceEnumerator();
        device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }
    public void Execute(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        var step = (float)int.Parse(argument);
        var volume = device.AudioEndpointVolume;

        if (step > 0)
            for (int i = 0; i < step; i++)
                volume.VolumeStepUp();

        else if (step < 0)
            for (int i = 0; i < -step; i++)
                volume.VolumeStepDown();
    }

    public async Task ExecuteAsync(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();
        var step = (float)int.Parse(argument);
        var volume = device.AudioEndpointVolume;

        if (step > 0)
            //for (int i = 0; i < step; i++)
                volume.VolumeStepUp();

        else if (step < 0)
            //for (int i = 0; i < -step; i++)
                volume.VolumeStepDown();
    }
}
#endif