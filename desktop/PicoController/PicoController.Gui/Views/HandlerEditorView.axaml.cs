using Avalonia.Controls;
using Avalonia.Input;
using PicoController.Gui.ViewModels;

namespace PicoController.Gui.Views
{
    public partial class HandlerEditorView : UserControl
    {
        public HandlerEditorView()
        {
            InitializeComponent();
            HandlerId.AddHandler(DragDrop.DropEvent, HandlerId_Drop);
        }

        public void HandlerId_Drop(object? sender, DragEventArgs args)
        {
            if (args.Data is not Avalonia.Input.IDataObject)
                return;
            var dataWrapper = (IDataObject)args.Data;

            
            var dataWrapped = dataWrapper.Get("");


            (DataContext as HandlerEditorViewModel)!.HandlerId = dataWrapped!.ToString();
        }
    }
}
