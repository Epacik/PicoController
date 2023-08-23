using Avalonia.Collections;
using Avalonia.Threading;
using OneOf;
using PicoController.Gui.Extensions.DisplayInfo;
using PicoController.Gui.ViewModels.DisplayInfoControls;
using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels;

public class DisplayInfoWindowViewModel : ViewModelBase
{

    internal void Update(IEnumerable<DisplayInformations> infos)
    {
        var infoArray = infos.ToImmutableArray();
        if(infoArray.Length < Controls.Count)
        {
            Controls.RemoveRange(infoArray.Length - 1, Controls.Count - infoArray.Length);
        }

        for (int i = 0; i < Math.Max(Controls.Count, infos.Count()); i++)
        {
            if (infoArray.Length > i && Controls.Count > i && !infoArray[i].Value.Equals(Controls[i].Item))
            {
                Controls[i] = infoArray[i].ConvertToViewModel();
            }
            else if (infoArray.Length > i && Controls.Count < infoArray.Length)
            {
                Controls.Add(infoArray[i].ConvertToViewModel());
            }

        }

        this.RaisePropertyChanged(nameof(Controls));

        Show = true;
    }

    internal void Close()
    {
        Show = false;
    }

    private object Lock = new();

    private bool _show;
    public bool Show
    {
        get => _show;
        set => this.RaiseAndSetIfChanged(ref _show, value);
    }

    private AvaloniaList<DisplayInfoViewModel> _controls = new();
    public AvaloniaList<DisplayInfoViewModel> Controls
    {
        get => _controls;
        set => this.RaiseAndSetIfChanged(ref _controls, value);
    }
}
