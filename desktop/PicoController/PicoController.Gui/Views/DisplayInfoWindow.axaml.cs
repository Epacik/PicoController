using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using OneOf.Types;
using PicoController.Gui.ViewModels;
using PicoController.Gui.Views.NativeHelpers;
using SuccincT.Functional;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PicoController.Gui.Views
{
    public partial class DisplayInfoWindow : Window
    {
        public DisplayInfoWindow()
        {
            InitializeComponent();
            Topmost = true;
            Controls.SizeChanged += Controls_SizeChanged;
        }

        private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as DisplayInfoWindowViewModel)!.Show = false;
        }

        private void Controls_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateWindowPosition();
        }



        public DisplayInfoWindow(DisplayInfoWindowViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayInfoWindowViewModel.Show):
                    var value = (DataContext as DisplayInfoWindowViewModel)?.Show == true;

                    if (value)
                    {
                        nint focusedWindowHandle = 0;
                        if(OperatingSystem.IsWindows())
                        {
                            focusedWindowHandle = WindowsNativeHelper.GetForegroundWindow();
                        }
                        Show();

                        if (OperatingSystem.IsWindows() && focusedWindowHandle != 0)
                        {
                            WindowsNativeHelper.SetForegroundWindow(focusedWindowHandle);
                        }
                    }
                    else Hide();
                    break;

                case nameof(DisplayInfoWindowViewModel.Controls):
                    UpdateWindowPosition();
                    break;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ShowOnAllDesktops();
        }


        private void UpdateWindowPosition()
        {
            var primaryScreen = Screens.Primary;

            if (primaryScreen is null)
                return;

            var taskbarHeight = primaryScreen.Bounds.Bottom - primaryScreen.WorkingArea.Bottom;

            var center = primaryScreen.WorkingArea.Center;
            var halfWith = (int)(WindowBorder.DesiredSize.Width / 2 + WindowBorder.Margin.Left);


            var bottomMargin = (int)(WindowBorder.DesiredSize.Height + WindowBorder.Margin.Bottom);

            var taskbarMargin = Math.Max(taskbarHeight / 2, 20);

            var adjustedPosition = center
                .WithX(center.X - halfWith)
                .WithY(primaryScreen.Bounds.Bottom - bottomMargin - taskbarMargin);

            Position = adjustedPosition;

        }


        private void ShowOnAllDesktops()
        {
            if (OperatingSystem.IsWindows())
            {
#if OS_WINDOWS
                var hwnd = this.TryGetPlatformHandle()?.Handle ?? 0;
                var style = NativeHelpers.WindowsNativeHelper.GetWindowLongPtr(hwnd, NativeHelpers.WindowsNativeHelper.GWL.GWL_EXSTYLE);
                style = new IntPtr(style.ToInt64() | NativeHelpers.WindowsNativeHelper.WS_EX_TOOLWINDOW);
                NativeHelpers.WindowsNativeHelper.SetWindowLongPtr(hwnd, NativeHelpers.WindowsNativeHelper.GWL.GWL_EXSTYLE, style);
#endif
            }
        }
    }
}
