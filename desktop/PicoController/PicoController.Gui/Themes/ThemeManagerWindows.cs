using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Themes
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Platform validation", Justification = "Windows only, constructor throws on any other platform")]
    internal class ThemeManagerWindows : ThemeManager
    {
        public ThemeManagerWindows()
        {
            if(!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("Available only on Windows");

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            switch (e.Category)
            {
                case UserPreferenceCategory.General:
                    OnThemeChanged(IsDarkMode());
                    break;
            }
        }

        public override bool IsDarkMode()
        {
            if (OperatingSystem.IsWindows())
            {
                using RegistryKey? registry = Registry.CurrentUser.OpenSubKey(
               @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                return ((int?)registry?.GetValue("SystemUsesLightTheme") ?? 1) != 1;
            }

            return false;
        }
    }
}
