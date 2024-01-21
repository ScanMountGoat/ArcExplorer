using ArcExplorer.Models;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public class FolderNode : FileNodeBase
    {
        public override ApplicationStyles.Icon DetailsIconKey => ApplicationStyles.Icon.FolderClosed;
        public override ApplicationStyles.Icon TreeViewIconKey => ApplicationStyles.Icon.FolderClosed;
        internal SmashArcNet.Nodes.ArcDirectoryNode arcNode;

        public override Dictionary<string, string> ObjectProperties => new Dictionary<string, string>()
        {
            // TODO: Add child count and additional info.
        };

        public FolderNode(string absolutePath, SmashArcNet.Nodes.ArcDirectoryNode node)
            : base(Tools.ArcPaths.GetDirectoryName(absolutePath, !ApplicationSettings.Instance.MergeTrailingSlash), absolutePath, false, false)
        {
            arcNode = node;
        }
    }
}
