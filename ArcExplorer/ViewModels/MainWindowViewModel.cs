using ArcExplorer.Logging;
using ArcExplorer.Models;
using ArcExplorer.Tools;
using Avalonia.Collections;
using ReactiveUI;
using SerilogTimings;
using SmashArcNet;
using SmashArcNet.RustTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // TODO: In the future the version could be loaded from a file updated by a build script.
        public string Title { get; } = "Arc Explorer v1.4.0";

        public AvaloniaList<FileGridItem> Files
        {
            get => files;
            set => this.RaiseAndSetIfChanged(ref files, value);
        }
        private AvaloniaList<FileGridItem> files = new AvaloniaList<FileGridItem>();

        public static Dictionary<Region, string> DescriptionByRegion { get; } = new Dictionary<Region, string>
        {
            { Region.Japanese, "Japanese" },
            { Region.UsEnglish, "English (US)" },
            { Region.UsFrench, "French (US)" },
            { Region.UsSpanish, "Spanish (US)" },
            { Region.EuEnglish, "English (EU)" },
            { Region.EuFrench, "French (EU)" },
            { Region.EuSpanish, "Spanish (EU)" },
            { Region.EuGerman, "German (EU)" },
            { Region.EuDutch, "Dutch (EU)" },
            { Region.EuItalian, "Italian (EU)" },
            { Region.EuRussian, "Russian (EU)" },
            { Region.Korean, "Korean" },
            { Region.ChinaChinese, "Chinese (China)" },
            { Region.TaiwanChinese, "Chinese (Taiwan)" },
        };

        public KeyValuePair<Region, string> SelectedRegion
        {
            get => new KeyValuePair<Region, string>(ApplicationSettings.Instance.ArcRegion, DescriptionByRegion[ApplicationSettings.Instance.ArcRegion]);
            set
            {
                ApplicationSettings.Instance.ArcRegion = value.Key;
                this.RaisePropertyChanged(nameof(SelectedRegion));
            }
        }

        public FileGridItem? SelectedFile
        {
            get => selectedFile;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedFile, value);
                SelectedNode = value?.Node;
            }
        }
        private FileGridItem? selectedFile;

        // Create a second property avoid binding to SelectedFile.Node when SelectedFile is null.
        public FileNodeBase? SelectedNode
        {
            get => selectedNode;
            set => this.RaiseAndSetIfChanged(ref selectedNode, value);
        }
        private FileNodeBase? selectedNode;

        // Use two properties to sync the directory path with the actual directory node.
        // This allows the user to update the currently displayed folder by typing in the absolute path.
        public string? CurrentDirectoryPath
        {
            get => currentDirectoryPath;
            set
            {
                if (arcFile != null && value != currentDirectoryPath)
                {
                    this.RaiseAndSetIfChanged(ref currentDirectoryPath, value);
                    LoadFolder(value);
                }
            }
        }

        public void RefreshIcons()
        {
            // Refresh any theme dependent icons.
            this.RaisePropertyChanged(nameof(SearchIcon));
        }

        private string? currentDirectoryPath;

        public FolderNode? CurrentDirectory
        {
            get => currentDirectory;
            set
            {
                if (value != currentDirectory)
                {
                    currentDirectory = value;
                    CurrentDirectoryPath = currentDirectory?.AbsolutePath;
                }
                this.RaiseAndSetIfChanged(ref currentDirectory, value);
            }
        }
        private FolderNode? currentDirectory;

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

        public bool IsLoading
        {
            get => isLoading;
            set => this.RaiseAndSetIfChanged(ref isLoading, value);
        }
        private bool isLoading = false;

        public bool UseDeterminateProgress
        {
            get => isDeterminate;
            set => this.RaiseAndSetIfChanged(ref isDeterminate, value);
        }
        private bool isDeterminate = true;

        public double ProgressValue
        {
            get => progressValue;
            set => this.RaiseAndSetIfChanged(ref progressValue, value);
        }
        private double progressValue = 0.0;

        public string LoadingDescription
        {
            get => loadingDescription;
            set => this.RaiseAndSetIfChanged(ref loadingDescription, value);
        }
        private string loadingDescription = "";

        public bool HasErrors
        {
            get => hasErrors;
            set => this.RaiseAndSetIfChanged(ref hasErrors, value);
        }
        private bool hasErrors = false;

        public string ErrorDescription
        {
            get => errorDescription;
            set => this.RaiseAndSetIfChanged(ref errorDescription, value);
        }

        private string errorDescription = "";

        public string SearchText
        {
            get => searchText;
            set
            {
                this.RaiseAndSetIfChanged(ref searchText, value);
                SearchArcFile(searchText);
            }
        }
        private string searchText = "";

        public ApplicationStyles.Icon SearchIcon { get; } = ApplicationStyles.Icon.Search;

        private ArcFile? arcFile;

        public MainWindowViewModel()
        {
            ApplicationSink.Instance.Value.ErrorEventRaised += LogEventHandled;

            // TODO: This is expensive and should be handled separately.
            // Run a background task and continue with something to enable opening an ARC?
            var hashesFile = "Hashes.txt";

            if (!HashLabels.TryLoadHashes(hashesFile))
            {
                Serilog.Log.Logger.Error("Failed to open Hashes file {@path}", hashesFile);
            }
        }

        private void LogEventHandled(object? sender, EventArgs e)
        {
            HasErrors = ApplicationSink.Instance.Value.ErrorCount > 0;
            ErrorDescription = $"{ApplicationSink.Instance.Value.ErrorCount} Errors";
        }

        public void OpenArcNetworked(string ipAddress)
        {
            OpenArcBackgroundTask($"Connecting to ARC file at address {ipAddress}", $"Failed to connect to ARC file at address {ipAddress}",
                () => ArcFile.TryOpenArcNetworked(ipAddress, out arcFile),
                () => InitializeArcFile(ipAddress));
        }

        public void OpenArcFile(string path)
        {
            OpenArcBackgroundTask($"Opening ARC file {path}", $"Failed to open ARC file {path}",
                () => ArcFile.TryOpenArc(path, out arcFile),
                () => InitializeArcFile(path));
        }

        private async void OpenArcBackgroundTask(string taskDescription, string errorLogText, Func<bool> tryOpenArc, Action arcAfterOpen)
        {
            BackgroundTaskStart(taskDescription, false);

            using (var operation = Operation.Begin(taskDescription))
            {
                var result = await Task.Run(() => tryOpenArc());
                if (!result)
                {
                    Serilog.Log.Logger.Error(errorLogText);
                    operation.Abandon();
                    BackgroundTaskEnd("");
                    return;
                }

                arcAfterOpen();
                operation.Complete();
            }

            BackgroundTaskEnd("");
        }

        private void InitializeArcFile(string arcPathText)
        {
            if (arcFile == null)
                return;

            FileCount = arcFile.FileCount.ToString();
            ArcPath = arcPathText;
            ArcVersion = arcFile.Version.ToString();

            LoadRootNodes(arcFile);
        }

        private void SearchArcFile(string searchText)
        {
            if (arcFile == null)
                return;

            Files.Clear();
            var nodes = FileTree.SearchAllNodes(arcFile, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd, searchText, ApplicationSettings.Instance.MergeTrailingSlash);
            Files = new AvaloniaList<FileGridItem>(nodes.Select(n => new FileGridItem(n)));
        }

        public void SelectNextFile()
        {
            // TODO: It might be faster to add an additional selected file index.
            if (SelectedFile is null)
            {
                SelectedFile = Files.FirstOrDefault();
            }
            else
            {
                var nextIndex = Files.IndexOf(SelectedFile) + 1;
                if (nextIndex < Files.Count)
                    SelectedFile = Files[nextIndex];
            }
        }

        public void SelectPreviousFile()
        {
            // TODO: It might be faster to add an additional selected file index.
            if (SelectedFile is null)
            {
                SelectedFile = Files.FirstOrDefault();
            }
            else
            {
                var previousIndex = Files.IndexOf(SelectedFile) - 1;
                if (Files.Count != 0 && previousIndex >= 0)
                    SelectedFile = Files[previousIndex];
            }
        }

        public void ExitFolder()
        {
            if (arcFile == null)
                return;

            if (CurrentDirectory == null)
                return;

            var originalPath = CurrentDirectory.AbsolutePath;

            // Load the root if there is no parent.
            string? parentPath = ArcPaths.GetParentPath(CurrentDirectory.AbsolutePath);
            if (parentPath == null)
            {
                LoadRootNodes(arcFile);

                // Select a file to facilitate keyboard navigation.
                // TODO: Handle this automatically within the properties.
                SelectedFile = Files.FirstOrDefault(f => f.AbsolutePath == originalPath);

                return;
            }

            // Go up one level in the file tree.
            var parent = FileTree.CreateNodeFromPath(arcFile, parentPath, 
                BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd, ApplicationSettings.Instance.MergeTrailingSlash);
            if (parent is FolderNode folder)
                LoadFolder(folder);
            else
                LoadRootNodes(arcFile);

            // Select a file to facilitate keyboard navigation.
            SelectedFile = Files.FirstOrDefault(f => f.AbsolutePath == originalPath);
        }

        private void LoadFolder(string? path)
        {
            if (arcFile != null && path != null)
            {
                var parent = FileTree.CreateNodeFromPath(arcFile, path, 
                    BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd, ApplicationSettings.Instance.MergeTrailingSlash);
                LoadFolder(parent as FolderNode);
            }
        }

        private void LoadFolder(FolderNode? parent)
        {
            if (arcFile == null)
                return;

            CurrentDirectory = parent;
            Files.Clear();

            if (CurrentDirectory == null)
                return;

            var newFiles = FileTree.CreateChildNodes(arcFile, CurrentDirectory,
                BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd, ApplicationSettings.Instance.MergeTrailingSlash);

            Files = new AvaloniaList<FileGridItem>(newFiles.Select(n => new FileGridItem(n)));
        }

        private void LoadRootNodes(ArcFile arcFile)
        {
            CurrentDirectory = null;
            Files.Clear();
            var newFiles = FileTree.CreateRootLevelNodes(arcFile,
                BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd,
                ApplicationSettings.Instance.MergeTrailingSlash);
            Files = new AvaloniaList<FileGridItem>(newFiles.Select(n => new FileGridItem(n)));
        }

        public void EnterSelectedFolder()
        {
            if (arcFile == null || SelectedFile == null)
                return;

            if (SelectedFile.Node is FolderNode folder)
            {
                LoadFolder(folder);

                // Select a file to facilitate keyboard navigation.
                SelectedFile = Files.FirstOrDefault();
            }
        }

        public void ErrorClick()
        {
            var window = new Views.LogWindow() { Items = ApplicationSink.Instance.Value.LogMessages };
            window.Show();
        }

        public void ExtractAllFiles()
        {
            if (arcFile == null)
                return;

            FileTree.ExtractAllFiles(arcFile,
                BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd,
                ApplicationSettings.Instance.MergeTrailingSlash);
        }

        public void ReloadCurrentDirectory()
        {
            // Store the paths since the nodes themselves need to be recreated.
            var previousPath = CurrentDirectory?.AbsolutePath;
            var previousSelectedPath = SelectedFile?.AbsolutePath;

            if (arcFile != null && previousPath != null)
            {
                LoadFolder(previousPath);
                SelectedFile = Files.FirstOrDefault(f => f.AbsolutePath == previousSelectedPath);
            }
        }

        public void BackgroundTaskStart(string taskDescription, bool isDeterminate)
        {
            // TODO: Correctly update the description for multiple background tasks.
            // The code currently only shows a description for the most recent task.
            ProgressValue = 0.0;
            IsLoading = true;
            LoadingDescription = taskDescription;
            UseDeterminateProgress = isDeterminate;
        }

        public void BackgroundTaskReportProgress(string description, double progressPercentage)
        {
            // TODO: Correctly update the description for multiple background tasks.
            // The code currently only shows a description for the most recent task.
            IsLoading = true;
            LoadingDescription = description;
            ProgressValue = progressPercentage;
        }

        public void BackgroundTaskEnd(string description)
        {
            ProgressValue = 100.0;
            LoadingDescription = description;
            IsLoading = false;
        }
    }
}
