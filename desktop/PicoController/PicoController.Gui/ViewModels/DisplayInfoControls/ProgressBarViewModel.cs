using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.DisplayInfoControls;

public class ProgressBarViewModel : ViewModelBase
{
    public ProgressBarViewModel(PicoController.Plugin.DisplayInfos.ProgressBar progress)
    {
        (Min, Max, Value, Indeterminate) = progress;
    }

    private float _min;
    public float Min
    {
        get => _min;
        set => this.RaiseAndSetIfChanged(ref _min, value);
    }

    private float _max;
    public float Max
    {
        get => _max;
        set => this.RaiseAndSetIfChanged(ref _max, value);
    }

    private float _value;
    public float Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    private bool _indeterminate;
    public bool Indeterminate
    {
        get => _indeterminate;
        set => this.RaiseAndSetIfChanged(ref _indeterminate, value);
    }
}
