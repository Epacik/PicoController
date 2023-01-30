using System.Runtime.InteropServices;
using System.Text;

internal static partial class VirtualDesktopAccessorInterop
{
    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial Int32 GetDesktopCount();

    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial Int32 GetCurrentDesktopNumber();

    [LibraryImport("VirtualDesktopAccessor.dll")]
    public static partial void GoToDesktopNumber(Int32 desktopNumber);

    [DllImport("VirtualDesktopAccessor.dll", CharSet = CharSet.Unicode)]
    public static extern int GetDesktopName(int desktopNumber, StringBuilder str, int length);
}