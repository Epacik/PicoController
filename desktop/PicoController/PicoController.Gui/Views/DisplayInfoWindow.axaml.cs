using Avalonia.Controls;
using OneOf.Types;
using PicoController.Gui.ViewModels;
using SuccincT.Functional;

namespace PicoController.Gui.Views
{
    public partial class DisplayInfoWindow : Window
    {
        public DisplayInfoWindow()
        {
            InitializeComponent();
        }

        public DisplayInfoWindow(DisplayInfoWindowViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DisplayInfoWindowViewModel.Show))
            {
                var value = (DataContext as DisplayInfoWindowViewModel)?.Show == true;

                if (value) Show();
                else Hide();
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            var primaryScreen = Screens.Primary;
            if (primaryScreen is null)
                return;

            var workingArea = primaryScreen.WorkingArea;

            var rel = (x: workingArea.Width / 2, y: workingArea.Height - 15);
            var screenPos = (x: rel.x + workingArea.X, y: rel.y + workingArea.Y);
            var winPos = (x: screenPos.x - (Bounds.Width / 2), y: screenPos.y - Bounds.Height);

            Position = Position.WithX((int)winPos.x).WithY((int)winPos.y);
        }
    }
}
