using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace ArcExplorer.Models
{
    public sealed class ApplicationSettings
    {
        private const string preferencesFilePath = "ApplicationPreferences.json";
        public static ApplicationSettings Instance { get; } = FromJson(preferencesFilePath);

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

        public string CurrentHashesCommitSha { get; set; } = "1b2da43a6e4cbeb0809acc2d5f325314a3ea2f72";

        public string ExtractLocation { get; set; } = "export";

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

        internal void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(preferencesFilePath, json);
        }
    }
}
