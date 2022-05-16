using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Timers;

namespace SerialControler.Gui.Views
{
    public partial class MainWindow : Window
    {
        ViewModels.MainWindowViewModel? viewModel => DataContext as ViewModels.MainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
