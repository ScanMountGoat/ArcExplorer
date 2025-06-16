using Avalonia.Data.Converters;
using System;
using System.Globalization;
using SmashArcNet.RustTypes;

namespace ArcExplorer.Converters
{
    public class RegionStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var region = (Region)value;
            return region switch
            {
                Region.None => "None",
                Region.Japanese => "Japanese",
                Region.UsEnglish => "English (US)",
                Region.UsFrench => "French (US)",
                Region.UsSpanish => "Spanish (US)",
                Region.EuEnglish => "English (EU)",
                Region.EuFrench => "French (EU)",
                Region.EuSpanish => "Spanish (EU)",
                Region.EuGerman => "German (EU)",
                Region.EuDutch => "Dutch (EU)",
                Region.EuItalian => "Italian (EU)",
                Region.EuRussian => "Russian (EU)",
                Region.Korean => "Korean",
                Region.ChinaChinese => "Chinese (China)",
                Region.TaiwanChinese => "Chinese (Taiwan)",
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
