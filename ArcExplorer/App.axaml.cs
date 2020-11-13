﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CrossArcAvaloniaConcept.ViewModels;
using CrossArcAvaloniaConcept.Views;
using Serilog;

namespace CrossArcAvaloniaConcept
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
            }
            base.OnFrameworkInitializationCompleted();

            ApplicationStyles.SetThemeFromSettings();
            ConfigureAndStartLogging();
        }

        private static void ConfigureAndStartLogging()
        {
            // TODO: Log to file.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .CreateLogger();
        }
    }
}
