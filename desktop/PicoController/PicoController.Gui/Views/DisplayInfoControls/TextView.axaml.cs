using Avalonia.Controls;
using Avalonia.Media;

namespace PicoController.Gui.Views.DisplayInfoControls
{
    public partial class TextView : UserControl
    {
        public TextView()
        {
            InitializeComponent();
            new TextBlock().TextWrapping = TextWrapping.Wrap;
        }
    }
}
