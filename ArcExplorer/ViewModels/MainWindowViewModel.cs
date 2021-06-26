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

namespace ArcExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public AvaloniaList<FileNodeBase> Files { get; } = new AvaloniaList<FileNodeBase>();

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

        public FileNodeBase? SelectedFile
        {
            get => selectedFile;
            set => this.RaiseAndSetIfChanged(ref selectedFile, value);
        }
        private FileNodeBase? selectedFile;

        // Use two properties to sync the directory path with the actual directory node.
        // This allows the user to update the currently displayed folder by typing in the absolute path.
        public string? CurrentDirectoryPath
        {
            get => currentDirectoryPath;
            set
            { 
                this.RaiseAndSetIfChanged(ref currentDirectoryPath, value);
                if (arcFile != null && value != null)
                {
                    var parent = FileTree.CreateFolderNode(arcFile, value);
                    if (parent != null)
                    {
                        // Update the current directory to allow exiting the folder to work properly.
                        currentDirectory = parent;

                        Files.Clear();
                        var newFiles = FileTree.CreateChildNodes(arcFile, parent, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
                        Files.AddRange(newFiles);
                    }
                }
            }
        }
        private string? currentDirectoryPath;

        public FolderNode? CurrentDirectory 
        { 
            get => currentDirectory;
            set
            {
                currentDirectory = value;
                CurrentDirectoryPath = currentDirectory?.AbsolutePath;
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

        private void OpenArcBackgroundTask(string taskDescription, string errorLogText, Func<bool> tryOpenArc, Action arcAfterOpen)
        {
            BackgroundTaskStart(taskDescription);

            using (var operation = Operation.Begin(taskDescription))
            {
                if (!tryOpenArc())
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

            Files.Clear();
            var newFiles = FileTree.CreateRootLevelNodes(arcFile, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
            Files.AddRange(newFiles);
        }

        public void SelectNextFile()
        {
            if (SelectedFile is null)
            {
                SelectedFile = Files.First();
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
            if (SelectedFile is null)
            {
                SelectedFile = Files.First();
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
                SelectedFile = Files.First(f => f.AbsolutePath == originalPath);

                return;
            }

            var parent = FileTree.CreateFolderNode(arcFile, parentPath);
            if (parent == null)
            {
                LoadRootNodes(arcFile);
            }
            else
            {
                // Go up one level in the file tree.
                CurrentDirectory = parent;
                Files.Clear();
                var newFiles = FileTree.CreateChildNodes(arcFile, CurrentDirectory, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
                Files.AddRange(newFiles);
            }

            // Select a file to facilitate keyboard navigation.
            SelectedFile = Files.First(f => f.AbsolutePath == originalPath);
        }

        private void LoadRootNodes(ArcFile arcFile)
        {
            CurrentDirectory = null;
            Files.Clear();
            var newFiles = FileTree.CreateRootLevelNodes(arcFile, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
            Files.AddRange(newFiles);
        }

        public void EnterSelectedFolder()
        {
            if (arcFile == null)
                return;

            if (SelectedFile is FolderNode folder)
            {
                CurrentDirectory = folder;
                Files.Clear();
                var newFiles = FileTree.CreateChildNodes(arcFile, CurrentDirectory, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
                Files.AddRange(newFiles);

                // Select a file to facilitate keyboard navigation.
                SelectedFile = newFiles.First();
            }
        }


        public void ErrorClick()
        {
            var window = new Views.LogWindow() { Items = ApplicationSink.Instance.Value.LogMessages };
            window.Show();
        }

        public void ExtractSelectedNode()
        {
            SelectedFile?.OnFileExtracting();
        }

        public void ExtractAllFiles()
        {
            if (arcFile == null)
                return;

            FileTree.ExtractAllFiles(arcFile, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
        }

        public void ReloadCurrentDirectory()
        {
            // Store the paths since the nodes themselves need to be recreated.
            var previousPath = CurrentDirectory?.AbsolutePath;
            var previousSelectedPath = SelectedFile?.AbsolutePath;

            if (arcFile != null && previousPath != null)
            {
                var parent = FileTree.CreateFolderNode(arcFile, previousPath);
                if (parent != null)
                {
                    Files.Clear();
                    var newFiles = FileTree.CreateChildNodes(arcFile, parent, BackgroundTaskStart, BackgroundTaskReportProgress, BackgroundTaskEnd);
                    Files.AddRange(newFiles);

                    SelectedFile = Files.First(f => f.AbsolutePath == previousSelectedPath);
                }
            }
        }

        public void BackgroundTaskStart(string taskDescription)
        {
            // TODO: Correctly update the description for multiple background tasks.
            // The code currently only shows a description for the most recent task.
            ProgressValue = 0.0;
            IsLoading = true;
            LoadingDescription = taskDescription;
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
