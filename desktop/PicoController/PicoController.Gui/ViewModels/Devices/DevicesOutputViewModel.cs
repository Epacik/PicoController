using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Extensions;
using PicoController.Core.Misc;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.Devices;

public class DevicesOutputViewModel : ViewModelBase
{
    public DevicesOutputViewModel(ObservableCircularBuffer<LogEventOutput> logsList)
    {
        Logs = logsList;
        this.RaisePropertyChanged(nameof(Logs));
    }
    public ObservableCircularBuffer<LogEventOutput> Logs { get; }

    public void CopyToClipboard(object? param)
    {
        Console.WriteLine("Copy item");
    }
}

public class LogEventOutput
{
    public LogEventOutput(LogEvent logEvent)
    {
        LogEvent = logEvent;
    }

    public LogEvent LogEvent { get; }

    private const int ShortenedTextLimit = 150;
    private string? _text;
    public string Text => _text ??= LogEvent.RenderMessage();
    private string? _shortenedString;
    public string ShortenedText => _shortenedString ??= (Text.Length > ShortenedTextLimit ? Text[..ShortenedTextLimit] + "..." : Text);
    public override string ToString() => Text;

    public async void CopyToClipboard()
    {
        Console.WriteLine("Copy self");

        await App.MainWindow?.Clipboard?.SetTextAsync(
            $"""
            {LogEvent?.Level} [{LogEvent?.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffff")}]

            {Text}

            {LogEvent?.Exception?.ToString()}
            """
            .Trim());
    }
}
