using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ArcExplorer.ViewModels;
using ArcExplorer.Views;
using Serilog;
using ArcExplorer.Logging;
using ArcExplorer.Tools;
using SmashArcNet;
using System.Threading.Tasks;
using SerilogTimings;
using Avalonia.Controls;

namespace ArcExplorer
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            ApplicationStyles.SetThemeFromSettings();
            ConfigureAndStartLogging();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new MainWindowViewModel();

                desktop.MainWindow = new MainWindow
                {
                    ViewModel = vm,
                    WindowState = Models.ApplicationSettings.Instance.StartMaximized ? WindowState.Maximized : WindowState.Normal
                };

                // Update hashes first since this downloads the file if not found.
                await UpdateHashesFromGithub(desktop.MainWindow, vm);

                // TODO: This is expensive and should be handled separately.
                // Run a background task and continue with something to enable opening an ARC?
                var hashesFile = ApplicationDirectory.CreateAbsolutePath("Hashes.txt");
                if (!HashLabels.TryLoadHashes(hashesFile))
                {
                    Log.Logger.Error("Failed to open Hashes file {@path}", hashesFile);
                }

                if (!string.IsNullOrEmpty(Models.ApplicationSettings.Instance.ArcStartupLocation))
                {
                    vm.OpenArcFile(Models.ApplicationSettings.Instance.ArcStartupLocation);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static async Task UpdateHashesFromGithub(Window window, MainWindowViewModel vm)
        {
            var hasHashes = System.IO.File.Exists(ApplicationDirectory.CreateAbsolutePath("Hashes.txt"));
            if (!hasHashes)
            {
                // No Hashes.txt is present, so we need to download one.
                // This will be the case when launching the application for the first time.
                var latestCommit = await HashLabelUpdater.GetCurrentCommit();
                if (latestCommit != null)
                {
                    vm.BackgroundTaskStart("Updating hashes", false);

                    await DownloadHashesFile();
                    LoadNewHashes(latestCommit);

                    vm.BackgroundTaskEnd("");
                }
            }
            else
            {
                // Update the time for the last update check to ensure updates are only checked once per day.
                // This avoids rate limits with the Github API used to check for updates.
                var currentTime = System.DateTime.UtcNow;
                var lastCheckTime = Models.ApplicationSettings.Instance.LastHashesUpdateCheckTime.Date;

                if (lastCheckTime.Date >= currentTime.Date)
                    return;

                // Check if a newly updated Hashes.txt is available and prompt downloading it.
                var latestCommit = await HashLabelUpdater.TryFindNewerHashesCommit();
                if (latestCommit != null)
                {
                    var author = latestCommit.Commit.Author.Name;
                    var date = latestCommit.Commit.Author.Date.ToString();
                    var message = latestCommit.Commit.Message;
                    Log.Logger.Information("Found a hashes update. Author: {@author}, Date: {@date}, Message: {@message}", author, date, message);

                    var dialog = new HashUpdateDialog()
                    {
                        ViewModel = new HashUpdateDialogViewModel(message, author, date)
                    };
                    dialog.Closed += async (s, e) =>
                    {
                        if (dialog.ViewModel?.WasCancelled == false)
                        {
                            vm.BackgroundTaskStart("Updating hashes", false);

                            await DownloadHashesFile();
                            LoadNewHashes(latestCommit);

                            vm.BackgroundTaskEnd("");
                        }
                    };

                    await dialog.ShowDialog(window);
                }
            }
        }

        private static void LoadNewHashes(Octokit.GitHubCommit latestCommit)
        {
            using (var updateHashes = Operation.Begin("Updating hashes"))
            {
                if (!HashLabels.TryLoadHashes(HashLabelUpdater.HashesPath))
                {
                    Log.Logger.Error("Failed to open hashes file {@path}", HashLabelUpdater.HashesPath);
                    updateHashes.Cancel();
                }

                // Keep track of what version was downloaded last and when it was downloaded.
                // This helps prevent downloading the same file twice and hitting API rate limits.
                Models.ApplicationSettings.Instance.LastHashesUpdateCheckTime = System.DateTime.UtcNow;
                Models.ApplicationSettings.Instance.CurrentHashesCommitSha = latestCommit.Sha;

                updateHashes.Complete();
            }
        }

        private static async Task DownloadHashesFile()
        {
            using (var downloadHashes = Operation.Begin("Downloading latest hashes"))
            {
                await HashLabelUpdater.DownloadHashes(HashLabelUpdater.HashesPath);
                downloadHashes.Complete();
            }
        }

        private static void ConfigureAndStartLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(ApplicationDirectory.CreateAbsolutePath("log.txt"))
                .WriteTo.ApplicationLog()
                .CreateLogger();
        }
    }
}
