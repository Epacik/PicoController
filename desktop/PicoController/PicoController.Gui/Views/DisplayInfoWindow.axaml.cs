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
            UpdateWindowTransparency(e.NewSize);
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
                    UpdateWindowTransparency();
                    break;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ShowOnAllDesktops();
        }

#if OS_WINDOWS
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [Flags]
        public enum LayeredWindowFlags : uint
        {
            LWA_ALPHA = 0x00000002,
            LWA_COLORKEY = 0x00000001,
        }

        [LibraryImport("user32.dll")]

        private static partial int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

        [LibraryImport("gdi32.dll")]

        private static partial IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);

        const long WS_EX_TOPMOST = 0x00000008L;
        const long WS_EX_TOOLWINDOW = 0x00000080;

        public enum GWL : int
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }
#endif
        private void UpdateWindowTransparency(Size? newSize = null)
        {
            WindowState = WindowState.FullScreen;

            if (OperatingSystem.IsWindows()
                && this.PlatformImpl is Avalonia.Win32.WindowImpl window)
            {
#if OS_WINDOWS

                var primaryScreen = Screens.Primary;
                var taskbarHeight = (primaryScreen?.Bounds.Bottom - primaryScreen?.WorkingArea.Bottom) ?? 0;
                var hwnd = window.Handle.Handle;
                var bounds = WindowBorder.Bounds;
                var padding = WindowBorder.Padding.Top + WindowBorder.Padding.Bottom;

                var (topLeft, bottomRight) =
                    newSize is Size s
                    ? (new Point(bounds.Left, bounds.Bottom - s.Height - padding), bounds.BottomRight)
                    : (bounds.TopLeft, bounds.BottomRight);
                var margin = WindowBorder.Margin;
                var rrect = CreateRoundRectRgn(
                    (int)(topLeft.X - margin.Left),
                    (int)(topLeft.Y - margin.Top),
                    (int)(bottomRight.X + margin.Right),
                    (int)(bottomRight.Y + margin.Bottom - taskbarHeight),
                    5,
                    5);

                SetWindowRgn(hwnd, rrect, true);
#endif

            }
        }


        private void ShowOnAllDesktops()
        {
            if (OperatingSystem.IsWindows()
                && PlatformImpl is Avalonia.Win32.WindowImpl window)
            {
#if OS_WINDOWS
                var style = GetWindowLongPtr(window.Handle.Handle, GWL.GWL_EXSTYLE);
                style = new IntPtr(style.ToInt64() | WS_EX_TOOLWINDOW);
                SetWindowLongPtr(window.Handle.Handle, GWL.GWL_EXSTYLE, style);
#endif
            }
        }
    }
}
