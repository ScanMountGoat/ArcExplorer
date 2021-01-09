using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public abstract class FileNodeBase : ViewModelBase
    {
        public string Name { get; } = "";

        public string AbsolutePath { get; } = "";


        public ApplicationStyles.Icon DetailsIconKey
        {
            get => detailsIconKey;
            protected set => this.RaiseAndSetIfChanged(ref detailsIconKey, value);
        }
        private ApplicationStyles.Icon detailsIconKey = ApplicationStyles.Icon.Document;

        public ApplicationStyles.Icon TreeViewIconKey
        {
            get => treeViewIconKey;
            protected set => this.RaiseAndSetIfChanged(ref treeViewIconKey, value);
        }
        private ApplicationStyles.Icon treeViewIconKey = ApplicationStyles.Icon.Document;

        public event EventHandler? FileExtracting;

        public ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;

        public ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;

        public bool IsShared { get; }

        public bool IsRegional { get; }

        public abstract bool IsExpanded { get; set; }

        public AvaloniaList<FileNodeBase> Children { get; } = new AvaloniaList<FileNodeBase>();

        public abstract Dictionary<string, string> ObjectProperties { get; }

        public string Description { get; protected set; } = "";

        public FileNodeBase(string name, string absolutePath, bool isShared, bool isRegional)
        {
            Name = name;
            AbsolutePath = absolutePath;
            IsShared = isShared;
            IsRegional = isRegional;
        }

        public void OnFileExtracting()
        {
            FileExtracting?.Invoke(this, EventArgs.Empty);
        }
    }
}
