using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using OneOf.Types;
using PicoController.Gui.ViewModels;
using SuccincT.Functional;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PicoController.Gui.Views
{
    public partial class DisplayInfoWindow : Window
    {
        public DisplayInfoWindow()
        {
            InitializeComponent();
            MakeNotClickable();
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
            //var primaryScreen = Screens.Primary;
            //if (primaryScreen is null)
            //    return;

            //var workingArea = primaryScreen.WorkingArea;

            //var rel = (x: workingArea.Width / 2, y: workingArea.Height - 15);
            //var screenPos = (x: rel.x + workingArea.X, y: rel.y + workingArea.Y);
            //var winPos = (x: screenPos.x - (Bounds.Width / 2), y: screenPos.y - Bounds.Height);

            //Position = Position.WithX((int)winPos.x).WithY((int)winPos.y);
        }

        //protected override void OnGotFocus(GotFocusEventArgs e)
        //{
        //    e.Handled = true;
        //}

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
#endif
        private void UpdateWindowTransparency()
        {
            if (OperatingSystem.IsWindows() 
                && this.PlatformImpl is Avalonia.Win32.WindowImpl window)
            {
#if OS_WINDOWS
                var hwnd = window.Handle.Handle;
                var topLeft = WindowBorder.Bounds.TopLeft;
                var bottomRight = WindowBorder.Bounds.BottomRight;
                var margin = WindowBorder.Margin;
                var rrect = CreateRoundRectRgn(
                    (int)(topLeft.X - margin.Left),
                    (int)(topLeft.Y - margin.Top),
                    (int)(bottomRight.X + margin.Right),
                    (int)(bottomRight.Y + margin.Bottom),
                    5,
                    5);

                SetWindowRgn(hwnd, rrect, true);
#endif
            }
        }
        private void MakeNotClickable()
        {
            if (OperatingSystem.IsWindows())
            {
                if (this.PlatformImpl is Avalonia.Win32.WindowImpl window)
                {
#if OS_WINDOWS
                    var hwnd = window.Handle.Handle;
#endif
                    //const int GWL_EXSTYLE = -20;
                    //const uint
                    //    //WS_EX_NOACTIVATE = 0x08000000,
                    //    WS_EX_LAYERED = 0x00080000,
                    //    //WS_EX_TRANSPARENT = 0x00000020,
                    //    WS_EX_TOPMOST = 0x00000008;
                    //                    var getWindowLong = window.GetType()
                    //                        .GetMethod("GetExtendedStyle", BindingFlags.Instance | BindingFlags.NonPublic);

                    //                    var setWindowLong = window.GetType()
                    //                        .GetMethod("SetExtendedStyle", BindingFlags.Instance | BindingFlags.NonPublic);

                    //                    if (getWindowLong is null || setWindowLong is null)
                    //                    {
                    //                        return;
                    //                    }


                    //                    var style = (uint)getWindowLong.Invoke(window, null)!;

                    //                    setWindowLong.Invoke(window, new object[]
                    //                    {
                    //                        style | WS_EX_LAYERED | WS_EX_TOPMOST,
                    //                        true
                    //                    });

                    //                    window.SetTopmost(true);

                    //#if OS_WINDOWS
                    //                    var val = SetLayeredWindowAttributes(hwnd, 0x00000000, 0, (uint)LayeredWindowFlags.LWA_ALPHA);
                    //#endif
                }
            }
        }
    }
}
