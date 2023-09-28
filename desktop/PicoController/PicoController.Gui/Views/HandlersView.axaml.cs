using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace PicoController.Gui.Views;

public partial class HandlersView : UserControl
{
    private PointerEventArgs? _pointerArgs;

    public HandlersView()
    {
        InitializeComponent();
        Handlers.SelectionChanged += Handlers_SelectionChanged;
        //Handlers.PointerEntered   += Handlers_PointerEntered;
        //Handlers.PointerExited    += Handlers_PointerExited;
        //Handlers.PointerPressed   += Handlers_PointerPressed;
        Handlers.SelectionMode = (SelectionMode)0;
    }

    private async void Handlers_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var data = (sender as Border)?.DataContext as string;
        if (data is null)
            return;

        var dataObject = new DataObject();
        dataObject.Set("", data);

        try
        {
            await DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Move);

        }
        catch { }

        Handlers.UnselectAll();
    }

    private void Handlers_PointerExited(object? sender, PointerEventArgs e)
    {
        _pointerArgs = null;
    }

    private void Handlers_PointerEntered(object? sender, PointerEventArgs e)
    {
        _pointerArgs = e;
        //_rootVisualPosition = e.Root
    }

    private  void Handlers_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        var list = (ListBox)(sender ?? Handlers);
        var data = e.AddedItems[0]! as string;

        if (data is null)
            return;

        var dataObject = new DataObject();
        dataObject.Set("", data);


        //await DragDrop.DoDragDrop(
        //    new PointerEventArgs(
        //        e.RoutedEvent!,
        //        e.Source,
        //        _pointerArgs!.Pointer!,
        //        list,
        //        list.Bounds.TopLeft,
        //        (ulong)DateTime.Now.Ticks,
        //        new PointerPointProperties(),
        //        KeyModifiers.None),
        //    dataObject,
        //    DragDropEffects.Move);

        list.UnselectAll();
    }
}
