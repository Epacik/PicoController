﻿using System.Runtime.InteropServices;
using System.Security;

namespace PicoController.VirtualDesktop.Windows;

internal static partial class VirtualDesktopAccessorInterop
{
    private const string _libName = "VirtualDesktopAccessor.dll";

    [LibraryImport(_libName), SuppressUnmanagedCodeSecurity]
    public static partial Int32 GetDesktopCount();

    [LibraryImport(_libName), SuppressUnmanagedCodeSecurity]
    public static partial Int32 GetCurrentDesktopNumber();

    [LibraryImport(_libName), SuppressUnmanagedCodeSecurity]
    public static partial void GoToDesktopNumber(Int32 desktopNumber);

    [DllImport(_libName), SuppressUnmanagedCodeSecurity]
    public static extern int GetDesktopName(int desktopNumber, IntPtr str, int length);
}
