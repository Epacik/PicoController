using FluentAvalonia.Styling;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Themes
{
    internal abstract class ThemeManager : IDisposable
    {
        public static ThemeManager CreateManager()
        {
            if (OperatingSystem.IsWindows())
                return new ThemeManagerWindows();
            else
                return new ThemeManagerStub();
        }

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        protected void OnThemeChanged(bool isDarkMode)
        {
            var faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
            faTheme!.RequestedTheme = isDarkMode ? FluentAvaloniaTheme.DarkModeString : FluentAvaloniaTheme.LightModeString;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(isDarkMode));
        }

        public abstract bool IsDarkMode();

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();
                disposedValue = true;
            }
        }

        protected virtual void DisposeUnmanaged() {}
        protected virtual void DisposeManaged() {}

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
