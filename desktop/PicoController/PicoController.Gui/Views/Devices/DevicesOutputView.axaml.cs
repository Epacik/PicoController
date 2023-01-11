using Avalonia.Threading;
using PicoController.Gui.ViewModels.Devices;
using System.Collections.Specialized;

namespace PicoController.Gui.Views.Devices;

public partial class DevicesOutputView : UserControl
{
    public DevicesOutputView()
    {
        InitializeComponent();

        Output.PropertyChanged += Output_PropertyChanged;
    }

    private void Output_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if(
            e.Property.Name == nameof(Output.ItemCount) &&
            e.NewValue is int count  &&
            sender is ListBox output &&
            output.Scroll is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToEnd();
        }
        //throw new NotImplementedException();
    }

}
