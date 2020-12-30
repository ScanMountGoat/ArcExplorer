﻿using ReactiveUI;
using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public class FolderNode : FileNodeBase
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

        public override Dictionary<string, string> ObjectProperties => new Dictionary<string, string>()
        {
            { "Child Count", Children.Count.ToString() }
        };

        public event EventHandler? Expanded;

        public FolderNode(string name, string absolutePath, bool isShared, bool isRegional) : base(name, absolutePath, isShared, isRegional)
        {
            TreeViewIconKey = ApplicationStyles.Icon.FolderClosed;
            DetailsIconKey = ApplicationStyles.Icon.FolderClosed;
        }
    }
}
