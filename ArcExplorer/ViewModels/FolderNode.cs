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

        public override bool IsExpanded
        {
            get => isExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref isExpanded, value);
                TreeViewIconKey = isExpanded ? ApplicationStyles.Icon.FolderOpened : ApplicationStyles.Icon.FolderClosed;
                Expanded?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool isExpanded;

        public bool HasInitialized { get; set; }

        public override Dictionary<string, string> ObjectProperties => new Dictionary<string, string>()
        {
            { "Child Count", Children.Count.ToString() }
        };

        public event EventHandler? Expanded;

        public FolderNode(string name, string absolutePath) : base(name, absolutePath)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
            DetailsIconKey = ApplicationStyles.Icon.FolderClosed;
        }
    }
}
