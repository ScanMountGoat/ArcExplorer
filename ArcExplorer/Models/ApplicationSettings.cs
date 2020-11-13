using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrossArcAvaloniaConcept.Models
{
    public sealed class ApplicationSettings
    {
        public static ApplicationSettings Instance { get; } = FromJson("ApplicationPreferences.json");

        public enum VisualTheme
        {
            Light,
            Dark
        }

        public enum IntegerDisplayFormat
        {
            Decimal,
            Hexadecimal,
            Binary
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public VisualTheme Theme { get; set; } = VisualTheme.Dark;


        [JsonConverter(typeof(StringEnumConverter))]
        public IntegerDisplayFormat DisplayFormat { get; set; } = IntegerDisplayFormat.Decimal;

        private ApplicationSettings()
        {

        }

        private static ApplicationSettings FromJson(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new ApplicationSettings();
            }

            return JsonConvert.DeserializeObject<ApplicationSettings>(System.IO.File.ReadAllText(path));
        }
    }
}
