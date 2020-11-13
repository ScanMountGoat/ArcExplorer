using Avalonia;
using Avalonia.Data.Converters;
using CrossArcAvaloniaConcept.Models;
using System;
using System.Globalization;

namespace CrossArcAvaloniaConcept.Converters
{
    public class IconKeyDrawingConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDarkTheme = ApplicationSettings.Instance.Theme == ApplicationSettings.VisualTheme.Dark;
            var icon = (ApplicationStyles.Icon)value;
            return icon switch
            {
                ApplicationStyles.Icon.FolderOpened => Application.Current.Resources["folderOpenedIcon"],
                ApplicationStyles.Icon.FolderClosed => Application.Current.Resources["folderClosedIcon"],
                ApplicationStyles.Icon.Document => isDarkTheme ? Application.Current.Resources["documentWhiteIcon"] : Application.Current.Resources["documentIcon"],
                ApplicationStyles.Icon.BinaryFile => Application.Current.Resources["binaryFileIcon"],
                ApplicationStyles.Icon.Bitmap => isDarkTheme ? Application.Current.Resources["bitmapWhiteIcon"] : Application.Current.Resources["bitmapIcon"],
                ApplicationStyles.Icon.MaterialSpecular => Application.Current.Resources["materialIcon"],
                ApplicationStyles.Icon.Link => isDarkTheme ? Application.Current.Resources["linkWhiteIcon"] : Application.Current.Resources["linkIcon"],
                ApplicationStyles.Icon.Settings => isDarkTheme ? Application.Current.Resources["settingsWhiteIcon"] : Application.Current.Resources["settingsIcon"],
                ApplicationStyles.Icon.OpenFile => Application.Current.Resources["openFileIcon"],
                ApplicationStyles.Icon.Lvd => isDarkTheme ? Application.Current.Resources["lvdWhiteIcon"] : Application.Current.Resources["lvdIcon"],
                ApplicationStyles.Icon.Web => isDarkTheme ? Application.Current.Resources["webWhiteIcon"] : Application.Current.Resources["webIcon"],
                _ => null,// Null will just render as blank.
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
