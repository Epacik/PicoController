using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using OneOf.Types;
using PicoController.Gui.ViewModels;
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
            UpdateWindoPosition(e.NewSize);
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

                    if (value) Show();
                    else Hide();
                    break;

                case nameof(DisplayInfoWindowViewModel.Controls):
                    UpdateWindoPosition();
                    break;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ShowOnAllDesktops();
        }


        private void UpdateWindoPosition(Size? newSize = null)
        {
            //WindowState = WindowState.FullScreen;

            var primaryScreen = Screens.Primary;

            if (primaryScreen is null)
                return;

            var taskbarHeight = primaryScreen.Bounds.Bottom - primaryScreen.WorkingArea.Bottom;

            var center = primaryScreen.WorkingArea.Center;
            var halfWith = (int)(WindowBorder.DesiredSize.Width / 2 + WindowBorder.Margin.Left);


            var bottomMargin = (int)(WindowBorder.DesiredSize.Height + WindowBorder.Margin.Bottom);

            var adjustedPosition = center
                .WithX(center.X - halfWith)
                .WithY(primaryScreen.Bounds.Bottom - bottomMargin - taskbarHeight);

            Position = adjustedPosition;

            if (OperatingSystem.IsWindows())
            {
#if OS_WINDOWS
                //var primaryScreen = Screens.Primary;
                //var taskbarHeight = (primaryScreen?.Bounds.Bottom - primaryScreen?.WorkingArea.Bottom) ?? 0;
                //var hwnd = this.TryGetPlatformHandle()?.Handle;
                //var bounds = WindowBorder.Bounds;
                //var padding = WindowBorder.Padding.Top + WindowBorder.Padding.Bottom;

                //var (topLeft, bottomRight) =
                //    newSize is Size s
                //    ? (new Point(bounds.Left, bounds.Bottom - s.Height - padding), bounds.BottomRight)
                //    : (bounds.TopLeft, bounds.BottomRight);
                //var margin = WindowBorder.Margin;
                //var rrect = NativeHelpers.WindowsNativeHelper.CreateRoundRectRgn(
                //    (int)(topLeft.X - margin.Left),
                //    (int)(topLeft.Y - margin.Top),
                //    (int)(bottomRight.X + margin.Right),
                //    (int)(bottomRight.Y + margin.Bottom - taskbarHeight),
                //    5,
                //    5);

                //if (hwnd is not null)
                //    NativeHelpers.WindowsNativeHelper.SetWindowRgn(hwnd ?? 0, rrect, true);
#endif

            }
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
