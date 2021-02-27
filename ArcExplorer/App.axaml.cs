using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ArcExplorer.ViewModels;
using ArcExplorer.Views;
using Serilog;
using ArcExplorer.Logging;
using ArcExplorer.Tools;

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

                // TODO: Wrap this code in a method.
                // TODO: Don't hardcode the hashes path.
                var canUpdate = HashLabelUpdater.Instance.CanUpdateHashes("Hashes.txt");
                //if (canUpdate)
                {
                    // TODO: Show a dialog?
                    var latestCommit = HashLabelUpdater.Instance.LatestHashesCommit;
                    var dialog = new HashUpdateDialog(latestCommit?.Message ?? "", latestCommit?.Author.Name ?? "", latestCommit?.Author.Date.ToString() ?? "");
                    dialog.Closed += (s, e) =>
                    {
                        if (!dialog.WasCancelled)
                        {
                            // TODO: Make this async?
                            HashLabelUpdater.Instance.UpdateHashes("Hashes.txt");
                            // TODO: Actually update the hashes.
                        }
                    };
                    await dialog.ShowDialog(desktop.MainWindow);
                }
            }
            base.OnFrameworkInitializationCompleted();
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
