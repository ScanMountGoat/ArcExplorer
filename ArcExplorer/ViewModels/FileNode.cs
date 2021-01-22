using ReactiveUI;
using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public sealed class FileNode : FileNodeBase
    {
        public ApplicationStyles.Icon DetailsIconKey
        {
            get => detailsIconKey;
            set => this.RaiseAndSetIfChanged(ref detailsIconKey, value);
        }
        private ApplicationStyles.Icon detailsIconKey = ApplicationStyles.Icon.Document;

        public ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;

        public ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;

        public bool IsShared { get; }

        public bool IsRegional { get; }
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

        public override bool IsExpanded { get; set; }

        private readonly Func<List<string>> getSharedFiles;

        public FileNode(string name, string absolutePath, string extension,
            bool isShared, bool isRegional, ulong offset, ulong compressedSize, ulong decompressedSize, bool isCompressed,
            Func<List<string>> getSharedFiles) : base(name, absolutePath)
        {
            Extension = extension;
            Offset = offset;
            IsShared = isShared;
            IsCompressed = isCompressed;
            IsRegional = isRegional;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            DetailsIconKey = FileFormatInfo.GetFileIconKey(Extension);
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
                info.Add("Compressed Size", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(CompressedSize)} bytes");
                info.Add("Decompressed Size", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(DecompressedSize)} bytes");
            }
            else
            {
                info.Add("Size", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(DecompressedSize)} bytes");
            }

            return info;
        }
    }
}
