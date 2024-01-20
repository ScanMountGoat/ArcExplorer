using ArcExplorer.Tools;
using SmashArcNet.RustTypes;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcExplorer.Models
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ApplicationSettings))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {

    }

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

        [JsonConverter(typeof(JsonStringEnumConverter<VisualTheme>))]
        public VisualTheme Theme { get; set; } = VisualTheme.Dark;

        [JsonConverter(typeof(JsonStringEnumConverter<IntegerDisplayFormat>))]
        public IntegerDisplayFormat DisplayFormat { get; set; } = IntegerDisplayFormat.Decimal;

        public string CurrentHashesCommitSha { get; set; } = "d24d397bc3cf5a306206d68ebf0077fc47a30f38";

        public string ExtractLocation { get; set; } = "export";

        public string? ArcStartupLocation { get; set; } = null;

        [JsonConverter(typeof(JsonStringEnumConverter<Region>))]
        public Region ArcRegion { get; set; } = Region.UsEnglish;

        // Default to something that will be before the current time.
        // This ensures the first run of the program tries to check for updates.
        public DateTime LastHashesUpdateCheckTime { get; set; } = DateTime.UnixEpoch;

        public string? ArcIpAddress { get; set; }

        public ApplicationSettings()
        {

        }

        private static ApplicationSettings FromJson(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new ApplicationSettings();
            }
            // Override the application settings if deserialization fails.
            return JsonSerializer.Deserialize(System.IO.File.ReadAllText(path), SourceGenerationContext.Default.ApplicationSettings) ?? new ApplicationSettings();
        }

        internal void SaveToFile()
        {
            var json = JsonSerializer.Serialize(this, SourceGenerationContext.Default.ApplicationSettings);
            System.IO.File.WriteAllText(preferencesFilePath, json);
        }
    }
}
