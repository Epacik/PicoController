using Avalonia.Collections;
using PicoController.Core;
using PicoController.Core.Extensions;
using PicoController.Core.Misc;
using PicoController.Gui.Helpers;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.Devices;

public class DevicesOutputViewModel : ViewModelBase
{
    private readonly IRepositoryHelper _repositoryHelper;
    private readonly LoggingLevelSwitch _logLevelSwitch;

    public DevicesOutputViewModel(
        ObservableCircularBuffer<LogEventOutput> logsList,
        IRepositoryHelper repositoryHelper,
        LoggingLevelSwitch logLevelSwitch)
    {
        Logs = logsList;
        _repositoryHelper = repositoryHelper;
        _logLevelSwitch = logLevelSwitch;
        this.RaisePropertyChanged(nameof(Logs));

        VerbosityLevels = new(Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>());
        var config = repositoryHelper.SavedConfigCopy;

        var selectedVerbosity = config?.Verbosity;

        SelectedVerbosity =  VerbosityLevels.FirstOrDefault(x => x.ToString() == selectedVerbosity);

    }
    public ObservableCircularBuffer<LogEventOutput> Logs { get; }

    private ObservableCollection<LogEventLevel> _verbosityLevels;
    public ObservableCollection<LogEventLevel> VerbosityLevels
    {
        get => _verbosityLevels;
        set => this.RaiseAndSetIfChanged(ref _verbosityLevels, value);
    }

    private LogEventLevel? _selectedVerbosity;

    public LogEventLevel? SelectedVerbosity
    {
        get => _selectedVerbosity;
        set
        {
            var verbosity = value ?? LogEventLevel.Verbose;
            this.RaiseAndSetIfChanged(ref _selectedVerbosity, verbosity);
            _logLevelSwitch.MinimumLevel = verbosity;

            var config = _repositoryHelper.SavedConfigCopy;
            if (config is null)
                return;

            config.Verbosity = verbosity.ToString();

            Task.Run(async () => await _repositoryHelper.Repository.SaveAsync(config))
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        _repositoryHelper.RequestReload();
                    }
                });
        }
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

        var level = LogEvent?.Level;
        var timestamp = LogEvent?.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fffff") ?? "";
        var text = Text ?? "";
        var exception = LogEvent?.Exception?.ToString() ?? "";
        var value = $"""
            {level} [{timestamp}]
            
            {text}
            
            {exception}
            """
            .Trim();

        var task = App.MainWindow?.Clipboard?.SetTextAsync(value) ?? Task.CompletedTask;
        await task;
    }
}
