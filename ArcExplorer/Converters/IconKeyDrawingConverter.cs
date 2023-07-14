using Avalonia;
using Avalonia.Data.Converters;
using ArcExplorer.Models;
using System;
using System.Globalization;
using System.Linq;

namespace ArcExplorer.Converters
{
    public class IconKeyDrawingConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDarkTheme = ApplicationSettings.Instance.Theme == ApplicationSettings.VisualTheme.Dark;
            var icon = (ApplicationStyles.Icon)value;
            return icon switch
            {
                ApplicationStyles.Icon.FolderOpened => GetIconResource("folderOpenedIcon"),
                ApplicationStyles.Icon.FolderClosed => GetIconResource("folderClosedIcon"),
                ApplicationStyles.Icon.Document => isDarkTheme ? GetIconResource("documentWhiteIcon") : GetIconResource("documentIcon"),
                ApplicationStyles.Icon.BinaryFile => GetIconResource("binaryFileIcon"),
                ApplicationStyles.Icon.Bitmap => isDarkTheme ? GetIconResource("bitmapWhiteIcon") : GetIconResource("bitmapIcon"),
                ApplicationStyles.Icon.MaterialSpecular => GetIconResource("materialIcon"),
                ApplicationStyles.Icon.Link => isDarkTheme ? GetIconResource("linkWhiteIcon") : GetIconResource("linkIcon"),
                ApplicationStyles.Icon.Settings => isDarkTheme ? GetIconResource("settingsWhiteIcon") : GetIconResource("settingsIcon"),
                ApplicationStyles.Icon.OpenFile => GetIconResource("openFileIcon"),
                ApplicationStyles.Icon.Lvd => isDarkTheme ? GetIconResource("lvdWhiteIcon") : GetIconResource("lvdIcon"),
                ApplicationStyles.Icon.Web => isDarkTheme ? GetIconResource("webWhiteIcon") : GetIconResource("webIcon"),
                ApplicationStyles.Icon.Warning => GetIconResource("warningIcon"),
                ApplicationStyles.Icon.Search => isDarkTheme ? GetIconResource("searchWhiteIcon") : GetIconResource("searchIcon"),
                _ => GetIconResource("noneIcon"),
            };
        }

        public object? GetIconResource(string key)
        {
            var dictionary = Application.Current?.Resources.MergedDictionaries.FirstOrDefault();

            // TODO: Theme variant?
            object? resource = null;
            dictionary?.TryGetResource(key, null, out resource);
            return resource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
