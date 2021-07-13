using ArcExplorer.Models;
using SerilogTimings;
using SmashArcNet;
using SmashArcNet.Nodes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ArcExplorer.ViewModels;

namespace ArcExplorer.Tools
{
    internal static class FileTree
    {
        private class ExtractResult
        {
            public bool Success { get; set; }
            public string CompletionSummary { get; set; } = "";
        }

        /// <summary>
        /// Loads the base level from <paramref name="arcFile"/>.
        /// The parameter to <paramref name="extractStartCallBack"/> is the task description.
        /// </summary>
        /// <param name="arcFile">the ARC to load</param>
        /// <param name="extractStartCallBack">called before starting an extract operation</param>
        /// <param name="extractReportProgressCallBack">contains the file path and progress percentage on extracting multiple files</param>
        /// <param name="extractEndCallBack">called after starting an extract operation with a progress message</param>
        /// <param name="mergeTrailingSlash">Directories with and without a trailing slash are considered identical when <c>true</c></param>
        /// <returns>The nodes for the root level</returns>
        public static List<FileNodeBase> CreateRootLevelNodes(ArcFile arcFile,
            Action<string> extractStartCallBack,
            Action<string, double> extractReportProgressCallBack,
            Action<string> extractEndCallBack,
            bool mergeTrailingSlash)
        {
            var files = new List<FileNodeBase>();
            foreach (var node in arcFile.GetRootNodes(ApplicationSettings.Instance.ArcRegion))
            {
                // There is no parent for root nodes, so leave the parent as null.
                var treeNode = CreateNode(arcFile, node, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, mergeTrailingSlash);
                files.Add(treeNode);
            }
            return files;
        }

        /// <summary>
        /// Loads the child nodes of <paramref name="parent"/> from <paramref name="arcFile"/>.
        /// The parameter to <paramref name="extractStartCallBack"/> is the task description.
        /// </summary>
        /// <param name="arcFile">the ARC to load</param>
        /// <param name="parent">the folder whose children will be loaded</param>
        /// <param name="extractStartCallBack">called before starting an extract operation</param>
        /// <param name="extractReportProgressCallBack">contains the file path and progress percentage on extracting multiple files</param>
        /// <param name="extractEndCallBack">called after starting an extract operation with a progress message</param>
        /// <param name="mergeTrailingSlash">Directories with and without a trailing slash are considered identical when <c>true</c></param>
        public static List<FileNodeBase> CreateChildNodes(ArcFile arcFile, FolderNode parent,
            Action<string> extractStartCallBack,
            Action<string, double> extractReportProgressCallBack,
            Action<string> extractEndCallBack,
            bool mergeTrailingSlash)
        {
            return CreateChildNodes(arcFile, parent.AbsolutePath,
                extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, mergeTrailingSlash);
        }

        /// <summary>
        /// Loads the child nodes of <paramref name="parent"/> from <paramref name="arcFile"/>.
        /// The parameter to <paramref name="extractStartCallBack"/> is the task description.
        /// </summary>
        /// <param name="arcFile">the ARC to load</param>
        /// <param name="parentPath">the folder whose children will be loaded</param>
        /// <param name="extractStartCallBack">called before starting an extract operation</param>
        /// <param name="extractReportProgressCallBack">contains the file path and progress percentage on extracting multiple files</param>
        /// <param name="extractEndCallBack">called after starting an extract operation with a progress message</param>
        /// <param name="mergeTrailingSlash">Directories with and without a trailing slash are considered identical when <c>true</c></param>
        public static List<FileNodeBase> CreateChildNodes(ArcFile arcFile,
            string parentPath,
            Action<string> extractStartCallBack,
            Action<string, double> extractReportProgressCallBack,
            Action<string> extractEndCallBack,
            bool mergeTrailingSlash)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                // An empty path should load the root level since we don't use a node for the root.
                return CreateRootLevelNodes(arcFile, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, mergeTrailingSlash);
            }

            // TODO: Handle the case where the input is a file by using the parent directory.

            var files = new List<FileNodeBase>();

            if (mergeTrailingSlash)
            {
                // Load with and without a trailing slash to be more forgiving about the input path.
                // It's safe to try both since no children will be added for invalid paths.
                var cleanedPath = parentPath.Trim('/');
                AddChildNodes(arcFile, cleanedPath, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, files, mergeTrailingSlash);
                AddChildNodes(arcFile, cleanedPath + '/', extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, files, mergeTrailingSlash);
            }
            else
            {
                AddChildNodes(arcFile, parentPath, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, files, mergeTrailingSlash);
            }

