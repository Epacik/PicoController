using Avalonia.Collections;
using Avalonia.Threading;
using OneOf;
using PicoController.Gui.ViewModels.DisplayInfoControls;
using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class DisplayInfoWindowViewModel : ViewModelBase
{

    public DisplayInfoWindowViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2),
        };
        _timer.Tick += Timer_Tick;
        
    }

    private readonly DispatcherTimer _timer;
    private void Timer_Tick(object? sender, EventArgs e)
    {
        Show = false;
    }

    internal void Update(IEnumerable<DisplayInformations> infos)
    {
        Controls.Clear();

        Controls.AddRange(
            infos.Select(
                x => x.Match<ViewModelBase>(
                    text => new TextViewModel(text),
                    progress => new ProgressBarViewModel(progress)
                    )
                )
            );

        Show = true;
        _timer.Stop();
        _timer.Start();
    }

    private object Lock = new();

    private bool _show;
    public bool Show
    {
        get => _show;
        set => this.RaiseAndSetIfChanged(ref _show, value);
    }

    private AvaloniaList<ViewModelBase> _controls = new();
    public AvaloniaList<ViewModelBase> Controls
    {
        get => _controls;
        set => this.RaiseAndSetIfChanged(ref _controls, value);
    }
}
