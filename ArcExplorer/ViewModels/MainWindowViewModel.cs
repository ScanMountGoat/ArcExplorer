using ArcExplorer.Logging;
using Avalonia.Collections;
using ReactiveUI;
using SerilogTimings;
using SmashArcNet;
using System;
using ArcExplorer.Tools;
using ArcExplorer.Views;

namespace ArcExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public AvaloniaList<FileNodeBase> Files { get; } = new AvaloniaList<FileNodeBase>();

        public FileNodeBase? SelectedFile
        {
            get => selectedFile;
            set => this.RaiseAndSetIfChanged(ref selectedFile, value);
        }
        private FileNodeBase? selectedFile;

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
                    BackgroundTaskEnd();
                    return;
                }

                arcAfterOpen();
                operation.Complete();
            }

            BackgroundTaskEnd();
        }

        private void InitializeArcFile(string arcPathText)
        {
            if (arcFile == null)
                return;

            FileCount = arcFile.FileCount.ToString();
            ArcPath = arcPathText;
            ArcVersion = arcFile.Version.ToString();

            FileTree.PopulateFileTree(arcFile, Files, BackgroundTaskStart, BackgroundTaskEnd);
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

        public void RebuildFileTree()
        {
            // Clear everything to ensure the proper icons get loaded when changing themes.
            SelectedFile = null;

            // TODO: Preserve the existing directory structure.
            if (arcFile != null)
            {
                FileTree.PopulateFileTree(arcFile, Files, BackgroundTaskStart, BackgroundTaskEnd);
            }
        }

        private void BackgroundTaskStart(string taskDescription)
        {
            // TODO: Correctly update the description for multiple background tasks.
            // The code currently only shows a description for the most recent task.
            IsLoading = true;
            LoadingDescription = taskDescription;
        }

        private void BackgroundTaskEnd()
        {
            LoadingDescription = "";
            IsLoading = false;
        }
    }
}
