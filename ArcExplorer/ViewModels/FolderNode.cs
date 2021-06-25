using ReactiveUI;
using System.Collections.Generic;
using System.IO;

namespace ArcExplorer.ViewModels
{
    public class FolderNode : FileNodeBase
    {
        public ApplicationStyles.Icon DetailsIconKey
        {
            get => detailsIconKey;
            set => this.RaiseAndSetIfChanged(ref detailsIconKey, value);
        }
        private ApplicationStyles.Icon detailsIconKey = ApplicationStyles.Icon.Document;

        internal SmashArcNet.Nodes.ArcDirectoryNode arcNode;

        public override Dictionary<string, string> ObjectProperties => new Dictionary<string, string>()
        {
        };

        public FolderNode(string absolutePath, SmashArcNet.Nodes.ArcDirectoryNode node) : base(GetDirectoryName(absolutePath), absolutePath)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
            DetailsIconKey = ApplicationStyles.Icon.FolderClosed;
            arcNode = node;
        }

        private static string GetDirectoryName(string absolutePath)
        {
            // DirectoryInfo doesn't handle null or empty strings.
            if (string.IsNullOrEmpty(absolutePath))
                return "";

            return new DirectoryInfo(absolutePath).Name;
        }
    }
}
