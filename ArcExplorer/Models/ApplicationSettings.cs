using ArcExplorer.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmashArcNet.RustTypes;
using System;

namespace ArcExplorer.Models
{
    public sealed class ApplicationSettings
    {
        public static readonly string preferencesFilePath = ApplicationDirectory.CreateAbsolutePath("ApplicationPreferences.json");

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

        public bool StartMaximized { get; set; } = true;

        public bool MergeTrailingSlash { get; set; } = true;

        [JsonConverter(typeof(StringEnumConverter))]
        public VisualTheme Theme { get; set; } = VisualTheme.Dark;

        [JsonConverter(typeof(StringEnumConverter))]
        public IntegerDisplayFormat DisplayFormat { get; set; } = IntegerDisplayFormat.Decimal;

        public string CurrentHashesCommitSha { get; set; } = "1b2da43a6e4cbeb0809acc2d5f325314a3ea2f72";

        public string ExtractLocation { get; set; } = "export";

        public string? ArcStartupLocation { get; set; } = null;

        [JsonConverter(typeof(StringEnumConverter))]
        public Region ArcRegion { get; set; } = Region.UsEnglish;

        // Default to something that will be before the current time.
        // This ensures the first run of the program tries to check for updates.
        public DateTime LastHashesUpdateCheckTime = DateTime.UnixEpoch;

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
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(preferencesFilePath, json);
        }
    }
}
