using System;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    public abstract class FileNodeBase : ViewModelBase
    {
        public string Name { get; } = "";

        public string AbsolutePath { get; } = "";


        public event EventHandler? FileExtracting;
        public event EventHandler? OpeningParentFolder;

        public abstract List<Tuple<string, string>> ObjectProperties { get; }

        public virtual ApplicationStyles.Icon TreeViewIconKey => ApplicationStyles.Icon.Document;
        public virtual ApplicationStyles.Icon DetailsIconKey => ApplicationStyles.Icon.Document;
        public virtual ApplicationStyles.Icon SharedIconKey => ApplicationStyles.Icon.None;
        public virtual ApplicationStyles.Icon RegionalIconKey => ApplicationStyles.Icon.None;
        public bool IsShared { get; }
        public bool IsRegional { get; }

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

        public void OnOpeningParentFolder()
        {
            OpeningParentFolder?.Invoke(this, EventArgs.Empty);
        }
    }
}
