using ArcExplorer.Models;
using Avalonia.Collections;
using SerilogTimings;
using SmashArcNet;
using SmashArcNet.Nodes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArcExplorer.ViewModels
{
    internal static class FileTree
    {
        /// <summary>
        /// Clears the existing items in <paramref name="files"/> and populates the base level from <paramref name="arcFile"/>.
        /// Directories are lazy loaded and will load their children after being expanded.
        /// The parameter to <paramref name="extractStartCallBack"/> is the task description.
        /// </summary>
        /// <param name="arcFile">the ARC to load</param>
        /// <param name="files">the file list to be cleared and updated</param>
        /// <param name="extractStartCallBack">called before starting an extract operation</param>
        /// <param name="extractEndCallBack">called after starting an extract operation</param>
        public static void PopulateFileTree(ArcFile arcFile, AvaloniaList<FileNodeBase> files, Action<string> extractStartCallBack, Action extractEndCallBack)
        {
            // Replace existing files with the new ARC.
            // TODO: Is memory being correctly freed?
            files.Clear();
            foreach (var node in arcFile.GetRootNodes())
            {
                var treeNode = LoadNodeAddToParent(arcFile, null, node, extractStartCallBack, extractEndCallBack);
                files.Add(treeNode);
            }
        }

        private static FileNodeBase LoadNodeAddToParent(ArcFile arcFile, FileNodeBase? parent, IArcNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            switch (arcNode)
            {
                case ArcDirectoryNode directoryNode:
                    var folder = CreateFolderLoadChildren(arcFile, directoryNode, taskStart, taskEnd);
                    parent?.Children.Add(folder);
                    return folder;
                case ArcFileNode fileNode:
                    var file = CreateFileNode(arcFile, fileNode, taskStart, taskEnd);
                    parent?.Children.Add(file);
                    return file;
                default:
                    throw new NotImplementedException($"Unable to create node from {arcNode}");
            }
        }

        private static FileNode CreateFileNode(ArcFile arcFile, ArcFileNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            // Lazy initialize the shared file list for performance reasons.
            List<string> getSharedFiles() => arcFile.GetSharedFilePaths(arcNode);

            var fileNode = new FileNode(arcNode.FileName, arcNode.Path, arcNode.Extension, arcNode.IsShared, arcNode.IsRegional, arcNode.Offset, arcNode.CompSize, arcNode.DecompSize, getSharedFiles);

            fileNode.FileExtracting += (s, e) => ExtractFileAsync(arcFile, arcNode, taskStart, taskEnd);

            return fileNode;
        }

        private static bool TryExtractFile(ArcFile arcFile, ArcFileNode arcNode)
        {
            var exportPath = GetExportPath(arcNode);

            // Extraction will fail if the directory doesn't exist.
            var exportFileDirectory = Path.GetDirectoryName(exportPath);
            try
            {
                if (!Directory.Exists(exportFileDirectory))
                    Directory.CreateDirectory(exportFileDirectory);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, "Error creating directory {@exportFileDirectory}", exportFileDirectory);
                return false;
            }


            // Extraction may fail.
            // TODO: Update the C# bindings to store more detailed error info?
            if (!arcFile.TryExtractFile(arcNode, exportPath))
            {
                Serilog.Log.Logger.Error("Failed to extract to {@path}", exportPath);
                return false;
            }

            return true;
        }

        private static string GetExportPath(ArcFileNode arcNode)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var paths = new string[] { currentDirectory, ApplicationSettings.Instance.ExtractLocation };

            // stream: -> stream and prebuilt: -> prebuilt to avoid invalid characters in paths.
            var filePath = arcNode.Path.Replace(":", "");
            if (arcNode.FileName.StartsWith("0x"))
                filePath += "." + arcNode.Extension;

            var exportPath = Path.Combine(paths.Concat(filePath.Split('/')).ToArray());
            return exportPath;
        }

        private static FolderNode CreateFolderLoadChildren(ArcFile arcFile, ArcDirectoryNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            var folder = CreateFolderNode(arcFile, arcNode, taskStart, taskEnd);

            var arcNodeTreeNode = new List<Tuple<IArcNode, FileNodeBase>>();
            foreach (var child in arcFile.GetChildren(arcNode))
            {
                FileNodeBase childNode = child switch
                {
                    ArcDirectoryNode directory => CreateFolderNode(arcFile, directory, taskStart, taskEnd),
                    ArcFileNode file => CreateFileNode(arcFile, file, taskStart, taskEnd),
                    _ => throw new NotImplementedException($"Unable to create node from {child}")
                };

                folder.Children.Add(childNode);

                arcNodeTreeNode.Add(new Tuple<IArcNode, FileNodeBase>(child, childNode));
            }

            // When the parent is expanded, load the grandchildren to support expanding the children.
            // TODO: There's probably a cleaner way to do this.
            folder.Expanded += (s, e) =>
            {
                if (!folder.HasInitialized)
                {
                    foreach (var pair in arcNodeTreeNode)
                    {
                        LoadChildrenAddToParent(arcFile, pair.Item1, pair.Item2, taskStart, taskEnd);
                    }
                    folder.HasInitialized = true;
                }
            };

            return folder;
        }

        private static async Task RunBackgroundTask(string taskDescription, Func<bool> taskToRun, Action<string> taskStart, Action taskEnd)
        {

            taskStart(taskDescription);

            await Task.Run(() =>
            {
                using (var operation = Operation.Begin(taskDescription))
                {
                    if (taskToRun())
                        operation.Complete();
                    else
                        operation.Cancel();
                }
            });

            taskEnd();
        }

        private static bool ExtractFilesRecursive(ArcFile arcFile, ArcDirectoryNode arcNode)
        {
            foreach (var child in arcFile.GetChildren(arcNode))
            {
                // Assume files have no children, so only recurse for directories.
                switch (child)
                {
                    case ArcFileNode file:
                        TryExtractFile(arcFile, file);
                        break;
                    case ArcDirectoryNode directory:
                        ExtractFilesRecursive(arcFile, directory);
                        break;
                    default:
                        break;
                }
            }

            // Assume the operation succeeds for now.
            // Individual file extractions may still fail.
            return true;
        }

        private static void LoadChildrenAddToParent(ArcFile arcFile, IArcNode arcNode, FileNodeBase parent, Action<string> taskStart, Action taskEnd)
        {
            if (arcNode is ArcDirectoryNode directoryNode)
            {
                foreach (var child in arcFile.GetChildren(directoryNode))
                {
                    LoadNodeAddToParent(arcFile, parent, child, taskStart, taskEnd);
                }
            }
        }

        private static FolderNode CreateFolderNode(ArcFile arcFile, ArcDirectoryNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            var folder = new FolderNode(new DirectoryInfo(arcNode.Path).Name, arcNode.Path, false, false);
            folder.FileExtracting += (s, e) => ExtractFolderAsync(arcFile, arcNode, taskStart, taskEnd);
            return folder;
        }

        private static async void ExtractFileAsync(ArcFile arcFile, ArcFileNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            // TODO: Files extract quickly, so there's no need to update the UI by calling taskStart.
            await RunBackgroundTask($"Extracting {arcNode.Path}", () => TryExtractFile(arcFile, arcNode), taskStart, taskEnd);
        }

        private static async void ExtractFolderAsync(ArcFile arcFile, ArcDirectoryNode arcNode, Action<string> taskStart, Action taskEnd)
        {
            await RunBackgroundTask($"Extracting files from {arcNode.Path}", () => ExtractFilesRecursive(arcFile, arcNode), taskStart, taskEnd);
        }

    }
}
