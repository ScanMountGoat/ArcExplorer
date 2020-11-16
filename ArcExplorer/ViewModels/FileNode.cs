using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;

namespace ArcExplorer.ViewModels
{
    public sealed class FileNode : FileNodeBase
    {
        public ulong Offset { get; }
        public ulong CompressedSize { get; }
        public ulong DecompressedSize { get; }
        public string Description { get; }


        public override Dictionary<string, string> ObjectProperties => GetPropertyInfo();

        /// <summary>
        /// Occurs when an extract operation is started. 
        /// </summary>
        public event EventHandler? FileExtracting;

        public void OnFileExtracting()
        {
            FileExtracting?.Invoke(this, EventArgs.Empty);
        }

        public FileNode(string name, bool isShared, bool isRegional, ulong offset, ulong compressedSize, ulong decompressedSize) : base(name, isShared, isRegional)
        {
            var extension = Path.GetExtension(name);
            Offset = offset;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            DetailsIconKey = FileFormatInfo.GetFileIconKey(extension);
            Description = FileFormatInfo.GetDescription(extension);
        }

        private Dictionary<string, string> GetPropertyInfo()
        {
            // TODO: This will depend on the integer display format preferences.

            return new Dictionary<string, string>()
            {
                { "Offset", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(Offset)} bytes" },
                { "Compressed Size", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(CompressedSize)} bytes" },
                { "Decompressed Size", $"{Tools.ValueConversion.GetValueFromPreferencesFormat(DecompressedSize)} bytes" },
            };
        }
    }
}
