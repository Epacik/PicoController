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
    private CancellationTokenSource? _tokenSource;
    private static readonly DisplayInfoWindowViewModel _viewModel = new();
    private static readonly DisplayInfoWindow _window = new(_viewModel);

    public void Close()
    {
        Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(() => _viewModel.Close()));
    }

    public void Display(IEnumerable<DisplayInformations> infos)
    {
        Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(() => _viewModel.Update(infos)))
            .ContinueWith(async t =>
            {
                _tokenSource?.Cancel();
                _tokenSource?.Dispose();
                _tokenSource = null;

                try
                {
                    _tokenSource = new CancellationTokenSource();
                    await Task.Delay(TimeSpan.FromSeconds(2), _tokenSource.Token);
                    Close();
                }
                catch (TaskCanceledException)
                {
                    // swallow
                }
            });
    }

    public void Display(params DisplayInformations[] infos)
    {
        Display(infos.AsEnumerable());
    }
}
