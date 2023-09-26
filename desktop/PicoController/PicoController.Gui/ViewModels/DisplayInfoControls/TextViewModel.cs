using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.DisplayInfoControls;

public class TextViewModel : DisplayInfoViewModel
{
    public TextViewModel(Text text)
        :base(text)
    {
        (Text, FontSize, FontWeight, Wrap, Alignment) = text;
    }

    public string? Text { get; }
    public double FontSize { get; }
    public int FontWeight { get; }
    public bool Wrap { get; }
    public TextAlignment Alignment { get; }
}
