using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class HandlersViewModel : ViewModelBase
{

    public HandlersViewModel()
    {
        Reload();
    }

    private AvaloniaList<string> _handlers = new();
    public AvaloniaList<string> Handlers
    {
        get => _handlers;
        set => this.RaiseAndSetIfChanged(ref _handlers, value);
    }

    public void Reload()
    {
        Handlers.Clear();
        Handlers.AddRange(Core.Plugins.AllAvailableActions());
    }
}
