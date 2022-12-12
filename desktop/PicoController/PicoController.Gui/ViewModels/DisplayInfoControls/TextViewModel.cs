using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.DisplayInfoControls;

public class TextViewModel : ViewModelBase
{
    public TextViewModel(Text text)
    {
        (Text, FontSize, FontWeight) = text;
    }

    public string? Text { get; }
    public double FontSize { get; }
    public int FontWeight { get; }
}
