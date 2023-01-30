using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.VirtualDesktop.Windows;

internal static partial class VirtualDesktopAccessorInterop
{
    private const string _libName = "VirtualDesktopAccessor.dll";

    [LibraryImport(_libName)]
    public static partial Int32 GetDesktopCount();

    [LibraryImport(_libName)]
    public static partial Int32 GetCurrentDesktopNumber();

    [LibraryImport(_libName)]
    public static partial void GoToDesktopNumber(Int32 desktopNumber);

    [DllImport(_libName)]
    public static extern int GetDesktopName(int desktopNumber, IntPtr str, int length);
}
