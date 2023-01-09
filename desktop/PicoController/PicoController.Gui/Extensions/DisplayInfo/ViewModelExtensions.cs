using PicoController.Gui.ViewModels.DisplayInfoControls;
using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Extensions.DisplayInfo;

internal static class ViewModelExtensions
{
    public static DisplayInfoViewModel ConvertToViewModel(this DisplayInformations info)
        => info.Match<DisplayInfoViewModel>(
            text => new TextViewModel(text),
            progress => new ProgressBarViewModel(progress));
}
