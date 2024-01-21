using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public sealed class FileNode : FileNodeBase
    {
        public ulong Offset { get; }
        public ulong CompressedSize { get; }
        public ulong DecompressedSize { get; }
        public List<string> SharedFilePaths
        {
            get
            {
                // Lazy load for performance reasons.
                if (sharedFilePaths == null)
                {
                    sharedFilePaths = getSharedFiles();
                    // Use ascending alphabetical order.
                    sharedFilePaths.Sort();
                }
                return sharedFilePaths;
            }
        }
        private List<string>? sharedFilePaths;

        public string Extension { get; }

        public string Description { get; }

        public bool IsCompressed { get; }

        public string SharedFileDescription => $"Shared with the following {SharedFilePaths.Count} files:";

        public override Dictionary<string, string> ObjectProperties => GetPropertyInfo();

        public readonly Func<List<string>> getSharedFiles;

        public override ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;
        public override ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;
        public override ApplicationStyles.Icon DetailsIconKey => FileFormatInfo.GetFileIconKey(Extension);

        public FileNode(string name, string absolutePath, string extension,
            bool isShared, bool isRegional, ulong offset, ulong compressedSize, ulong decompressedSize, bool isCompressed,
            Func<List<string>> getSharedFiles) : base(name, absolutePath, isShared, isRegional)
        {
            Extension = extension;
            Offset = offset;
            IsCompressed = isCompressed;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            Description = FileFormatInfo.GetDescription(Extension);
            this.getSharedFiles = getSharedFiles;
        }

        private Dictionary<string, string> GetPropertyInfo()
        {
            var info = new Dictionary<string, string>()
            {
                { "Description", Description },
                { "Offset", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(Offset)} bytes" },
                { "Compressed", $"{IsCompressed}" },
            };

            // It's redundant to show the compressed size for uncompressed files.
            if (IsCompressed)
            {
                info.Add("Compressed Size", $"{GetFormattedSize(CompressedSize)}");
                info.Add("Decompressed Size", $"{GetFormattedSize(DecompressedSize)}");
            }
            else
            {
                info.Add("Size", $"{GetFormattedSize(DecompressedSize)}");
            }

            return info;
        }

        public static string GetFormattedSize(ulong sizeInBytes, bool showBytes = true)
        {
            // Using MB, KB, etc only makes sense for decimal.
            if (Models.ApplicationSettings.Instance.DisplayFormat != Models.ApplicationSettings.IntegerDisplayFormat.Decimal)
            {
                return $"{Tools.ValueConversion.GetValueFromPreferencesFormat(sizeInBytes)} bytes";
            }

            var text = "";
            if (sizeInBytes > 1024 * 1024 * 1024)
            {
                text = $"{sizeInBytes / 1024.0 / 1024.0 / 1024.0:0.00} GB";
            }
            else if (sizeInBytes > 1024 * 1024)
            {
                text = $"{sizeInBytes / 1024.0 / 1024.0:0.00} MB";
            }
            else
            {
                text = $"{sizeInBytes / 1024.0:0.00} KB";
            }

            if (showBytes)
            {
                return $"{text} ({sizeInBytes} bytes)";
            }

            return text;
        }
    }
}
