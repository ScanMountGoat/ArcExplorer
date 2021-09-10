using ReactiveUI;

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

        public string Description
        {
            get
            {
                if (Node is FileNode file)
                    return file.Description;
                else
                    return "";
            }
        }

        public Size CompressedSize
        {
            get
            {
                if (Node is FileNode file)
                    return new Size(file.CompressedSize, FileNode.GetFormattedSize(file.CompressedSize, false));
                else
                    return new Size(0, "");
            }
        }

        public Size DecompressedSize
        {
            get
            {
                if (Node is FileNode file)
                    return new Size(file.DecompressedSize, FileNode.GetFormattedSize(file.DecompressedSize, false));
                else
                    return new Size(0, "");
            }
        }


        public class Size : ViewModelBase
        {
            public ulong SizeValue { get; }

            public string Description { get; }

            public Size(ulong value, string description)
            {
                SizeValue = value;
                Description = description;
            }
        }

        public FileNodeBase Node { get; }

        public FileGridItem(FileNodeBase node)
        {
            Node = node;
        }
    }
}
