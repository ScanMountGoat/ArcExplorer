using ReactiveUI;
using System.Collections.Generic;

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
            // TODO: Add child count and additional info.
        };

        public FolderNode(string absolutePath, SmashArcNet.Nodes.ArcDirectoryNode node) : base(Tools.ArcPaths.GetDirectoryName(absolutePath), absolutePath)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
            DetailsIconKey = ApplicationStyles.Icon.FolderClosed;
            arcNode = node;
        }
    }
}
