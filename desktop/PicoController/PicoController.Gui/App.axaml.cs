using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PicoController.Gui.Helpers;
using PicoController.Gui.Plugin;
using PicoController.Gui.ViewModels;
using PicoController.Gui.Views;
using PicoController.Plugin;
using Splat;
using System;

namespace PicoController.Gui
{
    public class App : Application
    {
        public override void Initialize()
        {
            Locator.CurrentMutable.RegisterLazySingleton(() => new Core.Config.ConfigRepository(), typeof(Core.Config.IConfigRepository));
            AvaloniaXamlLoader.Load(this);
        }

        public static IClassicDesktopStyleApplicationLifetime? DesktopApplicationLifetime => Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        private static MainWindow? MainWindow => DesktopApplicationLifetime?.MainWindow as MainWindow;

        public void ShowWindow_Click(object? sender, EventArgs e)
        {
            MainWindow?.RestoreWindowState();
        }

        public void ExitApp_Click(object? sender, EventArgs e)
        {
            DesktopApplicationLifetime?.Shutdown();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Locator.CurrentMutable.RegisterLazySingleton<IRepositoryHelper>(() => new RepositoryHelper());
            Locator.CurrentMutable.RegisterLazySingleton<Themes.ThemeManager>(Themes.ThemeManager.CreateManager);
            Locator.CurrentMutable.RegisterLazySingleton<IDisplayInfo>(() => new DisplayInfo());

            if (DesktopApplicationLifetime is not null)
            {
                DesktopApplicationLifetime.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