            return files;
        }
        
        /// <summary>
        /// Searches the entire ARC file using a fuzzy search.
        /// </summary>
        /// <param name="arcFile">The ARC to search</param>
        /// <param name="extractStartCallBack"></param>
        /// <param name="extractReportProgressCallBack"></param>
        /// <param name="extractEndCallBack"></param>
        /// <param name="searchText">The string to search for</param>
        /// <param name="mergeTrailingSlash"></param>
        /// <returns>The results sorted by matching score</returns>
        public static List<FileNodeBase> SearchAllNodes(ArcFile arcFile, Action<string> extractStartCallBack,
            Action<string, double> extractReportProgressCallBack, Action<string> extractEndCallBack, string searchText, bool mergeTrailingSlash)
        {
            // TODO: The max result count has a big impact on performance and is worth tuning.
            var paths = arcFile.SearchFiles(searchText, 2000, ApplicationSettings.Instance.ArcRegion);

            var result = new List<FileNodeBase>((int)arcFile.FileCount);
            foreach (var path in paths)
            {
                // TODO: This triggers an error when calling smash-arc from SmashArcNet with an invalid path.
                if (path.Contains("0x"))
                    continue;

                var treeNode = CreateNodeFromPath(arcFile, path, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, mergeTrailingSlash);
                if (treeNode != null)
                    result.Add(treeNode);
            }

            // Avoiding sorting by path to preserve the order based on matching score.
            return result;
        }

        private static void AddAllFilesRecursive(ArcFile arcFile, List<IArcNode> files, ArcDirectoryNode parent)
        {
            foreach (var child in arcFile.GetChildren(parent, ApplicationSettings.Instance.ArcRegion))
            {
                // Assume files have no children, so only recurse for directories.
                switch (child)
                {
                    case ArcFileNode file:
                        files.Add(file);
                        break;
                    case ArcDirectoryNode directory:
                        files.Add(directory);
                        AddAllFilesRecursive(arcFile, files, directory);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void AddChildNodes(ArcFile arcFile, string parentPath, Action<string> extractStartCallBack, 
            Action<string, double> extractReportProgressCallBack, Action<string> extractEndCallBack, List<FileNodeBase> files, bool mergeTrailingSlash)
        {
            var arcNode = arcFile.CreateNode(parentPath, ApplicationSettings.Instance.ArcRegion);

            if (arcNode is ArcDirectoryNode directoryNode)
            {
                // Keep track of visited names to avoid having duplicate nodes differing by a trailing slash.
                // Assume file names are also unique within a directory.
                var names = new HashSet<string>();

                var folder = CreateFolder(arcFile, directoryNode, extractStartCallBack, extractReportProgressCallBack, 
                    extractEndCallBack, mergeTrailingSlash);

                foreach (var node in arcFile.GetChildren(directoryNode, ApplicationSettings.Instance.ArcRegion))
                {
                    var treeNode = CreateNode(arcFile, node, extractStartCallBack, extractReportProgressCallBack, extractEndCallBack, mergeTrailingSlash);

                    // Only ignore duplicates if directories differing by a trailing slash should be merged.
                    if (!mergeTrailingSlash || !names.Contains(treeNode.Name))
                    {
                        files.Add(treeNode);
                        names.Add(treeNode.Name);
                    }
                }
            }
        }

        public static FileNodeBase? CreateNodeFromPath(ArcFile arcFile, string absolutePath, 
            Action<string> taskStart, Action<string, double> reportProgress, Action<string> taskEnd, bool mergeTrailingSlash)
        {
            // TODO: This doesn't properly handle extraction.
            var arcNode = arcFile.CreateNode(absolutePath, ApplicationSettings.Instance.ArcRegion);
            if (arcNode == null)
                return null;

            return CreateNode(arcFile, arcNode, taskStart, reportProgress, taskEnd, mergeTrailingSlash);
        }

        private static FileNodeBase CreateNode(ArcFile arcFile, IArcNode arcNode, Action<string> taskStart,
            Action<string, double> reportProgress, Action<string> taskEnd, bool mergeTrailingSlash)
        {
            switch (arcNode)
            {
                case ArcDirectoryNode directoryNode:
                    var folder = CreateFolder(arcFile, directoryNode, taskStart, reportProgress, taskEnd, mergeTrailingSlash);
                    return folder;
                case ArcFileNode fileNode:
                    var file = CreateFileNode(arcFile, fileNode, taskEnd);
                    return file;
                default:
                    throw new NotImplementedException($"Unable to create node from {arcNode}");
            }
        }

        private static FileNode CreateFileNode(ArcFile arcFile, ArcFileNode arcNode, Action<string> taskEnd)
        {
            // Lazy initialize the shared file list for performance reasons.
            List<string> getSharedFiles() => arcFile.GetSharedFilePaths(arcNode, ApplicationSettings.Instance.ArcRegion);

            var fileNode = new FileNode(arcNode.FileName, arcNode.Path, arcNode.Extension,
                arcNode.IsShared, arcNode.IsRegional, arcNode.Offset, arcNode.CompSize, arcNode.DecompSize, arcNode.IsCompressed,
                getSharedFiles);

            fileNode.FileExtracting += (s, e) => ExtractFileAsync(arcFile, arcNode, taskEnd);

            return fileNode;
        }

        private static ExtractResult TryExtractFile(ArcFile arcFile, ArcFileNode arcNode)
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
                return new ExtractResult { CompletionSummary = $"Error creating directory {exportFileDirectory}", Success = false };
            }

            // Extraction may fail.
            // TODO: Update the C# bindings to store more detailed error info?
            if (!arcFile.TryExtractFile(arcNode, exportPath, ApplicationSettings.Instance.ArcRegion))
            {
                Serilog.Log.Logger.Error("Failed to extract to {@path}", exportPath);
                return new ExtractResult { CompletionSummary = $"Failed to extract {arcNode.Path}", Success = false };
            }

            return new ExtractResult { CompletionSummary = $"Extracted {arcNode.Path}", Success = false };
        }

        private static string GetExportPath(ArcFileNode arcNode)
        {
            var filePath = ArcPaths.GetOsSafePath(arcNode.Path, arcNode.FileName, arcNode.Extension);

            // If the user selects an absolute path for the extract location, this overrides the current directory.
            var exportDirectory = ApplicationDirectory.CreateAbsolutePath(ApplicationSettings.Instance.ExtractLocation);
            var paths = new string[] { exportDirectory };

            // Use the OS directory separators instead of the ARC path separators. 
            var exportPath = Path.Combine(paths.Concat(filePath.Split('/')).ToArray());
            return exportPath;
        }

        private static FolderNode CreateFolder(ArcFile arcFile, ArcDirectoryNode arcNode,
            Action<string> taskStart, Action<string, double> reportProgress, Action<string> taskEnd, bool mergeTrailingSlash)
        {
            var folder = new FolderNode(arcNode.Path, arcNode);
            folder.FileExtracting += (s, e) => ExtractFolderAsync(arcFile, arcNode, taskStart, reportProgress, taskEnd, mergeTrailingSlash);

            return folder;
        }

        public static async void ExtractAllFiles(ArcFile arcFile, Action<string> taskStart, 
            Action<string, double> reportProgress, Action<string> taskEnd, bool mergeTrailingSlash)
        {
            await RunBackgroundTask("Extracting all files", () => TryExtractAllFiles(arcFile, reportProgress, mergeTrailingSlash), taskStart, taskEnd);
        }

        private static async Task RunBackgroundTask(string taskDescription,
            Func<ExtractResult> taskToRun, Action<string> taskStart, Action<string> taskEnd)
        {
            taskStart(taskDescription);

            ExtractResult? result = null;

            await Task.Run(() =>
            {
                using (var operation = Operation.Begin(taskDescription))
                {
                    result = taskToRun();
                    if (result.Success)
                        operation.Complete();
                    else
                        operation.Cancel();
                }
            });

            taskEnd(result?.CompletionSummary ?? "");
        }

        private static void AddFilesRecursive(ArcFile arcFile, ArcDirectoryNode root, List<ArcFileNode> files, bool mergeTrailingSlash)
        {
            if (mergeTrailingSlash)
            {
                var cleanedPath = root.Path.Trim('/');

                // We don't display separate nodes for directories differing only by a trailing slash.
                // Try both options here in case one was removed.
                var rootNoSlash = arcFile.CreateNode(cleanedPath, ApplicationSettings.Instance.ArcRegion);
                if (rootNoSlash is ArcDirectoryNode rootDirectoryNoSlash)
                {
                    AddChildrenRecursive(arcFile, files, rootDirectoryNoSlash, mergeTrailingSlash);
                }

                var rootWithSlash = arcFile.CreateNode(cleanedPath + '/', ApplicationSettings.Instance.ArcRegion);
                if (rootWithSlash is ArcDirectoryNode rootDirectoryWithSlash)
                {
                    AddChildrenRecursive(arcFile, files, rootDirectoryWithSlash, mergeTrailingSlash);
                }
            }
            else
            {
                AddChildrenRecursive(arcFile, files, root, mergeTrailingSlash);
            }
        }

        private static void AddChildrenRecursive(ArcFile arcFile, List<ArcFileNode> files, ArcDirectoryNode parent, bool mergeTrailingSlash)
        {
            foreach (var child in arcFile.GetChildren(parent, ApplicationSettings.Instance.ArcRegion))
            {
                // Assume files have no children, so only recurse for directories.
                switch (child)
                {
                    case ArcFileNode file:
                        files.Add(file);
                        break;
                    case ArcDirectoryNode directory:
                        AddFilesRecursive(arcFile, directory, files, mergeTrailingSlash);
                        break;
                    default:
                        break;
                }
            }
        }

        private static ExtractResult TryExtractAllFiles(ArcFile arcFile, Action<string, double> reportProgress, bool mergeTrailingSlash)
        {
            // Loading the files first uses more memory but allows for determinate progress.
            var filesToExtract = new List<ArcFileNode>();
            foreach (var arcNode in arcFile.GetRootNodes())
            {
                if (arcNode is ArcDirectoryNode directory)
                    AddFilesRecursive(arcFile, directory, filesToExtract, mergeTrailingSlash);
            }

            for (int i = 0; i < filesToExtract.Count; i++)
            {
                TryExtractFile(arcFile, filesToExtract[i]);
                var percentage = i / (double)filesToExtract.Count * 100;

                // Fix the string length to avoid strange flickering when the progress bar resizes.
                var desiredLength = 60;
                reportProgress(filesToExtract[i].Path.PadRight(desiredLength).Substring(0, desiredLength), percentage);
            }

            // Assume the operation succeeds for now.
            // Individual file extractions may still fail.
            var result = new ExtractResult
            {
                Success = true,
                CompletionSummary = $"Extracted {filesToExtract.Count} files"
            };
            return result;
        }

        private static ExtractResult TryExtractFilesRecursive(ArcFile arcFile, ArcDirectoryNode arcNode, Action<string, double> reportProgress, bool mergeTrailingSlash)
        {
            // Loading the files first uses more memory but allows for determinate progress.
            var filesToExtract = new List<ArcFileNode>();
            AddFilesRecursive(arcFile, arcNode, filesToExtract, mergeTrailingSlash);

            for (int i = 0; i < filesToExtract.Count; i++)
            {
                TryExtractFile(arcFile, filesToExtract[i]);
                var percentage = i / (double)filesToExtract.Count * 100;
                reportProgress(filesToExtract[i].Path, percentage);
            }

            // Assume the operation succeeds for now.
            // Individual file extractions may still fail.
            var result = new ExtractResult
            {
                Success = true,
                CompletionSummary = $"Extracted {filesToExtract.Count} files from {arcNode.Path}"
            };
            return result;
        }

        private static async void ExtractFileAsync(ArcFile arcFile, ArcFileNode arcNode, Action<string> taskEnd)
        {
            // Files extract quickly, so there's no need to update the UI by calling taskStart.
            await RunBackgroundTask($"Extracting {arcNode.Path}", () => TryExtractFile(arcFile, arcNode), (_) => { }, taskEnd);
        }

        private static async void ExtractFolderAsync(ArcFile arcFile, ArcDirectoryNode arcNode,
            Action<string> taskStart, Action<string, double> reportProgress, Action<string> taskEnd, bool mergeTrailingSlash)
        {
            await RunBackgroundTask($"Extracting files from {arcNode.Path}", () =>
                TryExtractFilesRecursive(arcFile, arcNode, reportProgress, mergeTrailingSlash), taskStart, taskEnd);
        }
    }
}
