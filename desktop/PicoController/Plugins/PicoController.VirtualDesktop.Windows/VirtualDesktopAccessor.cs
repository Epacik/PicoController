using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.VirtualDesktop.Windows;

internal static partial class VirtualDesktopAccessorInterop
{
    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial Int32 GetDesktopCount();

    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial Int32 GetCurrentDesktopNumber();

    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial void GoToDesktopNumber(Int32 desktopNumber);
}
