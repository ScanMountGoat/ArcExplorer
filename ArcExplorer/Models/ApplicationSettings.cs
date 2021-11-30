using ArcExplorer.Tools;
using SmashArcNet.RustTypes;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public bool StartMaximized { get; set; } = false;

        public bool MergeTrailingSlash { get; set; } = true;

        public VisualTheme Theme { get; set; } = VisualTheme.Dark;

        public IntegerDisplayFormat DisplayFormat { get; set; } = IntegerDisplayFormat.Decimal;

        public string CurrentHashesCommitSha { get; set; } = "d24d397bc3cf5a306206d68ebf0077fc47a30f38";

        public string ExtractLocation { get; set; } = "export";

        public string? ArcStartupLocation { get; set; } = null;

        public Region ArcRegion { get; set; } = Region.UsEnglish;

        // Default to something that will be before the current time.
        // This ensures the first run of the program tries to check for updates.
        public DateTime LastHashesUpdateCheckTime { get; set; } = DateTime.UnixEpoch;

        public string? ArcIpAddress { get; set; }

        private ApplicationSettings()
        {

        }

        private static ApplicationSettings FromJson(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new ApplicationSettings();
            }
            // Override the application settings if deserialization fails.
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            return JsonSerializer.Deserialize<ApplicationSettings>(System.IO.File.ReadAllText(path), options) ?? new ApplicationSettings();
        }

        internal void SaveToFile()
        {
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() }, WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);

            System.IO.File.WriteAllText(preferencesFilePath, json);
        }
    }
}
