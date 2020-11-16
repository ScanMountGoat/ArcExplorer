using System;

namespace ArcExplorer.Tools
{
    public static class ValueConversion
    {
        public static string GetValueFromPreferencesFormat(ulong value)
        {
            return Models.ApplicationSettings.Instance.DisplayFormat switch
            {
                Models.ApplicationSettings.IntegerDisplayFormat.Binary => Convert.ToString((long)value, 2),
                Models.ApplicationSettings.IntegerDisplayFormat.Decimal => value.ToString(),
                Models.ApplicationSettings.IntegerDisplayFormat.Hexadecimal => $"0x{value:X}",
                _ => throw new NotImplementedException($"Unsupported display format {Models.ApplicationSettings.Instance.DisplayFormat}")
            };
        }
    }
}
