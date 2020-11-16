using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ArcExplorer.ViewModels
{
    public abstract class FileNodeBase : ViewModelBase
    {
        public string Name { get; } = "";

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

        public ApplicationStyles.Icon SharedIconKey => IsShared ? ApplicationStyles.Icon.Link : ApplicationStyles.Icon.None;

        public ApplicationStyles.Icon RegionalIconKey => IsRegional ? ApplicationStyles.Icon.Web : ApplicationStyles.Icon.None;

        public bool IsShared { get; }

        public bool IsRegional { get; }

        public ObservableCollection<FileNodeBase> Children { get; } = new ObservableCollection<FileNodeBase>();

        public abstract Dictionary<string, string> ObjectProperties { get; }

        public FileNodeBase(string name, bool isShared, bool isRegional)
        {
            Name = name;
            IsShared = isShared;
            IsRegional = isRegional;
        }
    }
}
