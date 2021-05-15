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

        public ApplicationStyles.Icon TreeViewIconKey
        {
            get => treeViewIconKey;
            protected set => this.RaiseAndSetIfChanged(ref treeViewIconKey, value);
        }
        private ApplicationStyles.Icon treeViewIconKey = ApplicationStyles.Icon.Document;

        public event EventHandler? FileExtracting;

        public abstract Dictionary<string, string> ObjectProperties { get; }

        public FileNodeBase(string name, string absolutePath)
        {
            Name = name;
            AbsolutePath = absolutePath;
        }

        public void OnFileExtracting()
        {
            FileExtracting?.Invoke(this, EventArgs.Empty);
        }
    }
}
