namespace ArcExplorer.ViewModels
{
    public sealed class FileGridItem
    {
        public ApplicationStyles.Icon DetailsIconKey => Node.DetailsIconKey;
        public ApplicationStyles.Icon TreeViewIconKey => Node.TreeViewIconKey;
        public ApplicationStyles.Icon SharedIconKey => Node.SharedIconKey;
        public ApplicationStyles.Icon RegionalIconKey => Node.RegionalIconKey;
        public string AbsolutePath => Node.AbsolutePath;
        public string Name => Node.Name;
        public bool IsShared => Node.IsShared;
        public bool IsRegional => Node.IsRegional;

        public FileNodeBase Node { get; }

        public FileGridItem(FileNodeBase node)
        {
            Node = node;
        }
    }
}
