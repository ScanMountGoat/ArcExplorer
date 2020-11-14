using ReactiveUI;
using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public class FolderNode : FileNode
    {
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref isExpanded, value);
                TreeViewIconKey = isExpanded ? ApplicationStyles.Icon.FolderOpened : ApplicationStyles.Icon.FolderClosed;
                if (value)
                    Expanded?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool isExpanded;

        public event EventHandler? Expanded;

        public FolderNode(string name, bool isShared, bool isRegional) : base(name, isShared, isRegional)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
        }

        protected override Dictionary<string, string> GetPropertyInfo()
        {
            return new Dictionary<string, string>()
            {
                { "File Count", Children.Count.ToString() }
            };
        }
    }
}
