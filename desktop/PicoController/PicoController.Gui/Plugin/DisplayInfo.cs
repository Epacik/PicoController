using Avalonia.Threading;
using OneOf;
using PicoController.Gui.ViewModels;
using PicoController.Gui.Views;
using PicoController.Plugin;
using PicoController.Plugin.DisplayInfos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Plugin;

internal class DisplayInfo : IDisplayInfo
{
    private static readonly DisplayInfoWindowViewModel _viewModel = new DisplayInfoWindowViewModel();
    private static readonly DisplayInfoWindow _window = new DisplayInfoWindow(_viewModel);
    
    public void Display(IEnumerable<DisplayInformations> infos)
    {
        Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(() => _viewModel.Update(infos)));
    }
}
