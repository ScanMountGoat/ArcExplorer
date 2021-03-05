﻿using Avalonia;
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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };

                await UpdateHashesFromGithub(desktop);
            }
            base.OnFrameworkInitializationCompleted();
        }

        private static async Task UpdateHashesFromGithub(IClassicDesktopStyleApplicationLifetime desktop)
        {
            // TODO: Don't hardcode the hashes path.
            var canUpdate = await HashLabelUpdater.Instance.CanUpdateHashes("Hashes.txt");

            if (canUpdate)
            {
                var latestCommit = HashLabelUpdater.Instance.LatestHashesCommit;
                if (latestCommit == null)
                    return;

                Log.Logger.Information("Found a hashes update. Author: {@author}, Date: {@date}, Message: {@message}", latestCommit.Author.Name, 
                    latestCommit.Author.Date.ToString(), latestCommit.Message);

                var dialog = new HashUpdateDialog(latestCommit.Message, latestCommit.Author.Name, latestCommit.Author.Date.ToString());
                dialog.Closed += async (s, e) =>
                {
                    if (!dialog.WasCancelled)
                    {
                        using var downloadHashes = Operation.Begin("Downloading latest hashes");
                        await HashLabelUpdater.Instance.DownloadHashes("Hashes.txt");
                        downloadHashes.Complete();

                        using var updateHashes = Operation.Begin("Updating hashes");
                        if (!HashLabels.TryLoadHashes("Hashes.txt"))
                        {
                            Log.Logger.Error("Failed to open Hashes file {@path}", "Hashes.txt");
                            updateHashes.Cancel();
                        }
                        updateHashes.Complete();
                    }
                };

                await dialog.ShowDialog(desktop.MainWindow);
            }
        }

        private static void ConfigureAndStartLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt")
                .WriteTo.ApplicationLog()
                .CreateLogger();
        }
    }
}
