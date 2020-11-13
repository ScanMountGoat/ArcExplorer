using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using CrossArcAvaloniaConcept.Models;
using CrossArcAvaloniaConcept.ViewModels;
using CrossArcAvaloniaConcept.Views;
using System;

namespace CrossArcAvaloniaConcept
{
    public static class ApplicationStyles
    {
        public enum Icon
        {
            None,
            FolderOpened,
            FolderClosed,
            Document,
            BinaryFile,
            Bitmap,
            MaterialSpecular,
            Link,
            Settings,
            OpenFile,
            Lvd,
            Web
        }

        public static void SetThemeFromSettings()
        {
            switch (ApplicationSettings.Instance.Theme)
            {
                case ApplicationSettings.VisualTheme.Dark:
                    SetDarkTheme();
                    break;
                case ApplicationSettings.VisualTheme.Light:
                    SetLightTheme();
                    break;
                default:
                    break;
            }
        }

        public static void SetLightTheme() => UpdateTheme(baseLight);
        public static void SetDarkTheme() => UpdateTheme(baseDark);

        private static readonly StyleInclude baseLight = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
        {
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
        };

        private static readonly StyleInclude baseDark = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
        {
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
        };

        private static void UpdateTheme(StyleInclude theme)
        {
            var app = Application.Current;
            app.Styles[1] = theme;

            if (app.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime c && c.MainWindow is MainWindow mainWindow)
            {
                SetWindowTheme(mainWindow, theme);

                // Don't initialize the value here to avoid recursion in the Lazy<T> factory method.
                if (PreferencesWindow.Instance.IsValueCreated)
                    SetWindowTheme(PreferencesWindow.Instance.Value, theme);

                (mainWindow.DataContext as MainWindowViewModel)?.RebuildFileTree();
            }
        }

        private static void SetWindowTheme(Window window, StyleInclude theme)
        {
            if (window == null)
                return;

            if (window.Styles.Count == 0)
                window.Styles.Add(theme);
            else
                window.Styles[0] = theme;
        }
    }
}
