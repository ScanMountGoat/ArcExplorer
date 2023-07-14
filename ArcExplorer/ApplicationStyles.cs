using ArcExplorer.Models;
using Avalonia;

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
            if (Application.Current != null)
            {
                switch (ApplicationSettings.Instance.Theme)
                {
                    case ApplicationSettings.VisualTheme.Dark:
                        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                        break;
                    case ApplicationSettings.VisualTheme.Light:
                        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
