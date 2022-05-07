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

            var timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            

#if DEBUG
            this.AttachDevTools();
#endif
        }

        

        int i = 0;
        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                viewModel!.Greeting = $"Welcome: {i++}";
                viewModel!.Encoders.Add(new Encoder
                {
                    Name = $"Encoder {i}",
                    Turn = $"Turn {i}",
                    PressAndTurn = $"Press and turn {i}",
                    Press = $"Press {i}",
                });
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
