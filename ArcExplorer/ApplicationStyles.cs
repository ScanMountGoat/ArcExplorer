using ArcExplorer.Models;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using System;

namespace ArcExplorer
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
            Web,
            Warning,
            Search
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
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default"),
        };

        private static readonly StyleInclude baseDark = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
        {
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
        };

        private static void UpdateTheme(StyleInclude theme)
        {
            var app = Application.Current;
            app.Styles[1] = theme;
        }
    }
}
