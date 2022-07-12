using PicoController.Plugin.Interfaces;

namespace PicoControler.TidalVolume;

#if OS_WINDOWS
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NPSMLib;

internal class Volume : IPluginAction, IDisposable
{
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly NowPlayingSessionManager _playingSessionManager;
    private readonly List<NowPlayingSession> _sessions = new List<NowPlayingSession>();
    private readonly List<MediaPlaybackDataSource> _sources = new List<MediaPlaybackDataSource>();
    private bool disposedValue;
    private float _tidalVolume;

    public Volume()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        _playingSessionManager = new NowPlayingSessionManager();
        _playingSessionManager.SessionListChanged += _playingSessionManager_SessionListChanged;

        OnSessionListChanged();
    }

    private void _playingSessionManager_SessionListChanged(object? sender, NowPlayingSessionManagerEventArgs e)
    {
        OnSessionListChanged();
    }

    private void OnSessionListChanged()
    {
        var sessions = _playingSessionManager.GetSessions();

        foreach (var source in _sources)
        {
            source.MediaPlaybackDataChanged -= Src_MediaPlaybackDataChanged;
        }

        _sources.Clear();

        foreach (var session in sessions)
        {
            var src = session.ActivateMediaPlaybackDataSource();
            src.MediaPlaybackDataChanged += Src_MediaPlaybackDataChanged;
            _sources.Add(src);
        }
    }

    private async void Src_MediaPlaybackDataChanged(object? sender, MediaPlaybackDataChangedArgs e)
    {
        using var device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var appName = "TIDALPlayer";
        var sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            if (session.GetSessionInstanceIdentifier.Contains(appName, StringComparison.InvariantCulture))
            {
                session.SimpleAudioVolume.Volume = _tidalVolume;
                // on slower hardware tidal was slow enough to circumvent volume change above
                await Task.Delay(500).ConfigureAwait(false);
                session.SimpleAudioVolume.Volume = _tidalVolume;
            }
        }
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
        using var device = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        
        var appName = "TIDALPlayer";
        var action  = argument;
        var sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            if(session.GetSessionInstanceIdentifier.Contains(appName, StringComparison.InvariantCulture))
            {
                if (action.Equals("ToggleMute", StringComparison.InvariantCultureIgnoreCase))
                    session.SimpleAudioVolume.Mute = !session.SimpleAudioVolume.Mute;

                else
                {
                    step *= int.Parse(action);
                    if (step > 0 && session.SimpleAudioVolume.Volume + step > 1)
                        session.SimpleAudioVolume.Volume = 1;
                    else if (step < 0 && session.SimpleAudioVolume.Volume + step < 0)
                        session.SimpleAudioVolume.Volume = 0;
                    else
                        session.SimpleAudioVolume.Volume += step;

                    _tidalVolume = session.SimpleAudioVolume.Volume;
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _playingSessionManager.SessionListChanged -= _playingSessionManager_SessionListChanged;
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Nie zmieniaj tego kodu. Umieść kod czyszczący w metodzie „Dispose(bool disposing)”.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#endif