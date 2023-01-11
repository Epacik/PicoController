using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Extensions;
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
    public DevicesOutputViewModel(LimitedAvaloniaList<LogEventOutput> logsList)
    {
        Logs = logsList;
        this.RaisePropertyChanged(nameof(Logs));
        Logs.CollectionChanged += Logs_CollectionChanged;
    }

    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.NewItems is not null || e.NewStartingIndex == -1)
        {
            //LastIndex = Logs.Count - 1;
            this.RaisePropertyChanged(nameof(LastItem));
        }
    }

    public LogEventOutput? LastItem { get; set; }
    public LimitedAvaloniaList<LogEventOutput> Logs { get; }

    private int _lastIndex;
    public int LastIndex
    {
        get => _lastIndex;
        set => this.RaiseAndSetIfChanged(ref _lastIndex, value);
    }

}

public class LogEventOutput
{
    public LogEventOutput(LogEvent logEvent)
    {
        LogEvent = logEvent;
    }

    public LogEvent LogEvent { get; }
    private string? _text;
    public string Text => _text ??= LogEvent.RenderMessage();
    public override string ToString() => Text;
}
