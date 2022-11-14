namespace PicoController.Gui.Themes
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeChangedEventArgs(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
        }

        public bool IsDarkMode { get; }
    }
}