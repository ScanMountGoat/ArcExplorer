using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public sealed class FileGridItem
    {
        public ApplicationStyles.Icon DetailsIconKey { get; }
        public ApplicationStyles.Icon TreeViewIconKey { get; }
        public ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;
        public ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;

        public bool IsShared { get; }

        public bool IsRegional { get; }
        public ulong? Offset { get; }
        public ulong? CompressedSize { get; }
        public ulong? DecompressedSize { get; }
        public string AbsolutePath { get; }
        public string Name { get; }

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

        public string? Extension { get; }

        public string? Description { get; }

        public bool IsCompressed { get; }

        public string SharedFileDescription => $"Shared with the following {SharedFilePaths.Count} files:";

        public Dictionary<string, string>? ObjectProperties { get; }


        private readonly Func<List<string>> getSharedFiles;

        public FileGridItem(FolderNode folder)
        {
            Name = folder.Name;
            AbsolutePath = folder.AbsolutePath;
            DetailsIconKey = folder.DetailsIconKey;
            getSharedFiles = () => new List<string>();
            TreeViewIconKey = folder.TreeViewIconKey;
        }

        public FileGridItem(FileNode file)
        {
            Name = file.Name;
            AbsolutePath = file.AbsolutePath;
            DetailsIconKey = file.DetailsIconKey;
            IsShared = file.IsShared;
            IsRegional = file.IsRegional;
            Offset = file.Offset;
            CompressedSize = file.CompressedSize;
            DecompressedSize = file.DecompressedSize;
            sharedFilePaths = file.SharedFilePaths;
            Extension = file.Extension;
            Description = file.Description;
            IsCompressed = file.IsCompressed;
            ObjectProperties = file.ObjectProperties;
            getSharedFiles = file.getSharedFiles;
            TreeViewIconKey = file.TreeViewIconKey;
        }
    }
}
