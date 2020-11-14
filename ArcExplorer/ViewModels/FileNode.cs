using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ArcExplorer.ViewModels
{
    public class FileNode : ViewModelBase
    {
        public string Name { get; } = "";

        public ApplicationStyles.Icon DetailsIconKey { get; } = ApplicationStyles.Icon.Document;

        public ApplicationStyles.Icon TreeViewIconKey 
        { 
            get => treeViewIconKey; 
            protected set => this.RaiseAndSetIfChanged(ref treeViewIconKey, value); 
        }
        private ApplicationStyles.Icon treeViewIconKey = ApplicationStyles.Icon.Document;

        public ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;

        public ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;

        public bool IsShared { get; }

        public bool IsRegional { get; }

        public string Description { get; }

        public ulong Offset { get; }
        public ulong CompressedSize { get; }
        public ulong DecompressedSize { get; }

        public Dictionary<string, string> ObjectProperties => GetPropertyInfo();

        public ObservableCollection<FileNode> Children { get; } = new ObservableCollection<FileNode>();

        public FileNode(string name, bool isShared, bool isRegional, ulong offset, ulong compressedSize, ulong decompressedSize)
        {
            Name = name;
            IsShared = isShared;
            IsRegional = isRegional;
            var extension = Path.GetExtension(name);
            Offset = offset;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            DetailsIconKey = FileFormatInfo.GetFileIconKey(extension);
            Description = FileFormatInfo.GetDescription(extension);
        }

        public FileNode(string name, bool isShared, bool isRegional)
        {
            Name = name;
            IsShared = isShared;
            IsRegional = isRegional;
            var extension = Path.GetExtension(name);
            DetailsIconKey = FileFormatInfo.GetFileIconKey(extension);
            Description = FileFormatInfo.GetDescription(extension);
        }

        private static string GetValueFromFormat(ulong value)
        {
            return Models.ApplicationSettings.Instance.DisplayFormat switch
            {
                Models.ApplicationSettings.IntegerDisplayFormat.Binary => System.Convert.ToString((long)value, 2),
                Models.ApplicationSettings.IntegerDisplayFormat.Decimal => value.ToString(),
                Models.ApplicationSettings.IntegerDisplayFormat.Hexadecimal => $"0x{value:X}",
                _ => throw new System.NotImplementedException($"Unsupported display format {Models.ApplicationSettings.Instance.DisplayFormat}")
            };
        }

        protected virtual Dictionary<string, string> GetPropertyInfo()
        {
            // TODO: This will depend on the integer display format preferences.

            return new Dictionary<string, string>()
            {
                { "Offset", $"{GetValueFromFormat(Offset)} bytes" },
                { "Compressed Size", $"{GetValueFromFormat(CompressedSize)} bytes" },
                { "Decompressed Size", $"{GetValueFromFormat(DecompressedSize)} bytes" },
            };
        }
    }
}
