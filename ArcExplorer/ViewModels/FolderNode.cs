using ReactiveUI;
using System;
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

        public void OnLoadingChildren()
        {
            LoadingChildren?.Invoke(this, EventArgs.Empty);
        }

        public override Dictionary<string, string> ObjectProperties => new Dictionary<string, string>()
        {
            { "Child Count", "TODO" }
        };

        public event EventHandler? LoadingChildren;

        public FolderNode(string name, string absolutePath, SmashArcNet.Nodes.ArcDirectoryNode node) : base(name, absolutePath)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
            DetailsIconKey = ApplicationStyles.Icon.FolderClosed;
            arcNode = node;
        }
    }
}
