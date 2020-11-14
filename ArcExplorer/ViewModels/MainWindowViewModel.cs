using Avalonia.Collections;
using ReactiveUI;
using SmashArcNet;
using System;
using System.IO;

namespace ArcExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public AvaloniaList<FileNode> Files { get; } = new AvaloniaList<FileNode>();

        public FileNode? SelectedFile 
        { 
            get => selectedFile;
            set => this.RaiseAndSetIfChanged(ref selectedFile, value);
        }
        private FileNode? selectedFile;

        public string ArcVersion
        {
            get => arcVersion;
            set => this.RaiseAndSetIfChanged(ref arcVersion, value);
        }
        private string arcVersion = "";

        public string FileCount
        {
            get => fileCount;
            set => this.RaiseAndSetIfChanged(ref fileCount, value);
        }
        private string fileCount = "";

        public string ArcPath
        {
            get => arcPath;
            set => this.RaiseAndSetIfChanged(ref arcPath, value);
        }
        private string arcPath = "";

        public void OpenArc(string path)
        {
            // TODO: This is expensive and should be handled separately.
            HashLabels.Initialize("Hashes.txt");

            if (!ArcFile.TryOpenArc(path, out ArcFile? arcFile))
            {
                Serilog.Log.Logger.Information("Failed to open ARC file {@path}", path);
                return;
            }

            FileCount = arcFile.FileCount.ToString();
            ArcPath = path;

            PopulateFileTree(arcFile);
        }

        private void PopulateFileTree(ArcFile arcFile)
        {
            foreach (var node in arcFile.GetRootNodes())
            {
                var treeNode = LoadNodeAddToParent(arcFile, null, node);
                Files.Add(treeNode);
            }
        }

        private static FileNode LoadNodeAddToParent(ArcFile arcFile, FileNode? parent, ArcFileTreeNode arcNode)
        {
            switch (arcNode.Type)
            {
                case ArcFileTreeNode.FileType.Directory:
                    var folder = CreateFolderLoadChildren(arcFile, arcNode);
                    parent?.Children.Add(folder);
                    return folder;
                case ArcFileTreeNode.FileType.File:
                    var file = CreateFileNode(arcNode);
                    parent?.Children.Add(file);
                    return file;
                default:
                    throw new NotImplementedException($"Unable to create node from {arcNode.Type}");
            }
        }

        private static FileNode CreateFileNode(ArcFileTreeNode arcNode)
        {
            // TODO: Get regional, shared, etc info for files.
            // Assume no children for file nodes.
            return new FileNode(Path.GetFileName(arcNode.Path), arcNode.IsShared, arcNode.IsRegional, arcNode.Offset, arcNode.CompSize, arcNode.DecompSize);
        }

        private static FolderNode CreateFolderLoadChildren(ArcFile arcFile, ArcFileTreeNode arcNode)
        {
            // TODO: Get regional, shared, etc info for folders.
            // Use DirectoryInfo to account for trailing slashes.
            var folder = CreateFolderNode(arcNode);

            foreach (var child in arcFile.GetChildren(arcNode))
            {
                var childNode = child.Type switch
                {
                    ArcFileTreeNode.FileType.Directory => CreateFolderNode(child),
                    ArcFileTreeNode.FileType.File => CreateFileNode(child),
                    _ => throw new NotImplementedException($"Unsupported type {child.Type}")
                };

                // When the parent is expanded, load the grandchildren to support expanding the children.
                folder.Expanded += (s, e) => LoadChildrenAddToParent(arcFile, child, childNode);

                folder.Children.Add(childNode);
            }

            return folder;
        }

        private static FolderNode CreateFolderNode(ArcFileTreeNode arcNode)
        {
            return new FolderNode(new DirectoryInfo(arcNode.Path).Name, false, false);
        }

        private static void LoadChildrenAddToParent(ArcFile arcFile, ArcFileTreeNode arcNode, FileNode parent)
        {
            foreach (var child in arcFile.GetChildren(arcNode))
            {
                LoadNodeAddToParent(arcFile, parent, child);
            }
        }

        public void RebuildFileTree()
        {
            // TODO: Update icons without rebuilding the tree?
            //Files.Clear();
            //PopulateFileTree();
        }
    }
}
