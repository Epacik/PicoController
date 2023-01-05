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
    public DevicesOutputViewModel()
    {
        Logs = Locator.Current.GetRequiredService<LimitedAvaloniaList<LogEventOutput>>("LogList");
        this.RaisePropertyChanged(nameof(Logs));
        Logs.CollectionChanged += Logs_CollectionChanged;
    }

    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(LastItem));
    }

    public LogEventOutput? LastItem => Logs.LastOrDefault();
    public LimitedAvaloniaList<LogEventOutput> Logs { get; }
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
}
